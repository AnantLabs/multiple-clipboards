using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MultipleClipboards.Entities;
using MultipleClipboards.Messaging;
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

		public HistoryTab()
		{
			InitializeComponent();
			this.BindClipboardSelectDropdown();
			AppController.Settings.ClipboardDefinitions.CollectionChanged += this.ClipboardDefinitionsCollectionChanged;
		}

		private void ClipboardDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.BindClipboardSelectDropdown();
		}

		private void PasteButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			int clipboardId = (int)this.ClipboardSelectComboBox.SelectedValue;
			ClipboardData data = this.ClipboardHistoryDataGrid.SelectedItem as ClipboardData;

			if (data != null)
			{
				try
				{
					AppController.ClipboardManager.PlaceHistoricalEntryOnClipboard(clipboardId, data.Id);
					MessageBus.Instance.Publish(new MainWindowNotification
					{
						MessageBody = "The data has been placed on the selected clipboard.",
						IconType = IconType.Success
					});
				}
				catch (Exception exception)
				{
					ShowErrorPlacingDataOnClipboard(clipboardId, "An unexpected error occured while placing data on the desired clipboard.", exception);
				}
			}
			else
			{
				ShowErrorPlacingDataOnClipboard(clipboardId, "The data retrieved from the bound data grid was null.", null);
			}
		}

		private void BindClipboardSelectDropdown()
		{
			log.Debug("Binding the clipboard select dropdown on the history tab.");
			IList<ClipboardSelectBinding> bindings = new List<ClipboardSelectBinding>();
			bindings.Add(new ClipboardSelectBinding(ClipboardDefinition.SystemClipboardId, ClipboardDefinition.SystemClipboardDefinition.ToDisplayString()));

			foreach (ClipboardDefinition clipboard in AppController.Settings.ClipboardDefinitions)
			{
				bindings.Add(new ClipboardSelectBinding(clipboard.ClipboardId, string.Format("#{0} - {1}", clipboard.ClipboardId, clipboard.ToDisplayString())));
			}

			this.ClipboardSelectComboBox.ItemsSource = bindings;
			this.ClipboardSelectComboBox.SelectedIndex = 0;
		}

		private static void ShowErrorPlacingDataOnClipboard(int clipboardId, string additionalLogText, Exception exception)
		{
			ClipboardDefinition clipboard = AppController.ClipboardManager.AvailableClipboards.FirstOrDefault(c => c.ClipboardId == clipboardId);
			string baseErrorMessage = string.Format("Error placing historical clipboard data on the clipboard '{0}'.", clipboard == null ? clipboardId.ToString() : clipboard.ToDisplayString());
			log.Error(string.Concat(baseErrorMessage, "  ", additionalLogText), exception);
			MessageBus.Instance.Publish(new MainWindowNotification
			{
				MessageBody = baseErrorMessage,
				IconType = IconType.Error
			});
		}

		private class ClipboardSelectBinding
		{
			public ClipboardSelectBinding(int clipboardId, string displayString)
			{
				this.ClipboardId = clipboardId;
				this.DisplayString = displayString;
			}

			public int ClipboardId
			{
				get;
				set;
			}

			public string DisplayString
			{
				get;
				set;
			}
		}
	}
}
