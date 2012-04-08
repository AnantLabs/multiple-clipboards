using System;
using System.Diagnostics;
using System.Linq;

namespace MultipleClipboards.GlobalResources
{
	public static class EnvironmentHelper
	{
		public static bool IsMultipleClipboardsRunning()
		{
			try
			{
				return Process.GetProcessesByName(Constants.ProcessName).Any(p => p.Id != Process.GetCurrentProcess().Id);
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
				Process.GetProcessesByName(Constants.ProcessName)
					.Where(p => p.Id != Process.GetCurrentProcess().Id)
					.ToList()
					.ForEach(p => p.Kill());
			}
			catch (Exception e)
			{
				EventLog.WriteEntry("Multiple Clipboards", string.Format("Error killing existing multiple clipboards processes.{0}{0}{1}", Environment.NewLine, e), EventLogEntryType.Error);
			}
		}
	}
}
