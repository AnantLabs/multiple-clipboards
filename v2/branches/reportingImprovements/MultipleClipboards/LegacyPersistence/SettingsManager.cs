using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using log4net;
using MultipleClipboards.Entities;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.LegacyPersistence
{
	/// <summary>
	/// Class to manage the settings for the application.
	/// </summary>
	public sealed class SettingsManager
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SettingsManager));

		// Application Setting Keys.
		private const string numberOfClipboardHistoryRecordsSettingKey = "NumberOfClipboardHistoryRecords";
		private const string threadDelayTimeSettingKey = "ThreadDelayTime";
		private const string applicationLogLevelSettingKey = "LogLevel";
		private const string launchApplicationOnSystemStartupSettingKey = "LaunchApplicationOnSystemStartup";
		private const string showAdvancedOptionsSettingKey = "ShowAdvancedOptions";
		private const string showMessagesFromTraySettingKey = "ShowMessagesFromTray";
		private const string showDetailedClipboardInformationSettingKey = "ShowDetailClipboardInformation";
		private const string persistClipboardHistorySettingKey = "PersistClipboardHistory";
        private const string failureThresholdForBlacklist = "failureThresholdForBlacklist";

		// Application Settings Default Values.
		private static readonly IDictionary<string, object> defaultSettings =
			new Dictionary<string, object>
			{
				{ numberOfClipboardHistoryRecordsSettingKey, 20 },
				{ threadDelayTimeSettingKey, 250 },
				{ applicationLogLevelSettingKey, (int)LogLevel.Error },
				{ launchApplicationOnSystemStartupSettingKey, true },
				{ showAdvancedOptionsSettingKey, false },
				{ showMessagesFromTraySettingKey, true },
				{ showDetailedClipboardInformationSettingKey, false },
				{ persistClipboardHistorySettingKey, true },
                { failureThresholdForBlacklist, 3 }
			};

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
			get { return this.GetSettingSafe(numberOfClipboardHistoryRecordsSettingKey); }
			set { this.SaveApplicationSetting(numberOfClipboardHistoryRecordsSettingKey, value); }
		}

		/// <summary>
		/// Gets or sets the number of milliseconds the application should sleep between clipboard operations.
		/// </summary>
		public int ThreadDelayTime
		{
			get { return this.GetSettingSafe(threadDelayTimeSettingKey); }
			set { this.SaveApplicationSetting(threadDelayTimeSettingKey, value); }
		}

        /// <summary>
        /// Gets or sets the number of times a particular data format can fail before it is placed on the format blacklist.
        /// </summary>
	    public int FailureThresholdForBlacklist
	    {
	        get { return GetSettingSafe(failureThresholdForBlacklist); }
            set { SaveApplicationSetting(failureThresholdForBlacklist, value); }
	    }

		/// <summary>
		/// Gets or sets the application logging level.
		/// </summary>
		public LogLevel ApplicationLogLevel
		{
			get { return (LogLevel)this.GetSettingSafe(applicationLogLevelSettingKey); }
			set
			{
                this.SaveApplicationSetting(applicationLogLevelSettingKey, (int)value);
                LogHelper.SetLogLevel(value);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not the application should automatically launch when the system starts.
		/// </summary>
		public bool LaunchApplicationOnSystemStartup
		{
			get { return this.GetSettingSafe(launchApplicationOnSystemStartupSettingKey); }
			set
			{
				try
				{
					ToggleAutoLaunchShortcut(value);
					this.SaveApplicationSetting(launchApplicationOnSystemStartupSettingKey, value);
				}
				catch (Exception e)
				{
					const string baseErrorMessage = "There was an error setting or removing the auto-launch shortcut.";
					log.Error(baseErrorMessage, e);
					MessageBus.Instance.Publish(new MainWindowNotification
					{
						MessageBody = string.Concat(baseErrorMessage, "  A detailed error report has been saved to the log."),
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
			get { return this.GetSettingSafe(showAdvancedOptionsSettingKey); }
			set { this.SaveApplicationSetting(showAdvancedOptionsSettingKey, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to show application notification popups from the system tray.
		/// </summary>
		public bool ShowMessagesFromTray
		{
			get { return this.GetSettingSafe(showMessagesFromTraySettingKey); }
			set { this.SaveApplicationSetting(showMessagesFromTraySettingKey, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to show detailed clipboard information on the clipboard inspector tab.
		/// </summary>
		public bool ShowDetailedClipboardInformation
		{
			get { return this.GetSettingSafe(showDetailedClipboardInformationSettingKey); }
			set { this.SaveApplicationSetting(showDetailedClipboardInformationSettingKey, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether or not to persist the clipboard history content to disk when the application exits or the user logs off.
		/// </summary>
		public bool PersistClipboardHistory
		{
			get { return this.GetSettingSafe(persistClipboardHistorySettingKey); }
			set { this.SaveApplicationSetting(persistClipboardHistorySettingKey, value); }
		}

		/// <summary>
		/// Gets the list of all the defined clipboard.
		/// </summary>
		public ObservableCollection<ClipboardDefinition> ClipboardDefinitions
		{
			get { return this.DataStore.ClipboardDefinitions; }
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
				// Copy the application shortucut from AppData to the Startup folder.
                File.Copy(Constants.ShortcutPath, Constants.AutoLaunchShortcutPath, true);
			}
			else
			{
				// Copy the existing shortcut to AppData in case we need to put it back, then delete the shortcut from the Startup folder.
                File.Copy(Constants.AutoLaunchShortcutPath, Constants.ShortcutPath, true);
				File.Delete(Constants.AutoLaunchShortcutPath);
			}
		}
	}
}
