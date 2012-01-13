using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using MultipleClipboards.Entities;
using MultipleClipboards.Interop;
using MultipleClipboards.Persistence;
using SendKeys = System.Windows.Forms.SendKeys;

namespace MultipleClipboards.ClipboardManagement
{
	/// <summary>
	/// Class to manage clipboard entries over multiple clipboards.
	/// </summary>
	public class ClipboardManager : IDisposable
	{
		private static ClipboardManager _clipboardManager;
		private readonly bool _isInitialized;

		/// <summary>
		/// Gets the application-wide instance of the clipboard manager.
		/// </summary>
		public static ClipboardManager Instance
		{
			get
			{
				if (_clipboardManager == null || !_clipboardManager._isInitialized)
				{
					throw new InvalidOperationException("An attempt was made to use the application-wide instance of the ClipboardManager class before it has been initialized.  You must call the static initialize method prior to using the static instance of this class.");
				}
				return _clipboardManager;
			}
		}

		/// <summary>
		/// Initializes the application-wide instance of the clipboard manager for use with the given window handle.
		/// </summary>
		/// <param name="windowHandle">The handle to the main window of the application.</param>
		public static void Initialize(IntPtr windowHandle)
		{
			if (_clipboardManager != null && _clipboardManager._isInitialized)
			{
				throw new InvalidOperationException("An attempt was made to initialize the application-wide instance of the ClipboardManager class after is has already been initialized.  The application-wide instance of this class can only be initialized once.");
			}

			_clipboardManager = new ClipboardManager(windowHandle);
		}

		/// <summary>
		/// Constructs a new Clipboard Manager object for use with the given window handle.
		/// </summary>
		/// <param name="windowHandle">The handle of the window using this clipboard manager.</param>
		public ClipboardManager(IntPtr windowHandle)
		{
			SettingsManager.Instance.ClipboardDefinitions.CollectionChanged += this.ClipboardDefinitions_CollectionChanged;
			this.WindowHandle = windowHandle;
			this.HotKeys = new List<HotKey>();
			this.ClipboardHistory = new ObservableCollection<ClipboardData>();
			this.ClipboardDataByClipboardId = new Dictionary<int, ClipboardData>();

			this.PopulateAvailableClipboardList();
			this.RegisterAllClipboards();
			this.PreserveClipboardData();

			if (this.CurrentSystemClipboardData.Formats.Count() > 0)
			{
				this.EnqueueHistoricalEntry(this.CurrentSystemClipboardData);
			}

			this._isInitialized = true;
			LogManager.Debug("ClipboardManager initialized.  All hot keys are registered.");
		}

		/// <summary>
		/// Disposes of this clipboard manager instance.
		/// </summary>
		public void Dispose()
		{
			foreach (short hotKeyId in this.HotKeys.Select(hk => hk.HotKeyId))
			{
				this.UnRegisterHotKey(hotKeyId);
			}
			LogManager.Debug("ClipboardManager destroyed.  All hot keys have been un-registered.");
		}

		/// <summary>
		/// Gets the collection that holds all the historical clipboard entries.
		/// </summary>
		public ObservableCollection<ClipboardData> ClipboardHistory
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the Clipboards by Index dictionary.
		/// </summary>
		public Dictionary<int, ClipboardData> ClipboardDataByClipboardId
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the collection of clipboards available to the user.
		/// </summary>
		public IList<ClipboardDefinition> AvailableClipboards
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets the handle to the window that is using this instance of the Clipboard Manager.
		/// </summary>
		protected IntPtr WindowHandle
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the list of HotKeys registered by the application.
		/// </summary>
		protected List<HotKey> HotKeys
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the existing data on the clipboard
		/// </summary>
		protected ClipboardData CurrentSystemClipboardData
		{
			get
			{
				return this.ClipboardDataByClipboardId[ClipboardDefinition.SystemClipboardDefinition.ClipboardId];
			}
			set
			{
				this.ClipboardDataByClipboardId[ClipboardDefinition.SystemClipboardDefinition.ClipboardId] = value;
			}
		}

		/// <summary>
		/// Adds a new clipboard to be managed.
		/// </summary>
		/// <param name="clipboard">The clipboard to add.</param>
		public void AddClipboard(ClipboardDefinition clipboard)
		{
            SettingsManager.Instance.AddNewClipboard(clipboard);
			LogManager.DebugFormat("New clipboard added:\r\n{0}", clipboard);
			this.RegisterClipboard(clipboard);
		}

