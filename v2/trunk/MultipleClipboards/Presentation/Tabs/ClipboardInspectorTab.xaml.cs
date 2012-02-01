using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MultipleClipboards.Entities;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for ClipboardInspectorTab.xaml
	/// </summary>
	public partial class ClipboardInspectorTab : UserControl
	{
		// TODO: Do this dynamically somehow.
		private const int ContainerHeight = 520;
		private const int TextBoxPadding = 32;
		private const int MinTextBoxHeight = 100;

		public ClipboardInspectorTab()
		{
			InitializeComponent();
			this.Loaded += this.ClipboardInspectorTabLoaded;
		}

		private void ClipboardInspectorTabLoaded(object sender, RoutedEventArgs e)
		{
			this.Refresh();
		}

		private void RefreshButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			this.Refresh();
		}

		private void Refresh()
		{
			this.ClipboardInspectorStackPanel.Children.Clear();
			int textBoxHeight = (ContainerHeight / AppController.ClipboardManager.AvailableClipboards.Count) - TextBoxPadding;

			if (textBoxHeight < MinTextBoxHeight)
			{
				textBoxHeight = MinTextBoxHeight;
			}

			foreach (ClipboardDefinition clipboard in AppController.ClipboardManager.AvailableClipboards)
			{
				ClipboardData data = AppController.ClipboardManager.ClipboardDataByClipboardId[clipboard.ClipboardId];

				if (clipboard.ClipboardId != ClipboardDefinition.SystemClipboardId)
				{
					TextBlock labelTextBlock = new TextBlock
					{
						Text = clipboard.ToDisplayString(),
						Margin = new Thickness(5, 0, 0, 0),
						FontWeight = FontWeights.SemiBold
					};
					this.ClipboardInspectorStackPanel.Children.Add(labelTextBlock);
				}

				TextBox dataTextBox = new TextBox
				{
					Text = data == null ? string.Empty : data.ToLongDisplayString(),
					HorizontalAlignment = HorizontalAlignment.Stretch,
					VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
					HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
					Height = textBoxHeight,
					Padding = new Thickness(5),
					Background = Brushes.White,
					BorderThickness = new Thickness(1),
					BorderBrush = Brushes.Black,
					Margin = new Thickness(5)
				};

				this.ClipboardInspectorStackPanel.Children.Add(dataTextBox);
			}
		}
	}
}
