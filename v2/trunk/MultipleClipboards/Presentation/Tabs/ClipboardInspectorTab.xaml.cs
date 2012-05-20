using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using MultipleClipboards.Entities;
using MultipleClipboards.Messaging;
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
		private const string ClearLinkFakeUrlPrefix = "http://fake/";
		private readonly List<Hyperlink> clearLinks = new List<Hyperlink>();

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
			try
			{
				foreach (var clearLink in this.clearLinks)
				{
					clearLink.RequestNavigate -= this.OnClearButtonClick;
				}

				this.clearLinks.Clear();
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
						var labelTextBlock = new TextBlock
						{
							Text = string.Concat(clipboard.ToString(), "    "),
							Margin = new Thickness(5, 0, 0, 0),
							FontWeight = FontWeights.SemiBold
						};

						var clearLink = new Hyperlink();
						clearLink.NavigateUri = new Uri(string.Concat(ClearLinkFakeUrlPrefix, clipboard.ClipboardId));
						clearLink.RequestNavigate += this.OnClearButtonClick;
						clearLink.Inlines.Add("Clear");
						clearLinks.Add(clearLink);

						labelTextBlock.Inlines.Add(clearLink);
						this.ClipboardInspectorStackPanel.Children.Add(labelTextBlock);
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

		private void OnClearButtonClick(object sender, RequestNavigateEventArgs e)
		{
			int clipboardId = int.Parse(e.Uri.ToString().Replace(ClearLinkFakeUrlPrefix, string.Empty));
			AppController.ClipboardManager.ClearClipboardContents(clipboardId, isCalledFromUi: true);
			this.Refresh();
		}
	}
}