        public void RemoveClipboard(ClipboardDefinition clipboard)
        {
			this.UnRegisterHotKeysForClipboard(clipboard.ClipboardId);
        	this.ClipboardDataByClipboardId.Remove(clipboard.ClipboardId);
            SettingsManager.Instance.RemoveClipboard(clipboard);
			LogManager.DebugFormat("Clipboard removed:\r\n{0}", clipboard);
        }

		/// <summary>
		/// Stores the current contents of the Windows clipboard in the history queue.
		/// </summary>
		public void StoreClipboardContents()
		{
			// TODO: This same code exists in the CutCopy() method.  Figure out what the best way to consolidate it is.
			this.ClipboardDataByClipboardId[ClipboardDefinition.SystemClipboardDefinition.ClipboardId] = RetrieveDataFromClipboard();
			this.EnqueueHistoricalEntry(this.ClipboardDataByClipboardId[ClipboardDefinition.SystemClipboardDefinition.ClipboardId]);
		}

		/// <summary>
		/// Places the specified item from the clipboard history queue on the specified clipboard.
		/// </summary>
		/// <param name="clipboardId">The Id of the clipboard to place the historical data on.</param>
		/// /// <param name="clipboardDataId">The Id of the clipboard entry to place on a clipboard.</param>
		public void PlaceHistoricalEntryOnClipboard(int clipboardId, ulong clipboardDataId)
		{
			ClipboardData clipboardEntry = this.ClipboardHistory.FirstOrDefault(data => data.Id == clipboardDataId);

			if (clipboardId == ClipboardDefinition.SystemClipboardDefinition.ClipboardId)
			{
				// Put data on the windows clipboard.
				this.CurrentSystemClipboardData = clipboardEntry;
				this.RestoreClipboardData();
			}
			else
			{
				// Put the data on the specified clipboard.
				this.ClipboardDataByClipboardId[clipboardId] = clipboardEntry;
			}
		}

		/// <summary>
		/// Clears the contents of the clipboard history queue.
		/// </summary>
		public void ClearClipboardHistory()
		{
			this.ClipboardHistory.Clear();
		}

		/// <summary>
		/// Processes a hot key action.
		/// Called from the form when a registered hotkey is pressed.
		/// </summary>
		/// <param name="systemHotKey">The hot key that was pressed.</param>
		public void ProcessHotKey(HotKey systemHotKey)
		{
			LogManager.DebugFormat("About to process HotKey: {0}", systemHotKey);

			// 1) Find the matching hotkey in the local collection to get the Clipboard ID and Operation
			// 2) Switch on the operation for this specific key
			HotKey hotKey = this.HotKeys.Single(h => h == systemHotKey);

			switch (hotKey.HotKeyType)
			{
				case HotKeyType.Cut:
					this.CutCopy(hotKey.ClipboardId, HotKeyType.Cut);
					break;

				case HotKeyType.Copy:
					this.CutCopy(hotKey.ClipboardId, HotKeyType.Copy);
					break;

				case HotKeyType.Paste:
					this.Paste(hotKey.ClipboardId);
					break;

				default:
					throw new InvalidOperationException(string.Format("The HotKeyType '{0}' is not supported.", hotKey.HotKeyType));
			}

			LogManager.DebugFormat("Finished processing HotKey: {0}", hotKey);
		}

		/// <summary>
		/// Called when the clipboard definition collection is modified.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClipboardDefinitions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.PopulateAvailableClipboardList();
		}

		/// <summary>
		/// Fills the available clipboard list with all the clipboard definitions currently available to the user.
		/// </summary>
		private void PopulateAvailableClipboardList()
		{
			if (this.AvailableClipboards == null)
			{
				this.AvailableClipboards = new List<ClipboardDefinition>();
			}
			else
			{
				this.AvailableClipboards.Clear();
			}

			this.AvailableClipboards.Add(ClipboardDefinition.SystemClipboardDefinition);

			foreach (ClipboardDefinition clipboard in SettingsManager.Instance.ClipboardDefinitions)
			{
				this.AvailableClipboards.Add(clipboard);
			}
		}

