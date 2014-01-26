using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MultipleClipboards.Entities;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for SettingsTab.xaml
	/// </summary>
	public partial class SettingsTab : UserControl
	{
		public SettingsTab()
		{
			InitializeComponent();
			this.BindModifierKeyOneComboBox();
			this.AddNewClipboardButton.Click += this.AddNewClipboardButtonClick;
		}

		private void BindModifierKeyOneComboBox()
		{
			ICollectionView view = new CollectionViewSource
			{
				Source = Enum.GetValues(typeof(ModifierKeys))
			}.View;
			view.Filter = item => (ModifierKeys)item != ModifierKeys.None;
			view.MoveCurrentTo(ModifierKeys.Control);
			this.ModifierKeyOneComboBox.ItemsSource = view;
		}

		private void AddNewClipboardButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			if (string.IsNullOrWhiteSpace(this.CopyKeyTextBox.Text) || string.IsNullOrWhiteSpace(this.CutKeyTextBox.Text) || string.IsNullOrWhiteSpace(this.PasteKeyTextBox.Text))
			{
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = "All fields are required when creating a new clipboard.",
					IconType = IconType.Error
				});
				return;
			}

			var modifierKeysType = typeof(ModifierKeys);
			var keysType = typeof(Key);
			this.AddNewClipboardButton.IsEnabled = false;
			ClipboardDefinition clipboard = new ClipboardDefinition
			{
				ModifierOneKey = (ModifierKeys)Enum.Parse(modifierKeysType, this.ModifierKeyOneComboBox.SelectedValue.ToString(), true),
				ModifierTwoKey = (ModifierKeys)Enum.Parse(modifierKeysType, this.ModifierKeyTwoComboBox.SelectedValue.ToString(), true),
				CopyKey = (Key)Enum.Parse(keysType, this.CopyKeyTextBox.Text, true),
				CutKey = (Key)Enum.Parse(keysType, this.CutKeyTextBox.Text, true),
				PasteKey = (Key)Enum.Parse(keysType, this.PasteKeyTextBox.Text, true)
			};

			AppController.ClipboardManager.AddClipboard(clipboard);
			this.AddNewClipboardButton.IsEnabled = true;
		}

		private void DeleteButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			ClipboardDefinition clipboard = ((FrameworkElement)sender).DataContext as ClipboardDefinition;

			if (clipboard != null)
			{
				AppController.ClipboardManager.RemoveClipboard(clipboard);
			}
		}
	}
}
