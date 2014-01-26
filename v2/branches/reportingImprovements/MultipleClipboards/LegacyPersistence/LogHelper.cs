using System;
using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Core;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.LegacyPersistence
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

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

        public static void SetLogLevel(LogLevel level)
        {
            Level newLevel;
            switch (level)
            {
                case LogLevel.Debug:
                    newLevel = Level.Debug;
                    break;
                case LogLevel.Info:
                    newLevel = Level.Info;
                    break;
                case LogLevel.Warn:
                    newLevel = Level.Warn;
                    break;
                case LogLevel.Error:
                    newLevel = Level.Error;
                    break;
                case LogLevel.Fatal:
                    newLevel = Level.Fatal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }

            foreach (var appender in LogManager.GetAllRepositories().SelectMany(r => r.GetAppenders().OfType<AppenderSkeleton>()))
            {
                appender.Threshold = newLevel;
            }
        }
	}
}
