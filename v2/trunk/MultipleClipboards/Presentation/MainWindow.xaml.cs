using System.Windows;
using System.Windows.Input;

namespace MultipleClipboards.Presentation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			this.InitializeComponent();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void DragStart(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
			e.Handled = true;
		}
	}
}
