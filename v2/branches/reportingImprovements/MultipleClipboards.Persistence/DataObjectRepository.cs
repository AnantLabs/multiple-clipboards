using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MultipleClipboards.GlobalResources;
using log4net;

namespace MultipleClipboards.Persistence
{
    public enum ReportType
    {
        LastWeekDetails,
        DataFormats,
        FailedDataFormats
    }

    public class DataObjectRepository
    {
        private static readonly object storeDataLock = new object();
        private static readonly ILog log = LogManager.GetLogger(typeof(DataObjectRepository));

        public Task StoreDataObjectAsync(DataObject dataObject)
        {
            return Task.Run(() => StoreDataObject(dataObject));
        }

        public string GenerateReport(ReportType reportType)
        {
            using (var context = new MultipleClipboardsDataContext())
            {
                object data;
                var path = Path.Combine(Constants.BaseDataPath, string.Format("DataReport-{0}-{1}.html", reportType, DateTime.Now.ToString("MM-dd-yyyy")));

                switch (reportType)
                {
                    case ReportType.LastWeekDetails:
                        data = GenerateDetailedReport(context);
                        break;
                    case ReportType.DataFormats:
                        data = GenerateDataFormatsReport(context);
                        break;
                    case ReportType.FailedDataFormats:
                        data = GenerateFailedFormatsReport(context);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("reportType");
                }

                if (data == null)
                {
                    return null;
                }

                using (var writer = LINQPad.Util.CreateXhtmlWriter(true))
                {
                    writer.Write(data);
                    File.WriteAllText(path, writer.ToString());
                }

                return path;
            }
        }

        private static object GenerateDetailedReport(MultipleClipboardsDataContext context)
        {
            // Select out a new object so we don't see the EF-generated proxy class junk
            return from o in context.DataObjects
                   where o.Timestamp >= DateTime.Now.Subtract(TimeSpan.FromDays(7))
                   select new
                   {
                       o.Id,
                       o.Timestamp,
                       o.DescriptionText,
                       o.AllFormats,
                       o.FailedDataFormats
                   };
        }

        private static object GenerateDataFormatsReport(MultipleClipboardsDataContext context)
        {
            return from f in context.DataFormats
                   select new
                   {
                       Id = f.Id,
                       Format = f.Format,
                       TypeName = f.TypeName,
                       HasUnknownDescription = (from o in context.DataObjects
                                                where o.AllFormats.Contains(f) && o.DescriptionText == "Unknown"
                                                select o).Any(),
                       FailureCount = (from ff in context.FailedDataFormats
                                       where ff.DataFormat == f
                                       select ff).Count()
                   };
        }

        private static object GenerateFailedFormatsReport(MultipleClipboardsDataContext context)
        {
            return null;
        }

        private static void StoreDataObject(DataObject dataObject)
        {
            lock (storeDataLock)
            {
                try
                {
                    using (var context = new MultipleClipboardsDataContext())
                    {
                        var givenFormats = dataObject.AllFormats.Select(f => f.Format).ToList();
                        var existingFormats = (from format in context.DataFormats
                                               where givenFormats.Contains(format.Format)
                                               select format).ToList();

                        for (int i = 0; i < dataObject.AllFormats.Count; i++ )
                        {
                            var existing = existingFormats.SingleOrDefault(f => f.Format == dataObject.AllFormats[i].Format);
                            if (existing != null)
                            {
                                dataObject.AllFormats[i] = existing;
                            }
                        }

                        foreach (var format in dataObject.FailedDataFormats)
                        {
                            var existing = existingFormats.SingleOrDefault(f => f.Format == format.DataFormat.Format);
                            if (existing != null)
                            {
                                format.DataFormat = existing;
                            }
                        }

                        context.DataObjects.Add(dataObject);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error persisting data object.", ex);
                }
            }
        }
    }
}