using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		FileDrop,
		Find,
		Gear,
		History,
		Html,
		Image,
		Paste,
		Preferences,
		Refresh,
		Rtf,
		Text,
		Unknown
	}

	public enum IconSize
	{
		// ReSharper disable InconsistentNaming
		_16 = 16,
		_32 = 32,
		_48 = 48
		// ReSharper restore InconsistentNaming
	}

	public sealed class IconFactory
	{
		private const string IconPathFormatString = "/Presentation/Icons/{0}/{1}.{2}";
		private const string IconExtension = "png";

		private static readonly IDictionary<IconType, string> ToolTipByIcon = new Dictionary<IconType, string>
		{
			{ IconType.About, "About" },
			{ IconType.Add, "Add" },
			{ IconType.Audio, "Audio stream" },
			{ IconType.Clipboard, "Clipboard" },
			{ IconType.Delete, "Delete" },
			{ IconType.Error, "Error" },
			{ IconType.FileDrop, "A list of files that have been placed on the clipboard" },
			{ IconType.Find, "Find" },
			{ IconType.Gear, "Settings" },
			{ IconType.History, "Clipboard history" },
			{ IconType.Html, "Html text" },
			{ IconType.Image, "Bitmap image" },
			{ IconType.Paste, "Paste" },
			{ IconType.Preferences, "Preferences" },
			{ IconType.Refresh, "Refresh" },
			{ IconType.Rtf, "Rich text" },
			{ IconType.Text, "Plain text" },
			{ IconType.Unknown, "Unknown" }
		};

		public static string GetIcon16(IconType icon)
		{
			return GetIcon(icon, IconSize._16);
		}

		public static string GetIcon32(IconType icon)
		{
			return GetIcon(icon, IconSize._32);
		}

		public static string GetIcon48(IconType icon)
		{
			return GetIcon(icon, IconSize._48);
		}

		public static string GetIcon(IconType icon, IconSize size)
		{
			return string.Format(IconPathFormatString, (int) size, icon, IconExtension);
		}

		public static string GetToolTip(IconType icon)
		{
			return ToolTipByIcon[icon];
		}
	}
}
