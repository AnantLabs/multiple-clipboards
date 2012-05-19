using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MultipleClipboards.Entities;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Commands;
using MultipleClipboards.Presentation.Icons;
using log4net;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for HistoryTab.xaml
	/// </summary>
	public partial class HistoryTab : UserControl
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(HistoryTab));
		private ulong selectedClipboardDataId;

		public HistoryTab()
		{
			InitializeComponent();
		}

		private void ClearButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			var result = MessageBox.Show("Are you sure you wish to permenantly delete all items from the clipboard history?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);

			if (result == MessageBoxResult.Yes)
			{
				AppController.ClipboardManager.ClearClipboardHistory();
			}
		}

		private void ContextMenuOpened(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			this.selectedClipboardDataId = ((ClipboardData)this.ClipboardHistoryDataGrid.SelectedItem).Id;
		}

		private void GridMenuItemClick(object sender, RoutedEventArgs e)
		{
			int? clipboardId = null;

			try
			{
				clipboardId = ((ClipboardDefinition)((MenuItem)sender).DataContext).ClipboardId;
				AppController.ClipboardManager.PlaceHistoricalEntryOnClipboard(clipboardId.Value, this.selectedClipboardDataId);
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = "The data has been placed on the selected clipboard.",
					IconType = IconType.Success
				});
			}
			catch (Exception exception)
			{
				var clipboard = clipboardId.HasValue ? AppController.ClipboardManager.AvailableClipboards.FirstOrDefault(c => c.ClipboardId == clipboardId) : null;
				string baseErrorMessage = string.Format("Error placing historical clipboard data on the clipboard '{0}'.", clipboard == null ? clipboardId.ToString() : clipboard.ToString());
				log.Error(string.Concat(baseErrorMessage, "  ", "An unexpected error occured while placing data on the desired clipboard."), exception);
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = baseErrorMessage,
					IconType = IconType.Error
				});
			}
		}
	}
}
