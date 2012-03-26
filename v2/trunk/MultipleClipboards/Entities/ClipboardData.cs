using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using MultipleClipboards.GlobalResources;
using MultipleClipboards.Presentation.Icons;
using QuantumBitDesigns.Core;
using log4net;

namespace MultipleClipboards.Entities
{
	public class ClipboardData
	{
		private const string Tab = "  ";
		private const string UnknownDataPreviewString = "Unknown";
		private const string UnableToRetrieveDataMessage = "Unable to retrieve data in this format.";
		private static readonly ILog log = LogManager.GetLogger(typeof(ClipboardData));
		private static readonly object idLock = new object();
		private static ulong _idCounter;
		private string iconPath;
		private string iconToolTip;

		public ClipboardData(ClipboardData clipboardData)
			: this(clipboardData.DataObject)
		{
		}

		public ClipboardData(IDataObject dataObject)
		{
			this.PreserveDataObject(dataObject);
			this.SetDescriptionData();
			this.TimeStamp = DateTime.Now;

			lock (idLock)
			{
				_idCounter++;
				this.Id = _idCounter;
			}
		}

		public IDataObject DataObject
		{
			get;
			private set;
		}

		public IEnumerable<string> Formats
		{
			get;
			private set;
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

		public string ToShortDisplayString()
		{
			if (this.DataObject == null)
			{
				return string.Empty;
			}

			if (this.Formats.Contains(DataFormats.Text))
			{
				string data = this.DataObject.GetData(DataFormats.Text).ToString();
				return data.Length > 1000 ? data.Substring(0, 1000) : data;
			}
			else if (this.Formats.Count() > 0)
			{
				object data = this.DataObject.GetData(this.Formats.ElementAt(0));
				return data == null ? string.Empty : data.ToString();
			}
			else
			{
				return UnableToRetrieveDataMessage;
			}
		}

		/// <summary>
		/// Returns a long, descriptive string representation of this IDataObject.
		/// </summary>
		/// <returns>A long, descriptive string representation of this IDataObject.</returns>
		public string ToLongDisplayString()
		{
			if (this.DataObject == null)
			{
				return string.Empty;
			}

			StringBuilder displayStringBuilder = new StringBuilder();

			foreach (string format in this.Formats)
			{
				object data = this.DataObject.GetData(format);
				string dataString;

				if (format == DataFormats.WaveAudio || format == DataFormats.Bitmap)
				{
					dataString = this.DataPreview;
				}
				else if (format == DataFormats.FileDrop)
				{
					StringBuilder fileDropBuilder = new StringBuilder();
					IEnumerable<string> filePaths = data as IEnumerable<string>;

					foreach (string filePath in filePaths ?? Enumerable.Empty<string>())
					{
						fileDropBuilder.AppendLine(string.Concat(Tab, filePath));
					}

					dataString = fileDropBuilder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
				}
				else
				{
					dataString = data != null ? data.ToString() : UnableToRetrieveDataMessage;
				}

				displayStringBuilder.Append(string.Format("{1}:{0}{2}{0}{0}", Environment.NewLine, format, dataString));
			}

			return displayStringBuilder.ToString();
		}

		/// <summary>
		/// Clones the given IDataObject instance and preserves it in this ClipboardData instance for future use.
		/// </summary>
		/// <param name="sourceDataObject">The source data object.</param>
		private void PreserveDataObject(IDataObject sourceDataObject)
		{
			if (sourceDataObject == null)
			{
				this.DataObject = null;
				this.Formats = Enumerable.Empty<string>();
				return;
			}

			var allFormats = sourceDataObject.GetFormats().Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
			var validFormats = new List<string>();
			this.DataObject = new DataObject();

			foreach (string format in allFormats)
			{
				try
				{
					var data = sourceDataObject.GetData(format);

					if (data == null)
					{
						continue;
					}

					this.DataObject.SetData(format, data);
					validFormats.Add(format);
				}
				catch (SerializationException serializationException)
				{
					// When IDataObjects are used to set the contents of the system clipboard the data is serialized.
					// Since other applications can implment IDataObject, the actual data type might reside in a third party dll that this application does not reference.
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
						"PreserveDataObject(): Unable to preserve the data on the clipboard in the format '{0}'.  This is most likely due to the 127 character bug in the .NET frameworked discuessed here: {1}{2}The following exception was thrown:{2}{3}",
						format,
						"http://stackoverflow.com/questions/9452802/clipboard-behaves-differently-in-net-3-5-and-4-but-why",
						Environment.NewLine,
						comException);
				}
			}

			this.Formats = validFormats;
		}

		private void SetDescriptionData()
		{
			if (this.Formats.Contains(DataFormats.Html))
			{
				this.DataPreview = FormatDataPreviewString(this.DataObject.GetData(DataFormats.Text));
				this.IconType = IconType.Html;
			}
			else if (this.Formats.Contains(DataFormats.Rtf))
			{
				this.DataPreview = FormatDataPreviewString(this.DataObject.GetData(DataFormats.Text));
				this.IconType = IconType.Rtf;
			}
			else if (this.Formats.Contains(DataFormats.WaveAudio))
			{
				object dataObject = this.DataObject.GetData(DataFormats.WaveAudio);
				Stream audioStream = dataObject as Stream;
				this.DataPreview = audioStream != null ? string.Format("Audio stream ({0} bytes)", audioStream.Length) : dataObject.ToString();
				this.IconType = IconType.Audio;
			}
			else if (this.Formats.Contains(DataFormats.Bitmap))
			{
				object dataObject = this.DataObject.GetData(DataFormats.Bitmap);
				Bitmap bitmap = dataObject as Bitmap;
				InteropBitmap interopBitmap = dataObject as InteropBitmap;

				if (interopBitmap != null)
				{
					this.DataPreview = string.Format("Bitmap image - {0}x{1} - {2} DPI", interopBitmap.PixelWidth, interopBitmap.PixelHeight, interopBitmap.DpiX);
				}
				else if (bitmap != null)
				{
					this.DataPreview = string.Format("Bitmap image - Size: {0}x{1} - Format: {2}", bitmap.Width, bitmap.Height, bitmap.PixelFormat);
				}
				else
				{
					this.DataPreview = dataObject.ToString();
				}

				this.IconType = IconType.Image;
			}
			else if (this.Formats.Contains(DataFormats.FileDrop))
			{
				object dataObject = this.DataObject.GetData(DataFormats.FileDrop);
				IEnumerable<string> filePaths = dataObject as IEnumerable<string>;
				string data = string.Join(", ", filePaths ?? Enumerable.Empty<string>());
				this.DataPreview = FormatDataPreviewString(data);
				this.IconType = IconType.FileDrop;
			}
			else if (this.Formats.Contains(DataFormats.Text))
			{
				this.DataPreview = FormatDataPreviewString(this.DataObject.GetData(DataFormats.Text));
				this.IconType = IconType.Text;
			}
			else
			{
				this.DataPreview = FormatDataPreviewString(UnknownDataPreviewString);
				this.IconType = IconType.Unknown;
			}
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
