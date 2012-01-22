using System.Windows;
using System.Windows.Controls;
using MultipleClipboards.Persistence;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for ErrorLogTab.xaml
	/// </summary>
	public partial class ApplicationLogTab : UserControl
	{
		private const string EmptyLogFileMessage = "The log file does not exist.  That's a good thing!  There have been no errors.";

		public ApplicationLogTab()
		{
			InitializeComponent();
			this.Loaded += ApplicationLogTab_Loaded;
		}

		private void ApplicationLogTab_Loaded(object sender, RoutedEventArgs e)
		{
			this.Refresh();
		}

		private void RefreshButton_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			this.Refresh();
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			LogManager.ClearLog();
			this.Refresh();
		}

		private void Refresh()
		{
			string logText = LogManager.GetLogText();
			TextAlignment alignment = TextAlignment.Left;
			FontWeight fontWeight = FontWeights.Normal;

			if (string.IsNullOrWhiteSpace(logText))
			{
				logText = EmptyLogFileMessage;
				alignment = TextAlignment.Center;
				fontWeight = FontWeights.SemiBold;
				this.ErrorLogTextBox.Padding = new Thickness(0, 10, 0, 0);
			}

			this.ErrorLogTextBox.Text = logText;
			this.ErrorLogTextBox.TextAlignment = alignment;
			this.ErrorLogTextBox.FontWeight = fontWeight;
		}
	}
}
