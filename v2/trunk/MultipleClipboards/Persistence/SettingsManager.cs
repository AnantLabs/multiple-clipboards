using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using MultipleClipboards.Entities;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Icons;
using log4net;

namespace MultipleClipboards.Persistence
{
	public enum LogLevel
	{
		Debug,
		Info,
		Warn,
		Error,
		Fatal
	}

	/// <summary>
	/// Class to manage the settings for the application.
	/// </summary>
	public sealed class SettingsManager
	{
		private static readonly ManualResetEvent loggerConfigFileResetEvent = new ManualResetEvent(true);
		private static readonly ILog log = LogManager.GetLogger(typeof(SettingsManager));

		// Application Setting Keys.
		private const string NumberOfClipboardHistoryRecordsSettingKey = "NumberOfClipboardHistoryRecords";
		private const string ThreadDelayTimeSettingKey = "ThreadDelayTime";
		private const string ApplicationLogLevelSettingKey = "LogLevel";
		private const string LaunchApplicationOnSystemStartupSettingKey = "LaunchApplicationOnSystemStartup";
		private const string ShowAdvancedOptionsSettingKey = "ShowAdvancedOptions";
		private const string ShowMessagesFromTraySettingKey = "ShowMessagesFromTray";
		private const string ShowDetailedClipboardInformationSettingKey = "ShowDetailClipboardInformation";
		private const string PersistClipboardHistorySettingKey = "PersistClipboardHistory";

		// Application Settings Default Values.
		private static readonly IDictionary<string, object> defaultSettings =
			new Dictionary<string, object>
			{
				{ NumberOfClipboardHistoryRecordsSettingKey, 20 },
				{ ThreadDelayTimeSettingKey, 250 },
				{ ApplicationLogLevelSettingKey, LogLevel.Error },
				{ LaunchApplicationOnSystemStartupSettingKey, true },
				{ ShowAdvancedOptionsSettingKey, false },
				{ ShowMessagesFromTraySettingKey, true },
				{ ShowDetailedClipboardInformationSettingKey, false },
				{ PersistClipboardHistorySettingKey, true }
			};

		// The in memory version of the log level.
		// This setting gets saved to the seperate logger config file, so it works differently than the rest.
		// Perhaps eventually I should just move the log4net config section into my own settings file.
		private LogLevel? logLevel;

		/// <summary>
		/// Constructs a new Settings Manager object.
		/// </summary>
		public SettingsManager()
		{
			this.LoadSettings();
		}

