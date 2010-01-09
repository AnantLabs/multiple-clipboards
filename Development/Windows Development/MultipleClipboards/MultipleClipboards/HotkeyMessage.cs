using System;

namespace MultipleClipboards
{
	/// <summary>
	/// Class to represent a unique Hot Key message.
	/// </summary>
	public class HotkeyMessage
	{
		/// <summary>
		/// Constructs a new HotkeyMessage object.
		/// </summary>
		public HotkeyMessage()
		{
			this.MessageString = "";
			this.MessageTime = DateTime.Now;
		}

		/// <summary>
		/// Constructs a new HotkeyMessage object.
		/// </summary>
		/// <param name="messageString">The message string.</param>
		/// <param name="messageTime">The time of the message.</param>
		public HotkeyMessage(string messageString, DateTime messageTime)
		{
			this.MessageString = messageString;
			this.MessageTime = messageTime;
		}

		/// <summary>
		/// Gets or sets the message string.
		/// </summary>
		public string MessageString
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the time of the message.
		/// </summary>
		public DateTime MessageTime
		{
			get;
			set;
		}

		/// <summary>
		/// Compares two HotkeyMessage objects.
		/// </summary>
		/// <remarks>
		/// A HotkeyMessage is equal to another if the message string is identical and the message time is the same, disregarding milliseconds.
		/// </remarks>
		/// <param name="lhs">The HotkeyMessage on the left side of the comparison.</param>
		/// <param name="rhs">The HotkeyMessage on the right side of the comparison.</param>
		/// <returns>True if the two messages are equal, false if they are not.</returns>
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

		/// <summary>
		/// Compares two HotkeyMessage objects.
		/// </summary>
		/// <remarks>
		/// A HotkeyMessage is equal to another if the message string is identical and the message time is the same, disregarding milliseconds.
		/// </remarks>
		/// <param name="lhs">The HotkeyMessage on the left side of the comparison.</param>
		/// <param name="rhs">The HotkeyMessage on the right side of the comparison.</param>
		/// <returns>True if the two messages are not equal, false if they are.</returns>
		public static bool operator !=(HotkeyMessage lhs, HotkeyMessage rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// Compares a HotkeyMessage to another object.
		/// </summary>
		/// <param name="obj">The object to compare the HotkeyMessage to.</param>
		/// <returns>True if the two objects are equal, false if they are not.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is HotkeyMessage))
			{
				return false;
			}

			return this == (HotkeyMessage)obj;
		}

		/// <summary>
		/// Gets the Hash Code for the object.
		/// </summary>
		/// <returns>The Hash Code.</returns>
		public override int GetHashCode()
		{
			return this.MessageString.GetHashCode() + this.MessageTime.GetHashCode();
		}
	}
}
