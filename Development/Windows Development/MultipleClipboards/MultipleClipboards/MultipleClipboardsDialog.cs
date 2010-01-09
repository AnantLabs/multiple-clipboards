using System;
using System.IO;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MultipleClipboards
{
	/// <summary>
	/// The main dialog class for the form.
	/// </summary>
	public partial class MultipleClipboardsDialog : Form
	{
		// Private members
		private SettingsManager settingsManager;
		private ClipboardManager clipboardManager;
		private List<short> hotkeyIDs;
		private HotkeyMessage lastMessageProcessed;
		private HotkeyMessage currentMessage;
		private IntPtr nextClipboardViewer;
		private string errorLogFile;
		private string aboutFile;
		private bool isFirstClipboardMessage = true;

		// Grid elements
		private BindingSource dgClipboardbindingSource;
		private BindingSource modifierKeyBindingSource;
		private BindingSource operationKeyBindingSource;
		private DataGridViewTextBoxColumn numberColumn;
		private DataGridViewComboBoxColumn modifierKey1Column;
		private DataGridViewComboBoxColumn modifierKey2Column;
		private DataGridViewComboBoxColumn copyKeyColumn;
		private DataGridViewComboBoxColumn cutKeyColumn;
		private DataGridViewComboBoxColumn pasteKeyColumn;

		// Windows API functions and constants
		private const int CP_NOCLOSE_BUTTON = 0x200;
		private const int WM_HOTKEY = 0x312;
		private const int WM_DRAWCLIPBOARD = 0x308;
		private const int WM_CHANGECBCHAIN = 0x30D;

		[DllImport("user32", SetLastError = true)]
		private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

		[DllImport("user32", SetLastError = true)]
		private static extern int UnregisterHotKey(IntPtr hwnd, int id);

		[DllImport("user32", SetLastError = true)]
		private static extern short GetAsyncKeyState(int vKey);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32", SetLastError = true)]
		private static extern short GlobalAddAtom(string lpString);

		[DllImport("kernel32", SetLastError = true)]
		private static extern short GlobalDeleteAtom(short nAtom);

		/// <summary>
		/// Constructs a new MultipleClipboardsDialog object.
		/// </summary>
		public MultipleClipboardsDialog()
		{
			this.InitializeComponent();
			this.Init();
		}

		/// <summary>
		/// Initializes all the variables and controls on the dialog.
		/// </summary>
		private void Init()
		{
			// this is the amount of time that the thread will sleep for between clipboard operations
			// this value comes from the app.config file so the (experienced) end user can modify it
			// for a detailed description about this see ClipboardManager.cs
			int threadDelayTime = 100;
			int.TryParse(ConfigurationManager.AppSettings["threadDelayTime"], out threadDelayTime);

			this.hotkeyIDs = new List<short>();
			this.settingsManager = new SettingsManager();
			this.clipboardManager = new ClipboardManager(threadDelayTime, this.settingsManager.NumberOfClipboardManagerRecords);
			this.trayIcon.Visible = true;
			this.txtNumClipboards.Value = this.settingsManager.NumberOfClipboards;
			this.errorLogFile = this.settingsManager.ErrorLogFilePath;
			this.aboutFile = this.settingsManager.AboutTextFilePath;
			this.lastMessageProcessed = new HotkeyMessage();
			this.currentMessage = new HotkeyMessage();

			// History tab
			this.FillClipboardSelectDropdown();

			// Grid tab
			this.InitGrid();

			// About tab
			if (File.Exists(this.aboutFile))
			{
				this.txtAbout.LoadFile(aboutFile);
			}
			else
			{
				this.txtAbout.Text = "The file containing information about this program cannot be found.\r\n\r\nPerhaps you deleted it?\r\n\r\nRe-installing should fix this problem.";
			}

			// Bind this app to the clipboard chain
			this.nextClipboardViewer = MultipleClipboardsDialog.SetClipboardViewer(this.Handle);

			// handle the dispose event of the window
			this.Disposed += new EventHandler(this.MultipleClipboardsDialog_Disposed);
		}

		/// <summary>
		/// Fills the clipboard select dropdown on the history tab with all the clipboards that are currently setup.
		/// </summary>
		private void FillClipboardSelectDropdown()
		{
			// clear the existing items
			this.ddlClipboardSelect.Items.Clear();

			// now add an option for each additional clipboard
			foreach (clipboardDS.clipboardRow row in this.settingsManager.ClipboardDS.clipboard)
			{
				string itemText = string.Format("#{0} - {1}{2} + {3}", row.number, row.modifier_key_codesRowByFK_accessor_key_codes_clipboard.display_text,
					(row.modifier_key_2 == 0) ? string.Empty : " + " + row.modifier_key_codesRowByFK_accessor_key_codes_clipboard1.display_text,
					row.operation_key_codesRowByoperation_key_codes_clipboard_copy.display_text);

				this.ddlClipboardSelect.Items.Add(itemText);
			}

			// select the windows clipboard by default
			this.ddlClipboardSelect.SelectedIndex = 0;
		}

		private void InitGrid()
		{
			// instantiate variables
			dgClipboardbindingSource = new BindingSource(this.components);
			modifierKeyBindingSource = new BindingSource(this.components);
			operationKeyBindingSource = new BindingSource(this.components);
			numberColumn = new DataGridViewTextBoxColumn();
			modifierKey1Column = new DataGridViewComboBoxColumn();
			modifierKey2Column = new DataGridViewComboBoxColumn();
			copyKeyColumn = new DataGridViewComboBoxColumn();
			cutKeyColumn = new DataGridViewComboBoxColumn();
			pasteKeyColumn = new DataGridViewComboBoxColumn();

			// setup binding sources
			dgClipboardbindingSource.DataMember = "clipboard";
			dgClipboardbindingSource.DataSource = settingsManager.ClipboardDS;
			modifierKeyBindingSource.DataMember = "modifier_key_codes";
			modifierKeyBindingSource.DataSource = settingsManager.ClipboardDS;
			operationKeyBindingSource.DataMember = "operation_key_codes";
			operationKeyBindingSource.DataSource = settingsManager.ClipboardDS;

			// setup grid
			dgClipboards.AutoGenerateColumns = false;
			dgClipboards.Columns.AddRange(new DataGridViewColumn[] { numberColumn, modifierKey1Column, modifierKey2Column, copyKeyColumn, cutKeyColumn, pasteKeyColumn });
			dgClipboards.DataSource = dgClipboardbindingSource;

			// number column
			numberColumn.DataPropertyName = "number";
			numberColumn.HeaderText = "#";
			numberColumn.Name = "numberColumn";
			numberColumn.ReadOnly = true;
			numberColumn.Resizable = DataGridViewTriState.False;
			numberColumn.Width = 25;

			// accessor key 1 column
			modifierKey1Column.DataPropertyName = "modifier_key_1";
			modifierKey1Column.DataSource = modifierKeyBindingSource;
			modifierKey1Column.DisplayMember = "display_text";
			modifierKey1Column.HeaderText = "Modifier Key 1";
			modifierKey1Column.Name = "modifierKey1Column";
			modifierKey1Column.Resizable = DataGridViewTriState.False;
			modifierKey1Column.ValueMember = "key_code";
			modifierKey1Column.Width = 85;

			// accessor key 2 column
			modifierKey2Column.DataPropertyName = "modifier_key_2";
			modifierKey2Column.DataSource = modifierKeyBindingSource;
			modifierKey2Column.DisplayMember = "display_text";
			modifierKey2Column.HeaderText = "Modifier Key 2";
			modifierKey2Column.Name = "modifierKey2Column";
			modifierKey2Column.Resizable = DataGridViewTriState.False;
			modifierKey2Column.ValueMember = "key_code";
			modifierKey2Column.Width = 85;

			// copy column
			copyKeyColumn.DataPropertyName = "copy_key";
			copyKeyColumn.DataSource = operationKeyBindingSource;
			copyKeyColumn.DisplayMember = "display_text";
			copyKeyColumn.HeaderText = "Copy Key";
			copyKeyColumn.Name = "copyKeyColumn";
			copyKeyColumn.Resizable = DataGridViewTriState.False;
			copyKeyColumn.ValueMember = "key_code";
			copyKeyColumn.Width = 85;

			// cut column
			cutKeyColumn.DataPropertyName = "cut_key";
			cutKeyColumn.DataSource = operationKeyBindingSource;
			cutKeyColumn.DisplayMember = "display_text";
			cutKeyColumn.HeaderText = "Cut Key";
			cutKeyColumn.Name = "cutKeyColumn";
			cutKeyColumn.Resizable = DataGridViewTriState.False;
			cutKeyColumn.ValueMember = "key_code";
			cutKeyColumn.Width = 85;

			// paste column
			pasteKeyColumn.DataPropertyName = "paste_key";
			pasteKeyColumn.DataSource = operationKeyBindingSource;
			pasteKeyColumn.DisplayMember = "display_text";
			pasteKeyColumn.HeaderText = "Paste Key";
			pasteKeyColumn.Name = "pasteKeyColumn";
			pasteKeyColumn.Resizable = DataGridViewTriState.False;
			pasteKeyColumn.ValueMember = "key_code";
			pasteKeyColumn.Width = 85;
		}

		// This diables the close button on the dialog
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams myCp = base.CreateParams;
				myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
				return myCp;
			}
		}

		// Called when the window is destroyed
		private void MultipleClipboardsDialog_Disposed(object sender, EventArgs e)
		{
			ChangeClipboardChain(this.Handle, nextClipboardViewer);
		}

		// Called before the form is first rendered
		private void MultipleClipboardsDialog_Load(object sender, EventArgs e)
		{
			Hide();
			WindowState = FormWindowState.Minimized;
			RegisterAllHotkeys();
		}

		// Called after the form is closed
		private void MultipleClipboardsDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			UnregisterAllGlobalHotKeys();
		}

		// Called when the form is resized / minimized
		private void MultipleClipboardsDialog_SizeChanged(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		}

		// Called when the user clicks the Edit menu option
		private void editMultipleClipboardOptionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowMainDialog();
			tabControl.SelectedTab = tabSettings;
		}

		// Called when the user double clicks the tray icon
		private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ShowMainDialog();
			tabControl.SelectedTab = tabSettings;
		}

		// Called when the user clicks the Exit menu option
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		// Called when the Save button is clicked
		private void btnSave_Click(object sender, EventArgs e)
		{
			SaveSettings();
		}

		// Called when the exit button is clicked
		private void btnExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		// Called when the user clicks the hide dialog button
		private void btnHide_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;
			Hide();
		}

		/// <summary>
		/// Called when the place selected row on selected clipboard button is clicked from the history tab.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void BtnPlaceRowOnClipboard_Click(object sender, EventArgs e)
		{
			if (this.dgClipboardHistory.SelectedRows.Count > 0)
			{
				int clipboardHistoryIndex = ((ClipboardHistoryDataGridViewRow)this.dgClipboardHistory.SelectedRows[0]).ClipboardHistoryIndex;
				this.clipboardManager.PlaceHistoricalEntryOnClipboard(clipboardHistoryIndex, this.ddlClipboardSelect.SelectedIndex);
			}
		}

		/// <summary>
		/// Called when the user clicks the refresh history grid button.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void BtnRefreshHistory_Click(object sender, EventArgs e)
		{
			this.RefreshHistoryTab();
		}

		// Called when the user clicks the show error log menu item
		private void errorLogMenuItem_Click(object sender, EventArgs e)
		{
			ShowMainDialog();
			tabControl.SelectedTab = tabErrorLog;
		}

		/// <summary>
		/// Called when the user hovers over the history menu item.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void ClipboardHistoryMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			// first clear out all the items except the last 2
			while (this.clipboardHistoryMenuItem.DropDownItems.Count > 2)
			{
				this.clipboardHistoryMenuItem.DropDownItems.RemoveAt(0);
			}

			// now add the clipboard history items
			if (this.clipboardManager.ClipboardHistory.Count > 0)
			{
				int dropDownIndex = 0;
				for (int i = this.clipboardManager.ClipboardHistory.Count; i > 0; i--)
				{
					string text = this.GetClipboardHistoryDataString(this.clipboardManager.ClipboardHistory.ElementAt(i - 1), true);
					ClipboardHistoryToolStripMenuItem item = new ClipboardHistoryToolStripMenuItem(text, i - 1, new EventHandler(this.ClipboardHistoryMenuItem_Click));
					this.clipboardHistoryMenuItem.DropDownItems.Insert(dropDownIndex, item);
					dropDownIndex++;
				}
			}
			else
			{
				ToolStripMenuItem item = new ToolStripMenuItem("No Clipboard History Exists");
				item.Enabled = false;
				this.clipboardHistoryMenuItem.DropDownItems.Insert(0, item);
			}
		}

		/// <summary>
		/// Called when the user clicks a clipboard entry from the History right click menu.
		/// </summary>
		/// <param name="sender">The ClipboardHistoryToolStripMenuItem object that caused the event.</param>
		/// <param name="e">The event args.</param>
		private void ClipboardHistoryMenuItem_Click(object sender, EventArgs e)
		{
			this.clipboardManager.PlaceHistoricalEntryOnClipboard(((ClipboardHistoryToolStripMenuItem)sender).ClipboardHistoryIndex, 0);
		}

		/// <summary>
		/// Called when the user clicks the view detailed history menu item.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void ViewDetailedHistoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.ShowMainDialog();
			this.tabControl.SelectedTab = this.tabHistory;
			this.RefreshHistoryTab();
		}

		// Called when the user clicks the about menu item
		private void aboutMenuItem_Click(object sender, EventArgs e)
		{
			ShowMainDialog();
			tabControl.SelectedTab = tabAbout;
		}

		// Called when the user changes the number of clipboards
		private void txtNumClipboards_ValueChanged(object sender, EventArgs e)
		{
			int newNumClipboards = (int)txtNumClipboards.Value;
			if (newNumClipboards > settingsManager.NumberOfClipboards)
			{
				settingsManager.AddNewClipboard();
			}
			else if (newNumClipboards < settingsManager.NumberOfClipboards)
			{
				settingsManager.RemoveClipboard();
			}
		}

		/// <summary>
		/// Called when the selected tab in the tab control changes.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The tab control event args.</param>
		private void TabControl_Selected(object sender, TabControlEventArgs e)
		{
			if (e.TabPage == this.tabErrorLog)
			{
				// if the user clicked the error tab then load the most recent contents of the error file and place it into the text control
				if (File.Exists(this.errorLogFile))
				{
					this.txtErrorLog.Text = File.ReadAllText(this.errorLogFile);
					this.txtErrorLog.SelectionStart = 0;
					this.txtErrorLog.SelectionLength = 0;
				}
				else
				{
					this.txtErrorLog.Text = "The error file does not exist.\r\n\r\nThat's a good thing!  There have not been any errors.";
				}
			}
			else if (e.TabPage == this.tabHistory)
			{
				this.RefreshHistoryTab();
			}
		}

		// Show the edit dialog
		private void ShowMainDialog()
		{
			Show();
			WindowState = FormWindowState.Normal;
			Focus();
		}

		/// <summary>
		/// Refreshes the clipboard history with the most current data.
		/// </summary>
		private void RefreshHistoryTab()
		{
			// clear the history grid
			this.dgClipboardHistory.Rows.Clear();

			// fill the history grid will the clipboard history
			ClipboardEntry clipboardEntry = null;
			int visibleIndex = 1;
			for (int i = this.clipboardManager.ClipboardHistory.Count; i > 0; i--)
			{
				clipboardEntry = this.clipboardManager.ClipboardHistory.ElementAt(i - 1);
				this.dgClipboardHistory.Rows.Add(new ClipboardHistoryDataGridViewRow(i - 1, this.dgClipboardHistory, visibleIndex, this.GetClipboardHistoryDataString(clipboardEntry), clipboardEntry.timestamp));
				visibleIndex++;
			}
		}

		/// <summary>
		/// Gets the data to display for the given clipboard entry.
		/// </summary>
		/// <param name="clipboardEntry">The clipboard entry to get the data string for.</param>
		/// <returns>The string representation of the data for this clipboard entry.</returns>
		private string GetClipboardHistoryDataString(ClipboardEntry clipboardEntry)
		{
			return this.GetClipboardHistoryDataString(clipboardEntry, false);
		}

		/// <summary>
		/// Gets the data to display for the given clipboard entry.
		/// </summary>
		/// <param name="clipboardEntry">The clipboard entry to get the data string for.</param>
		/// <param name="addTimeStamp">A flag that determines whether or not to display the timestamp of the entry in the display string.</param>
		/// <returns>The string representation of the data for this clipboard entry.</returns>
		private string GetClipboardHistoryDataString(ClipboardEntry clipboardEntry, bool addTimeStamp)
		{
			string data;
			if (clipboardEntry.dataType == ClipboardDataType.TEXT)
			{
				data = clipboardEntry.data.ToString().Replace("\r\n", " ").Replace("\t", " ");
				if (data.Length > 60)
				{
					data = data.Substring(0, 60) + "...";
				}
			}
			else
			{
				data = clipboardEntry.dataType.ToString();
			}

			if (addTimeStamp)
			{
				data += "  -  " + clipboardEntry.timestamp.ToShortTimeString();
			}

			return data;
		}

		// Save settings to file
		private void SaveSettings()
		{
			try
			{
				settingsManager.SaveSettings();
				MessageBox.Show("Settings saved successfully.", "Save Complete");
			}
			catch (Exception e)
			{
				MessageBox.Show("There was an error saving the settings.  Check the log.", "Error");
				LogError(e.Message);
			}

			try
			{
				UnregisterAllGlobalHotKeys();
				RegisterAllHotkeys();
				clipboardManager.NumberOfHistoricalRecords = settingsManager.NumberOfClipboardManagerRecords;
				clipboardManager.Reset();
				this.FillClipboardSelectDropdown();
			}
			catch (Exception e)
			{
				MessageBox.Show("There was an error re-initializing the application with the new settings.  This should never happen.  Try re-starting the application.", "Error");
				LogError(e.Message);
			}
		}

		// Log error to file
		private void LogError(string errorMessage)
		{
			try
			{
				FileInfo errorFile = new FileInfo(errorLogFile);
				StreamWriter errorOutput = errorFile.AppendText();
				errorOutput.WriteLine(string.Format("{0}:\r\n{1}\r\n\r\n", DateTime.Now.ToString(), errorMessage));
				errorOutput.Close();
			}
			catch
			{
				MessageBox.Show("There was an error logging an error!\r\n\r\nBasically this application is all F'd up.\r\n\r\nYou should re-install it.", "Fatal Error");
			}
		}

		// Register all the hotkeys
		private void RegisterAllHotkeys()
		{
			HotKey cutHotKey;
			HotKey copyHotKey;
			HotKey pasteHotKey;

			try
			{
				foreach (clipboardDS.clipboardRow row in settingsManager.ClipboardDS.clipboard)
				{
					// cut
					cutHotKey = new HotKey(row.modifier_key_1, row.modifier_key_2, row.cut_key, HotKeyType.CUT, row.number);
					RegisterGlobalHotKey(cutHotKey);

					// copy
					copyHotKey = new HotKey(row.modifier_key_1, row.modifier_key_2, row.copy_key, HotKeyType.COPY, row.number);
					RegisterGlobalHotKey(copyHotKey);

					// paste
					pasteHotKey = new HotKey(row.modifier_key_1, row.modifier_key_2, row.paste_key, HotKeyType.PASTE, row.number);
					RegisterGlobalHotKey(pasteHotKey);

					// create a new clipboard entry for this set of hotkeys
					clipboardManager.AddClipboard(row.number, cutHotKey, copyHotKey, pasteHotKey);
				}
			}
			catch (Exception e)
			{
				string errorMessage = "Error registering the hotkeys from the settings file.\r\n\r\nIf this problem persists after restarting the app you should re-install.";
				MessageBox.Show(errorMessage + "\r\n\r\nProgram will now exit.", "Error Reading Settings File");
				LogError(errorMessage + "\r\n" + e.ToString());
				Close();
			}
		}

		// register a global hot key
		private void RegisterGlobalHotKey(HotKey hotKey)
		{
			short hotkeyID = 0;
			try
			{
				// use the GlobalAddAtom API to get a unique ID (as suggested by MSDN docs)
				string atomName = string.Format("{0}_{1}_{2}", this.Name, hotKey.Modifiers.ToString(), hotKey.Key.ToString());
				hotkeyID = GlobalAddAtom(atomName);
				if (hotkeyID == 0)
				{
					throw new Exception("Unable to generate unique hotkey ID. Error code: " + Marshal.GetLastWin32Error().ToString());
				}

				// register the hotkey, throw if any error
				if (RegisterHotKey(this.Handle, hotkeyID, hotKey.Modifiers, (int)hotKey.Key) == 0)
				{
					throw new Exception("Unable to register hotkey. Error code: " + Marshal.GetLastWin32Error().ToString());
				}

				// add the hotkeyID to the list of all hotkeys
				hotkeyIDs.Add(hotkeyID);
			}
			catch (Exception e)
			{
				// clean up if hotkey registration failed
				MessageBox.Show("Unable to register hotkey combination:\r\n" + hotKey.ToString() + "\r\n\r\nPerhaps this key combination is already in use?", "Error Registering Hotkey");
				LogError("Unable to register hotkey combination:\r\n" + hotKey.ToString() + "\r\n" + e.ToString());
				UnregisterGlobalHotKey(hotkeyID);
			}
		}

		// unregister all global hotkeys
		private void UnregisterAllGlobalHotKeys()
		{
			foreach (short hotkeyID in hotkeyIDs)
			{
				UnregisterGlobalHotKey(hotkeyID);
			}
			hotkeyIDs.Clear();
			clipboardManager.Reset();
		}

		// unregister a single global hotkey
		private void UnregisterGlobalHotKey(short hotkeyID)
		{
			if (hotkeyID != 0)
			{
				UnregisterHotKey(this.Handle, hotkeyID);
				// clean up the atom list
				GlobalDeleteAtom(hotkeyID);
				hotkeyID = 0;
			}
		}

		// Check if a certain key is held down
		private bool IsKeyPressed(Keys key)
		{
			return ((int)GetAsyncKeyState((int)key) & 0x00008000) == 0x8000;
		}

		// Checks if any of the possible modifier keys are held down
		private bool ModifierKeysPressed()
		{
			return (IsKeyPressed(Keys.LShiftKey) || IsKeyPressed(Keys.RShiftKey) || IsKeyPressed(Keys.LControlKey) || IsKeyPressed(Keys.RControlKey) || IsKeyPressed(Keys.LWin) || IsKeyPressed(Keys.RWin) || IsKeyPressed(Keys.Alt));
		}

		// Called when a hotkey combination has been pressed
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_HOTKEY:
					// figure out what key was pressed and send it along to the clipboard manager
					currentMessage.MessageString = m.ToString();
					currentMessage.MessageTime = DateTime.Now;

					// make sure that the message we just got is not the same one that was just processed
					// this happens when the user holds down the hotkey combination for a few seconds
					if (currentMessage != lastMessageProcessed)
					{
						int key = (int)(((uint)m.LParam & 0xFFFF0000) >> 16);
						int modifiers = (int)((uint)m.LParam & 0x0000FFFF);
						HotKey hotKey = new HotKey(modifiers, key);

						// wait while there are any modifier keys held down
						// this causes unpredictable results when the user has setup a combination of different hotkeys
						while (ModifierKeysPressed())
							;

						try
						{
							clipboardManager.ProcessHotKey(hotKey);
						}
						catch (Exception e)
						{
							LogError("Error processing the hotkey:\r\n" + hotKey.ToString() + "\r\n" + e.ToString());
						}
					}
					break;

				case WM_DRAWCLIPBOARD:
					if (!clipboardManager.IsProcessingClipboardAction && !this.isFirstClipboardMessage)
					{
						// the data on the clipboard has changed
						// this means the user used the regular windows clipboard
						// track the data on the clipboard for the history viewer
						// data coppied using any additional clipboards will be tracked internally
						clipboardManager.StoreClipboardContents();
					}
					// send the message to the next app in the clipboard chain
					SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
					this.isFirstClipboardMessage = false;
					break;

				case WM_CHANGECBCHAIN:
					if (m.WParam == nextClipboardViewer)
					{
						nextClipboardViewer = m.LParam;
					}
					else
					{
						SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
					}
					break;

				default:
					// let the base class process the message
					base.WndProc(ref m);
					break;
			}
		}
	}
}
