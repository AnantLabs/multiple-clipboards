using System;
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

			// If app settings already exist then we want to keep them.
			// However, we don't want to keep an old log around when the user installs a new version.
			RenameExistingLogFile();

			// Launch the application.
			string path = string.Concat(savedState["TargetDirectory"], @"\", Constants.ApplicationExecutableName);
			Process.Start(path, "-fromShortcut");
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);
			PurgeExistingAppData();
		}

		private static void PurgeExistingAppData()
		{
			if (Directory.Exists(Constants.BaseDataPath))
			{
				Directory.Delete(Constants.BaseDataPath, true);
			}
		}

		private static void RenameExistingLogFile()
		{
			if (!File.Exists(Constants.LogFilePath))
			{
				return;
			}

			if (!Directory.Exists(Constants.BackupDataPath))
			{
				Directory.CreateDirectory(Constants.BackupDataPath);
			}

			FileInfo file = new FileInfo(Constants.LogFilePath);
			string newFileName = string.Concat(Constants.BackupDataPath, file.Name.Replace(file.Extension, string.Empty), "-", DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss"), file.Extension);
			File.Copy(Constants.LogFilePath, newFileName);
			File.Delete(Constants.LogFilePath);
		}
	}
}
