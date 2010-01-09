using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MultipleClipboards
{
	/// <summary>
	/// A Data Grid View Row object specifically for clipboard history entries.
	/// </summary>
	public class ClipboardHistoryDataGridViewRow : DataGridViewRow
	{
		private int _clipboardHistoryIndex;

		/// <summary>
		/// Constructs a new ClipbaordHistoryDataGridView object.
		/// </summary>
		/// <param name="clipboardHistoryIndex">The index of the clipboard history item in the history queue.</param>
		/// <param name="gridView">A reference to the grid that this row will be a part of.</param>
		/// <param name="visibleIndex">The index of the clipboard history item that will display in the grid.</param>
		/// <param name="text">The text to display in the grid.</param>
		/// <param name="timestamp">The timestamp of the item.</param>
		public ClipboardHistoryDataGridViewRow(int clipboardHistoryIndex, DataGridView gridView, int visibleIndex, string text, DateTime timestamp)
			: base()
		{
			this._clipboardHistoryIndex = clipboardHistoryIndex;
			this.CreateCells(gridView, new object[] { visibleIndex.ToString(), text, timestamp.ToShortTimeString() });
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
