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
		/// Overrides the CreateParams property to diable the close button on the dialog.
		/// </summary>
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams myCp = base.CreateParams;
				myCp.ClassStyle = myCp.ClassStyle | MultipleClipboardsDialog.CP_NOCLOSE_BUTTON;
				return myCp;
			}
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

			// add an option of the Windows Clipboard
			this.ddlClipboardSelect.Items.Add("Windows Clipboard");

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

		/// <summary>
		/// Initializes the main grid on the settings page.
		/// </summary>
		private void InitGrid()
		{
			// instantiate variables
			this.dgClipboardbindingSource = new BindingSource(this.components);
			this.modifierKeyBindingSource = new BindingSource(this.components);
			this.operationKeyBindingSource = new BindingSource(this.components);
			this.numberColumn = new DataGridViewTextBoxColumn();
			this.modifierKey1Column = new DataGridViewComboBoxColumn();
			this.modifierKey2Column = new DataGridViewComboBoxColumn();
			this.copyKeyColumn = new DataGridViewComboBoxColumn();
			this.cutKeyColumn = new DataGridViewComboBoxColumn();
			this.pasteKeyColumn = new DataGridViewComboBoxColumn();

			// setup binding sources
			this.dgClipboardbindingSource.DataMember = "clipboard";
			this.dgClipboardbindingSource.DataSource = this.settingsManager.ClipboardDS;
			this.modifierKeyBindingSource.DataMember = "modifier_key_codes";
			this.modifierKeyBindingSource.DataSource = this.settingsManager.ClipboardDS;
			this.operationKeyBindingSource.DataMember = "operation_key_codes";
			this.operationKeyBindingSource.DataSource = this.settingsManager.ClipboardDS;

			// setup grid
			this.dgClipboards.AutoGenerateColumns = false;
			this.dgClipboards.Columns.AddRange(new DataGridViewColumn[] { this.numberColumn, this.modifierKey1Column, this.modifierKey2Column, this.copyKeyColumn, this.cutKeyColumn, this.pasteKeyColumn });
			this.dgClipboards.DataSource = dgClipboardbindingSource;

			// number column
			this.numberColumn.DataPropertyName = "number";
			this.numberColumn.HeaderText = "#";
			this.numberColumn.Name = "numberColumn";
			this.numberColumn.ReadOnly = true;
			this.numberColumn.Resizable = DataGridViewTriState.False;
			this.numberColumn.Width = 25;

			// accessor key 1 column
			this.modifierKey1Column.DataPropertyName = "modifier_key_1";
			this.modifierKey1Column.DataSource = this.modifierKeyBindingSource;
			this.modifierKey1Column.DisplayMember = "display_text";
			this.modifierKey1Column.HeaderText = "Modifier Key 1";
			this.modifierKey1Column.Name = "modifierKey1Column";
			this.modifierKey1Column.Resizable = DataGridViewTriState.False;
			this.modifierKey1Column.ValueMember = "key_code";
			this.modifierKey1Column.Width = 85;

			// accessor key 2 column
			this.modifierKey2Column.DataPropertyName = "modifier_key_2";
			this.modifierKey2Column.DataSource = this.modifierKeyBindingSource;
			this.modifierKey2Column.DisplayMember = "display_text";
			this.modifierKey2Column.HeaderText = "Modifier Key 2";
			this.modifierKey2Column.Name = "modifierKey2Column";
			this.modifierKey2Column.Resizable = DataGridViewTriState.False;
			this.modifierKey2Column.ValueMember = "key_code";
			this.modifierKey2Column.Width = 85;

			// copy column
			this.copyKeyColumn.DataPropertyName = "copy_key";
			this.copyKeyColumn.DataSource = this.operationKeyBindingSource;
			this.copyKeyColumn.DisplayMember = "display_text";
			this.copyKeyColumn.HeaderText = "Copy Key";
			this.copyKeyColumn.Name = "copyKeyColumn";
			this.copyKeyColumn.Resizable = DataGridViewTriState.False;
			this.copyKeyColumn.ValueMember = "key_code";
			this.copyKeyColumn.Width = 85;

			// cut column
			this.cutKeyColumn.DataPropertyName = "cut_key";
			this.cutKeyColumn.DataSource = this.operationKeyBindingSource;
			this.cutKeyColumn.DisplayMember = "display_text";
			this.cutKeyColumn.HeaderText = "Cut Key";
			this.cutKeyColumn.Name = "cutKeyColumn";
			this.cutKeyColumn.Resizable = DataGridViewTriState.False;
			this.cutKeyColumn.ValueMember = "key_code";
			this.cutKeyColumn.Width = 85;

			// paste column
			this.pasteKeyColumn.DataPropertyName = "paste_key";
			this.pasteKeyColumn.DataSource = this.operationKeyBindingSource;
			this.pasteKeyColumn.DisplayMember = "display_text";
			this.pasteKeyColumn.HeaderText = "Paste Key";
			this.pasteKeyColumn.Name = "pasteKeyColumn";
			this.pasteKeyColumn.Resizable = DataGridViewTriState.False;
			this.pasteKeyColumn.ValueMember = "key_code";
			this.pasteKeyColumn.Width = 85;
		}

		/// <summary>
		/// Called when the window is destroyed.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void MultipleClipboardsDialog_Disposed(object sender, EventArgs e)
		{
			MultipleClipboardsDialog.ChangeClipboardChain(this.Handle, this.nextClipboardViewer);
		}

		/// <summary>
		/// Called before the form is first rendered.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void MultipleClipboardsDialog_Load(object sender, EventArgs e)
		{
			this.Hide();
			this.WindowState = FormWindowState.Minimized;
			this.RegisterAllHotkeys();
		}

		/// <summary>
		/// Called after the form is closed.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void MultipleClipboardsDialog_FormClosed(object sender, FormClosedEventArgs e)
		{
			this.UnregisterAllGlobalHotKeys();
		}

		/// <summary>
		/// Called when the form is resized / minimized.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void MultipleClipboardsDialog_SizeChanged(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.Hide();
			}
		}

		/// <summary>
		/// Called when the user clicks the Edit menu option.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void EditMultipleClipboardOptionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.ShowMainDialog();
			this.tabControl.SelectedTab = this.tabSettings;
		}

		/// <summary>
		/// Called when the user double clicks the tray icon.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			this.ShowMainDialog();
			this.tabControl.SelectedTab = this.tabSettings;
		}

		/// <summary>
		/// Called when the user clicks the Exit menu option.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Called when the Save button is clicked.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void BtnSave_Click(object sender, EventArgs e)
		{
			this.SaveSettings();
		}

		/// <summary>
		/// Called when the exit button is clicked.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void BtnExit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Called when the user clicks the hide dialog button.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void BtnHide_Click(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
			this.Hide();
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

		/// <summary>
		/// Called when the user clicks the show error log menu item.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void ErrorLogMenuItem_Click(object sender, EventArgs e)
		{
			this.ShowMainDialog();
			this.tabControl.SelectedTab = this.tabErrorLog;
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

		/// <summary>
		/// Called when the user clicks the about menu item.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void AboutMenuItem_Click(object sender, EventArgs e)
		{
			this.ShowMainDialog();
			this.tabControl.SelectedTab = this.tabAbout;
		}

		/// <summary>
		/// Called when the user changes the number of clipboards.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event args.</param>
		private void TxtNumClipboards_ValueChanged(object sender, EventArgs e)
		{
			int newNumClipboards = (int)txtNumClipboards.Value;
			if (newNumClipboards > this.settingsManager.NumberOfClipboards)
			{
				this.settingsManager.AddNewClipboard();
			}
			else if (newNumClipboards < this.settingsManager.NumberOfClipboards)
			{
				this.settingsManager.RemoveClipboard();
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

		/// <summary>
		/// Show the edit dialog.
		/// </summary>
		private void ShowMainDialog()
		{
			this.Show();
			this.WindowState = FormWindowState.Normal;
			this.Focus();
		}

		/// <summary>
		/// Refreshes the clipboard history grid with the most current data.
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

		/// <summary>
		/// Save settings to file and re-initialize the app with the new settings.
		/// </summary>
		private void SaveSettings()
		{
			try
			{
				this.settingsManager.SaveSettings();
				MessageBox.Show("Settings saved successfully.", "Save Complete");
			}
			catch (Exception e)
			{
				MessageBox.Show("There was an error saving the settings.  Check the log.", "Error");
				this.LogError(e.Message);
			}

			try
			{
				this.UnregisterAllGlobalHotKeys();
				this.RegisterAllHotkeys();
				this.clipboardManager.NumberOfHistoricalRecords = this.settingsManager.NumberOfClipboardManagerRecords;
				this.clipboardManager.Reset();
				this.FillClipboardSelectDropdown();
			}
			catch (Exception e)
			{
				MessageBox.Show("There was an error re-initializing the application with the new settings.  This should never happen.  Try re-starting the application.", "Error");
				this.LogError(e.Message);
			}
		}

		/// <summary>
		/// Log error to file.
		/// </summary>
		/// <param name="errorMessage">The error message to write to the file.</param>
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

		/// <summary>
		/// Register all the hotkeys.
		/// </summary>
		private void RegisterAllHotkeys()
		{
			HotKey cutHotKey;
			HotKey copyHotKey;
			HotKey pasteHotKey;

			try
			{
				foreach (clipboardDS.clipboardRow row in this.settingsManager.ClipboardDS.clipboard)
				{
					// cut
					cutHotKey = new HotKey(row.modifier_key_1, row.modifier_key_2, row.cut_key, HotKeyType.CUT, row.number);
					this.RegisterGlobalHotKey(cutHotKey);

					// copy
					copyHotKey = new HotKey(row.modifier_key_1, row.modifier_key_2, row.copy_key, HotKeyType.COPY, row.number);
					this.RegisterGlobalHotKey(copyHotKey);

					// paste
					pasteHotKey = new HotKey(row.modifier_key_1, row.modifier_key_2, row.paste_key, HotKeyType.PASTE, row.number);
					this.RegisterGlobalHotKey(pasteHotKey);

					// create a new clipboard entry for this set of hotkeys
					this.clipboardManager.AddClipboard(row.number, cutHotKey, copyHotKey, pasteHotKey);
				}
			}
			catch (Exception e)
			{
				string errorMessage = "Error registering the hotkeys from the settings file.\r\n\r\nIf this problem persists after restarting the app you should re-install.";
				MessageBox.Show(errorMessage + "\r\n\r\nProgram will now exit.", "Error Reading Settings File");
				this.LogError(errorMessage + "\r\n" + e.ToString());
				this.Close();
			}
		}

		/// <summary>
		/// Register a global hot key.
		/// </summary>
		/// <param name="hotKey">The hot key to register.</param>
		private void RegisterGlobalHotKey(HotKey hotKey)
		{
			short hotkeyID = 0;
			try
			{
				// use the GlobalAddAtom API to get a unique ID (as suggested by MSDN docs)
				string atomName = string.Format("{0}_{1}_{2}", this.Name, hotKey.Modifiers.ToString(), hotKey.Key.ToString());
				hotkeyID = MultipleClipboardsDialog.GlobalAddAtom(atomName);
				if (hotkeyID == 0)
				{
					throw new Exception("Unable to generate unique hotkey ID. Error code: " + Marshal.GetLastWin32Error().ToString());
				}

				// register the hotkey, throw if any error
				if (MultipleClipboardsDialog.RegisterHotKey(this.Handle, hotkeyID, hotKey.Modifiers, (int)hotKey.Key) == 0)
				{
					throw new Exception("Unable to register hotkey. Error code: " + Marshal.GetLastWin32Error().ToString());
				}

				// add the hotkeyID to the list of all hotkeys
				this.hotkeyIDs.Add(hotkeyID);
			}
			catch (Exception e)
			{
				// clean up if hotkey registration failed
				MessageBox.Show("Unable to register hotkey combination:\r\n" + hotKey.ToString() + "\r\n\r\nPerhaps this key combination is already in use?", "Error Registering Hotkey");
				this.LogError("Unable to register hotkey combination:\r\n" + hotKey.ToString() + "\r\n" + e.ToString());
				this.UnregisterGlobalHotKey(hotkeyID);
			}
		}

		/// <summary>
		/// Unregister all global hotkeys.
		/// </summary>
		private void UnregisterAllGlobalHotKeys()
		{
			foreach (short hotkeyID in hotkeyIDs)
			{
				this.UnregisterGlobalHotKey(hotkeyID);
			}
			this.hotkeyIDs.Clear();
			this.clipboardManager.Reset();
		}

		/// <summary>
		/// Unregister a single global hotkey.
		/// </summary>
		/// <param name="hotkeyID">The ID of the hot key to unregister.</param>
		private void UnregisterGlobalHotKey(short hotkeyID)
		{
			if (hotkeyID != 0)
			{
				MultipleClipboardsDialog.UnregisterHotKey(this.Handle, hotkeyID);
				// clean up the atom list
				MultipleClipboardsDialog.GlobalDeleteAtom(hotkeyID);
				hotkeyID = 0;
			}
		}

		/// <summary>
		/// Check if a certain key is held down.
		/// </summary>
		/// <param name="key">The key to check.</param>
		/// <returns>True if the key is pressed, false if it is not.</returns>
		private bool IsKeyPressed(Keys key)
		{
			return ((int)MultipleClipboardsDialog.GetAsyncKeyState((int)key) & 0x00008000) == 0x8000;
		}

		/// <summary>
		/// Checks if any of the possible modifier keys are held down.
		/// </summary>
		/// <returns>True if any of the modifier keys are pressed, false if not.</returns>
		private bool ModifierKeysPressed()
		{
			return (this.IsKeyPressed(Keys.LShiftKey) ||
					this.IsKeyPressed(Keys.RShiftKey) ||
					this.IsKeyPressed(Keys.LControlKey) ||
					this.IsKeyPressed(Keys.RControlKey) ||
					this.IsKeyPressed(Keys.LWin) ||
					this.IsKeyPressed(Keys.RWin) ||
					this.IsKeyPressed(Keys.Alt));
		}

		/// <summary>
		/// Overrides the Windows message loop.
		/// </summary>
		/// <param name="m">The current message.</param>
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case MultipleClipboardsDialog.WM_HOTKEY:
					// figure out what key was pressed and send it along to the clipboard manager
					this.currentMessage.MessageString = m.ToString();
					this.currentMessage.MessageTime = DateTime.Now;

					// make sure that the message we just got is not the same one that was just processed
					// this happens when the user holds down the hotkey combination for a few seconds
					if (this.currentMessage != this.lastMessageProcessed)
					{
						int key = (int)(((uint)m.LParam & 0xFFFF0000) >> 16);
						int modifiers = (int)((uint)m.LParam & 0x0000FFFF);
						HotKey hotKey = new HotKey(modifiers, key);

						// wait while there are any modifier keys held down
						// this causes unpredictable results when the user has setup a combination of different hotkeys
						while (this.ModifierKeysPressed())
							;

						try
						{
							this.clipboardManager.ProcessHotKey(hotKey);
						}
						catch (Exception e)
						{
							this.LogError("Error processing the hotkey:\r\n" + hotKey.ToString() + "\r\n" + e.ToString());
						}
					}
					break;

				case MultipleClipboardsDialog.WM_DRAWCLIPBOARD:
					if (!this.clipboardManager.IsProcessingClipboardAction && !this.isFirstClipboardMessage)
					{
						// the data on the clipboard has changed
						// this means the user used the regular windows clipboard
						// track the data on the clipboard for the history viewer
						// data coppied using any additional clipboards will be tracked internally
						this.clipboardManager.StoreClipboardContents();
					}
					// send the message to the next app in the clipboard chain
					MultipleClipboardsDialog.SendMessage(this.nextClipboardViewer, m.Msg, m.WParam, m.LParam);
					this.isFirstClipboardMessage = false;
					break;

				case MultipleClipboardsDialog.WM_CHANGECBCHAIN:
					if (m.WParam == this.nextClipboardViewer)
					{
						this.nextClipboardViewer = m.LParam;
					}
					else
					{
						MultipleClipboardsDialog.SendMessage(this.nextClipboardViewer, m.Msg, m.WParam, m.LParam);
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
