namespace MultipleClipboards
{
	partial class MultipleClipboardsDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultipleClipboardsDialog));
			this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.trayRightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.editMultipleClipboardOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clipboardHistoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.viewDetailedHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.errorLogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.btnSave = new System.Windows.Forms.Button();
			this.btnExit = new System.Windows.Forms.Button();
			this.btnHide = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabSettings = new System.Windows.Forms.TabPage();
			this.txtNumberOfHistoricalRecords = new System.Windows.Forms.NumericUpDown();
			this.lblNumHistoricalRecords = new System.Windows.Forms.Label();
			this.txtNumClipboards = new System.Windows.Forms.NumericUpDown();
			this.dgClipboards = new System.Windows.Forms.DataGridView();
			this.lblNumClipboards = new System.Windows.Forms.Label();
			this.tabHistory = new System.Windows.Forms.TabPage();
			this.btnRefreshHistory = new System.Windows.Forms.Button();
			this.dgClipboardHistory = new System.Windows.Forms.DataGridView();
			this.colNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colData = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.btnPlaceRowOnClipboard = new System.Windows.Forms.Button();
			this.lblClipboardSelect = new System.Windows.Forms.Label();
			this.ddlClipboardSelect = new System.Windows.Forms.ComboBox();
			this.tabAbout = new System.Windows.Forms.TabPage();
			this.txtAbout = new System.Windows.Forms.RichTextBox();
			this.tabErrorLog = new System.Windows.Forms.TabPage();
			this.txtErrorLog = new System.Windows.Forms.TextBox();
			this.trayRightClickMenu.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.txtNumberOfHistoricalRecords)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtNumClipboards)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgClipboards)).BeginInit();
			this.tabHistory.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgClipboardHistory)).BeginInit();
			this.tabAbout.SuspendLayout();
			this.tabErrorLog.SuspendLayout();
			this.SuspendLayout();
			// 
			// trayIcon
			// 
			this.trayIcon.ContextMenuStrip = this.trayRightClickMenu;
			this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
			this.trayIcon.Text = "Multiple Clipboards";
			this.trayIcon.Visible = true;
			this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TrayIcon_MouseDoubleClick);
			// 
			// trayRightClickMenu
			// 
			this.trayRightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editMultipleClipboardOptionsToolStripMenuItem,
            this.clipboardHistoryMenuItem,
            this.aboutMenuItem,
            this.errorLogMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
			this.trayRightClickMenu.Name = "trayRightClickMenu";
			this.trayRightClickMenu.Size = new System.Drawing.Size(272, 130);
			// 
			// editMultipleClipboardOptionsToolStripMenuItem
			// 
			this.editMultipleClipboardOptionsToolStripMenuItem.Name = "editMultipleClipboardOptionsToolStripMenuItem";
			this.editMultipleClipboardOptionsToolStripMenuItem.Size = new System.Drawing.Size(271, 24);
			this.editMultipleClipboardOptionsToolStripMenuItem.Text = "Edit Multiple Clipboard Options";
			this.editMultipleClipboardOptionsToolStripMenuItem.Click += new System.EventHandler(this.EditMultipleClipboardOptionsToolStripMenuItem_Click);
			// 
			// clipboardHistoryMenuItem
			// 
			this.clipboardHistoryMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator2,
            this.viewDetailedHistoryToolStripMenuItem});
			this.clipboardHistoryMenuItem.Name = "clipboardHistoryMenuItem";
			this.clipboardHistoryMenuItem.Size = new System.Drawing.Size(271, 24);
			this.clipboardHistoryMenuItem.Text = "Clipboard History";
			this.clipboardHistoryMenuItem.DropDownOpening += new System.EventHandler(this.ClipboardHistoryMenuItem_DropDownOpening);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(206, 6);
			// 
			// viewDetailedHistoryToolStripMenuItem
			// 
			this.viewDetailedHistoryToolStripMenuItem.Name = "viewDetailedHistoryToolStripMenuItem";
			this.viewDetailedHistoryToolStripMenuItem.Size = new System.Drawing.Size(209, 24);
			this.viewDetailedHistoryToolStripMenuItem.Text = "View Detailed History";
			this.viewDetailedHistoryToolStripMenuItem.Click += new System.EventHandler(this.ViewDetailedHistoryToolStripMenuItem_Click);
			// 
			// aboutMenuItem
			// 
			this.aboutMenuItem.Name = "aboutMenuItem";
			this.aboutMenuItem.Size = new System.Drawing.Size(271, 24);
			this.aboutMenuItem.Text = "About Multiple Clipboards";
			this.aboutMenuItem.Click += new System.EventHandler(this.AboutMenuItem_Click);
			// 
			// errorLogMenuItem
			// 
			this.errorLogMenuItem.Name = "errorLogMenuItem";
			this.errorLogMenuItem.Size = new System.Drawing.Size(271, 24);
			this.errorLogMenuItem.Text = "Open Error Log";
			this.errorLogMenuItem.Click += new System.EventHandler(this.ErrorLogMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(268, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(271, 24);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
			// 
			// btnSave
			// 
			this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnSave.Location = new System.Drawing.Point(397, 14);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(100, 23);
			this.btnSave.TabIndex = 2;
			this.btnSave.Text = "Save Settings";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// btnExit
			// 
			this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExit.Location = new System.Drawing.Point(272, 378);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(100, 23);
			this.btnExit.TabIndex = 3;
			this.btnExit.Text = "Exit Application";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);
			// 
			// btnHide
			// 
			this.btnHide.Location = new System.Drawing.Point(166, 378);
			this.btnHide.Name = "btnHide";
			this.btnHide.Size = new System.Drawing.Size(100, 23);
			this.btnHide.TabIndex = 7;
			this.btnHide.Text = "Hide Dialog";
			this.btnHide.UseVisualStyleBackColor = true;
			this.btnHide.Click += new System.EventHandler(this.BtnHide_Click);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabSettings);
			this.tabControl.Controls.Add(this.tabHistory);
			this.tabControl.Controls.Add(this.tabAbout);
			this.tabControl.Controls.Add(this.tabErrorLog);
			this.tabControl.Location = new System.Drawing.Point(12, 13);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(514, 359);
			this.tabControl.TabIndex = 8;
			this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.TabControl_Selected);
			// 
			// tabSettings
			// 
			this.tabSettings.Controls.Add(this.txtNumberOfHistoricalRecords);
			this.tabSettings.Controls.Add(this.lblNumHistoricalRecords);
			this.tabSettings.Controls.Add(this.txtNumClipboards);
			this.tabSettings.Controls.Add(this.dgClipboards);
			this.tabSettings.Controls.Add(this.btnSave);
			this.tabSettings.Controls.Add(this.lblNumClipboards);
			this.tabSettings.Location = new System.Drawing.Point(4, 22);
			this.tabSettings.Name = "tabSettings";
			this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
			this.tabSettings.Size = new System.Drawing.Size(506, 333);
			this.tabSettings.TabIndex = 0;
			this.tabSettings.Text = "Settings";
			this.tabSettings.UseVisualStyleBackColor = true;
			// 
			// txtNumberOfHistoricalRecords
			// 
			this.txtNumberOfHistoricalRecords.Location = new System.Drawing.Point(172, 39);
			this.txtNumberOfHistoricalRecords.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.txtNumberOfHistoricalRecords.Name = "txtNumberOfHistoricalRecords";
			this.txtNumberOfHistoricalRecords.Size = new System.Drawing.Size(50, 20);
			this.txtNumberOfHistoricalRecords.TabIndex = 11;
			this.txtNumberOfHistoricalRecords.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
			// 
			// lblNumHistoricalRecords
			// 
			this.lblNumHistoricalRecords.AutoSize = true;
			this.lblNumHistoricalRecords.Location = new System.Drawing.Point(54, 41);
			this.lblNumHistoricalRecords.Name = "lblNumHistoricalRecords";
			this.lblNumHistoricalRecords.Size = new System.Drawing.Size(112, 13);
			this.lblNumHistoricalRecords.TabIndex = 10;
			this.lblNumHistoricalRecords.Text = "Clipboard History Size:";
			// 
			// txtNumClipboards
			// 
			this.txtNumClipboards.BackColor = System.Drawing.SystemColors.Window;
			this.txtNumClipboards.Location = new System.Drawing.Point(172, 12);
			this.txtNumClipboards.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.txtNumClipboards.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.txtNumClipboards.Name = "txtNumClipboards";
			this.txtNumClipboards.ReadOnly = true;
			this.txtNumClipboards.Size = new System.Drawing.Size(50, 20);
			this.txtNumClipboards.TabIndex = 7;
			this.txtNumClipboards.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.txtNumClipboards.ValueChanged += new System.EventHandler(this.TxtNumClipboards_ValueChanged);
			// 
			// dgClipboards
			// 
			this.dgClipboards.AllowUserToAddRows = false;
			this.dgClipboards.AllowUserToDeleteRows = false;
			this.dgClipboards.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgClipboards.Location = new System.Drawing.Point(3, 75);
			this.dgClipboards.Name = "dgClipboards";
			this.dgClipboards.Size = new System.Drawing.Size(494, 252);
			this.dgClipboards.TabIndex = 9;
			// 
			// lblNumClipboards
			// 
			this.lblNumClipboards.AutoSize = true;
			this.lblNumClipboards.Location = new System.Drawing.Point(6, 14);
			this.lblNumClipboards.Name = "lblNumClipboards";
			this.lblNumClipboards.Size = new System.Drawing.Size(160, 13);
			this.lblNumClipboards.TabIndex = 8;
			this.lblNumClipboards.Text = "Number of Additional Clipboards:";
			// 
			// tabHistory
			// 
			this.tabHistory.Controls.Add(this.btnRefreshHistory);
			this.tabHistory.Controls.Add(this.dgClipboardHistory);
			this.tabHistory.Controls.Add(this.btnPlaceRowOnClipboard);
			this.tabHistory.Controls.Add(this.lblClipboardSelect);
			this.tabHistory.Controls.Add(this.ddlClipboardSelect);
			this.tabHistory.Location = new System.Drawing.Point(4, 22);
			this.tabHistory.Name = "tabHistory";
			this.tabHistory.Padding = new System.Windows.Forms.Padding(3);
			this.tabHistory.Size = new System.Drawing.Size(506, 333);
			this.tabHistory.TabIndex = 3;
			this.tabHistory.Text = "Clipboard History";
			this.tabHistory.UseVisualStyleBackColor = true;
			// 
			// btnRefreshHistory
			// 
			this.btnRefreshHistory.Location = new System.Drawing.Point(400, 35);
			this.btnRefreshHistory.Name = "btnRefreshHistory";
			this.btnRefreshHistory.Size = new System.Drawing.Size(100, 23);
			this.btnRefreshHistory.TabIndex = 4;
			this.btnRefreshHistory.Text = "Refresh Grid";
			this.btnRefreshHistory.UseVisualStyleBackColor = true;
			this.btnRefreshHistory.Click += new System.EventHandler(this.BtnRefreshHistory_Click);
			// 
			// dgClipboardHistory
			// 
			this.dgClipboardHistory.AllowUserToAddRows = false;
			this.dgClipboardHistory.AllowUserToDeleteRows = false;
			this.dgClipboardHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgClipboardHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colNumber,
            this.colData,
            this.colTime});
			this.dgClipboardHistory.Location = new System.Drawing.Point(3, 64);
			this.dgClipboardHistory.MultiSelect = false;
			this.dgClipboardHistory.Name = "dgClipboardHistory";
			this.dgClipboardHistory.ReadOnly = true;
			this.dgClipboardHistory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgClipboardHistory.Size = new System.Drawing.Size(497, 263);
			this.dgClipboardHistory.TabIndex = 3;
			// 
			// colNumber
			// 
			this.colNumber.HeaderText = "#";
			this.colNumber.Name = "colNumber";
			this.colNumber.ReadOnly = true;
			this.colNumber.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.colNumber.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colNumber.Width = 30;
			// 
			// colData
			// 
			this.colData.HeaderText = "Data";
			this.colData.MinimumWidth = 305;
			this.colData.Name = "colData";
			this.colData.ReadOnly = true;
			this.colData.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			this.colData.Width = 305;
			// 
			// colTime
			// 
			this.colTime.HeaderText = "Timestamp";
			this.colTime.Name = "colTime";
			this.colTime.ReadOnly = true;
			this.colTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// btnPlaceRowOnClipboard
			// 
			this.btnPlaceRowOnClipboard.Location = new System.Drawing.Point(256, 6);
			this.btnPlaceRowOnClipboard.Name = "btnPlaceRowOnClipboard";
			this.btnPlaceRowOnClipboard.Size = new System.Drawing.Size(244, 23);
			this.btnPlaceRowOnClipboard.TabIndex = 2;
			this.btnPlaceRowOnClipboard.Text = "Place Selected Row on Selected Clipboard";
			this.btnPlaceRowOnClipboard.UseVisualStyleBackColor = true;
			this.btnPlaceRowOnClipboard.Click += new System.EventHandler(this.BtnPlaceRowOnClipboard_Click);
			// 
			// lblClipboardSelect
			// 
			this.lblClipboardSelect.AutoSize = true;
			this.lblClipboardSelect.Location = new System.Drawing.Point(6, 9);
			this.lblClipboardSelect.Name = "lblClipboardSelect";
			this.lblClipboardSelect.Size = new System.Drawing.Size(87, 13);
			this.lblClipboardSelect.TabIndex = 1;
			this.lblClipboardSelect.Text = "Select Clipboard:";
			// 
			// ddlClipboardSelect
			// 
			this.ddlClipboardSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ddlClipboardSelect.FormattingEnabled = true;
			this.ddlClipboardSelect.Location = new System.Drawing.Point(99, 6);
			this.ddlClipboardSelect.Name = "ddlClipboardSelect";
			this.ddlClipboardSelect.Size = new System.Drawing.Size(151, 21);
			this.ddlClipboardSelect.TabIndex = 0;
			// 
			// tabAbout
			// 
			this.tabAbout.Controls.Add(this.txtAbout);
			this.tabAbout.Location = new System.Drawing.Point(4, 22);
			this.tabAbout.Name = "tabAbout";
			this.tabAbout.Padding = new System.Windows.Forms.Padding(3);
			this.tabAbout.Size = new System.Drawing.Size(506, 333);
			this.tabAbout.TabIndex = 1;
			this.tabAbout.Text = "About";
			this.tabAbout.UseVisualStyleBackColor = true;
			// 
			// txtAbout
			// 
			this.txtAbout.BackColor = System.Drawing.SystemColors.Window;
			this.txtAbout.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtAbout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtAbout.Location = new System.Drawing.Point(3, 3);
			this.txtAbout.Name = "txtAbout";
			this.txtAbout.ReadOnly = true;
			this.txtAbout.Size = new System.Drawing.Size(500, 327);
			this.txtAbout.TabIndex = 0;
			this.txtAbout.Text = "";
			// 
			// tabErrorLog
			// 
			this.tabErrorLog.Controls.Add(this.txtErrorLog);
			this.tabErrorLog.Location = new System.Drawing.Point(4, 22);
			this.tabErrorLog.Name = "tabErrorLog";
			this.tabErrorLog.Size = new System.Drawing.Size(506, 333);
			this.tabErrorLog.TabIndex = 2;
			this.tabErrorLog.Text = "Error Log";
			this.tabErrorLog.UseVisualStyleBackColor = true;
			// 
			// txtErrorLog
			// 
			this.txtErrorLog.BackColor = System.Drawing.SystemColors.Window;
			this.txtErrorLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.txtErrorLog.Location = new System.Drawing.Point(4, 4);
			this.txtErrorLog.Multiline = true;
			this.txtErrorLog.Name = "txtErrorLog";
			this.txtErrorLog.ReadOnly = true;
			this.txtErrorLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtErrorLog.Size = new System.Drawing.Size(500, 326);
			this.txtErrorLog.TabIndex = 0;
			// 
			// MultipleClipboardsDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(532, 407);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.btnHide);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(550, 450);
			this.MinimumSize = new System.Drawing.Size(550, 450);
			this.Name = "MultipleClipboardsDialog";
			this.Text = "Multiple Clipboards";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.Load += new System.EventHandler(this.MultipleClipboardsDialog_Load);
			this.SizeChanged += new System.EventHandler(this.MultipleClipboardsDialog_SizeChanged);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MultipleClipboardsDialog_FormClosed);
			this.trayRightClickMenu.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabSettings.ResumeLayout(false);
			this.tabSettings.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.txtNumberOfHistoricalRecords)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtNumClipboards)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgClipboards)).EndInit();
			this.tabHistory.ResumeLayout(false);
			this.tabHistory.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgClipboardHistory)).EndInit();
			this.tabAbout.ResumeLayout(false);
			this.tabErrorLog.ResumeLayout(false);
			this.tabErrorLog.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon trayIcon;
		private System.Windows.Forms.ContextMenuStrip trayRightClickMenu;
		private System.Windows.Forms.ToolStripMenuItem editMultipleClipboardOptionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Button btnExit;
		private System.Windows.Forms.Button btnHide;
		private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
		private System.Windows.Forms.ToolStripMenuItem errorLogMenuItem;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabSettings;
		private System.Windows.Forms.NumericUpDown txtNumClipboards;
		private System.Windows.Forms.DataGridView dgClipboards;
		private System.Windows.Forms.Label lblNumClipboards;
		private System.Windows.Forms.TabPage tabAbout;
		private System.Windows.Forms.TabPage tabErrorLog;
		private System.Windows.Forms.RichTextBox txtAbout;
		private System.Windows.Forms.TextBox txtErrorLog;
		private System.Windows.Forms.Label lblNumHistoricalRecords;
		private System.Windows.Forms.NumericUpDown txtNumberOfHistoricalRecords;
		private System.Windows.Forms.TabPage tabHistory;
		private System.Windows.Forms.ComboBox ddlClipboardSelect;
		private System.Windows.Forms.DataGridView dgClipboardHistory;
		private System.Windows.Forms.Button btnPlaceRowOnClipboard;
		private System.Windows.Forms.Label lblClipboardSelect;
		private System.Windows.Forms.ToolStripMenuItem clipboardHistoryMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewDetailedHistoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.Button btnRefreshHistory;
		private System.Windows.Forms.DataGridViewTextBoxColumn colNumber;
		private System.Windows.Forms.DataGridViewTextBoxColumn colData;
		private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
	}
}

