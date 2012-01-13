﻿using System;
using System.Collections.Generic;

namespace MultipleClipboards.Persistence
{
	public static class Constants
	{
		// File names.  These will never change without a new build.
		private const string SettingsFileName = "MultipleClipboardsSettings.xml";
		private const string LogFileName = "MultipleClipboardsLog.txt";
		private const string AboutTextFileName = "AboutText.rtf";

		// Application Setting Keys.
		public const string NumberOfClipboardHistoryRecordsSettingKey = "NumberOfClipboardHistoryRecords";
		public const string ThreadDelayTimeSettingKey = "ThreadDelayTime";
		public const string ApplicationLogLevelSettingKey = "LogLevel";
		public const string NumberOfClipboardOperationRetriesSettingKey = "NumberOfClipboardOperationRetries";

		// Application Settings Default Values.
		public static readonly IDictionary<string, dynamic> DefaultSettings =
			new Dictionary<string, dynamic>
			{
				{ NumberOfClipboardHistoryRecordsSettingKey, 20 },
				{ ThreadDelayTimeSettingKey, 250 },
				{ ApplicationLogLevelSettingKey, LogLevel.Error },
				{ NumberOfClipboardOperationRetriesSettingKey, 2 }
			};

		// File paths, which are runtime constants.
		public static readonly string BaseDataPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"\MultipleClipboards\");
		public static readonly string SettingsFilePath = GetFilePath(SettingsFileName);
		public static readonly string LogFilePath = GetFilePath(LogFileName);
		public static readonly string AboutTextFilePath = GetFilePath(AboutTextFileName);

		private static string GetFilePath(string fileName)
		{
			return string.Concat(BaseDataPath, fileName);
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
