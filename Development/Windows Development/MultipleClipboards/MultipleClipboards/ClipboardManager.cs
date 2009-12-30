using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MultipleClipboards
{
	class ClipboardManager
	{
		private List<HotKey> hotKeys;
		private List<ClipboardEntry> clipboardHistory;
		private Dictionary<int, ClipboardEntry> clipboards;
		private ClipboardEntry existingData;
		private int threadDelayTime;
		private int _numberOfHistoricalRecords;
		private bool isProcessingClipboardAction = false;

		public enum ClipboardDataType
		{
			NO_DATA = 0,
			AUDIO = 1,
			FILE_LIST = 2,
			IMAGE = 3,
			TEXT = 4
		}

		// class to hold clipboard data in memory
		public class ClipboardEntry
		{
			public ClipboardDataType dataType;
			public object data;
			public DateTime timestamp;

			public ClipboardEntry(ClipboardDataType dataType, object data)
			{
				this.dataType = dataType;
				this.data = data;
			}

			public ClipboardEntry(ClipboardDataType dataType, object data, DateTime timestamp)
			{
				this.dataType = dataType;
				this.data = data;
				this.timestamp = timestamp;
			}
		}

		public ClipboardManager()
		{
			Init(100, 20);
		}

		public ClipboardManager(int threadDelayTime, int numberOfHistoricalRecords)
		{
			Init(threadDelayTime, numberOfHistoricalRecords);
		}

		private void Init(int threadDelayTime, int numberOfHistoricalRecords)
		{
			NumberOfHistoricalRecords = numberOfHistoricalRecords;
			hotKeys = new List<HotKey>();
			clipboardHistory = new List<ClipboardEntry>(NumberOfHistoricalRecords);
			clipboards = new Dictionary<int, ClipboardEntry>();
			existingData = new ClipboardEntry(ClipboardDataType.NO_DATA, null);
			this.threadDelayTime = threadDelayTime;
			PreserveClipboardData();
		}

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

		public List<ClipboardEntry> ClipboardHistory
		{
			get
			{
				return this.clipboardHistory;
			}
		}

		public bool IsProcessingClipboardAction
		{
			get
			{
				return this.isProcessingClipboardAction;
			}
		}

		public void AddClipboard(int clipboardID, HotKey cutKey, HotKey copyKey, HotKey pasteKey)
		{
			// add the hotkeys to the local collection
			hotKeys.Add(cutKey);
			hotKeys.Add(copyKey);
			hotKeys.Add(pasteKey);

			// create a new dictionary item for this clipboard ID
			// this is the local copy of the item currently stored on the clipbaord that goes with this set of hotkeys
			clipboards.Add(clipboardID, new ClipboardEntry(ClipboardDataType.NO_DATA, null));
		}

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

			clipboardHistory.Add(entry);
		}

		public void PlaceHistoricalEntryOnClipboard(int clipboardHistoryIndex, int clipboardIndex)
		{

		}

		public void Reset()
		{
			hotKeys.Clear();
			clipboards.Clear();
			clipboardHistory = new List<ClipboardEntry>(NumberOfHistoricalRecords);
		}

		// This is called from the form when a registered hotkey is pressed
		// All that is contained in the parameter is the modifier and key
		//
		// 1) Find the matching hotkey in the local collection to get the Clipboard ID and Operation
		// 2) Switch on the operation for this specific key
		public void ProcessHotKey(HotKey systemHotKey)
		{
			this.isProcessingClipboardAction = true;

			HotKey hotKey = hotKeys.Find(h => h == systemHotKey);

			switch (hotKey.Operation)
			{
				case HotKey.HotKeyType.CUT:
					CutCopy(hotKey.ClipboardID, HotKey.HotKeyType.CUT);
					break;

				case HotKey.HotKeyType.COPY:
					CutCopy(hotKey.ClipboardID, HotKey.HotKeyType.COPY);
					break;

				case HotKey.HotKeyType.PASTE:
					Paste(hotKey.ClipboardID);
					break;

				default:
					break;
			}

			this.isProcessingClipboardAction = false;
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
		//		 Figure out if there's a way to make CUT actually cut, and not just copy

		// handles the cut and copy operation
		private void CutCopy(int clipboardID, HotKey.HotKeyType operation)
		{
			PreserveClipboardData();

			// send the system cut or copy command to get the new data on the clipboard
			if (operation == HotKey.HotKeyType.CUT)
				SendKeys.SendWait("^(x)");
			else
				SendKeys.SendWait("^(c)");

			// according the MSDN (and my own experiences) all the SendKeys methods are subject to timing issues
			// this seems to work for me, but sometimes the SendWait function returns before the new data has actually been placed on the clipboard
			// when this happens you wind up with the original clipboard data still on the clipboard, but also stored in whatever clipboard matches the hotkey that we're processing
			// just to be safe, have the program sleep for a fraction of a second before trying to retrieve the new clipboard data
			// this shouldn't yield any noticable delay
			System.Threading.Thread.Sleep(threadDelayTime);

			// store the new data in the correct clipboard
			if (Clipboard.ContainsText())
			{
				clipboards[clipboardID].dataType = ClipboardDataType.TEXT;
				clipboards[clipboardID].data = Clipboard.GetText();
			}
			else if (Clipboard.ContainsFileDropList())
			{
				clipboards[clipboardID].dataType = ClipboardDataType.FILE_LIST;
				clipboards[clipboardID].data = Clipboard.GetFileDropList();
			}
			else if (Clipboard.ContainsAudio())
			{
				clipboards[clipboardID].dataType = ClipboardDataType.AUDIO;
				clipboards[clipboardID].data = Clipboard.GetAudioStream();
			}
			else if (Clipboard.ContainsImage())
			{
				clipboards[clipboardID].dataType = ClipboardDataType.IMAGE;
				clipboards[clipboardID].data = Clipboard.GetImage();
			}
			else
			{
				clipboards[clipboardID].dataType = ClipboardDataType.NO_DATA;
				clipboards[clipboardID].data = null;
			}

			// store this in the clipboard history list
			clipboardHistory.Add(new ClipboardEntry(clipboards[clipboardID].dataType, clipboards[clipboardID].data, DateTime.Now));

			RestoreClipboardData();
		}

		// handles the paste operation
		private void Paste(int clipboardID)
		{
			PreserveClipboardData();
			bool sendPasteSignal = true;

			// place the data from the correct clipboard onto the system clipboard
			// no need to leave the data on the clipboard after the program exists because this is a paste operation
			switch (clipboards[clipboardID].dataType)
			{
				case ClipboardDataType.TEXT:
					Clipboard.SetText((string)clipboards[clipboardID].data);
					break;
				case ClipboardDataType.FILE_LIST:
					Clipboard.SetFileDropList((System.Collections.Specialized.StringCollection)clipboards[clipboardID].data);
					break;
				case ClipboardDataType.AUDIO:
					Clipboard.SetAudio((System.IO.Stream)clipboards[clipboardID].data);
					break;
				case ClipboardDataType.IMAGE:
					Clipboard.SetImage((System.Drawing.Image)clipboards[clipboardID].data);
					break;
				default:
					Clipboard.Clear();
					sendPasteSignal = false;
					break;
			}

			// send the system paste command
			if (sendPasteSignal)
				SendKeys.SendWait("^(v)");

			// a little delay for the same reason as in CutCopy
			System.Threading.Thread.Sleep(threadDelayTime);

			RestoreClipboardData();
		}

		// preserves the existing data on the clipboard
		private void PreserveClipboardData()
		{
			if (Clipboard.ContainsText())
			{
				existingData.dataType = ClipboardDataType.TEXT;
				existingData.data = Clipboard.GetText();
			}
			else if (Clipboard.ContainsFileDropList())
			{
				existingData.dataType = ClipboardDataType.FILE_LIST;
				existingData.data = Clipboard.GetFileDropList();
			}
			else if (Clipboard.ContainsAudio())
			{
				existingData.dataType = ClipboardDataType.AUDIO;
				existingData.data = Clipboard.GetAudioStream();
			}
			else if (Clipboard.ContainsImage())
			{
				existingData.dataType = ClipboardDataType.IMAGE;
				existingData.data = Clipboard.GetImage();
			}
			else
			{
				existingData.dataType = ClipboardDataType.NO_DATA;
				existingData.data = null;
			}
		}

		// restores the clipboard to its original state
		private void RestoreClipboardData()
		{
			switch (existingData.dataType)
			{
				case ClipboardDataType.TEXT:
					Clipboard.SetText((string)existingData.data);
					break;
				case ClipboardDataType.FILE_LIST:
					Clipboard.SetFileDropList((System.Collections.Specialized.StringCollection)existingData.data);
					break;
				case ClipboardDataType.AUDIO:
					Clipboard.SetAudio((System.IO.Stream)existingData.data);
					break;
				case ClipboardDataType.IMAGE:
					Clipboard.SetImage((System.Drawing.Image)existingData.data);
					break;
				default:
					Clipboard.Clear();
					break;
			}
		}
	}
}
