using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using MultipleClipboards.GlobalResources;

namespace MultipleClipboards.WixCustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CheckForRunningInstances(Session session)
        {
            if (!EnvironmentHelper.IsMultipleClipboardsRunning())
            {
                session["APP_RUNNING"] = "0";
                return ActionResult.Success;
            }

            const string message = "There is already an instance of Multiple Clipboards running.\r\n\r\nWould you like us to terminate the running instance and proceed with the install?";

            if (MessageBox.Show(message, "Multiple Clipboards Already Running", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                session["APP_RUNNING"] = "1";
                return ActionResult.UserExit;
            }

            try
            {
                EnvironmentHelper.KillRunningMultipleClipboardsInstances();
                session["APP_RUNNING"] = "0";
                return ActionResult.Success;
            }
            catch
            {
                session["APP_RUNNING"] = "1";
                return ActionResult.Failure;
            }
        }
    }
}
