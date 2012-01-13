using System;
using System.Windows.Input;

namespace MultipleClipboards.Entities
{
	/// <summary>
	/// The Hot Key Types.
	/// </summary>
	public enum HotKeyType
	{
		Undefined = 0,
		Cut = 1,
		Copy = 2,
		Paste = 3
	}

	public static class HotKeyTypeExtensions
	{
		private const string CutSendKeyCode = "^(x)";
		private const string CopySendKeyCode = "^(c)";
		private const string PasteSendKeyCode = "^(v)";

		/// <summary>
		/// Converts this HotKeyType instance to the correct code used to pass into the SendKeys.Send() / SendWait() methods.
		/// </summary>
		/// <param name="hotKeyType"></param>
		/// <returns></returns>
		public static string ToSendKeysCode(this HotKeyType hotKeyType)
		{
			switch (hotKeyType)
			{
				case HotKeyType.Cut:
					return CutSendKeyCode;

				case HotKeyType.Copy:
					return CopySendKeyCode;

				case HotKeyType.Paste:
					return PasteSendKeyCode;

				default:
					throw new InvalidOperationException(string.Format("Unknown HotKeyType: '{0}'.", hotKeyType));
			}
		}
	}
}