using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace MultipleClipboards
{
	/// <summary>
	/// Class to manage the settings for the application.
	/// </summary>
	public class SettingsManager
	{
		private const string SETTINGS_FILE_NAME = "multipleClipboardsSettings.xml";
		private const string ERROR_LOG_FILE_NAME = "errorLog.txt";
		private const string ABOUT_TEXT_FILE_NAME = "aboutText.rtf";

		private int _numberOfClipboards;
		private int _numberOfClipboardManagerRecords;
		private XmlDataDocument _settingsDoc;

		/// <summary>
		/// Constructs a new Settings Manager object.
		/// </summary>
		public SettingsManager()
		{
			if (!Directory.Exists(this.AppDataPath))
			{
				Directory.CreateDirectory(this.AppDataPath);
			}

			this.ClipboardDS = new clipboardDS();
			this._settingsDoc = new XmlDataDocument(this.ClipboardDS);
			this.LoadSettings();
		}

		/// <summary>
		/// Gets or sets the DataSet used to store the settings for the application.
		/// </summary>
		public clipboardDS ClipboardDS
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the number of additional clipboards.
		/// </summary>
		public int NumberOfClipboards
		{
			get
			{
				if (this.ClipboardDS.general_settings.Rows.Count > 0)
				{
					this._numberOfClipboards = this.ClipboardDS.general_settings[0].number_of_clipboards;
				}
				return this._numberOfClipboards;
			}
			set
			{
				if (this.ClipboardDS.general_settings.Rows.Count > 0)
				{
					this.ClipboardDS.general_settings[0].number_of_clipboards = value;
				}
				else
				{
					this.ClipboardDS.general_settings.Addgeneral_settingsRow(value, 20);
				}
				this._numberOfClipboards = value;
			}
		}

		/// <summary>
		/// Gets the number of historical records to keep track of.
		/// </summary>
		public int NumberOfClipboardManagerRecords
		{
			get
			{
				if (this.ClipboardDS.general_settings.Rows.Count > 0)
				{
					this._numberOfClipboardManagerRecords = this.ClipboardDS.general_settings[0].number_of_clipboard_manager_records;
				}
				return this._numberOfClipboardManagerRecords;
			}
			set
			{
				if (this.ClipboardDS.general_settings.Rows.Count > 0)
				{
					this.ClipboardDS.general_settings[0].number_of_clipboard_manager_records = value;
				}
				else
				{
					this.ClipboardDS.general_settings.Addgeneral_settingsRow(1, value);
				}
				this._numberOfClipboardManagerRecords = value;
			}
		}

		/// <summary>
		/// Gets the path to the base folder where the programs data is stored.
		/// </summary>
		public string AppDataPath
		{
			get
			{
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MultipleClipboards";
			}
		}

		/// <summary>
		/// Gets the path to the settings file.
		/// </summary>
		public string SettingsFilePath
		{
			get
			{
				return this.AppDataPath + "\\" + SettingsManager.SETTINGS_FILE_NAME;
			}
		}

		/// <summary>
		/// Gets the path to the error log file.
		/// </summary>
		public string ErrorLogFilePath
		{
			get
			{
				return this.AppDataPath + "\\" + SettingsManager.ERROR_LOG_FILE_NAME;
			}
		}

		/// <summary>
		/// Gets the path to the file that is displayed on the about tab of the app.
		/// </summary>
		public string AboutTextFilePath
		{
			get
			{
				return this.AppDataPath + "\\" + SettingsManager.ABOUT_TEXT_FILE_NAME;
			}
		}

		/// <summary>
		/// Loads the settings that have been saved, or the defaults if there is no settings file.
		/// </summary>
		public void LoadSettings()
		{
			FileInfo settingsFileInfo = new FileInfo(this.SettingsFilePath);

			if (settingsFileInfo.Exists)
			{
				this.ClipboardDS.ReadXml(this.SettingsFilePath);
			}
			else
			{
				// load default values

				// 1 additional clipboard, 20 historical entries
				this.ClipboardDS.general_settings.Addgeneral_settingsRow(1, 20);

				// the accessor key options
				this.ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)ModifierKeys.NONE, "--SELECT--");
				this.ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)ModifierKeys.CONTROL, "CTRL");
				this.ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)ModifierKeys.ALT, "ALT");
				this.ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)ModifierKeys.SHIFT, "SHIFT");
				this.ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)ModifierKeys.WINDOWS, "WINDOWS");

				// the operation key options
				this.ClipboardDS.operation_key_codes.Addoperation_key_codesRow((int)System.Windows.Forms.Keys.None, "--SELECT--");
				int keycode;
				string displayText;
				for (keycode = 65; keycode <= 90; keycode++)
				{
					displayText = ((char)keycode).ToString().ToUpper();
					this.ClipboardDS.operation_key_codes.Addoperation_key_codesRow(keycode, displayText);
				}

				// default cliipboard row
				clipboardDS.clipboardRow row = this.ClipboardDS.clipboard.NewclipboardRow();
				row.number = 1;
				row.modifier_key_1 = (int)ModifierKeys.WINDOWS;
				row.modifier_key_2 = (int)ModifierKeys.NONE;
				row.copy_key = (int)System.Windows.Forms.Keys.C;
				row.cut_key = (int)System.Windows.Forms.Keys.X;
				row.paste_key = (int)System.Windows.Forms.Keys.V;
				this.ClipboardDS.clipboard.AddclipboardRow(row);
			}
		}

		/// <summary>
		/// Saves all the settings to disk.
		/// </summary>
		public void SaveSettings()
		{
			this._settingsDoc.Save(this.SettingsFilePath);
		}

		/// <summary>
		/// Adds a new clipboard to the dataset.
		/// </summary>
		public void AddNewClipboard()
		{
			this.NumberOfClipboards++;
			clipboardDS.clipboardRow newRow = this.ClipboardDS.clipboard.NewclipboardRow();
			newRow.number = this.NumberOfClipboards;
			this.ClipboardDS.clipboard.AddclipboardRow(newRow);
		}

		/// <summary>
		/// Removes a clipboard from teh dataset.
		/// </summary>
		public void RemoveClipboard()
		{
			this.NumberOfClipboards--;
			this.ClipboardDS.clipboard[this.NumberOfClipboards].Delete();
		}
	}
}
