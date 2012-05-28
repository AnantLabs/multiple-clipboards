using System;
using System.IO;
using System.Reflection;

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
		public static readonly string LogConfigFileName = string.Concat(BaseApplicationDirectory, "log4net.config");

		// File paths, which are runtime constants.
		public static readonly string BaseDataPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\MultipleClipboards\");
		public static readonly string BackupDataPath = string.Concat(BaseDataPath, @"Backup\");
		public static readonly string SettingsFilePath = GetFilePath(SettingsFileName);
		public static readonly string PersistedHistoryFilePath = GetFilePath(PersistedHistoryFileName);
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
				if (string.IsNullOrWhiteSpace(_applicationDirectory))
				{
					try
					{
						var assembly = Assembly.GetEntryAssembly();
					
						if (assembly.ManifestModule.Name == ApplicationExecutableName)
						{
							FileInfo assemblyFileInfo = new FileInfo(Assembly.GetEntryAssembly().Location);
							_applicationDirectory = string.Concat(assemblyFileInfo.DirectoryName, @"\");
						}
						else
						{
							_applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
						}
					}
					catch
					{
						// This code is used in my installer custom actions, and this property throws an exception.
						// Maybe some day I will make this better.
						_applicationDirectory = string.Empty;
					}
				}

				return _applicationDirectory;
			}
		}
	}
}
