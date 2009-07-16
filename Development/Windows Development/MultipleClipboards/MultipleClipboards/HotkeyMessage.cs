using System;

namespace MultipleClipboards
{
	class HotkeyMessage
	{
		public string MessageString { get; set; }
		public DateTime MessageTime { get; set; }

		public HotkeyMessage()
		{
			MessageString = "";
			MessageTime = DateTime.Now;
		}

		public HotkeyMessage(string messageString, DateTime messageTime)
		{
			MessageString = messageString;
			MessageTime = messageTime;
		}

		// A HotkeyMessage is equal to another if the message string is identical and the message time is the same, disregarding milliseconds
		public static bool operator ==(HotkeyMessage lhs, HotkeyMessage rhs)
		{
			if (lhs.MessageString == rhs.MessageString)
			{
				return (lhs.MessageTime.Day == rhs.MessageTime.Day &&
						lhs.MessageTime.Hour == rhs.MessageTime.Hour &&
						lhs.MessageTime.Minute == rhs.MessageTime.Minute &&
						lhs.MessageTime.Second == rhs.MessageTime.Second);
			}
			return false;
		}

		public static bool operator !=(HotkeyMessage lhs, HotkeyMessage rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is HotkeyMessage))
				return false;

			return this == (HotkeyMessage)obj;
		}

		public override int GetHashCode()
		{
			return MessageString.GetHashCode() + MessageTime.GetHashCode();
		}
	}
}
