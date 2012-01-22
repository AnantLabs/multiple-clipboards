using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using MultipleClipboards.Persistence;
using MultipleClipboards.Presentation;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;

			// Create the hidden window that will handle the message loop for this app.
			// Because this is the first window that gets created the framework automatically makes this
			// the MainWindow, which is not at all what we want!
			this.ClipboardWindow = new ClipboardWindow();
			this.MainWindow = null;

			// Create the tray icon for the app.
			// This MUST be done after the clipboard window, which initializes the clipboard manager, is loaded
			// because the tray icon's context menu binds to properties on the clipboard manager.
			AppController.TrayIcon = (TaskbarIcon)this.FindResource("TrayIcon");

			// TODO: Only show the main window on application launch if it is launched from the installed shortcut.
			AppController.ShowMainWindow();

			LogManager.Debug("Application initialized!");
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.ClipboardWindow.Dispose();
			LogManager.DebugFormat("Application exiting with exit code {0}.", e.ApplicationExitCode);
			base.OnExit(e);
		}

		private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			LogManager.Error("An unhandled expception has occured.", e.Exception);
			e.Handled = true;
		}

		private ClipboardWindow ClipboardWindow
		{
			get;
			set;
		}
	}
}
