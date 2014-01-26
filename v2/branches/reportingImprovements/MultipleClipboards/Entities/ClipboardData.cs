using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using log4net;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.Entities
{
	[Serializable]
	public class ClipboardData : ISerializable
	{
		private const string Tab = "  ";
		private const string UnknownDataPreviewString = "Unknown";
		private const string UnableToRetrieveDataMessage = "Unable to retrieve data in this format.";
		private static readonly ILog log = LogManager.GetLogger(typeof(ClipboardData));
		private static readonly object idLock = new object();
		private static readonly string alternateBitmapFormat = typeof(Bitmap).ToString();
		private static readonly Type dataByFormatType = typeof(Dictionary<string, object>);
		private static ulong _idCounter;
		private string iconPath;
		private string iconToolTip;
		private Func<string> singleFormatDetailedDataStringProducer;

		public ClipboardData(ClipboardData clipboardData)
		{
			this.DataByFormat = clipboardData.DataByFormat.ToDictionary(pair => pair.Key, pair => pair.Value);
			this.Initialize(clipboardData.TimeStamp);
		}

		public ClipboardData(IDataObject dataObject, IEnumerable<string> formats)
		{
			this.PreserveDataObject(dataObject, formats);
			this.Initialize(DateTime.Now);
		}

		private ClipboardData(SerializationInfo info, StreamingContext context)
		{
			this.DataByFormat = (Dictionary<string, object>)info.GetValue("DataByFormat", dataByFormatType);
			this.Initialize(info.GetDateTime("TimeStamp"), false);
		}

		public void Initialize(DateTime timeStamp, bool setDescriptionData = true)
		{
			if (setDescriptionData)
			{
				this.SetDescriptionData();
			}

			this.TimeStamp = timeStamp;

			lock (idLock)
			{
				_idCounter++;
				this.Id = _idCounter;
			}
		}

		public ulong Id
		{
			get;
			private set;
		}

		public DateTime TimeStamp
		{
			get;
			set;
		}

		public string DataPreview
		{
			get;
			private set;
		}

		public IconType IconType
		{
			get;
			private set;
		}

		public string IconPath
		{
			get
			{
				return this.iconPath ?? (iconPath = IconFactory.GetIconPath16(this.IconType));
			}
		}

		public string IconToolTip
		{
			get
			{
				return this.iconToolTip ?? (this.iconToolTip = IconFactory.GetToolTip(this.IconType));
			}
		}

		private Dictionary<string, object> DataByFormat
		{
			get;
			set;
		}

		public bool ContainsData
		{
			get
			{
				return DataByFormat.Any();
			}
		}
		
		public DataObject GetDataObject()
		{
			var dataObject = new DataObject();

			foreach (var format in this.DataByFormat.Keys)
			{
				dataObject.SetData(format, this.DataByFormat[format]);
			}

			return dataObject;
		}

		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			// When serializing clipboard data all we want to preserve is the original time and the collection of data by format.
			// Then, when deserializing, we will re-construct the other properties.
			// IMPORTANT: Some data formats (bitmaps) are not marked as serializable.  We must filter those types out here.
			// NOTE: Attribute.IsDefined isn't good enough here.  For some reason string[] returns false from that method.
			var serializableData = this.DataByFormat
				.Where(pair => pair.Value.GetType().GetCustomAttributesData()
				               	.Any(t => typeof(SerializableAttribute).GetConstructors().Contains(t.Constructor)))
				.ToDictionary(pair => pair.Key, pair => pair.Value);
			
			info.AddValue("TimeStamp", this.TimeStamp);
			info.AddValue("DataByFormat", serializableData, dataByFormatType);
		}

		public string ToLogString()
		{
			if (!this.DataByFormat.Any())
			{
				return string.Empty;
			}

			if (this.DataByFormat.ContainsKey(DataFormats.Text))
			{
				string data = this.DataByFormat[DataFormats.Text].ToString();
				return data.Length > 1000 ? data.Substring(0, 1000) : data;
			}
		    
            if (this.DataByFormat.Keys.Any())
		    {
		        // There is a chance that the data we are trying to retrieve is owned by another thread (bitmaps),
		        // which means calling .ToString() will throw an exception.
		        try
		        {
		            object data = this.DataByFormat[this.DataByFormat.Keys.ElementAt(0)];
		            return data == null ? string.Empty : data.ToString();
		        }
		        catch (InvalidOperationException)
		        {
		            return string.Empty;
		        }
		    }
		    
            return UnableToRetrieveDataMessage;
		}

		/// <summary>
		/// Returns a descriptive string representation of this IDataObject.
		/// </summary>
		/// <returns>A descriptive string representation of this IDataObject.</returns>
		public string ToDisplayString()
		{
			if (!this.DataByFormat.Any())
			{
				return string.Empty;
			}

			if (!AppController.Settings.ShowDetailedClipboardInformation)
			{
				return this.singleFormatDetailedDataStringProducer();
			}

			StringBuilder displayStringBuilder = new StringBuilder();

			foreach (string format in this.DataByFormat.Keys)
			{
				object data = this.DataByFormat[format];
				string dataString;

				if (format == DataFormats.WaveAudio || format == DataFormats.Bitmap || format == alternateBitmapFormat)
				{
					dataString = this.DataPreview;
				}
				else if (format == DataFormats.FileDrop)
				{
					dataString = GetFileDropDisplayString(data);
				}
				else
				{
					if (data == null)
					{
						dataString = UnableToRetrieveDataMessage;
					}
					else
					{
						try
						{
							dataString = data.ToString();
						}
						catch (InvalidOperationException invalidOperationException)
						{
							// This can be thrown when the data we are trying to call ToString() on was created on a different thread.
							// This happens when .NET reference typess are copied from other WPF applications.
							log.WarnFormat("Unable to call ToString() on this data object for the format '{0}'.{1}{2}", format, Environment.NewLine, invalidOperationException);
							dataString = UnknownDataPreviewString;
						}
					}
				}

				displayStringBuilder.Append(string.Format("{1}:{0}{2}{0}{0}", Environment.NewLine, format, dataString));
			}

			return displayStringBuilder.ToString();
		}

		/// <summary>
		/// Clones the given IDataObject instance and preserves it in this ClipboardData instance for future use.
		/// </summary>
		/// <param name="sourceDataObject">The source data object.</param>
		/// <param name="formats">The collection of formats for the source data object.</param>
		private void PreserveDataObject(IDataObject sourceDataObject, IEnumerable<string> formats)
		{
			this.DataByFormat = new Dictionary<string, object>();

			if (sourceDataObject == null)
			{
				return;
			}

			foreach (string format in formats.Where(f => !string.IsNullOrWhiteSpace(f)))
			{
				try
				{
				    var data = sourceDataObject.GetData(format);

				    if (data == null)
				    {
				        continue;
				    }

				    this.DataByFormat.Add(format, data);
				}
				catch (SerializationException serializationException)
				{
				    // When IDataObjects are used to set the contents of the system clipboard the data is serialized.
				    // Since other applications can implement IDataObject, the actual data type might reside in a third party dll that this application does not reference.
				    // Therefore, the clipboard will not be able to serialize it and will throw this exception.
				    // If that happens then just swallow the exception because there is nothing we can do about it.
				    // May as well log something so I have a chance at knowing what formats cause this error.
				    log.InfoFormat(
				        "PreserveDataObject(): Unable to de-serialize data from the clipboard in the format '{0}'.  The following exception was thrown:{1}{2}",
				        format,
				        Environment.NewLine,
				        serializationException);
				}
				catch (COMException comException)
				{
				    // There is a bug in the .NET framework that truncates the full type name of types on the clipboard to 127 characters.
				    // For longer type names (usually generic collections) the public key token gets truncated and the data becomes corrupt.
				    // It seems that Microsoft has fixed this in February 2012 in System.Windows.Forms.Clipboard, but not in System.Windows.Clipboard (WPF).
				    // http://stackoverflow.com/questions/9452802/clipboard-behaves-differently-in-net-3-5-and-4-but-why
				    // https://connect.microsoft.com/VisualStudio/feedback/details/726652/clipboard-truncates-type-name-to-127-characters
				    // Only log a warning here since there is nothing I can do about it in this app.
				    log.WarnFormat(
				        "PreserveDataObject(): Unable to preserve the data on the clipboard in the format '{0}'.  This is most likely due to the 127 character bug in the .NET framework discussed here: {1}{2}The following exception was thrown:{2}{3}",
				        format,
				        "http://stackoverflow.com/questions/9452802/clipboard-behaves-differently-in-net-3-5-and-4-but-why",
				        Environment.NewLine,
				        comException);
				}
				catch (OutOfMemoryException memoryException)
				{
				    log.Warn("PreserveDataObject(): Ran out of memory trying to preserve the data on the clipboard.", memoryException);
				}
			}
		}

		public void SetDescriptionData()
		{
			if (this.DataByFormat.ContainsKey(DataFormats.Html))
			{
				this.SetTextDescriptionData(IconType.Html);
			}
			else if (this.DataByFormat.ContainsKey(DataFormats.Rtf))
			{
				this.SetTextDescriptionData(IconType.Rtf);
			}
			else if (this.DataByFormat.ContainsKey(DataFormats.WaveAudio))
			{
				object dataObject = this.DataByFormat[DataFormats.WaveAudio];
				var audioStream = dataObject as Stream;
				this.DataPreview = audioStream != null ? string.Format("Audio stream ({0} bytes)", audioStream.Length) : dataObject.ToString();
				this.singleFormatDetailedDataStringProducer = () => this.DataPreview;
				this.IconType = IconType.Audio;
			}
			else if (this.DataByFormat.ContainsKey(DataFormats.Bitmap) || this.DataByFormat.ContainsKey(alternateBitmapFormat))
			{
				const string bitmapFormatString = "Bitmap image - {0}x{1} - {2} DPI";
				object dataObject = this.DataByFormat[this.DataByFormat.Keys.First(k => k == DataFormats.Bitmap || k == alternateBitmapFormat)];
				var bitmap = dataObject as Bitmap;
				var interopBitmap = dataObject as InteropBitmap;

				if (interopBitmap != null)
				{
					this.DataPreview = string.Format(bitmapFormatString, interopBitmap.PixelWidth, interopBitmap.PixelHeight, (int)interopBitmap.DpiX);
				}
				else if (bitmap != null)
				{
					this.DataPreview = string.Format(bitmapFormatString, bitmap.Width, bitmap.Height, (int)bitmap.HorizontalResolution);
				}
				else
				{
					this.DataPreview = dataObject.ToString();
				}

				this.singleFormatDetailedDataStringProducer = () => this.DataPreview;
				this.IconType = IconType.Image;
			}
			else if (this.DataByFormat.ContainsKey(DataFormats.FileDrop))
			{
				object dataObject = this.DataByFormat[DataFormats.FileDrop];
				var filePaths = dataObject as IEnumerable<string>;
				string dataString = string.Join(", ", filePaths ?? Enumerable.Empty<string>());
				this.DataPreview = FormatDataPreviewString(dataString);
				this.singleFormatDetailedDataStringProducer = () => string.Format("File Drop List:{0}{1}", Environment.NewLine, GetFileDropDisplayString(dataObject));
				this.IconType = IconType.FileDrop;
			}
			else if (this.DataByFormat.ContainsKey(DataFormats.Text))
			{
				this.SetTextDescriptionData(IconType.Text);
			}
			else
			{
				this.singleFormatDetailedDataStringProducer = () => UnknownDataPreviewString;
				this.DataPreview = FormatDataPreviewString(UnknownDataPreviewString);
				this.IconType = IconType.Unknown;
			}
		}

		private void SetTextDescriptionData(IconType iconType)
		{
			var textData = this.DataByFormat[DataFormats.Text];
			this.singleFormatDetailedDataStringProducer = textData.ToString;
			this.DataPreview = FormatDataPreviewString(textData);
			this.IconType = iconType;
		}

		private static string GetFileDropDisplayString(object data)
		{
			var fileDropBuilder = new StringBuilder();
			var filePaths = data as IEnumerable<string>;

			foreach (string filePath in filePaths ?? Enumerable.Empty<string>())
			{
				fileDropBuilder.AppendLine(string.Concat(Tab, filePath));
			}

			return fileDropBuilder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
		}

		private static string FormatDataPreviewString(object data)
		{
			string dataString = data.ToString().Trim().Replace("\r\n", " ").Replace("\t", " ");
			return dataString.Length > Constants.ClipboardHistoryPreviewLength
				? dataString.Substring(0, Constants.ClipboardHistoryPreviewLength)
				: dataString;
		}
	}
}
