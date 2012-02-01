using System;
using System.IO;
using MultipleClipboards.GlobalResources;

namespace MultipleClipboards.Persistence
{
	public static class LogHelper
	{
		private const string ErrorReadingLogFileMessage = "There was an error reading the log file.  This usually means the applications logger has a lock on it.  Refreshing this tab should fix this.";

		public static void ClearLog()
		{
			File.Delete(Constants.LogFilePath);
		}

		public static string GetLogText()
		{
			try
			{
				return !File.Exists(Constants.LogFilePath) ? string.Empty : File.ReadAllText(Constants.LogFilePath);
			}
			catch (Exception e)
			{
				return string.Format("{0}{1}{1}{2}", ErrorReadingLogFileMessage, Environment.NewLine, e);
			}
		}
	}
}
