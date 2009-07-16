using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace MultipleClipboards
{
    class SettingsManager
    {
        private const string SETTINGS_FILE_NAME = "multipleClipboardsSettings.xml";

        private XmlDataDocument _settingsDoc;
        private string _settingsFilePath;

		public clipboardDS ClipboardDS { get; set; }

		private int _numberOfClipboards;
		public int NumberOfClipboards
		{
			get
			{
				if (ClipboardDS.general_settings.Rows.Count > 0)
				{
					_numberOfClipboards = ClipboardDS.general_settings[0].number_of_clipboards;
				}
				return _numberOfClipboards;
			}
			set
			{
				if (ClipboardDS.general_settings.Rows.Count > 0)
				{
					ClipboardDS.general_settings[0].number_of_clipboards = value;
				}
				else
				{
					ClipboardDS.general_settings.Addgeneral_settingsRow(value);
				}
				_numberOfClipboards = value;
			}
		}

        public SettingsManager()
        {
            _settingsFilePath = string.Format("{0}\\Settings\\{1}", Environment.CurrentDirectory, SETTINGS_FILE_NAME);
			ClipboardDS = new clipboardDS();
			_settingsDoc = new XmlDataDocument(ClipboardDS);
			LoadSettings();
        }

        public void LoadSettings()
        {
			FileInfo settingsFileInfo = new FileInfo(_settingsFilePath);

			if (settingsFileInfo.Exists)
			{
				ClipboardDS.ReadXml(_settingsFilePath);
			}
			else
			{
				// load default values

				// 1 additional clipboard
				ClipboardDS.general_settings.Addgeneral_settingsRow(1);

				// the accessor key options
				ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)HotKey.ModifierKeys.NONE, "--SELECT--");
				ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)HotKey.ModifierKeys.CONTROL, "CTRL");
				ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)HotKey.ModifierKeys.ALT, "ALT");
				ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)HotKey.ModifierKeys.SHIFT, "SHIFT");
				ClipboardDS.modifier_key_codes.Addmodifier_key_codesRow((int)HotKey.ModifierKeys.WINDOWS, "WINDOWS");

				// the operation key options
				ClipboardDS.operation_key_codes.Addoperation_key_codesRow((int)System.Windows.Forms.Keys.None, "--SELECT--");
				int keycode;
				string displayText;
				for (keycode = 65; keycode <= 90; keycode++)
				{
					displayText = ((char)keycode).ToString().ToUpper();
					ClipboardDS.operation_key_codes.Addoperation_key_codesRow(keycode, displayText);
				}

				// default cliipboard row
				clipboardDS.clipboardRow row = ClipboardDS.clipboard.NewclipboardRow();
				row.number = 1;
				row.modifier_key_1 = (int)HotKey.ModifierKeys.WINDOWS;
				row.modifier_key_2 = (int)HotKey.ModifierKeys.NONE;
				row.copy_key = (int)System.Windows.Forms.Keys.C;
				row.cut_key = (int)System.Windows.Forms.Keys.X;
				row.paste_key = (int)System.Windows.Forms.Keys.V;
				ClipboardDS.clipboard.AddclipboardRow(row);
			}
        }

		public void SaveSettings()
        {
			FileInfo settingsFileInfo = new FileInfo(_settingsFilePath);

			if (!settingsFileInfo.Directory.Exists)
			{
				settingsFileInfo.Directory.Create();
			}

            _settingsDoc.Save(_settingsFilePath);
        }

		public void AddNewClipboard()
		{
			NumberOfClipboards++;
			clipboardDS.clipboardRow newRow = ClipboardDS.clipboard.NewclipboardRow();
			newRow.number = NumberOfClipboards;
			ClipboardDS.clipboard.AddclipboardRow(newRow);
		}

		public void RemoveClipboard()
		{
			NumberOfClipboards--;
			ClipboardDS.clipboard[NumberOfClipboards].Delete();
		}
    }
}
