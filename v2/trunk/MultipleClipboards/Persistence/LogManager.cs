using System;
using System.IO;
using System.Text;
using System.Threading;
using MultipleClipboards.Interop;
using MultipleClipboards.GlobalResources;

namespace MultipleClipboards.Persistence
{
	public static class LogManager
	{
		private const string InitLogMessage = "\r\n*********************************************************\r\n{0}\tApplication Starting.\r\n*********************************************************";
		private const string DateTimeFormatString = "MM-dd-yyyy hh:mm:ss.fff";
		private static readonly Object FileLock = new Object();

		static LogManager()
		{
			Log(string.Format(InitLogMessage, DateTime.Now.ToString(DateTimeFormatString)), LogLevel.Debug, false);
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
				if (string.IsNullOrWhiteSpace(message) || (level & AppController.Settings.ApplicationLogLevel) != level)
				{
					return;
				}

				lock (FileLock)
				{
					StringBuilder logBuilder = new StringBuilder();
					logBuilder.Append(level.ToString().ToUpperInvariant());

					if (includeTimeStamp)
					{
						logBuilder.AppendFormat(" | {0}", DateTime.Now.ToString(DateTimeFormatString));
					}

					logBuilder.AppendFormat(" | Managed Thread ID: {0}  |  Native Thread ID: {1}\r\n{2}\r\n\r\n",
					                        Thread.CurrentThread.ManagedThreadId,
					                        Win32API.GetCurrentThreadId(),
					                        message);

					File.AppendAllText(Constants.LogFilePath, logBuilder.ToString());
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