		/// <summary>
		/// Registers all the hotkeys associated with the all the currently registered clipboards.
		/// </summary>
		private void RegisterAllClipboards()
		{
			// TODO: Capture any errors here (NOT try catch), build a report card of what went wrong, and figure out a way to return it to the UI.
			this.ClipboardDataByClipboardId.Add(ClipboardDefinition.SystemClipboardDefinition.ClipboardId, null);

			foreach (ClipboardDefinition clipboard in SettingsManager.Instance.ClipboardDefinitions)
			{
				this.RegisterClipboard(clipboard);
			}
		}

		private void RegisterClipboard(ClipboardDefinition clipboard)
		{
			// Create a new dictionary item for this clipboard ID.
			// This is the local copy of the item currently stored on the clipbaord that goes with this set of hotkeys.
			this.ClipboardDataByClipboardId.Add(clipboard.ClipboardId, null);

			HotKey cutHotKey = new HotKey(clipboard.ClipboardId, HotKeyType.Cut, clipboard.CutKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey);
			this.RegisterHotKey(cutHotKey);

			HotKey copyHotKey = new HotKey(clipboard.ClipboardId, HotKeyType.Copy, clipboard.CopyKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey);
			this.RegisterHotKey(copyHotKey);

			HotKey pasteHotKey = new HotKey(clipboard.ClipboardId, HotKeyType.Paste, clipboard.PasteKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey);
			this.RegisterHotKey(pasteHotKey);
		}

		/// <summary>
		/// Registers a the given hot key.
		/// </summary>
		/// <param name="hotKey">The hot key to register.</param>
		/// <returns>A value indicating whether or not the HotKey was registered successfully.</returns>
		private bool RegisterHotKey(HotKey hotKey)
		{
			try
			{
				// Use the GlobalAddAtom API to get a unique ID (as suggested by MSDN docs).
				short hotKeyId = Win32API.GlobalAddAtom(hotKey.ToString());
				if (hotKeyId == 0)
				{
					LogManager.ErrorFormat("Unable to generate unique hotkey ID by using the string '{0}'. Error code: {1}", hotKey, Marshal.GetLastWin32Error());
					return false;
				}
				hotKey.HotKeyId = hotKeyId;

				// Register the hotkey.
				int modifierBitmask = hotKey.ModifierBitmask;
				if (Constants.SupportsNoRepeat)
				{
					modifierBitmask |= Win32API.MOD_NOREPEAT;
				}

				if (Win32API.RegisterHotKey(this.WindowHandle, hotKeyId, modifierBitmask, hotKey.KeyCode) == 0)
				{
					LogManager.ErrorFormat("Unable to register hotkey combination: {0}.  ErrorCode: {1}", hotKey, Marshal.GetLastWin32Error());
					return false;
				}

				this.HotKeys.Add(hotKey);
				LogManager.DebugFormat("New HotKey registered: {0}", hotKey);
				return true;
			}
			catch (Exception e)
			{
				// Clean up if hotkey registration failed.
				LogManager.ErrorFormat("Unable to register hotkey combination: {0}", e, hotKey);
				this.UnRegisterHotKey(hotKey.HotKeyId);
				return false;
			}
		}

		/// <summary>
		/// Un-Registers all the hot keys for the given clipboard.
		/// </summary>
		/// <param name="clipboardId">The ID of the clipboard whose hot keys to un-register.</param>
		private void UnRegisterHotKeysForClipboard(int clipboardId)
		{
			foreach (short hotKeyId in this.HotKeys.Where(hk => hk.ClipboardId  == clipboardId).Select(hk => hk.HotKeyId))
			{
				this.UnRegisterHotKey(hotKeyId);
			}
		}

		/// <summary>
		/// Un-Registers the given hotkey.
		/// </summary>
		/// <param name="hotKeyId">The hot key to unregister.</param>
		private void UnRegisterHotKey(short hotKeyId)
		{
			if (hotKeyId != 0)
			{
				Win32API.UnregisterHotKey(this.WindowHandle, hotKeyId);
				Win32API.GlobalDeleteAtom(hotKeyId);
			}
		}

		/// <summary>
		/// Enqueues the given entry in the clipboard history queue.
		/// </summary>
		/// <param name="clipboardData">The clipboard data to enqueue.</param>
		private void EnqueueHistoricalEntry(ClipboardData clipboardData)
		{
			if (this.ClipboardHistory.Count == SettingsManager.Instance.NumberOfClipboardHistoryRecords)
			{
				this.ClipboardHistory.RemoveAt(0);
			}
			this.ClipboardHistory.Add(new ClipboardData(clipboardData));
		}

