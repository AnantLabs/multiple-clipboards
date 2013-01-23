using System;
using WindowsInput;

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
		private const string cutSendKeyCode = "^(x)";
		private const string copySendKeyCode = "^(c)";
		private const string pasteSendKeyCode = "^(v)";

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
					return cutSendKeyCode;

				case HotKeyType.Copy:
					return copySendKeyCode;

				case HotKeyType.Paste:
					return pasteSendKeyCode;

				default:
					throw new ArgumentOutOfRangeException("hotKeyType", string.Format("Unknown HotKeyType: '{0}'.", hotKeyType));
			}
		}

        /// <summary>
        /// Simulates the key strokes for this HotKeyType instance.
        /// </summary>
        /// <param name="hotKeyType"></param>
        public static void SimulateHotKeyPress(this HotKeyType hotKeyType)
        {
            switch (hotKeyType)
            {
                case HotKeyType.Cut:
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_X);
                    break;

                case HotKeyType.Copy:
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    break;

                case HotKeyType.Paste:
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("hotKeyType", string.Format("Unknown HotKeyType: '{0}'.", hotKeyType));
            }
        }
	}
}