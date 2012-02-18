using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Messaging;
using MultipleClipboards.Presentation;
using log4net;
using log4net.Config;

namespace MultipleClipboards
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static readonly ILog log;
		private static readonly ManualResetEvent staticInitializationComplete = new ManualResetEvent(false);

		static App()
		{
			// Make sure the app data directory exists.
			if (!Directory.Exists(Constants.BaseDataPath))
			{
				Directory.CreateDirectory(Constants.BaseDataPath);
			}

			// Configure the logger.
			GlobalContext.Properties["LogFilePath"] = Constants.LogFilePath;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(Constants.LogConfigFileName));
			log = LogManager.GetLogger(typeof(App));
			staticInitializationComplete.Set();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
#if DEBUG
			if (e.Args.Contains("--debug"))
			{
				Debugger.Launch();
			}
#endif

			base.OnStartup(e);
			this.DispatcherUnhandledException += AppDispatcherUnhandledException;

			// Make sure the static constructor has done it's thing.
			staticInitializationComplete.WaitOne();

			if (Process.GetProcessesByName(Constants.ProcessName).Any(p => p.Id != Process.GetCurrentProcess().Id))
			{
				MessageBox.Show(
					"There is already an instance of Multiple Clipboards running.\r\n\r\nThere can only be one instance of this application running at a time.",
					"Application Already Running",
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);
				Environment.Exit(0);
			}

			// Create the hidden window that will handle the message loop for this app.
			// Because this is the first window that gets created the framework automatically makes this
			// the MainWindow, which is not at all what we want!
			this.ClipboardWindow = new ClipboardWindow();
			this.MainWindow = null;

			// Create the tray icon for the app.
			// This MUST be done after the clipboard window, which initializes the clipboard manager, is loaded
			// because the tray icon's context menu binds to properties on the clipboard manager.
			AppController.TrayIcon = (TaskbarIcon)this.FindResource("TrayIcon");

			// Only show the main UI if the application was launched by the installed shortcut.
			if (e.Args.Contains("-fromShortcut"))
			{
				AppController.ShowMainWindow();
			}

			log.Debug("Application initialized!");
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.ClipboardWindow.Dispose();
			log.DebugFormat("Application exiting with exit code {0}.", e.ApplicationExitCode);
			base.OnExit(e);
		}

		private static void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			log.Error("An unhandled expception has occured.", e.Exception);
			e.Handled = true;
		}

		private ClipboardWindow ClipboardWindow
		{
			get;
			set;
		}
	}
}
