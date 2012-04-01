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
			catch (Exception)
			{
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
			catch (Exception)
			{
			}
		}
	}
}
