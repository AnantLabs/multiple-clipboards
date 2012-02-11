using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using MultipleClipboards.Entities;
using MultipleClipboards.Exceptions;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Interop;
using log4net;
using SendKeys = System.Windows.Forms.SendKeys;

namespace MultipleClipboards.ClipboardManagement
{
	/// <summary>
	/// Class to manage clipboard entries over multiple clipboards.
	/// </summary>
	public class ClipboardManager : IDisposable
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ClipboardManager));

		/// <summary>
		/// Constructs a new Clipboard Manager object for use with the given window handle.
		/// </summary>
		/// <param name="windowHandle">The handle of the window using this clipboard manager.</param>
		public ClipboardManager(IntPtr windowHandle)
		{
			AppController.Settings.ClipboardDefinitions.CollectionChanged += this.ClipboardDefinitionsCollectionChanged;
			this.WindowHandle = windowHandle;
			this.HotKeys = new List<HotKey>();
			this.ClipboardHistory = new ObservableCollection<ClipboardData>();
			this.ClipboardDataByClipboardId = new Dictionary<int, ClipboardData>();
			this.AllowStoreClipboardContents = true;

			this.PopulateAvailableClipboardList();
			this.RegisterAllClipboards();
			this.PreserveClipboardData();

			if (this.CurrentSystemClipboardData.Formats.Count() > 0)
			{
				this.EnqueueHistoricalEntry(this.CurrentSystemClipboardData);
			}

			log.Debug("ClipboardManager initialized.  All hot keys are registered.");
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
			log.Debug("ClipboardManager destroyed.  All hot keys have been un-registered.");
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
		/// Gets or sets a value indicating whether or not the current contents of the clipboard should be stored.
		/// </summary>
		protected bool AllowStoreClipboardContents
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
				return this.ClipboardDataByClipboardId[ClipboardDefinition.SystemClipboardId];
			}
			set
			{
				this.ClipboardDataByClipboardId[ClipboardDefinition.SystemClipboardId] = value;
			}
		}

		/// <summary>
		/// Adds a new clipboard to be managed.
		/// </summary>
		/// <param name="clipboard">The clipboard to add.</param>
		public void AddClipboard(ClipboardDefinition clipboard)
		{
			if (AppController.Settings.ClipboardDefinitions.Any(c => c == clipboard))
			{
				// TODO: Once the messaging / notification system is in place post a message here.
				return;
			}

            AppController.Settings.AddNewClipboard(clipboard);
			log.DebugFormat("AddClipboard(): New clipboard added:\r\n{0}", clipboard);
			this.RegisterClipboard(clipboard);
		}

        public void RemoveClipboard(ClipboardDefinition clipboard)
        {
			this.UnRegisterHotKeysForClipboard(clipboard.ClipboardId);
        	this.ClipboardDataByClipboardId.Remove(clipboard.ClipboardId);
            AppController.Settings.RemoveClipboard(clipboard);
			log.DebugFormat("RemoveClipboard(): Clipboard removed:\r\n{0}", clipboard);
        }

		/// <summary>
		/// Stores the current contents of the Windows clipboard in the history queue.
		/// </summary>
		public void StoreClipboardContents()
		{
			if (!this.AllowStoreClipboardContents)
			{
				return;
			}
			
			var clipboardData = RetrieveDataFromClipboard();
			this.CurrentSystemClipboardData = clipboardData;
			this.EnqueueHistoricalEntry(clipboardData);
		}

		/// <summary>
		/// Places the specified item from the clipboard history queue on the specified clipboard.
		/// </summary>
		/// <param name="clipboardId">The Id of the clipboard to place the historical data on.</param>
		/// /// <param name="clipboardDataId">The Id of the clipboard entry to place on a clipboard.</param>
		public void PlaceHistoricalEntryOnClipboard(int clipboardId, ulong clipboardDataId)
		{
			try
			{
				this.AllowStoreClipboardContents = false;
				ClipboardData clipboardEntry = this.ClipboardHistory.FirstOrDefault(data => data.Id == clipboardDataId);

				if (clipboardEntry != null)
				{
					// Put the data on the specified clipboard.
					this.ClipboardDataByClipboardId[clipboardId] = clipboardEntry;

					if (clipboardId == ClipboardDefinition.SystemClipboardId)
					{
						this.RestoreClipboardData();
					}
				}
			}
			finally
			{
				this.AllowStoreClipboardContents = true;
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
		/// <param name="processHotKeyArguments">The process hot key arguments.</param>
		public void ProcessHotKey(object processHotKeyArguments)
		{
			var arguments = processHotKeyArguments as ProcessHotKeyArguments;

			if (arguments == null)
			{
				log.Error("ProcessHotKey(): Unable to process hot key because the arguments passed to ProcessHotKey are not of type ProcessHotKeyArguments.");
				return;
			}

			try
			{
				log.DebugFormat("ProcessHotKey(): About to process HotKey: {0}", arguments.HotKey);

				// 1) Find the matching hotkey in the local collection to get the Clipboard ID and Operation
				// 2) Switch on the operation for this specific key
				HotKey hotKey = this.HotKeys.Single(h => h == arguments.HotKey);

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

				log.DebugFormat("ProcessHotKey(): Finished processing HotKey: {0}", hotKey);
			}
			catch (Exception e)
			{
				log.Error("Unexpected error while processing hot key.", e);
			}
			finally
			{
				arguments.Callback();
			}
		}

		/// <summary>
		/// Called when the clipboard definition collection is modified.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClipboardDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

			foreach (ClipboardDefinition clipboard in AppController.Settings.ClipboardDefinitions)
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
			this.ClipboardDataByClipboardId.Add(ClipboardDefinition.SystemClipboardId, null);

			foreach (ClipboardDefinition clipboard in AppController.Settings.ClipboardDefinitions)
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
					log.ErrorFormat("RegisterHotKey(): Unable to generate unique hotkey ID by using the string '{0}'. Error code: {1}", hotKey, Marshal.GetLastWin32Error());
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
					log.ErrorFormat("RegisterHotKey(): Unable to register hotkey combination: {0}.  ErrorCode: {1}", hotKey, Marshal.GetLastWin32Error());
					return false;
				}

				this.HotKeys.Add(hotKey);
				log.DebugFormat("RegisterHotKey(): New HotKey registered: {0}", hotKey);
				return true;
			}
			catch (Exception e)
			{
				// Clean up if hotkey registration failed.
				log.Error(string.Format("RegisterHotKey(): Unable to register hotkey combination: {0}", hotKey), e);
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
			if (this.ClipboardHistory.Count == AppController.Settings.NumberOfClipboardHistoryRecords)
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
			log.DebugFormat("CutCopy(): Sending {0} command via SendKeys.", hotKeyType);
			SendKeys.SendWait(hotKeyType.ToSendKeysCode());

			// According the MSDN (and my own experiences) all the SendKeys methods are subject to timing issues.
			// This seems to work for me, but sometimes the SendWait function returns before the new data has actually been placed on the clipboard.
			// When this happens you wind up with the original clipboard data still on the clipboard, but also stored in whatever clipboard matches the hotkey that we're processing.
			// Just to be safe, have the program sleep for a fraction of a second before trying to retrieve the new clipboard data.
			// This shouldn't yield any noticable delay.
			Thread.Sleep(AppController.Settings.ThreadDelayTime);

			// It is essential this this check comes AFTER the initial delay.
			// The initial delay allows SendKeys to do its thing, but there's no telling what other applications are going to do with the
			// Draw Clipboard message that is sent as a result.  Some applications (Google Chrome for example) hold the clipboard open for a LONG time.
			// Therefore, to ensure clipboard data integrity we must wait for exclusive access.
			WaitForExclusiveClipboardAccess();
			log.Debug("CutCopy(): Now have exclusive clipboard access.");

			try
			{
				// Store the new data in the correct clipboard.
				this.ClipboardDataByClipboardId[clipboardId] = RetrieveDataFromClipboard();

				// Store this in the clipboard history list.
				// This has to be done on the UI thread since it is an observable collection.
				Application.Current.Dispatcher.Invoke(new Action<ClipboardData>(this.EnqueueHistoricalEntry), this.ClipboardDataByClipboardId[clipboardId]);
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
					// Wait for exclusive access to avoid COMExceptions if the clipboard is in use.
					// This is much less important (possibly even unnecessary) than the CutCopy case because sending Ctrl + V
					// does not result in any Win32 messages that I know of.
					WaitForExclusiveClipboardAccess();
					log.Debug("Paste(): Now have exclusive clipboard access.");
					PutDataOnClipboard(this.ClipboardDataByClipboardId[clipboardId]);
				}

				// Send the system paste command.
				if (sendPasteSignal)
				{
					log.Debug("Paste(): Sending paste command via SendKeys.");
					SendKeys.SendWait(HotKeyType.Paste.ToSendKeysCode());
				}

				// A little delay for the same reason as in CutCopy.
				Thread.Sleep(AppController.Settings.ThreadDelayTime);
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
			log.Debug("PreserveClipboardData(): About to preserve system clipboard data.");
			this.CurrentSystemClipboardData = RetrieveDataFromClipboard();
		}

		/// <summary>
		/// Restores the clipboard to its original state.
		/// </summary>
		private void RestoreClipboardData()
		{
			log.Debug("RestoreClipboardData(): About to restore system clipboard data.");
			PutDataOnClipboard(this.CurrentSystemClipboardData);
		}

		private static void PutDataOnClipboard(ClipboardData clipboardData)
		{
			Clipboard.SetDataObject(clipboardData.DataObject, true);
			log.DebugFormat("PutDataOnClipboard(): The following data was just placed on the clipboard:\r\n\t{0}", clipboardData.ToShortDisplayString());
		}

		private static ClipboardData RetrieveDataFromClipboard()
		{
			ClipboardData clipboardData = new ClipboardData(Clipboard.GetDataObject());
			log.DebugFormat("RetrieveDataFromClipboard(): The following data was just retrieved from the clipboard:\r\n\t{0}", clipboardData.ToShortDisplayString());
			return clipboardData;
		}

		private static void WaitForExclusiveClipboardAccess()
		{
			const int maxTimeToWaitMs = 10000;
			int numberOfTries = 0;

			while (true)
			{
				var clipboardOwner = Win32API.GetOpenClipboardWindow();

				if (clipboardOwner == IntPtr.Zero)
				{
					return;
				}

				numberOfTries++;

				if (numberOfTries * AppController.Settings.ThreadDelayTime > maxTimeToWaitMs)
				{
					// This truly exceptional case will most likely result in clipboard data loss.
					throw new ClipboardInUseException(
						string.Format(
							"Despite waiting a total of {0}s the application was unable to gain exclusive access to the clipboard.\r\nThe clipboard was held by the window with the handle {1}.",
							TimeSpan.FromMilliseconds(numberOfTries * AppController.Settings.ThreadDelayTime).Seconds,
							clipboardOwner));
				}

				Thread.Sleep(AppController.Settings.ThreadDelayTime);
			}
		}
	}
}
