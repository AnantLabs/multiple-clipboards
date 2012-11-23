using System;
using System.Diagnostics;

namespace MultipleClipboards.GlobalResources
{
	public static class EnvironmentHelper
	{
		public static bool IsMultipleClipboardsRunning()
		{
			try
			{
                Process thisProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(Constants.ProcessName);

                if (processes.Length == 0)
                {
                    return false;
                }

                foreach (var process in processes)
                {
                    if (process.Id != thisProcess.Id)
                    {
                        return true;
                    }
                }

                return false;
			}
			catch (Exception e)
			{
				EventLog.WriteEntry("Multiple Clipboards", string.Format("Error determining whether or not multiple clipboards is running.{0}{0}{1}", Environment.NewLine, e), EventLogEntryType.Error);
				return false;
			}
		}

		public static void KillRunningMultipleClipboardsInstances()
		{
			try
			{
                Process thisProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcessesByName(Constants.ProcessName);

                foreach (var process in processes)
                {
                    if (process.Id == thisProcess.Id)
                    {
                        continue;
                    }

                    process.Kill();
                }
			}
			catch (Exception e)
			{
				EventLog.WriteEntry("Multiple Clipboards", string.Format("Error killing existing multiple clipboards processes.{0}{0}{1}", Environment.NewLine, e), EventLogEntryType.Error);
			}
		}
	}
}
