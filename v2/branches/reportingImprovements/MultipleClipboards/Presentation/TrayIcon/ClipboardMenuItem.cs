using System;
using System.Windows.Forms;

namespace MultipleClipboards.Presentation.TrayIcon
{
	public class ClipboardMenuItem : MenuItem
	{
		public ClipboardMenuItem(string text, int clipboardId)
			: this(MenuMerge.Add, 0, Shortcut.None, text, clipboardId, null, null, null, null)
		{
		}

		public ClipboardMenuItem(string text, int clipboardId, EventHandler onClick)
			: this(MenuMerge.Add, 0, Shortcut.None, text, clipboardId, onClick, null, null, null)
		{
		}

		public ClipboardMenuItem(string text, int clipboardId, EventHandler onClick, Shortcut shortcut)
			: this(MenuMerge.Add, 0, shortcut, text, clipboardId, onClick, null, null, null)
		{
		}

		public ClipboardMenuItem(string text, int clipboardId, MenuItem[] items)
			: this(MenuMerge.Add, 0, Shortcut.None, text, clipboardId, null, null, null, items)
		{
		}

		private ClipboardMenuItem(MenuMerge mergeType, int mergeOrder, Shortcut shortcut, string text, int clipboardId, EventHandler onClick, EventHandler onPopup, EventHandler onSelect, MenuItem[] items)
			: base(mergeType, mergeOrder, shortcut, text, onClick, onPopup, onSelect, items)
		{
			this.ClipboardId = clipboardId;
		}

		public int ClipboardId
		{
			get;
			set;
		}
	}
}