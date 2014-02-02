using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultipleClipboards.GlobalResources;
using Newtonsoft.Json;
using log4net;
using EntityFramework.Extensions;

namespace MultipleClipboards.Persistence
{
    public enum ReportType
    {
        LastWeekDetails,
        DataFormats,
        FailedDataFormats
    }

    public class MultipleClipboardsDataRepository
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MultipleClipboardsDataRepository));
        private readonly MultipleClipboardsDataContext context;

        public MultipleClipboardsDataRepository()
        {
            context = DbContextScope<MultipleClipboardsDataContext>.AmbientDbContext;
            if (context == null)
            {
                throw new NullReferenceException("An attempt was made to construct a new MultipleClipboardsDataRepository instance, but there is no ambient data context.");
            }
        }

        public MultipleClipboardsDataRepository(MultipleClipboardsDataContext context)
        {
            this.context = context;
        }

        public IEnumerable<string> GetDataFormatBlacklist()
        {
            return context.BlacklistedFormats.Select(f => f.Format).ToList();
        }

        public void AddFormatToBlacklist(string format, bool isLocked = false)
        {
            context.BlacklistedFormats.Add(new DataFormatBlacklist(format, isLocked));
        }

        public void PurgeData()
        {
            context.DataObjects.Delete();
            context.FailedDataFormats.Delete();
            context.DataFormats.Delete();
            context.BlacklistedFormats.Delete(f => !f.IsLocked);
        }

        public void StoreDataObject(DataObject dataObject)
        {
            try
            {
                var givenFormats = dataObject.AllFormats.Select(f => f.Format).ToList();
                var existingFormats = (from format in context.DataFormats
                                       where givenFormats.Contains(format.Format)
                                       select format).ToList();

                for (int i = 0; i < dataObject.AllFormats.Count; i++)
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
            catch (Exception ex)
            {
                log.Error("Error persisting data object.", ex);
            }
        }

        public string GenerateReport(ReportType reportType)
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

        private static object GenerateDetailedReport(MultipleClipboardsDataContext context)
        {
            // Select out a new object so we don't see the EF-generated proxy class junk
            var startTime = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            return from o in context.DataObjects
                   where o.Timestamp >= startTime
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
            var query = from ff in context.FailedDataFormats
                        group ff by ff.DataFormat
                        into formatGroups
                        select new
                        {
                            Format = formatGroups.Key.Format,
                            TypeName = formatGroups.Key.TypeName,
                            FailureCount = formatGroups.Count(),
                            ExceptionInfo = from e in formatGroups
                                            group e by e.ExceptionType
                                            into exceptionGroups
                                            select new
                                            {
                                                TypeName = exceptionGroups.Key,
                                                ExceptionJsonCollection = exceptionGroups.Select(e => e.ExceptionJson).Distinct()
                                            }
                        };

            var reportData = new List<object>();
            foreach (var row in query.ToList())
            {
                var exceptions = new List<SimpleException>();
                foreach (var exceptionInfo in row.ExceptionInfo)
                {
                    try
                    {
                        exceptions.AddRange(exceptionInfo.ExceptionJsonCollection.Select(
                            json =>
                            {
                                var exception = JsonConvert.DeserializeObject<SimpleException>(json);
                                exception.OriginalExceptionType = exceptionInfo.TypeName;
                                return exception;
                            }));
                    }
                    catch (Exception)
                    {
                        exceptions.Add(
                            new SimpleException(
                                "This is NOT the actual exception that was thrown by the application. See the ExceptionJson property for the serialized exception that was thrown by the application.",
                                exceptionInfo.TypeName,
                                exceptionInfo.ExceptionJsonCollection.FirstOrDefault()));
                    }
                }

                reportData.Add(new
                {
                    Format = row.Format,
                    TypeName = row.TypeName,
                    FailureCount = row.FailureCount,
                    Exceptions = exceptions
                });
            }

            return reportData;
        }

        private class SimpleException
        {
            public SimpleException()
            {
            }

            public SimpleException(string message, string originalExceptionType, string originalExceptionJson)
            {
                Message = message;
                StackTraceString = originalExceptionJson;
                OriginalExceptionType = originalExceptionType;
            }

            public string Message { get; set; }

            public string StackTraceString { get; set; }

            public Exception InnerException { get; set; }

            public string OriginalExceptionType { get; set; }
        }
    }
}