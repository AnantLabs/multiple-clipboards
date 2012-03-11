using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace MultipleClipboards.Presentation.Icons
{
	public enum IconType
	{
		About,
		Add,
		Audio,
		Clipboard,
		Delete,
		Error,
		Exit,
		FileDrop,
		Find,
		Gear,
		History,
		Html,
		Image,
		Info,
		Log,
		Paste,
		Preferences,
		Refresh,
		Rtf,
		Success,
		Text,
		Warning,
		Unknown
	}

	public enum IconSize
	{
		// ReSharper disable InconsistentNaming
		_16 = 16,
		_32 = 32
		// ReSharper restore InconsistentNaming
	}

	public sealed class IconFactory
	{
		private const string IconPathFormatString = "/Presentation/Icons/{0}/{1}.{2}";
		private const string EmbeddedIconPathFormatString = "MultipleClipboards.Presentation.Icons.TrayContextMenu.{0}.{1}";
		private const string IconExtension = "png";

		private static readonly IDictionary<IconType, string> toolTipByIcon = new Dictionary<IconType, string>
		{
			{ IconType.About, "About" },
			{ IconType.Add, "Add" },
			{ IconType.Audio, "Audio stream" },
			{ IconType.Clipboard, "Clipboard" },
			{ IconType.Delete, "Delete" },
			{ IconType.Exit, "Exit" },
			{ IconType.FileDrop, "A list of files that have been placed on the clipboard" },
			{ IconType.Find, "Find" },
			{ IconType.Gear, "Settings" },
			{ IconType.History, "Clipboard history" },
			{ IconType.Html, "Html text" },
			{ IconType.Image, "Bitmap image" },
			{ IconType.Info, "Info" },
			{ IconType.Log, "Application Log" },
			{ IconType.Paste, "Paste" },
			{ IconType.Preferences, "Preferences" },
			{ IconType.Refresh, "Refresh" },
			{ IconType.Rtf, "Rich text" },
			{ IconType.Success, "Success" },
			{ IconType.Text, "Plain text" },
			{ IconType.Warning, "Warning" },
			{ IconType.Unknown, "Unknown" }
		};

		private static readonly IDictionary<IconType, string> iconFileNameOverridesByType = new Dictionary<IconType, string>
		{
			{ IconType.Info, "About" }
		};

		public static string GetIconPath16(IconType icon)
		{
			return GetIconPath(icon, IconSize._16);
		}

		public static string GetIconPath32(IconType icon)
		{
			return GetIconPath(icon, IconSize._32);
		}

		public static string GetIconPath(IconType icon, IconSize size)
		{
			return string.Format(IconPathFormatString, (int)size, GetIconFileName(icon), IconExtension);
		}

		public static Bitmap GetTrayContextMenuBitmap(IconType icon)
		{
			var fullyQualifiedIconName = string.Format(EmbeddedIconPathFormatString, GetIconFileName(icon), IconExtension);
			var iconStream = Assembly.GetEntryAssembly().GetManifestResourceStream(fullyQualifiedIconName);

			if (iconStream == null)
			{
				throw new NullReferenceException(string.Format("Unable to find the image '{0}' embedded in the executing assembly.", fullyQualifiedIconName));
			}

			return new Bitmap(iconStream);
		}

		public static string GetToolTip(IconType icon)
		{
			return toolTipByIcon[icon];
		}

		private static string GetIconFileName(IconType iconType)
		{
			return iconFileNameOverridesByType.ContainsKey(iconType)
				? iconFileNameOverridesByType[iconType]
				: iconType.ToString();
		}
	}
}
