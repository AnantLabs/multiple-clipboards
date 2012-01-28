using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using MultipleClipboards.GlobalResources;

namespace MultipleClipboards.CustomInstallerActions
{
	[RunInstaller(true)]
	public partial class InstallerActions : Installer
	{
		public InstallerActions()
		{
			InitializeComponent();
		}

		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);

			// Save the target directory for use in later stages of the installer.
			stateSaver.Add("TargetDirectory", this.Context.Parameters["AppTargetDirectory"]);
		}

		public override void Commit(IDictionary savedState)
		{
			base.Commit(savedState);

			// Launch the application.
			string path = string.Concat(savedState["TargetDirectory"], @"\", Constants.ApplicationExecutableName);
			Process.Start(path, "-fromShortcut");
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			// Remove app data folder.
			if (Directory.Exists(Constants.BaseDataPath))
			{
				Directory.Delete(Constants.BaseDataPath, true);
			}
		}
	}
}
