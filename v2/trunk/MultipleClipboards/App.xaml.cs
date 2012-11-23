using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using log4net;
using log4net.Config;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Messaging;
using MultipleClipboards.Persistence;
using MultipleClipboards.Presentation;
using MultipleClipboards.Presentation.Icons;
using MultipleClipboards.Presentation.TrayIcon;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace MultipleClipboards
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static readonly ILog log;
		private static readonly ManualResetEvent staticInitializationComplete = new ManualResetEvent(false);
        private static readonly bool errorInStaticInitializer;
		private TrayIconManager trayIconManager;

		static App()
		{
            try
            {
                // Make sure the app data directory exists.
                if (!Directory.Exists(Constants.BaseDataPath))
                {
                    Directory.CreateDirectory(Constants.BaseDataPath);
                }

                // Configure the logger.
                GlobalContext.Properties["LogFilePath"] = Constants.LogFilePath;
                XmlConfigurator.Configure();
                LogHelper.SetLogLevel(AppController.Settings.ApplicationLogLevel);
                log = LogManager.GetLogger(typeof(App));
            }
            catch (Exception e)
            {
                errorInStaticInitializer = true;
                var exceptionBuilder = new StringBuilder();

                while (e != null)
                {
                    exceptionBuilder.AppendLine(e.Message);
                    exceptionBuilder.AppendLine(e.StackTrace);
                    exceptionBuilder.AppendLine();
                    e = e.InnerException;
                }

                EventLog.WriteEntry(
                    "Multiple Clipboards",
                    string.Format("Error initializing the logger.{0}{1}", Environment.NewLine, exceptionBuilder.ToString()),
                    EventLogEntryType.Error);
            }
            finally
            {
                staticInitializationComplete.Set();
            }
		}

		protected override void OnStartup(StartupEventArgs e)
		{
#if DEBUG
			if (e.Args.Contains("--debug"))
			{
                MessageBox.Show("This is your chance to attach to the process and debug!");
			}
#endif
			base.OnStartup(e);
			this.DispatcherUnhandledException += AppDispatcherUnhandledException;

			// Make sure the static constructor has done it's thing.
			staticInitializationComplete.WaitOne();

			if (EnvironmentHelper.IsMultipleClipboardsRunning())
			{
				MessageBox.Show(
					"There is already an instance of Multiple Clipboards running.\r\n\r\nThere can only be one instance of this application running at a time.",
					"Application Already Running",
					MessageBoxButton.OK,
					MessageBoxImage.Exclamation);
				Environment.Exit(0);
			}

			// Create the system tray icon.
			// This establishes the tray icon and sets up tray notifications, but does not yet bind to the clipboard manager.
			// This is done first so that any errors encountered initializing the clipboard manager can be posted to the
			// tray notification popup.
			this.InitializeTrayIcon();

			// Create the hidden window that will handle the message loop for this app.
			// Because this is the first window that gets created the framework automatically makes this
			// the MainWindow, which is not at all what we want!
			this.ClipboardWindow = new ClipboardWindow();
			this.MainWindow = null;

			// Tell the tray icon that the clipboard manager has been initialized so it can bind to the clipboard history collection.
			this.trayIconManager.OnClipboardManagerInitialized();

			// Only show the main UI if the application was launched by the installed shortcut.
			if (e.Args.Contains("-fromShortcut"))
			{
				AppController.ShowMainWindow();
			}

            // Show a tray notification if there was an error in the static initializer.
            if (errorInStaticInitializer)
            {
                MessageBus.Instance.Publish(new TrayNotification
                {
                    MessageBody = "There was an error initializing the application.  Some features may be unavailable.  A detailed error report has been logged to the Windows Event Logs.",
                    IconType = IconType.Error
                });
            }

			log.Debug("Application initialized!");
		}

		protected override void OnExit(ExitEventArgs e)
		{
            AppController.ClipboardManager.SaveClipboardHistory();
			this.ClipboardWindow.Dispose();
			this.trayIconManager.Dispose();
			log.DebugFormat("Application exiting with exit code {0}.", e.ApplicationExitCode);
			base.OnExit(e);
		}

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            try
            {
                AppController.ClipboardManager.SaveClipboardHistory();
            }
            catch (Exception ex)
            {
                log.Error("The session is ending and therefore the application is closing, but there was an error saving the clipboard history to disk.", ex);
            }

            base.OnSessionEnding(e);
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

		private void InitializeTrayIcon()
		{
			const string trayIconResourcePath = "MultipleClipboards.Presentation.Icons.TrayContextMenu.Clipboard.ico";
			var iconStream = Assembly.GetEntryAssembly().GetManifestResourceStream(trayIconResourcePath);

			if (iconStream == null)
			{
				throw new NullReferenceException(string.Format("Unable to find the resource '{0}' in the executing assembly.", trayIconResourcePath));
			}

			var notifyIcon = new NotifyIcon
			{
				Icon = new Icon(iconStream),
				Text = "Multiple Clipboards"
			};
			this.trayIconManager = new TrayIconManager(notifyIcon);
			this.trayIconManager.ShowTrayIcon();
		}
	}
}
