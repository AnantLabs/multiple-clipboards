using System;
using System.Windows;
using MultipleClipboards.ClipboardManagement;
using MultipleClipboards.LegacyPersistence;
using MultipleClipboards.Presentation;

namespace MultipleClipboards
{
	public static class AppController
	{
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

		public static void ExecuteOnUiThread(Action action)
		{
			Application.Current.Dispatcher.Invoke(new Action(action));
		}
	}
}
