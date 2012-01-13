using System;
using System.IO;
using System.Text;

namespace MultipleClipboards.Persistence
{
	[Flags]
	public enum LogLevel
	{
		None = 0x0,
		Error = 0x1,
		Warning = 0x3,
		Debug = 0x7
	}

	public static class LogManager
	{
		private const string InitLogMessage = "\r\n*********************************************************\r\n{0} (UTC)\tApplication Starting.\r\n*********************************************************";
		private const string LogMessageFormatStringWithTimeStamp = "{0}\t-\t{1} (UTC)\r\n{2}\r\n\r\n";
		private const string LogMessageFormatString = "{0}\r\n\r\n";
		private const string DateTimeFormatString = "MM-dd-yyyy hh:mm:ss.fff";
		private static readonly Object FileLock = new Object();

		static LogManager()
		{
			Log(string.Format(InitLogMessage, DateTime.UtcNow.ToString(DateTimeFormatString)), LogLevel.Debug, false);
		}

		public static void Error(Exception exception)
		{
			Error(null, exception);
		}

		public static void Error(string errorMessage)
		{
			Error(errorMessage, null);
		}

		public static void Error(string errorMessage, Exception exception)
		{
			if (exception == null && string.IsNullOrWhiteSpace(errorMessage))
			{
				return;
			}

			StringBuilder errorBuilder = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				errorBuilder.AppendLine(errorMessage);
			}

			while (exception != null)
			{
				errorBuilder.AppendLine(string.Format("An exception of type {0} was thrown.", exception.GetType()));
				errorBuilder.AppendLine(exception.Message);
				errorBuilder.AppendLine(exception.StackTrace);
				errorBuilder.AppendLine();
				exception = exception.InnerException;
			}

			Log(errorBuilder.ToString(), LogLevel.Error);
		}

		public static void ErrorFormat(string errorMessage, params object[] args)
		{
			Error(string.Format(errorMessage, args), null);
		}

		public static void ErrorFormat(string errorMessage, Exception exception, params object[] args)
		{
			Error(string.Format(errorMessage, args), exception);
		}

		public static void Warning(string warningMessage)
		{
			Log(warningMessage, LogLevel.Warning);
		}

		public static void WarningFormat(string warningMessage, params object[] args)
		{
			Warning(string.Format(warningMessage, args));
		}

		public static void Debug(string debugMessage)
		{
			Log(debugMessage, LogLevel.Debug);
		}

		public static void DebugFormat(string debugMessage, params object[] args)
		{
			Debug(string.Format(debugMessage, args));
		}

		private static void Log(string message, LogLevel level)
		{
			Log(message, level, true);
		}

		private static void Log(string message, LogLevel level, bool includeTimeStamp)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(message) || (level & SettingsManager.Instance.ApplicationLogLevel) != level)
				{
					return;
				}

				lock (FileLock)
				{
					string logMessage = includeTimeStamp
						? string.Format(LogMessageFormatStringWithTimeStamp, level.ToString().ToUpperInvariant(), DateTime.UtcNow.ToString(DateTimeFormatString), message)
						: string.Format(LogMessageFormatString, message);

					File.AppendAllText(Constants.LogFilePath, logMessage);
				}
			}
			catch
			{
				// Ya damn right this is an empty catch block.
				// What am I supposed to do?  My logger just died.
			}	
		}

		public static void ClearLog()
		{
			lock (FileLock)
			{
				File.Delete(Constants.LogFilePath);
			}
		}

		public static string GetLogText()
		{
			lock (FileLock)
			{
				try
				{
					return File.ReadAllText(Constants.LogFilePath);
				}
				catch
				{
					return string.Empty;
				}
			}
		}
	}
}