		/// <summary>
		/// Gets or sets the persisted data store for all the settings.
		/// </summary>
		private MultipleClipboardsDataModel DataStore
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the number of historical records to keep track of.
		/// </summary>
		public int NumberOfClipboardHistoryRecords
		{
			get
			{
				return this.GetSettingSafe(NumberOfClipboardHistoryRecordsSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(NumberOfClipboardHistoryRecordsSettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets the number of milliseconds the application should sleep between clipboard operations.
		/// </summary>
		public int ThreadDelayTime
		{
			get
			{
				return this.GetSettingSafe(ThreadDelayTimeSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(ThreadDelayTimeSettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets the application logging level.
		/// </summary>
		/// <remarks>
		/// This property is different than all the rest.  I still store it in the regular settings file because I have the framework and it's easy,
		/// but any changes to this need to be persisted to the App.config file so log4net picks up on the change.
		/// </remarks>
		public LogLevel ApplicationLogLevel
		{
			get
			{
				if (!this.logLevel.HasValue)
				{
					this.logLevel = GetLog4NetLogLevel();
				}
				return this.logLevel.Value;
			}
			set
			{
				this.logLevel = value;
				UpdateLog4NetConfig(value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not the application should automatically launch when the system starts.
		/// </summary>
		public bool LaunchApplicationOnSystemStartup
		{
			get
			{
				return this.GetSettingSafe(LaunchApplicationOnSystemStartupSettingKey);
			}
			set
			{
				try
				{
					ToggleAutoLaunchShortcut(value);
					this.SaveApplicationSetting(LaunchApplicationOnSystemStartupSettingKey, value);
				}
				catch (Exception e)
				{
					const string baseErrorMessage = "There was an error setting or removing the auto-launch shortcut.";
					log.Error(baseErrorMessage, e);
					MessageBus.Instance.Publish(new MainWindowNotification
					{
						MessageBody = baseErrorMessage + "  A detailed error report has been saved to the log.",
						IconType = IconType.Error
					});
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to show the advanced options portion of the UI.
		/// </summary>
		public bool ShowAdvancedOptions
		{
			get
			{
				return this.GetSettingSafe(ShowAdvancedOptionsSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(ShowAdvancedOptionsSettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to show application notification popups from the system tray.
		/// </summary>
		public bool ShowMessagesFromTray
		{
			get
			{
				return this.GetSettingSafe(ShowMessagesFromTraySettingKey);
			}
			set
			{
				this.SaveApplicationSetting(ShowMessagesFromTraySettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to show detailed clipboard information on the clipboard inspector tab.
		/// </summary>
		public bool ShowDetailedClipboardInformation
		{
			get
			{
				return this.GetSettingSafe(ShowDetailedClipboardInformationSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(ShowDetailedClipboardInformationSettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to persist the clipboard history content to disk when the application exits or the user logs off.
		/// </summary>
		public bool PersistClipboardHistory
		{
			get
			{
				return this.GetSettingSafe(PersistClipboardHistorySettingKey);
			}
			set
			{
				this.SaveApplicationSetting(PersistClipboardHistorySettingKey, value);
			}
		}

		/// <summary>
		/// Gets the list of all the defined clipboard.
		/// </summary>
		public ObservableCollection<ClipboardDefinition> ClipboardDefinitions
		{
			get
			{
				return this.DataStore.ClipboardDefinitions;
			}
		}

		/// <summary>
		/// Gets the next available clipboard ID.
		/// </summary>
		/// <returns></returns>
		public int GetNextClipboardId()
		{
			if (this.ClipboardDefinitions == null || !this.ClipboardDefinitions.Any())
			{
				return 1;
			}

			return this.ClipboardDefinitions.Max(cd => cd.ClipboardId + 1);
		}

		/// <summary>
		/// Adds a new clipboard to the data store.
		/// </summary>
		/// <param name="clipboard">The clipboard to add.</param>
		/// <returns>The ID of the new clipboard.</returns>
		public int AddNewClipboard(ClipboardDefinition clipboard)
		{
			this.DataStore.ClipboardDefinitions.Add(clipboard);
			this.SaveSettings();
			return clipboard.ClipboardId;
		}

		/// <summary>
		/// Removes a clipboard from the data store.
		/// </summary>
		public void RemoveClipboard(int clipboardId)
		{
			this.RemoveClipboard(this.DataStore.ClipboardDefinitions.Single(c => c.ClipboardId == clipboardId));
		}

		/// <summary>
		/// Removes a clipboard from the data store.
		/// </summary>
		public void RemoveClipboard(ClipboardDefinition clipboard)
		{
			this.DataStore.ClipboardDefinitions.Remove(clipboard);
			this.SaveSettings();
		}

		/// <summary>
		/// Writes the given serialized clipboard history data to disk.
		/// </summary>
		/// <param name="serializedData">The serialized clipboard history collection.</param>
		public void SaveClipboardHistory(byte[] serializedData)
		{
			using (var fileStream = File.OpenWrite(Constants.PersistedHistoryFilePath))
			{
				fileStream.Write(serializedData, 0, serializedData.Length);
				fileStream.Close();
			}
		}

		/// <summary>
		/// Loads and returns the serialized clipboard history data from disk.
		/// </summary>
		/// <returns>The serialized clipboard history data loaded from disk.</returns>
		public byte[] LoadClipboardHistory()
		{
			if (!File.Exists(Constants.PersistedHistoryFilePath))
			{
				return null;
			}

			byte[] serializedData;

			using (var fileStream = File.OpenRead(Constants.PersistedHistoryFilePath))
			{
				serializedData = new byte[fileStream.Length];
				fileStream.Read(serializedData, 0, (int)fileStream.Length);
				fileStream.Close();
			}

			return serializedData;
		}

		/// <summary>
		/// Deletes any persisted clipboard history data that may exist.
		/// </summary>
		public void DeleteClipboardHistory()
		{
			if (File.Exists(Constants.PersistedHistoryFilePath))
			{
				File.Delete(Constants.PersistedHistoryFilePath);
			}
		}

		/// <summary>
		/// Loads the settings that have been saved, or the defaults if there is no settings file.
		/// </summary>
		private void LoadSettings()
		{
			if (File.Exists(Constants.SettingsFilePath))
			{
				var serializer = new XmlSerializer(typeof(MultipleClipboardsDataModel));
				using (var fileStream = new FileStream(Constants.SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					this.DataStore = (MultipleClipboardsDataModel)serializer.Deserialize(fileStream);
				}
			}
			else
			{
				// This is the initial launch, so just initialize the data store.
				// We do not want to create a default clipboard since different users might have hot keys already registered to something else.
				// The settings will initialize themselves the first time they are accessed.
				this.DataStore = new MultipleClipboardsDataModel();
			}
		}

		/// <summary>
		/// Saves all the settings to disk.
		/// </summary>
		private void SaveSettings()
		{
			var serializer = new XmlSerializer(typeof(MultipleClipboardsDataModel));
			using (var writer = new StreamWriter(Constants.SettingsFilePath))
			{
				serializer.Serialize(writer, this.DataStore);
			}
		}

		/// <summary>
		/// Gets the specified setting without the possibility of an exception.
		/// </summary>
		/// <param name="key">The setting key.</param>
		/// <returns>The specified setting.</returns>
		private dynamic GetSettingSafe(string key)
		{
			if (!this.DataStore.ApplicationSettings.ContainsKey(key))
			{
				// The setting does not exist in the persisted data store.
				// Add it here with the default value.
				this.SaveApplicationSetting(key, defaultSettings[key]);
			}
			
			return this.DataStore.ApplicationSettings[key];
		}

		/// <summary>
		/// Saves the specified setting with the given value to the in-memory backing store as well as the persisted data store.
		/// </summary>
		/// <param name="key">The setting key.</param>
		/// <param name="value">The value to save for this setting.</param>
		private void SaveApplicationSetting(string key, dynamic value)
		{
			if (value == null)
			{
				return;
			}

			this.DataStore.ApplicationSettings[key] = value;
			this.SaveSettings();
		}

		private static void ToggleAutoLaunchShortcut(bool launchOnStartup)
		{
			if (launchOnStartup)
			{
				// Copy the application shortucut from the working directory to the Startup folder.
				if (File.Exists(Constants.ShortcutPath))
				{
					File.Copy(Constants.ShortcutPath, Constants.AutoLaunchShortcutPath, true);
				}
			}
			else
			{
				// Delete the shortcut from the Startup folder.
				File.Delete(Constants.AutoLaunchShortcutPath);
			}
		}

		private static void UpdateLog4NetConfig(LogLevel level)
		{
			// Don't wait long if someone has the file.
			if (!loggerConfigFileResetEvent.WaitOne(250))
			{
				return;
			}

			XDocument loggerConfigDoc = XDocument.Load(Constants.LogConfigFileName);
			XElement logLevelElement = loggerConfigDoc.Descendants("level").FirstOrDefault();
			
			if (logLevelElement != null)
			{
				XAttribute attribute = logLevelElement.Attribute("value");

				if (attribute != null)
				{
					attribute.Value = level.ToString().ToUpperInvariant();
					loggerConfigDoc.Save(Constants.LogConfigFileName);
				}
			}

			loggerConfigFileResetEvent.Set();
		}

		private static LogLevel GetLog4NetLogLevel()
		{
			LogLevel level = LogLevel.Error;

			// Don't wait long if someone has the file.
			if (!loggerConfigFileResetEvent.WaitOne(250))
			{
				return level;
			}

			XDocument loggerConfigDoc = XDocument.Load(Constants.LogConfigFileName);
			XElement logLevelElement = loggerConfigDoc.Descendants("level").FirstOrDefault();

			if (logLevelElement != null)
			{
				XAttribute attribute = logLevelElement.Attribute("value");

				if (attribute != null)
				{
					Enum.TryParse(attribute.Value, true, out level);
				}
			}

			loggerConfigFileResetEvent.Set();
			return level;
		}
	}
}
