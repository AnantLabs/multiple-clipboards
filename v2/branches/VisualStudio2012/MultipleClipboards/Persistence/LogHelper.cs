using System;
using System.IO;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.Persistence
{
	public static class LogHelper
	{
		private const string ErrorAccessingLogFileMessageFormat =
			"There was an error {0} the log file.  This usually means the applications logger has a lock on it.  Trying again should fix this.";

		public static void ClearLog()
		{
			try
			{
				File.Delete(Constants.LogFilePath);
			}
			catch (Exception)
			{
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = string.Format(ErrorAccessingLogFileMessageFormat, "deleting"),
					IconType = IconType.Error
				});
			}
		}

		public static string GetLogText()
		{
			try
			{
				return !File.Exists(Constants.LogFilePath) ? string.Empty : File.ReadAllText(Constants.LogFilePath);
			}
			catch (Exception e)
			{
				string errorMessaage = string.Format(ErrorAccessingLogFileMessageFormat, "reading");
				MessageBus.Instance.Publish(new MainWindowNotification
				{
					MessageBody = errorMessaage,
					IconType = IconType.Error
				});
				return string.Format("{0}{1}{1}{2}", errorMessaage, Environment.NewLine, e);
			}
		}
	}
}