		/// <summary>
		/// Handles the cut and copy operation.
		/// </summary>
		/// <param name="clipboardId">The ID of the clipboard that the operation is for.</param>
		/// <param name="hotKeyType">The operation to perform.</param>
		private void CutCopy(int clipboardId, HotKeyType hotKeyType)
		{
			this.PreserveClipboardData();

			// Send the system cut or copy command to get the new data on the clipboard.
			SendKeys.SendWait(hotKeyType.ToSendKeysCode());

			// According the MSDN (and my own experiences) all the SendKeys methods are subject to timing issues.
			// This seems to work for me, but sometimes the SendWait function returns before the new data has actually been placed on the clipboard.
			// When this happens you wind up with the original clipboard data still on the clipboard, but also stored in whatever clipboard matches the hotkey that we're processing.
			// Just to be safe, have the program sleep for a fraction of a second before trying to retrieve the new clipboard data.
			// This shouldn't yield any noticable delay.
			Thread.Sleep(SettingsManager.Instance.ThreadDelayTime);

			try
			{
				// Store the new data in the correct clipboard.
				this.ClipboardDataByClipboardId[clipboardId] = RetrieveDataFromClipboard();

				// Store this in the clipboard history list.
				this.EnqueueHistoricalEntry(this.ClipboardDataByClipboardId[clipboardId]);
			}
			finally
			{
				this.RestoreClipboardData();	 
			}
		}

		/// <summary>
		/// Handles the paste operation.
		/// </summary>
		/// <param name="clipboardId">The ID of the clipboard that the operation is for.</param>
		private void Paste(int clipboardId)
		{
			this.PreserveClipboardData();
			try
			{
				bool sendPasteSignal = true;
				
				// Place the data from the correct clipboard onto the system clipboard.
				if (this.ClipboardDataByClipboardId[clipboardId] == null)
				{
					sendPasteSignal = false;
				}
				else
				{
					PutDataOnClipboard(this.ClipboardDataByClipboardId[clipboardId]);
				}

				// Send the system paste command.
				if (sendPasteSignal)
				{
					SendKeys.SendWait(HotKeyType.Paste.ToSendKeysCode());
				}

				// A little delay for the same reason as in CutCopy.
				Thread.Sleep(SettingsManager.Instance.ThreadDelayTime);
			}
			finally
			{
				this.RestoreClipboardData();
			}
		}

		/// <summary>
		/// Preserves the existing data on the clipboard.
		/// </summary>
		private void PreserveClipboardData()
		{
			this.CurrentSystemClipboardData = RetrieveDataFromClipboard();
		}

		/// <summary>
		/// Restores the clipboard to its original state.
		/// </summary>
		private void RestoreClipboardData()
		{
			PutDataOnClipboard(this.CurrentSystemClipboardData);
		}

		private static void PutDataOnClipboard(ClipboardData clipboardData)
		{
			PerformClipboardOperation(() => Clipboard.SetDataObject(clipboardData.DataObject, true));
			LogManager.DebugFormat("The following data was just placed on the clipboard:\r\n\t{0}", clipboardData.ToShortDisplayString());
		}

		private static ClipboardData RetrieveDataFromClipboard()
		{
			ClipboardData clipboardData = null;
			PerformClipboardOperation(() => clipboardData = new ClipboardData(Clipboard.GetDataObject()));

			LogManager.DebugFormat("The following data was just retrieved from the clipboard:\r\n\t{0}", clipboardData == null ? "NULL" : clipboardData.ToShortDisplayString());
			return clipboardData;
		}

		private static void PerformClipboardOperation(Action action)
		{
			int numberOfTries = 1;
			while (true)
			{
				try
				{
					LogManager.DebugFormat("Attempting Clipboard Action.  Try #{0}.", numberOfTries);
					action();
					break;
				}
				catch (COMException comException)
				{
					if (numberOfTries >= SettingsManager.Instance.NumberOfClipboardOperationRetries)
					{
						throw;
					}
					LogManager.ErrorFormat("Attempt #{0} at performing a clipboard operation resulted in the following error.  Trying again in {1}ms.", comException, numberOfTries, SettingsManager.Instance.ThreadDelayTime);
					Thread.Sleep(SettingsManager.Instance.ThreadDelayTime);
					numberOfTries++;
				}
			}
		}
	}
}
