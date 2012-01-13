using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MultipleClipboards
{
	/// <summary>
	/// The data type of a clipboard entry.
	/// </summary>
	public enum ClipboardDataType
	{
		NO_DATA = 0,
		AUDIO = 1,
		FILE_LIST = 2,
		IMAGE = 3,
		TEXT = 4
	}

	/// <summary>
	/// Data structure to hold a single clipboard entry.
	/// </summary>
	public class ClipboardEntry
	{
		/// <summary>
		/// The data type of the clipboard entry.
		/// </summary>
		public ClipboardDataType dataType;

		/// <summary>
		/// The data for this entry.
		/// </summary>
		public object data;

		/// <summary>
		/// The time the data was originally placed on the clipboard.
		/// </summary>
		public DateTime timestamp;

		/// <summary>
		/// Constructs a new Clipboard Entry object.
		/// </summary>
		/// <param name="dataType">The data type of the clipboard entry.</param>
		/// <param name="data">The data for this entry.</param>
		public ClipboardEntry(ClipboardDataType dataType, object data)
		{
			this.dataType = dataType;
			this.data = data;
		}

		/// <summary>
		/// Constructs a new Clipboard Entry object.
		/// </summary>
		/// <param name="dataType">The data type of the clipboard entry.</param>
		/// <param name="data">The data for this entry.</param>
		/// <param name="timestamp">The time the data was originally placed on the clipboard.</param>
		public ClipboardEntry(ClipboardDataType dataType, object data, DateTime timestamp)
		{
			this.dataType = dataType;
			this.data = data;
			this.timestamp = timestamp;
		}
	}

	/// <summary>
	/// Class to manage clipboard entries over multiple clipboards.
	/// </summary>
	public class ClipboardManager
	{
		private List<HotKey> hotKeys;
		private Queue<ClipboardEntry> clipboardHistory;
		private Dictionary<int, ClipboardEntry> clipboards;
		private ClipboardEntry existingData;
		private int threadDelayTime;
		private int _numberOfHistoricalRecords;
		private bool isProcessingClipboardAction = false;

		/// <summary>
		/// Constructs a new Clipboard Manager object.
		/// </summary>
		public ClipboardManager()
		{
			this.Init(100, 20);
		}

		/// <summary>
		/// Constructs a new Clipboard Manager object.
		/// </summary>
		/// <param name="threadDelayTime">The time to delay between each clipboard operation.</param>
		/// <param name="numberOfHistoricalRecords">The number of historical records to keep track of.</param>
		public ClipboardManager(int threadDelayTime, int numberOfHistoricalRecords)
		{
			this.Init(threadDelayTime, numberOfHistoricalRecords);
		}

		/// <summary>
		/// Gets or sets the number of historical clipboard entries to keep track of.
		/// </summary>
		public int NumberOfHistoricalRecords
		{
			get
			{
				return this._numberOfHistoricalRecords;
			}
			set
			{
				this._numberOfHistoricalRecords = value;
			}
		}

		/// <summary>
		/// Gets the Queue that holds all the historical clipboard entries.
		/// </summary>
		public Queue<ClipboardEntry> ClipboardHistory
		{
			get
			{
				return this.clipboardHistory;
			}
		}

		/// <summary>
		/// Gets the flag that tells whether or not the app is currently processing a clipboard action.
		/// </summary>
		public bool IsProcessingClipboardAction
		{
			get
			{
				return this.isProcessingClipboardAction;
			}
		}

		/// <summary>
		/// Adds a new clipboard to be managed.
		/// </summary>
		/// <param name="clipboardID">The ID of the clipboard.</param>
		/// <param name="cutKey">The hot key for the cut operation.</param>
		/// <param name="copyKey">The hot key for the copy operation.</param>
		/// <param name="pasteKey">The hot key for the paste operation.</param>
		public void AddClipboard(int clipboardID, HotKey cutKey, HotKey copyKey, HotKey pasteKey)
		{
			// add the hotkeys to the local collection
			this.hotKeys.Add(cutKey);
			this.hotKeys.Add(copyKey);
			this.hotKeys.Add(pasteKey);

			// create a new dictionary item for this clipboard ID
			// this is the local copy of the item currently stored on the clipbaord that goes with this set of hotkeys
			this.clipboards.Add(clipboardID, new ClipboardEntry(ClipboardDataType.NO_DATA, null));
		}

		/// <summary>
		/// Stores the current contents of the Windows clipboard in the history queue.
		/// </summary>
		public void StoreClipboardContents()
		{
			ClipboardEntry entry = new ClipboardEntry(ClipboardDataType.NO_DATA, null, DateTime.Now);

			if (Clipboard.ContainsText())
			{
				entry.dataType = ClipboardDataType.TEXT;
				entry.data = Clipboard.GetText();
			}
			else if (Clipboard.ContainsFileDropList())
			{
				entry.dataType = ClipboardDataType.FILE_LIST;
				entry.data = Clipboard.GetFileDropList();
			}
			else if (Clipboard.ContainsAudio())
			{
				entry.dataType = ClipboardDataType.AUDIO;
				entry.data = Clipboard.GetAudioStream();
			}
			else if (Clipboard.ContainsImage())
			{
				entry.dataType = ClipboardDataType.IMAGE;
				entry.data = Clipboard.GetImage();
			}
			else
			{
				entry.dataType = ClipboardDataType.NO_DATA;
				entry.data = null;
			}

			this.EnqueueHistoricalEntry(entry);
		}

		/// <summary>
		/// Places the specified item from the clipboard history queue on the specified clipboard.
		/// </summary>
		/// <param name="clipboardHistoryIndex">The index in the clipboard history queue of the clipboard entry to place on a clipboard.</param>
		/// <param name="clipboardIndex">The index of the clipboard to place the historical data on.</param>
		public void PlaceHistoricalEntryOnClipboard(int clipboardHistoryIndex, int clipboardIndex)
		{
			this.isProcessingClipboardAction = true;

			if (clipboardIndex == 0)
			{
				// put data on the windows clipboard
				this.existingData.data = this.clipboardHistory.ElementAt(clipboardHistoryIndex).data;
				this.existingData.dataType = this.clipboardHistory.ElementAt(clipboardHistoryIndex).dataType;
				this.existingData.timestamp = this.clipboardHistory.ElementAt(clipboardHistoryIndex).timestamp;
				this.RestoreClipboardData();
			}
			else
			{
				// put the data on the specified clipboard
				this.clipboards[clipboardIndex].data = this.clipboardHistory.ElementAt(clipboardHistoryIndex).data;
				this.clipboards[clipboardIndex].dataType = this.clipboardHistory.ElementAt(clipboardHistoryIndex).dataType;
				this.clipboards[clipboardIndex].timestamp = this.clipboardHistory.ElementAt(clipboardHistoryIndex).timestamp;
			}

			this.isProcessingClipboardAction = false;
		}

		/// <summary>
		/// Resets the clipboard manager.
		/// </summary>
		public void Reset()
		{
			this.hotKeys.Clear();
			this.clipboards.Clear();
			this.clipboardHistory = new Queue<ClipboardEntry>(this.NumberOfHistoricalRecords);
		}

		/// <summary>
		/// Clears the contents of the clipboard history queue.
		/// </summary>
		public void ClearClipboardHistory()
		{
			this.clipboardHistory.Clear();
		}

		/// <summary>
		/// Processes a hot key action.
		/// Called from the form when a registered hotkey is pressed.
		/// </summary>
		/// <param name="systemHotKey">The hot key that was pressed.</param>
		public void ProcessHotKey(HotKey systemHotKey)
		{
			// 1) Find the matching hotkey in the local collection to get the Clipboard ID and Operation
			// 2) Switch on the operation for this specific key

			this.isProcessingClipboardAction = true;

			HotKey hotKey = this.hotKeys.Find(h => h == systemHotKey);

			switch (hotKey.Operation)
			{
				case HotKeyType.CUT:
					this.CutCopy(hotKey.ClipboardID, HotKeyType.CUT);
					break;

				case HotKeyType.COPY:
					this.CutCopy(hotKey.ClipboardID, HotKeyType.COPY);
					break;

				case HotKeyType.PASTE:
					this.Paste(hotKey.ClipboardID);
					break;

				default:
					break;
			}

			this.isProcessingClipboardAction = false;
		}

		/// <summary>
		/// Initializes the clipboard manager.
		/// </summary>
		/// <param name="threadDelayTime">The time to delay between each clipboard operation.</param>
		/// <param name="numberOfHistoricalRecords">The number of historical records to keep track of.</param>
		private void Init(int threadDelayTime, int numberOfHistoricalRecords)
		{
			this.NumberOfHistoricalRecords = numberOfHistoricalRecords;
			this.hotKeys = new List<HotKey>();
			this.clipboardHistory = new Queue<ClipboardEntry>(this.NumberOfHistoricalRecords);
			this.clipboards = new Dictionary<int, ClipboardEntry>();
			this.existingData = new ClipboardEntry(ClipboardDataType.NO_DATA, null);
			this.threadDelayTime = threadDelayTime;
			this.PreserveClipboardData();
		}

		/// <summary>
		/// Enqueues the given entry in the clipboard history queue.
		/// </summary>
		/// <param name="entry">The clipboard entry to enqueue.</param>
		private void EnqueueHistoricalEntry(ClipboardEntry entry)
		{
			if (entry.dataType != ClipboardDataType.NO_DATA)
			{
				if (this.clipboardHistory.Count == this.NumberOfHistoricalRecords)
				{
					this.clipboardHistory.Dequeue();
				}
				this.clipboardHistory.Enqueue(entry);
			}
		}

		/************************************************************************************************************
		 * 
		 * I'm sure there's a better / faster / more generic way to get the correct data format out of the clipboard.
		 * But, this is certainly the easiest and it seems to work quite nicely
		 * Maybe in a later version of this, if there is one, I'll do some more research and figure it out
		 * So if you don't like big if else statements, I'm sorry, you'll hate this code
		 * 
		 * *********************************************************************************************************/

		// TODO: Add ability to handle HTML and Rich Text formats
		//		 Figure out if there's a way to make CUT actually cut files, and not just copy

		/// <summary>
		/// Handles the cut and copy operation.
		/// </summary>
		/// <param name="clipboardID">The ID of the clipboard that the operation is for.</param>
		/// <param name="operation">The operation to perform.</param>
		private void CutCopy(int clipboardID, HotKeyType operation)
		{
			this.PreserveClipboardData();

			// send the system cut or copy command to get the new data on the clipboard
			if (operation == HotKeyType.CUT)
			{
				SendKeys.SendWait("^(x)");
			}
			else
			{
				SendKeys.SendWait("^(c)");
			}

			// according the MSDN (and my own experiences) all the SendKeys methods are subject to timing issues
			// this seems to work for me, but sometimes the SendWait function returns before the new data has actually been placed on the clipboard
			// when this happens you wind up with the original clipboard data still on the clipboard, but also stored in whatever clipboard matches the hotkey that we're processing
			// just to be safe, have the program sleep for a fraction of a second before trying to retrieve the new clipboard data
			// this shouldn't yield any noticable delay
			System.Threading.Thread.Sleep(this.threadDelayTime);

			// store the new data in the correct clipboard
			if (Clipboard.ContainsText())
			{
				this.clipboards[clipboardID].dataType = ClipboardDataType.TEXT;
				this.clipboards[clipboardID].data = Clipboard.GetText();
			}
			else if (Clipboard.ContainsFileDropList())
			{
				this.clipboards[clipboardID].dataType = ClipboardDataType.FILE_LIST;
				this.clipboards[clipboardID].data = Clipboard.GetFileDropList();
			}
			else if (Clipboard.ContainsAudio())
			{
				this.clipboards[clipboardID].dataType = ClipboardDataType.AUDIO;
				this.clipboards[clipboardID].data = Clipboard.GetAudioStream();
			}
			else if (Clipboard.ContainsImage())
			{
				this.clipboards[clipboardID].dataType = ClipboardDataType.IMAGE;
				this.clipboards[clipboardID].data = Clipboard.GetImage();
			}
			else
			{
				this.clipboards[clipboardID].dataType = ClipboardDataType.NO_DATA;
				this.clipboards[clipboardID].data = null;
			}

			// store this in the clipboard history list
			this.EnqueueHistoricalEntry(new ClipboardEntry(this.clipboards[clipboardID].dataType, this.clipboards[clipboardID].data, DateTime.Now));

			this.RestoreClipboardData();
		}

		/// <summary>
		/// Handles the paste operation.
		/// </summary>
		/// <param name="clipboardID">The ID of the clipboard that the operation is for.</param>
		private void Paste(int clipboardID)
		{
			this.PreserveClipboardData();
			bool sendPasteSignal = true;

			// place the data from the correct clipboard onto the system clipboard
			// no need to leave the data on the clipboard after the program exists because this is a paste operation
			switch (this.clipboards[clipboardID].dataType)
			{
				case ClipboardDataType.TEXT:
					Clipboard.SetText((string)this.clipboards[clipboardID].data);
					break;
				case ClipboardDataType.FILE_LIST:
					Clipboard.SetFileDropList((System.Collections.Specialized.StringCollection)this.clipboards[clipboardID].data);
					break;
				case ClipboardDataType.AUDIO:
					Clipboard.SetAudio((System.IO.Stream)this.clipboards[clipboardID].data);
					break;
				case ClipboardDataType.IMAGE:
					Clipboard.SetImage((System.Drawing.Image)this.clipboards[clipboardID].data);
					break;
				default:
					Clipboard.Clear();
					sendPasteSignal = false;
					break;
			}

			// send the system paste command
			if (sendPasteSignal)
			{
				SendKeys.SendWait("^(v)");
			}

			// a little delay for the same reason as in CutCopy
			System.Threading.Thread.Sleep(this.threadDelayTime);

			this.RestoreClipboardData();
		}

		/// <summary>
		/// Preserves the existing data on the clipboard.
		/// </summary>
		private void PreserveClipboardData()
		{
			if (Clipboard.ContainsText())
			{
				this.existingData.dataType = ClipboardDataType.TEXT;
				this.existingData.data = Clipboard.GetText();
			}
			else if (Clipboard.ContainsFileDropList())
			{
				this.existingData.dataType = ClipboardDataType.FILE_LIST;
				this.existingData.data = Clipboard.GetFileDropList();
			}
			else if (Clipboard.ContainsAudio())
			{
				this.existingData.dataType = ClipboardDataType.AUDIO;
				this.existingData.data = Clipboard.GetAudioStream();
			}
			else if (Clipboard.ContainsImage())
			{
				this.existingData.dataType = ClipboardDataType.IMAGE;
				this.existingData.data = Clipboard.GetImage();
			}
			else
			{
				this.existingData.dataType = ClipboardDataType.NO_DATA;
				this.existingData.data = null;
			}
		}

		/// <summary>
		/// Restores the clipboard to its original state.
		/// </summary>
		private void RestoreClipboardData()
		{
			switch (this.existingData.dataType)
			{
				case ClipboardDataType.TEXT:
					Clipboard.SetText((string)this.existingData.data);
					break;
				case ClipboardDataType.FILE_LIST:
					Clipboard.SetFileDropList((System.Collections.Specialized.StringCollection)this.existingData.data);
					break;
				case ClipboardDataType.AUDIO:
					Clipboard.SetAudio((System.IO.Stream)this.existingData.data);
					break;
				case ClipboardDataType.IMAGE:
					Clipboard.SetImage((System.Drawing.Image)this.existingData.data);
					break;
				default:
					Clipboard.Clear();
					break;
			}
		}
	}
}
