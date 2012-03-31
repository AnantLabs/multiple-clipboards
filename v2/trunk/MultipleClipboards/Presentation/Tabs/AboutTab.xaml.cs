using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MultipleClipboards.Presentation.Tabs
{
	/// <summary>
	/// Interaction logic for AboutTab.xaml
	/// </summary>
	public partial class AboutTab : UserControl
	{
		public AboutTab()
		{
			InitializeComponent();
			this.VersionTextBlock.Text = string.Format("Version {0}", Assembly.GetEntryAssembly().GetName().Version);
		}

		private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.ToString());
		}
	}
}
