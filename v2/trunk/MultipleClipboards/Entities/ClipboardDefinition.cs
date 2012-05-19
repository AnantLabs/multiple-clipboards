using System;
using System.Text;
using System.Windows.Input;

namespace MultipleClipboards.Entities
{
	[Serializable]
	public class ClipboardDefinition : IEquatable<ClipboardDefinition>
	{
		public const int SystemClipboardId = -1;
		private const string ClipboardDisplayStringFormat = "{0}{1}+{2}";

		public static ClipboardDefinition SystemClipboardDefinition = new ClipboardDefinition
		{
			ClipboardId = SystemClipboardId,
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

		public override string ToString()
		{
			if (!string.IsNullOrWhiteSpace(this.DisplayStringOverride))
			{
				return this.DisplayStringOverride;
			}

			string modifierTwoKeyString = this.ModifierTwoKey == ModifierKeys.None ? string.Empty : "+" + this.ModifierTwoKey;
			return string.Format(ClipboardDisplayStringFormat, this.ModifierOneKey, modifierTwoKeyString, this.CopyKey);
		}

		public static bool operator ==(ClipboardDefinition left, ClipboardDefinition right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}
			else if ((object)left == null || (object)right == null)
			{
				return false;
			}

			return
				left.ModifierOneKey == right.ModifierOneKey &&
				left.ModifierTwoKey == right.ModifierTwoKey &&
				left.CopyKey == right.CopyKey &&
				left.CutKey == right.CutKey &&
				left.PasteKey == right.PasteKey;
		}

		public static bool operator !=(ClipboardDefinition left, ClipboardDefinition right)
		{
			return !(left == right);
		}

		public override int GetHashCode()
		{
			return
				this.ClipboardId.GetHashCode() ^
				this.ModifierOneKey.GetHashCode() ^
				this.ModifierTwoKey.GetHashCode() ^
				this.CopyKey.GetHashCode() ^
				this.CutKey.GetHashCode() ^
				this.PasteKey.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ClipboardDefinition))
			{
				return false;
			}

			return this == (ClipboardDefinition)obj;
		}

		public bool Equals(ClipboardDefinition other)
		{
			return this == other;
		}

		public string ToLogString()
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
