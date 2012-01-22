using System;
using System.Text;

namespace MultipleClipboards.Entities
{
	/// <summary>
	/// Class to represent a unique Hot Key message.
	/// </summary>
	public class HotKeyMessage
	{
		public HotKeyMessage()
		{
			this.MessageTime = DateTime.UtcNow;
		}

		public IntPtr Hwnd
		{
			get;
			set;
		}

		public int Msg
		{
			get;
			set;
		}

		public IntPtr WParam
		{
			get;
			set;
		}

		public IntPtr LParam
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
			private set;
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
		public static bool operator ==(HotKeyMessage lhs, HotKeyMessage rhs)
		{
			if (Object.ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			else if ((object)lhs == null || (object)rhs == null)
			{
				return false;
			}

			return (lhs.Hwnd == rhs.Hwnd &&
					lhs.Msg == rhs.Msg &&
					lhs.WParam == rhs.WParam &&
					lhs.LParam == rhs.LParam &&
					lhs.MessageTime.Day == rhs.MessageTime.Day &&
					lhs.MessageTime.Hour == rhs.MessageTime.Hour &&
					lhs.MessageTime.Minute == rhs.MessageTime.Minute &&
					lhs.MessageTime.Second == rhs.MessageTime.Second);
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
		public static bool operator !=(HotKeyMessage lhs, HotKeyMessage rhs)
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
			if (obj == null || !(obj is HotKeyMessage))
			{
				return false;
			}

			return this == (HotKeyMessage)obj;
		}

		/// <summary>
		/// Gets the Hash Code for the object.
		/// </summary>
		/// <returns>The Hash Code.</returns>
		public override int GetHashCode()
		{
			return
				this.Hwnd.GetHashCode() ^
				this.Msg.GetHashCode() ^
				this.WParam.GetHashCode() ^
				this.LParam.GetHashCode() ^
				this.MessageTime.GetHashCode();
		}

		/// <summary>
		/// Gets the string representation of this HotKeyMessage object.
		/// </summary>
		/// <returns>The string representation of this HotKeyMessage object.</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("Hwnd: 0x{0}\r\n", this.Hwnd.ToString("X8"));
			builder.AppendFormat("Msg: 0x{0}\r\n", this.Msg.ToString("X8"));
			builder.AppendFormat("WParam: 0x{0}\r\n", this.WParam.ToString("X8"));
			builder.AppendFormat("LParam: 0x{0}\r\n", this.LParam.ToString("X8"));
			builder.AppendFormat("Message Time: {0}", this.MessageTime.ToString("MM-dd-yyyy hh:mm:ss.fff"));
			return builder.ToString();
		}
	}
}
