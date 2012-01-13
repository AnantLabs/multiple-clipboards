using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MultipleClipboards
{
	/// <summary>
	/// A Tool Strip Menu Item object specifically for clipboard history entries.
	/// </summary>
	public class ClipboardHistoryToolStripMenuItem : ToolStripMenuItem
	{
		private int _clipboardHistoryIndex;

		/// <summary>
		/// Constructs a new ClipboardHistoryToolStripMenuItem.
		/// </summary>
		/// <param name="text">The text to display on the menu item.</param>
		/// <param name="clipboardHistoryIndex">The index for this item in the clipboard history queue.</param>
		/// <param name="onClick">The on click event handler for this item.</param>
		public ClipboardHistoryToolStripMenuItem(string text, int clipboardHistoryIndex, EventHandler onClick)
			: base(text, null, onClick)
		{
			this._clipboardHistoryIndex = clipboardHistoryIndex;
		}

		/// <summary>
		/// Gets the index for this item in the clipboard history queue.
		/// </summary>
		public int ClipboardHistoryIndex
		{
			get
			{
				return this._clipboardHistoryIndex;
			}
		}
	}
}
