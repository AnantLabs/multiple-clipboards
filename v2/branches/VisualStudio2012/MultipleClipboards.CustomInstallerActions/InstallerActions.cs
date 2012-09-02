using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Windows;
using MultipleClipboards.GlobalResources;

namespace MultipleClipboards.CustomInstallerActions
{
	[RunInstaller(true)]
	public partial class InstallerActions : Installer
	{
		private const string UninstallConfirmMessage =
			"Would you like to preserve existing application data?\r\n\r\n" +
			"Choosing yes will keep all data and settings files created by this application.  If you choose to re-install this application at a later date your previous settings will be there waiting for you.";

		public InstallerActions()
		{
			InitializeComponent();
		}

		public override void Install(IDictionary stateSaver)
		{
			if (EnvironmentHelper.IsMultipleClipboardsRunning())
			{
				const string message = "There is already an instance of Multiple Clipboards running.\r\n\r\nWould you like us to terminate the running instance and proceed with the install?";

				if (MessageBox.Show(message, "Multiple Clipboards Already Running", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
				{
					throw new InstallException("Unable to install.  Multiple Clipboards is already running.");
				}

				EnvironmentHelper.KillRunningMultipleClipboardsInstances();
			}

			base.Install(stateSaver);

			// Save the target directory for use in later stages of the installer.
			stateSaver.Add("TargetDirectory", this.Context.Parameters["AppTargetDirectory"]);
		}

		public override void Commit(IDictionary savedState)
		{
			base.Commit(savedState);
			BackupExistingAppData();

			// Launch the application.
			string path = string.Concat(savedState["TargetDirectory"], @"\", Constants.ApplicationExecutableName);
			Process.Start(path, "-fromShortcut");
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);

			if (MessageBox.Show(UninstallConfirmMessage, "Preserve App Data?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				PurgeExistingAppData();
			}
		}

		private static void PurgeExistingAppData()
		{
			if (Directory.Exists(Constants.BaseDataPath))
			{
				Directory.Delete(Constants.BaseDataPath, true);
			}
		}

		private static void BackupExistingAppData()
		{
			if (!Directory.Exists(Constants.BackupDataPath))
			{
				Directory.CreateDirectory(Constants.BackupDataPath);
			}

			string backupFolderName = string.Concat(Constants.BackupDataPath, DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss"), @"\");
			Directory.CreateDirectory(backupFolderName);

			foreach (var fileName in Directory.GetFiles(Constants.BaseDataPath))
			{
				FileInfo file = new FileInfo(fileName);
				file.CopyTo(string.Concat(backupFolderName, file.Name));
				
				if (!fileName.Equals(Constants.SettingsFilePath, StringComparison.InvariantCultureIgnoreCase))
				{
					file.Delete();
				}
			}
		}
	}
}
