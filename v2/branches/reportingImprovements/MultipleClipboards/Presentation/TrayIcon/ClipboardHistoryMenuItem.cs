using System;
using System.Windows.Forms;

namespace MultipleClipboards.Presentation.TrayIcon
{
	public class ClipboardHistoryMenuItem : MenuItem
	{
		public ClipboardHistoryMenuItem(string text, ulong clipboardDataId)
			: this(MenuMerge.Add, 0, Shortcut.None, text, clipboardDataId, null, null, null, null)
		{
		}

		public ClipboardHistoryMenuItem(string text, ulong clipboardDataId, EventHandler onClick)
			: this(MenuMerge.Add, 0, Shortcut.None, text, clipboardDataId, onClick, null, null, null)
		{
		}

		public ClipboardHistoryMenuItem(string text, ulong clipboardDataId, EventHandler onClick, Shortcut shortcut)
			: this(MenuMerge.Add, 0, shortcut, text, clipboardDataId, onClick, null, null, null)
		{
		}

		public ClipboardHistoryMenuItem(string text, ulong clipboardDataId, MenuItem[] items)
			: this(MenuMerge.Add, 0, Shortcut.None, text, clipboardDataId, null, null, null, items)
		{
		}

		private ClipboardHistoryMenuItem(MenuMerge mergeType, int mergeOrder, Shortcut shortcut, string text, ulong clipboardDataId, EventHandler onClick, EventHandler onPopup, EventHandler onSelect, MenuItem[] items)
			: base(mergeType, mergeOrder, shortcut, text, onClick, onPopup, onSelect, items)
		{
			this.ClipboardDataId = clipboardDataId;
		}

		public ulong ClipboardDataId
		{
			get;
			set;
		}
	}
}