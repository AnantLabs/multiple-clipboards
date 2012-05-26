using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MultipleClipboards.Entities;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Commands;
using MultipleClipboards.Presentation.Icons;
using log4net;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for ClipboardInspectorTab.xaml
	/// </summary>
	public partial class ClipboardInspectorTab : UserControl
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ClipboardInspectorTab));
		private readonly ClearClipboardCommand clearClipboardCommand = new ClearClipboardCommand();
		private readonly Lazy<Style> linkButtonStyle;

		// TODO: Do this dynamically somehow.
		private const int ContainerHeight = 520;
		private const int TextBoxPadding = 32;
		private const int MinTextBoxHeight = 100;

		public ClipboardInspectorTab()
		{
			InitializeComponent();
			this.linkButtonStyle = new Lazy<Style>(() => (Style)this.FindResource("LinkButton"));
			this.Loaded += this.ClipboardInspectorTabLoaded;			
		}

		private void ClipboardInspectorTabLoaded(object sender, RoutedEventArgs e)
		{
			this.clearSystemClipboardButton.CommandParameter = new ClearClipboardCommandArguments(ClipboardDefinition.SystemClipboardId, this.Refresh);
			this.Refresh();
		}

		private void RefreshButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			this.Refresh();
		}

		private void Refresh()
		{
			try
			{
				this.ClipboardInspectorStackPanel.Children.Clear();
				int textBoxHeight = (ContainerHeight / AppController.ClipboardManager.AvailableClipboards.Count) - TextBoxPadding;

				if (textBoxHeight < MinTextBoxHeight)
				{
					textBoxHeight = MinTextBoxHeight;
				}

				foreach (var clipboard in AppController.ClipboardManager.AvailableClipboards)
				{
					var data = AppController.ClipboardManager.GetClipboardDataByClipboardId(clipboard.ClipboardId);

					if (clipboard.ClipboardId != ClipboardDefinition.SystemClipboardId)
					{
						var horizontalPanel = new StackPanel
						{
							Orientation = Orientation.Horizontal,
							MinHeight = 17
						};

						var labelTextBlock = new TextBlock
						{
							Text = clipboard.ToString(),
							Margin = new Thickness(5, 0, 0, 0),
							FontWeight = FontWeights.SemiBold
						};

						var clearButton = new Button
						{
							Margin = new Thickness(10, 0, 0, 0),
							HorizontalAlignment = HorizontalAlignment.Left,
							VerticalAlignment = VerticalAlignment.Bottom,
							Content = "Clear",
							Style = this.linkButtonStyle.Value,
							Command = this.clearClipboardCommand,
							CommandParameter = new ClearClipboardCommandArguments(clipboard.ClipboardId, this.Refresh)
						};

						horizontalPanel.Children.Add(labelTextBlock);
						horizontalPanel.Children.Add(clearButton);
						this.ClipboardInspectorStackPanel.Children.Add(horizontalPanel);
					}

					var dataTextBox = new TextBox
					{
						Text = data == null ? string.Empty : data.ToDisplayString(),
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
			catch (Exception exception)
			{
				const string errorMessage = "An unexpected error occured while refreshing the Clipboard Inspector tab.";
				log.Error(errorMessage, exception);
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = errorMessage,
					IconType = IconType.Error
				});
			}
		}

		private void ShowDetailedClipboardInformationCheckBoxClicked(object sender, RoutedEventArgs e)
		{
			this.Refresh();
		}
	}
}
