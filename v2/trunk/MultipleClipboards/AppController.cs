using System;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using MultipleClipboards.ClipboardManagement;
using MultipleClipboards.Persistence;
using MultipleClipboards.Presentation;

namespace MultipleClipboards
{
	public static class AppController
	{
		private static TaskbarIcon _trayIcon;

		static AppController()
		{
			Settings = new SettingsManager();
		}

		public static SettingsManager Settings
		{
			get;
			private set;
		}

		public static ClipboardManager ClipboardManager
		{
			get;
			private set;
		}

		public static TaskbarIcon TrayIcon
		{
			get
			{
				return _trayIcon;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("The application tried to set the tray icon property, but the value given was null.  That's not cool.");
				}

				if (_trayIcon != null)
				{
					throw new InvalidOperationException("You can only set the application tray icon property once!");
				}

				_trayIcon = value;
			}
		}

		public static void InitializeClipboardManager(IntPtr handle)
		{
			if (ClipboardManager != null)
			{
				throw new InvalidOperationException("An attempt was made to initialize the application-wide instance of the clipboard manager after it has already been initialized.  This is a no-no!");
			}

			ClipboardManager = new ClipboardManager(handle);
		}

		public static void ShowMainWindow()
		{
			Application app = Application.Current;

			if (app.MainWindow == null)
			{
				app.MainWindow = new MainWindow();
				app.MainWindow.Show();
				app.MainWindow.Activate();
			}
			else
			{
				app.MainWindow.WindowState = WindowState.Normal;
				app.MainWindow.Focus();
				app.MainWindow.Activate();
			}
		}

		public static void Shutdown()
		{
			Application.Current.Shutdown();
		}
	}
}
