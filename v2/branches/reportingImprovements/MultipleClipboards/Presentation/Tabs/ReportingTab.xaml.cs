using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MultipleClipboards.Messaging;
using MultipleClipboards.Persistence;
using MultipleClipboards.Presentation.Icons;
using log4net;

namespace MultipleClipboards.Presentation.Tabs
{
    /// <summary>
    /// Interaction logic for ReportingTab.xaml
    /// </summary>
    public partial class ReportingTab : UserControl
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ReportingTab));

        public ReportingTab()
        {
            InitializeComponent();
        }

        private void GenerateReportButtonClick(object sender, RoutedEventArgs e)
        {
            ReportType type;
            if (!Enum.TryParse(ReportTypeDropDown.SelectedValue.ToString(), true, out type))
            {
                MessageBus.Instance.Publish(new MainWindowNotification
                {
                    IconType = IconType.Error,
                    MessageBody = "Unknown report type"
                });
                return;
            }
            Task.Run(() => GenerateReport(type));
        }

        private static void GenerateReport(ReportType type)
        {
            try
            {
                var reportPath = new DataObjectRepository().GenerateReport(type);

                if (string.IsNullOrWhiteSpace(reportPath) || !File.Exists(reportPath))
                {
                    MessageBus.Instance.Publish(new MainWindowNotification
                    {
                        IconType = IconType.Warning,
                        MessageBody = "No exception was thrown, but there was an error generating a data report."
                    });
                    return;
                }

                MessageBus.Instance.Publish(new MainWindowNotification
                {
                    IconType = IconType.Success,
                    MessageBody = string.Format("Report saved to: {0}", reportPath)
                });

                Process.Start(reportPath);
            }
            catch (Exception ex)
            {
                const string message = "There was an error generating a data report.";
                log.Error(message, ex);
                MessageBus.Instance.Publish(new MainWindowNotification
                {
                    IconType = IconType.Error,
                    MessageBody = message
                });
            }
        }
    }
}
