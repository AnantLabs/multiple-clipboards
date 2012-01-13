using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using MultipleClipboards.Entities;

namespace MultipleClipboards.Persistence
{
	/// <summary>
	/// Class to manage the settings for the application.
	/// </summary>
	public sealed class SettingsManager
	{
		private static SettingsManager _settingsManager;

		/// <summary>
		/// Gets the singleton instance of the SettingsManager object.
		/// </summary>
		public static SettingsManager Instance
		{
			get
			{
				return _settingsManager ?? (_settingsManager = new SettingsManager());
			}
		}

		/// <summary>
		/// Constructs a new Settings Manager object.
		/// </summary>
		private SettingsManager()
		{
			if (!Directory.Exists(Constants.BaseDataPath))
			{
				Directory.CreateDirectory(Constants.BaseDataPath);
			}

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
				return this.GetSettingSafe(Constants.NumberOfClipboardHistoryRecordsSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(Constants.NumberOfClipboardHistoryRecordsSettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets the number of milliseconds the application should sleep between clipboard operations.
		/// </summary>
		public int ThreadDelayTime
		{
			get
			{
				return this.GetSettingSafe(Constants.ThreadDelayTimeSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(Constants.ThreadDelayTimeSettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets the application logging level.
		/// </summary>
		public LogLevel ApplicationLogLevel
		{
			get
			{
				return (LogLevel)this.GetSettingSafe(Constants.ApplicationLogLevelSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(Constants.ApplicationLogLevelSettingKey, value);
			}
		}

		/// <summary>
		/// Gets or sets the number of times the application will attempt to access the clipboard before throwing the caught exception.
		/// </summary>
		public int NumberOfClipboardOperationRetries
		{
			get
			{
				return this.GetSettingSafe(Constants.NumberOfClipboardOperationRetriesSettingKey);
			}
			set
			{
				this.SaveApplicationSetting(Constants.NumberOfClipboardOperationRetriesSettingKey, value);
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
		/// Adds a new clipboard to the data store.
		/// </summary>
		/// <param name="clipboard">The clipboard to add.</param>
		/// <returns>The ID of the new clipboard.</returns>
		public int AddNewClipboard(ClipboardDefinition clipboard)
		{
			clipboard.ClipboardId = this.DataStore.ClipboardDefinitions.Count + 1;
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
		/// Loads the settings that have been saved, or the defaults if there is no settings file.
		/// </summary>
		private void LoadSettings()
		{
			if (File.Exists(Constants.SettingsFilePath))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(MultipleClipboardsDataModel));
				using (FileStream fileStream = new FileStream(Constants.SettingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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
			XmlSerializer serializer = new XmlSerializer(typeof(MultipleClipboardsDataModel));
			using (StreamWriter writer = new StreamWriter(Constants.SettingsFilePath))
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
			if (this.DataStore.ApplicationSettings.ContainsKey(key))
			{
				// The setting exists in the data store, so just return it.
				return this.DataStore.ApplicationSettings[key];
			}
			else
			{
				// The setting does not exist in the persisted data store.
				// Add it here with the default value.
				this.SaveApplicationSetting(key, Constants.DefaultSettings[key]);
				return this.DataStore.ApplicationSettings[key];
			}
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
	}
}
