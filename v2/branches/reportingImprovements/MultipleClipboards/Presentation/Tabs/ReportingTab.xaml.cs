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

            Task.Run(
                () =>
                {
                    var message = new MainWindowNotification();

                    try
                    {
                        string reportPath;
                        using (new DbContextScope<MultipleClipboardsDataContext>())
                        {
                            var repo = new MultipleClipboardsDataRepository();
                            reportPath = repo.GenerateReport(type);
                        }

                        if (string.IsNullOrWhiteSpace(reportPath) || !File.Exists(reportPath))
                        {
                            message.IconType = IconType.Warning;
                            message.MessageBody = "No exception was thrown, but there was an error generating a data report.";
                        }
                        else
                        {
                            message.IconType = IconType.Success;
                            message.MessageBody = string.Format("Report saved to: {0}", reportPath);
                            Process.Start(reportPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        const string errorMessage = "There was an error generating a data report.";
                        log.Error(errorMessage, ex);
                        message.IconType = IconType.Error;
                        message.MessageBody = errorMessage;
                    }

                    MessageBus.Instance.Publish(message);
                });
        }

        private void PurgeHistoricalDataButtonClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to purge ALL historical data gathered by this application?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            Task.Run(
                () =>
                {
                    try
                    {
                        using (new DbContextScope<MultipleClipboardsDataContext>())
                        {
                            var repo = new MultipleClipboardsDataRepository();
                            repo.PurgeData();
                        }
                        
                        MessageBus.Instance.Publish(new MainWindowNotification
                        {
                            IconType = IconType.Success,
                            MessageBody = "Historical data purged successfully."
                        });
                    }
                    catch (Exception exception)
                    {
                        const string errorMessage = "There was an error purging historical data.";
                        log.Error(errorMessage, exception);
                        MessageBus.Instance.Publish(new MainWindowNotification
                        {
                            IconType = IconType.Error,
                            MessageBody = errorMessage
                        });
                    }
                });
        }
    }
}
