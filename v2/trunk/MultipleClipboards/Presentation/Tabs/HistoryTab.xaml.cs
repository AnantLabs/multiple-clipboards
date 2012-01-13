using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MultipleClipboards.ClipboardManagement;
using MultipleClipboards.Entities;
using MultipleClipboards.Persistence;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for HistoryTab.xaml
	/// </summary>
	public partial class HistoryTab : UserControl
	{
		public HistoryTab()
		{
			InitializeComponent();
			this.BindClipboardSelectDropdown();
			SettingsManager.Instance.ClipboardDefinitions.CollectionChanged += this.ClipboardDefinitions_CollectionChanged;
		}

		private void ClipboardDefinitions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.BindClipboardSelectDropdown();
		}

		private void PasteButton_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			int clipboardId = (int)this.ClipboardSelectComboBox.SelectedValue;
			ClipboardData data = this.ClipboardHistoryDataGrid.SelectedItem as ClipboardData;

			if (data != null)
			{
				ClipboardManager.Instance.PlaceHistoricalEntryOnClipboard(clipboardId, data.Id);
			}
			else
			{
				ClipboardDefinition clipboard = ClipboardManager.Instance.AvailableClipboards.FirstOrDefault(c => c.ClipboardId == clipboardId);
				LogManager.ErrorFormat("Error placing historical clipboard data on the clipboard '{0}'.  The data retrieved from the bound data grid was null.", clipboard == null ? clipboardId.ToString() : clipboard.ToDisplayString());
				// TODO: Display some error message to the user.
			}
		}

		private void BindClipboardSelectDropdown()
		{
			LogManager.Debug("Binding the clipboard select dropdown on the history tab.");
			IList<ClipboardSelectBinding> bindings = new List<ClipboardSelectBinding>();
			bindings.Add(new ClipboardSelectBinding(ClipboardDefinition.SystemClipboardDefinition.ClipboardId, ClipboardDefinition.SystemClipboardDefinition.ToDisplayString()));

			foreach (ClipboardDefinition clipboard in SettingsManager.Instance.ClipboardDefinitions)
			{
				bindings.Add(new ClipboardSelectBinding(clipboard.ClipboardId, string.Format("#{0} - {1}", clipboard.ClipboardId, clipboard.ToDisplayString())));
			}

			this.ClipboardSelectComboBox.ItemsSource = bindings;
			this.ClipboardSelectComboBox.SelectedIndex = 0;
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
