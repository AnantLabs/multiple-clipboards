using System;
using System.Text;
using System.Windows.Input;

namespace MultipleClipboards.Entities
{
	[Serializable]
	public class ClipboardDefinition
	{
		private const string ClipboardDisplayStringFormat = "{0}{1}+{2}";

		public static ClipboardDefinition SystemClipboardDefinition = new ClipboardDefinition
		{
			ClipboardId = -1,
			ModifierOneKey = ModifierKeys.Control,
			ModifierTwoKey = ModifierKeys.None,
			CutKey = Key.X,
			CopyKey = Key.C,
			PasteKey = Key.V,
			DisplayStringOverride = "System Clipboard"
		};

		public int ClipboardId
		{
			get;
			set;
		}

		public ModifierKeys ModifierOneKey
		{
			get;
			set;
		}

		public ModifierKeys ModifierTwoKey
		{
			get;
			set;
		}

		public Key CopyKey
		{
			get;
			set;
		}

		public Key CutKey
		{
			get;
			set;
		}

		public Key PasteKey
		{
			get;
			set;
		}

		private string DisplayStringOverride
		{
			get;
			set;
		}

		public string ToDisplayString()
		{
			if (!string.IsNullOrWhiteSpace(this.DisplayStringOverride))
			{
				return this.DisplayStringOverride;
			}

			string modifierTwoKeyString = this.ModifierTwoKey == ModifierKeys.None ? string.Empty : "+" + this.ModifierTwoKey;
			return string.Format(ClipboardDisplayStringFormat, this.ModifierOneKey, modifierTwoKeyString, this.CopyKey);
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("Clipboard ID: {0}\r\n", this.ClipboardId);
			builder.AppendFormat("Modifier One Key: {0}\r\n", this.ModifierOneKey);
			builder.AppendFormat("Modifier Two Key: {0}\r\n", this.ModifierTwoKey);
			builder.AppendFormat("Copy Key: {0}\r\n", this.CopyKey);
			builder.AppendFormat("Cut Key: {0}\r\n", this.CutKey);
			builder.AppendFormat("Paste Key: {0}", this.PasteKey);
			return builder.ToString();
		}
	}
}
