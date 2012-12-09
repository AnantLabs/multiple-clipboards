using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using MultipleClipboards.Entities;
using MultipleClipboards.Exceptions;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Interop;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Icons;
using QuantumBitDesigns.Core;
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
		private static readonly object clipboardOperationLock = new object();
		private readonly ConcurrentDictionary<int, ClipboardData> clipboardDataByClipboardId = new ConcurrentDictionary<int, ClipboardData>();

		/// <summary>
		/// Constructs a new Clipboard Manager object for use with the given window handle.
		/// </summary>
		/// <param name="windowHandle">The handle of the window using this clipboard manager.</param>
		public ClipboardManager(IntPtr windowHandle)
		{
			AppController.Settings.ClipboardDefinitions.CollectionChanged += this.ClipboardDefinitionsCollectionChanged;
			this.WindowHandle = windowHandle;
			this.AllowStoreClipboardContents = true;
			this.HotKeys = new List<HotKey>();
			this.ClipboardHistory = new ObservableList<ClipboardData>(Application.Current.Dispatcher);

			this.CreateAvailableClipboardCollection();
			this.RegisterAllClipboards();
			this.LoadClipboardHistory();
			this.PreserveClipboardData();

			if (this.CurrentSystemClipboardData != null && this.CurrentSystemClipboardData.ContainsData)
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
			foreach (short hotKeyId in this.HotKeys.Select(hk => hk.HotKeyId).ToList())
			{
				this.UnRegisterHotKey(hotKeyId);
			}
			log.Debug("ClipboardManager destroyed.  All hot keys have been un-registered.");
		}

		/// <summary>
		/// Gets the collection that holds all the historical clipboard entries.
		/// </summary>
		public ObservableList<ClipboardData> ClipboardHistory
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the collection of clipboards available to the user.
		/// </summary>
		public ObservableCollection<ClipboardDefinition> AvailableClipboards
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
				return this.GetClipboardDataByClipboardId(ClipboardDefinition.SystemClipboardId);
			}
			set
			{
				this.SetClipboardDataForClipboard(ClipboardDefinition.SystemClipboardId, value);
			}
		}

		/// <summary>
		/// Saves the clipboard history data to disk if the setting to do so is enabled.
		/// </summary>
		public void SaveClipboardHistory()
		{
			if (!AppController.Settings.PersistClipboardHistory)
			{
				return;
			}

			byte[] serializedData;
			List<ClipboardData> preservedHistory;

			using (this.ClipboardHistory.AcquireLock())
			{
				preservedHistory = this.ClipboardHistory.ToList();
			}

			if (!preservedHistory.Any())
			{
				AppController.Settings.DeleteClipboardHistory();
				return;
			}

			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, preservedHistory);
				serializedData = stream.ToArray();
				stream.Close();
			}

			AppController.Settings.SaveClipboardHistory(serializedData);
		}

		/// <summary>
		/// Loads the persisted clipboard history from disk and adds all items to the current in-memory history collection if the setting to do so is enabled.
		/// </summary>
		protected void LoadClipboardHistory()
		{
			if (!AppController.Settings.PersistClipboardHistory)
			{
				return;
			}

			var serializedData = AppController.Settings.LoadClipboardHistory();

			if (serializedData == null)
			{
				return;
			}

			List<ClipboardData> preservedHistory;

			using (var stream = new MemoryStream(serializedData))
			{
				var formatter = new BinaryFormatter();
				preservedHistory = (List<ClipboardData>)formatter.Deserialize(stream);
				stream.Close();
			}

			foreach (var clipboardData in preservedHistory)
			{
				clipboardData.SetDescriptionData();
				this.EnqueueHistoricalEntry(clipboardData, false);
			}
		}

		/// <summary>
		/// Removes the items with the given id from the clipboard history collection.
		/// </summary>
		/// <param name="itemId">The id of the item to remove.</param>
		public void RemoveItemFromHistory(ulong itemId)
		{
			using (this.ClipboardHistory.AcquireLock())
			{
				var data = this.ClipboardHistory.SingleOrDefault(d => d.Id == itemId);

				if (data != null)
				{
					this.ClipboardHistory.Remove(data);
				}
			}
		}

		/// <summary>
		/// Gets the data stored on the clipboard with the given ID.
		/// </summary>
		/// <param name="clipboardId">The clipboard ID.</param>
		/// <returns>The data stored on the clipboard with the given ID.</returns>
		public ClipboardData GetClipboardDataByClipboardId(int clipboardId)
		{
			ClipboardData clipboardData;

			if (!this.clipboardDataByClipboardId.TryGetValue(clipboardId, out clipboardData))
			{
				log.ErrorFormat(
					"Unable to get the data stored on the clipboard with ID {0} because we were not able to read from the concurrent dictionary.{1}{2}",
					clipboardId,
					Environment.NewLine,
					new StackTrace());
				clipboardData = null;
			}

			return clipboardData;
		}

		/// <summary>
		/// Sets the clipboard data for the clipbaord with the given ID.
		/// </summary>
		/// <param name="clipboardId">The clipboard ID.</param>
		/// <param name="data">The data to store.</param>
		protected void SetClipboardDataForClipboard(int clipboardId, ClipboardData data)
		{
			this.clipboardDataByClipboardId.AddOrUpdate(clipboardId, data, (id, originalValue) => data);
		}

		/// <summary>
		/// Removes the data stored on the clipboard with the given ID.
		/// </summary>
		/// <param name="clipboardId">The clipboard ID.</param>
		private void RemoveClipboardDataFromClipboard(int clipboardId)
		{
			ClipboardData removedValue;

			if (!this.clipboardDataByClipboardId.TryRemove(clipboardId, out removedValue))
			{
				log.ErrorFormat(
					"Unable to remove the data stored on the clipboard with ID {0} because we were not able to delete from the concurrent dictionary.{1}{2}",
					clipboardId,
					Environment.NewLine,
					new StackTrace());
			}
		}

		/// <summary>
		/// Adds a new clipboard to be managed.
		/// </summary>
		/// <param name="clipboard">The clipboard to add.</param>
		public void AddClipboard(ClipboardDefinition clipboard)
		{
			if (this.AvailableClipboards.Contains(clipboard))
			{
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = string.Format("The clipboard '{0}' already exists.", clipboard),
					IconType = IconType.Error
				});
				return;
			}

			string message;
			clipboard.ClipboardId = AppController.Settings.GetNextClipboardId();
			var result = this.RegisterClipboard(clipboard);

			if (result.WasCompleteFailure)
			{
				message = string.Format("Failed to register the clipboard '{0}'.", clipboard);
			}
			else
			{
				AppController.Settings.AddNewClipboard(clipboard);
				log.InfoFormat("AddClipboard(): New clipboard added:\r\n{0}", clipboard);
				message = result.HadFailures
					? result.HotKeyRegistrationErrorMessage
					: string.Format("The clipboard '{0}' has been registered successfully!", clipboard);
			}

			MessageBus.Instance.Publish(new MainWindowNotification
			{
				MessageBody = message,
				IconType = result.ResultIcon
			});
		}

		public void RemoveClipboard(ClipboardDefinition clipboard)
		{
			try
			{
				this.UnRegisterHotKeysForClipboard(clipboard.ClipboardId);
				this.RemoveClipboardDataFromClipboard(clipboard.ClipboardId);
				AppController.Settings.RemoveClipboard(clipboard);
				log.InfoFormat("RemoveClipboard(): Clipboard removed:\r\n{0}", clipboard);
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = string.Format("The clipboard '{0}' has been removed.", clipboard),
					IconType = IconType.Success
				});
			}
			catch (Exception e)
			{
				string errorMessage = string.Format("There was an error removing the clipboard '{0}'.", clipboard);
				log.Error(errorMessage, e);
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = errorMessage,
					IconType = IconType.Error
				});
			}
		}

		/// <summary>
		/// Stores the current contents of the Windows clipboard in the history queue.
		/// </summary>
		/// <param name="asyncOperationArguments">The AsyncClipboardOperationArguments.</param>
		public void StoreClipboardContentsAsync(object asyncOperationArguments)
		{
			var arguments = asyncOperationArguments as AsyncClipboardOperationArguments;

			if (arguments == null)
			{
				log.Error("StoreClipboardContentsAsync(): Unable to store the contents of the clipboard because the arguments passed to ProcessHotKeyAsync are not of type ProcessHotKeyArguments.");
				return;
			}

			Monitor.Enter(clipboardOperationLock);

			try
			{
				if (!this.AllowStoreClipboardContents)
				{
					// When the user places historical data on the system clipboard a DrawClipboard message is sent to the clipboard window,
					// which will immediately call this method and try to store the data.  In this case we already have the data preserved
					// so we don't want to do anything.
					this.AllowStoreClipboardContents = true;
					return;
				}

				log.Debug("StoreClipboardContentsAsync(): System clipboard has changed.  About to store the contents of the clipboard.");
				WaitForExclusiveClipboardAccess();
				var clipboardData = RetrieveDataFromClipboard();
				this.CurrentSystemClipboardData = clipboardData;
				this.EnqueueHistoricalEntry(clipboardData);
				log.Debug("StoreClipboardContentsAsync(): Stored clipboard contents successfully.");
			}
			catch (Exception e)
			{
				log.Error("StoreClipboardContentsAsync(): Error storing clipboard contents", e);

				MessageBus.Instance.Publish(new TrayNotification
				{
					MessageBody = "There was an error storing the contents of the clipboard.",
					IconType = IconType.Error
				});
			}
			finally
			{
				arguments.Callback();
				Monitor.Exit(clipboardOperationLock);
			}
		}

		/// <summary>
		/// Places the specified item from the clipboard history queue on the specified clipboard.
		/// </summary>
		/// <param name="clipboardId">The Id of the clipboard to place the historical data on.</param>
		/// /// <param name="clipboardDataId">The Id of the clipboard entry to place on a clipboard.</param>
		public void PlaceHistoricalEntryOnClipboard(int clipboardId, ulong clipboardDataId)
		{
			Monitor.Enter(clipboardOperationLock);

			try
			{
				this.AllowStoreClipboardContents = false;
				var clipboardEntry = this.ClipboardHistory.FirstOrDefault(data => data.Id == clipboardDataId);

				if (clipboardEntry != null)
				{
					this.SetClipboardDataForClipboard(clipboardId, clipboardEntry);

					if (clipboardId == ClipboardDefinition.SystemClipboardId)
					{
						this.RestoreClipboardData();
					}
				}
			}
			finally
			{
				Monitor.Exit(clipboardOperationLock);
			}
		}

		/// <summary>
		/// Clears the contents of the clipboard history queue.
		/// </summary>
		public void ClearClipboardHistory()
		{
			using (this.ClipboardHistory.AcquireLock())
			{
				this.ClipboardHistory.Clear();
			}

			AppController.Settings.DeleteClipboardHistory();
		}

		/// <summary>
		/// Removes the data stored on the clipboard with the given ID.
		/// </summary>
		/// <param name="clipboardId">The clipboard ID.</param>
		/// <param name="isCalledFromUi">A flag indicating whether or not this method was called from the UI.</param>
		public void ClearClipboardContents(int clipboardId, bool isCalledFromUi = false)
		{
			this.SetClipboardDataForClipboard(clipboardId, null);

			if (clipboardId != ClipboardDefinition.SystemClipboardId)
			{
				return;
			}

			Monitor.Enter(clipboardOperationLock);

			try
			{
				WaitForExclusiveClipboardAccess();
				Clipboard.Clear();
			}
			catch (Exception exception)
			{
				const string errorMessage = "There was an error clearing the contents of the system clipboard.";
				log.Error(errorMessage, exception);
				NotificationBase message;

				if (isCalledFromUi)
				{
					message = new MainWindowNotification();
				}
				else
				{
					message = new TrayNotification();
				}

				message.MessageBody = errorMessage;
				message.IconType = IconType.Error;
				MessageBus.Instance.Publish(message);
			}
			finally
			{
				Monitor.Exit(clipboardOperationLock);
			}
		}

		/// <summary>
		/// Processes a hot key action.
		/// Called from the form when a registered hotkey is pressed.
		/// </summary>
		/// <param name="processHotKeyArguments">The process hot key arguments.</param>
		public void ProcessHotKeyAsync(object processHotKeyArguments)
		{
			var arguments = processHotKeyArguments as ProcessHotKeyArguments;

			if (arguments == null)
			{
				log.Error("ProcessHotKeyAsync(): Unable to process hot key because the arguments passed to ProcessHotKeyAsync are not of type ProcessHotKeyArguments.");
				return;
			}

			Monitor.Enter(clipboardOperationLock);

			try
			{
				log.DebugFormat("ProcessHotKeyAsync(): About to process HotKey: {0}", arguments.HotKey);

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

				log.DebugFormat("ProcessHotKeyAsync(): Finished processing HotKey: {0}", hotKey);
			}
			catch (Exception e)
			{
				log.Error("Unexpected error while processing hot key.", e);

				MessageBus.Instance.Publish(new TrayNotification
				{
					MessageBody = string.Format("There was an error processing the hot key {0}.", arguments.HotKey),
					IconType = IconType.Error
				});
			}
			finally
			{
				arguments.Callback();
				Monitor.Exit(clipboardOperationLock);
			}
		}

		/// <summary>
		/// Called when the clipboard definition collection is modified.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClipboardDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				this.AvailableClipboards.Clear();
				return;
			}

			if (e.OldItems != null)
			{
				foreach (var clipboard in e.OldItems.Cast<ClipboardDefinition>())
				{
					this.AvailableClipboards.Remove(clipboard);
				}
			}

			if (e.NewItems != null)
			{
				foreach (var clipboard in e.NewItems.Cast<ClipboardDefinition>())
				{
					this.AvailableClipboards.Add(clipboard);
				}
			}
		}

		/// <summary>
		/// Fills the available clipboard list with all the clipboard definitions currently available to the user.
		/// </summary>
		private void CreateAvailableClipboardCollection()
		{
			var tempList = new List<ClipboardDefinition>();
			tempList.Add(ClipboardDefinition.SystemClipboardDefinition);
			tempList.AddRange(AppController.Settings.ClipboardDefinitions);

			this.AvailableClipboards = new ObservableCollection<ClipboardDefinition>(tempList);
		}

		/// <summary>
		/// Registers all the hotkeys associated with the all the currently registered clipboards.
		/// </summary>
		private void RegisterAllClipboards()
		{
			this.SetClipboardDataForClipboard(ClipboardDefinition.SystemClipboardId, null);
			var result = this.RegisterClipboards(AppController.Settings.ClipboardDefinitions);

			if (result.HadFailures)
			{
				MessageBus.Instance.Publish(new TrayNotification
				{
					MessageBody = result.WasCompleteFailure
						? "FATAL ERROR:\r\n\r\nNone of the clipboard hot keys could be registered."
						: result.HotKeyRegistrationErrorMessage,
					IconType = result.ResultIcon
				});
			}
		}

		private RegisterClipboardsResult RegisterClipboard(ClipboardDefinition clipboard)
		{
			return this.RegisterClipboards(new[] { clipboard });
		}

		private RegisterClipboardsResult RegisterClipboards(IEnumerable<ClipboardDefinition> clipboards)
		{
			bool wasCompleteFailure = true;
			var failedHotKeys = new List<HotKey>();

			foreach (var clipboard in clipboards)
			{
				HotKey cutHotKey = new HotKey(clipboard.ClipboardId, HotKeyType.Cut, clipboard.CutKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey);
				bool cutRegistrationSuccess = this.RegisterHotKey(cutHotKey);

				HotKey copyHotKey = new HotKey(clipboard.ClipboardId, HotKeyType.Copy, clipboard.CopyKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey);
				bool copyRegistrationSuccess = this.RegisterHotKey(copyHotKey);

				HotKey pasteHotKey = new HotKey(clipboard.ClipboardId, HotKeyType.Paste, clipboard.PasteKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey);
				bool pasteRegistrationSuccess = this.RegisterHotKey(pasteHotKey);

				wasCompleteFailure &= !cutRegistrationSuccess && !copyRegistrationSuccess && !pasteRegistrationSuccess;

				if (!cutRegistrationSuccess)
				{
					failedHotKeys.Add(new HotKey(clipboard.CutKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey));
				}
				if (!copyRegistrationSuccess)
				{
					failedHotKeys.Add(new HotKey(clipboard.CopyKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey));
				}
				if (!pasteRegistrationSuccess)
				{
					failedHotKeys.Add(new HotKey(clipboard.PasteKey, clipboard.ModifierOneKey, clipboard.ModifierTwoKey));
				}

				if (!wasCompleteFailure)
				{
					// Create a new dictionary item for this clipboard ID.
					// This is the local copy of the item currently stored on the clipbaord that goes with this set of hotkeys.
					this.SetClipboardDataForClipboard(clipboard.ClipboardId, null);
				}
			}

			return new RegisterClipboardsResult(wasCompleteFailure, failedHotKeys);
		}

		/// <summary>
		/// Registers a the given hot key.
		/// </summary>
		/// <param name="hotKey">The hot key to register.</param>
		/// <returns>A value indicating whether or not the HotKey was registered successfully.</returns>
		private bool RegisterHotKey(HotKey hotKey)
		{
			if (this.HotKeys.Contains(hotKey) || HotKey.ReservedHotKeys.Contains(hotKey))
			{
				return false;
			}

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
					this.UnRegisterHotKey(hotKey.HotKeyId);
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
			// The .ToList() is required because the UnRegisterHotKey method modifies the HotKeys collection.
			foreach (short hotKeyId in this.HotKeys.Where(hk => hk.ClipboardId == clipboardId).Select(hk => hk.HotKeyId).ToList())
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
			this.HotKeys.RemoveAll(hk => hk.HotKeyId == hotKeyId);
			Win32API.UnregisterHotKey(this.WindowHandle, hotKeyId);
			Win32API.GlobalDeleteAtom(hotKeyId);
		}

		/// <summary>
		/// Enqueues the given entry in the clipboard history queue.
		/// </summary>
		/// <param name="clipboardData">The clipboard data to enqueue.</param>
		/// <param name="cloneClipboardData">A flag to determine whether or not to clone the given CliboardData object before adding it to the history collection.</param>
		private void EnqueueHistoricalEntry(ClipboardData clipboardData, bool cloneClipboardData = true)
		{
			if (clipboardData == null)
			{
				return;
			}

			using (this.ClipboardHistory.AcquireLock())
			{
				if (this.ClipboardHistory.Count == AppController.Settings.NumberOfClipboardHistoryRecords)
				{
					this.ClipboardHistory.RemoveAt(0);
				}
				this.ClipboardHistory.Add(cloneClipboardData ? new ClipboardData(clipboardData) : clipboardData);
			}
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
            //log.DebugFormat("CutCopy(): Sending {0} command via SendKeys.", hotKeyType);
            //SendKeys.SendWait(hotKeyType.ToSendKeysCode());

            log.DebugFormat("CutCopy(): Sending {0} command via InputSimulator.", hotKeyType);
            hotKeyType.SimulateHotKeyPress();

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
				this.SetClipboardDataForClipboard(clipboardId, RetrieveDataFromClipboard());

				// Store this in the clipboard history list.
				this.EnqueueHistoricalEntry(this.GetClipboardDataByClipboardId(clipboardId));
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
				if (this.GetClipboardDataByClipboardId(clipboardId) == null)
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
					PutDataOnClipboard(this.GetClipboardDataByClipboardId(clipboardId));
				}

				// Send the system paste command.
				if (sendPasteSignal)
				{
                    //log.Debug("Paste(): Sending paste command via SendKeys.");
                    //SendKeys.SendWait(HotKeyType.Paste.ToSendKeysCode());
                    log.Debug("Paste(): Sending paste command via InputSimulator.");
                    HotKeyType.Paste.SimulateHotKeyPress();
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
			if (clipboardData == null)
			{
				return;
			}

			Clipboard.SetDataObject(clipboardData.GetDataObject(), true);
			
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("PutDataOnClipboard(): The following data was just placed on the clipboard:\r\n\t{0}", clipboardData.ToLogString());
			}
		}

		private static ClipboardData RetrieveDataFromClipboard()
		{
			var dataObject = Clipboard.GetDataObject();
			var formats = new List<string>();

			if (dataObject != null)
			{
				try
				{
					formats = dataObject.GetFormats().ToList();
				}
				catch (OutOfMemoryException memoryException)
				{
					// First, log the original error as a warning, since we will try and recover.
					log.Warn("RetrieveDataFromClipboard(): Unable to get the collection of formats from the retrieved data object.", memoryException);

					// Although I have never seen this error personally, it has been reported.
					// My theory is that you can only reliably call GetFormats() on an IDataObject once,
					// especially when the data is failarly large, like an image.
					// That's why I am now reading the formats once here, instead of once here to check
					// to see if it's empty and then again in the constructor of ClipboardData.
					// Even so, if this exception occurs then try to look for a few known types
					// and build a limited format collection.
					if (Clipboard.ContainsImage())
					{
						formats.Add(DataFormats.Bitmap);
					}
					else if (Clipboard.ContainsAudio())
					{
						formats.Add(DataFormats.WaveAudio);
					}
					else if (Clipboard.ContainsFileDropList())
					{
						formats.Add(DataFormats.FileDrop);
					}
					else if (Clipboard.ContainsText())
					{
						formats.Add(DataFormats.Text);
					}
				}
			}

			if (dataObject == null || !formats.Any())
			{
				log.Debug("RetrieveDataFromClipboard(): The clipboard is empty.  Returnning null.");
				return null;
			}

			var clipboardData = new ClipboardData(dataObject, formats);
			
			if (log.IsDebugEnabled)
			{
				// I usually don't like checking IsLogLevelEnabled flags because it makes the code ugly, but in this case
				// ToLogString() actually does some work, so there's no point to do it if we're not going to log it.
				log.DebugFormat("RetrieveDataFromClipboard(): The following data was just retrieved from the clipboard:\r\n\t{0}", clipboardData.ToLogString());
			}
			
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

		private class RegisterClipboardsResult
		{
			public RegisterClipboardsResult(bool wasCompleteFailure, IList<HotKey> failedHotKeys)
			{
				this.WasCompleteFailure = wasCompleteFailure;
				this.HadFailures = failedHotKeys.Any();

				if (this.WasCompleteFailure)
				{
					this.ResultIcon = IconType.Error;
				}
				else if (this.HadFailures)
				{
					this.ResultIcon = IconType.Warning;
				}
				else
				{
					this.ResultIcon = IconType.Success;
				}

				if (!this.HadFailures)
				{
					return;
				}

				var errorMessageBuilder = new StringBuilder();
				errorMessageBuilder.AppendLine("There was an error registering the following hot keys:");

				foreach (var hotKey in failedHotKeys)
				{
					errorMessageBuilder.AppendLine(string.Format("\t{0}", hotKey));
				}

				this.HotKeyRegistrationErrorMessage = errorMessageBuilder.ToString();
			}

			public IconType ResultIcon
			{
				get;
				private set;
			}

			public bool WasCompleteFailure
			{
				get;
				private set;
			}

			public bool HadFailures
			{
				get;
				private set;
			}

			public string HotKeyRegistrationErrorMessage
			{
				get;
				private set;
			}
		}
	}
}
