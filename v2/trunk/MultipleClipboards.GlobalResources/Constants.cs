using System;
using System.IO;

namespace MultipleClipboards.GlobalResources
{
	public static class Constants
	{
		public const int ClipboardHistoryPreviewLength = 60;

		// File names.  These will never change without a new build.
		private const string SettingsFileName = "MultipleClipboardsSettings.xml";
		private const string PersistedHistoryFileName = "ClipboardHistory.dat";
		private const string LogFileName = "MultipleClipboardsLog.log";
		private const string AboutTextFileName = "AboutText.rtf";
		private const string ShortcutFileName = "Multiple Clipboards.lnk";
		public const string ProcessName = "MultipleClipboards";
		public static readonly string ApplicationExecutableName = string.Concat(ProcessName, ".exe");

		// File paths, which are runtime constants.
		public static readonly string BaseDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultipleClipboards");
		public static readonly string BackupDataPath = GetDataFilePath("Backup");
		public static readonly string SettingsFilePath = GetDataFilePath(SettingsFileName);
		public static readonly string PersistedHistoryFilePath = GetDataFilePath(PersistedHistoryFileName);
		public static readonly string LogFilePath = GetDataFilePath(LogFileName);
		public static readonly string AboutTextFilePath = GetDataFilePath(AboutTextFileName);
        public static readonly string ShortcutPath = GetDataFilePath(ShortcutFileName);
		public static readonly string AutoLaunchShortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ShortcutFileName);

		private static string GetDataFilePath(string fileName)
		{
			return Path.Combine(BaseDataPath, fileName);
		}

		// Environment settings, which are runtime constants.
		private const int MinimumOSMajorVersionForNoRepeatSupport = 6;
		private const int MinimumOSMinorVersionForNoRepeatSupport = 1;
		private static bool? _supportsNoRepeat;

		public static bool SupportsNoRepeat
		{
			get
			{
				if (!_supportsNoRepeat.HasValue)
				{
					Version version = Environment.OSVersion.Version;
					_supportsNoRepeat = version.Major == MinimumOSMajorVersionForNoRepeatSupport && version.Minor == MinimumOSMinorVersionForNoRepeatSupport;
				}

				return _supportsNoRepeat.Value;
			}
		}
	}
}
