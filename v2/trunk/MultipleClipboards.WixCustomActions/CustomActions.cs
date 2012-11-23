using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using MultipleClipboards.GlobalResources;

namespace MultipleClipboards.WixCustomActions
{
    public class CustomActions
    {
        private const string AlreadyRunningMessage = "There is already an instance of Multiple Clipboards running.\r\n\r\nIn order for setup to continue we must terminate the running instance.  Is that OK?";
        private const string UninstallConfirmMessage =
            "Would you like to preserve existing application data?\r\n\r\n" +
            "Choosing yes will keep all data and settings files created by this application.  If you choose to re-install this application at a later date your previous settings will be there waiting for you.";

        [CustomAction]
        public static ActionResult CheckForRunningInstances(Session session)
        {
            if (IsUninstall(session) || !EnvironmentHelper.IsMultipleClipboardsRunning())
            {
                return ActionResult.Success;
            }

            if (MessageBox.Show(AlreadyRunningMessage, "Multiple Clipboards Already Running", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) != DialogResult.Yes)
            {
                return ActionResult.UserExit;
            }

            try
            {
                EnvironmentHelper.KillRunningMultipleClipboardsInstances();
                return ActionResult.Success;
            }
            catch
            {
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult PurgeExistingAppData(Session session)
        {
            if (IsUninstall(session) &&
                MessageBox.Show(UninstallConfirmMessage, "Preserve App Data?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes)
            {
                try
                {
                    if (Directory.Exists(Constants.BaseDataPath))
                    {
                        Directory.Delete(Constants.BaseDataPath, true);
                    }
                }
                catch
                {
                }
            }

            return ActionResult.Success;
        }

        private static bool IsUninstall(Session session)
        {
            return !string.IsNullOrEmpty(session["REMOVE"]);
        }
    }
}
