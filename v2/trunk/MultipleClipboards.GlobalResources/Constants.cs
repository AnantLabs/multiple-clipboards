using System;
using System.IO;
using System.Reflection;

namespace MultipleClipboards.GlobalResources
{
	public static class Constants
	{
		// File names.  These will never change without a new build.
		private const string SettingsFileName = "MultipleClipboardsSettings.xml";
		private const string LogFileName = "MultipleClipboardsLog.log";
		private const string AboutTextFileName = "AboutText.rtf";
		private const string ShortcutFileName = "Multiple Clipboards.lnk";
		public const string ProcessName = "MultipleClipboards";
		public static readonly string ApplicationExecutableName = string.Concat(ProcessName, ".exe");
		public static readonly string LogConfigFileName = string.Concat(BaseApplicationDirectory, "log4net.config");

		// File paths, which are runtime constants.
		public static readonly string BaseDataPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\MultipleClipboards\");
		public static readonly string BackupDataPath = string.Concat(BaseDataPath, @"Backup\");
		public static readonly string SettingsFilePath = GetFilePath(SettingsFileName);
		public static readonly string LogFilePath = GetFilePath(LogFileName);
		public static readonly string AboutTextFilePath = GetFilePath(AboutTextFileName);
		public static readonly string ShortcutPath = string.Concat(BaseApplicationDirectory, ShortcutFileName);
		public static readonly string AutoLaunchShortcutPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Startup), @"\", ShortcutFileName);

		private static string GetFilePath(string fileName)
		{
			return string.Concat(BaseDataPath, fileName);
		}

		// Environment settings, which are runtime constants.
		private const int MinimumOSMajorVersionForNoRepeatSupport = 6;
		private const int MinimumOSMinorVersionForNoRepeatSupport = 1;
		private static string _applicationDirectory;
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

		private static string BaseApplicationDirectory
		{
			get
			{
				try
				{
					if (string.IsNullOrWhiteSpace(_applicationDirectory))
					{
						FileInfo assembly = new FileInfo(Assembly.GetEntryAssembly().Location);
						_applicationDirectory = string.Concat(assembly.DirectoryName, @"\");
					}
				}
				catch
				{
					// TODO: Move the application directory stuff up into the app project.
					// The only reason this file is in a seperate project is because the installer needs the app data paths.
					// But the reflection stuff fucks it all up.
				}
				return _applicationDirectory;
			}
		}
	}
}
