using System;
using System.Text;
using MultipleClipboards.Interop;

namespace MultipleClipboards.Entities
{
	/// <summary>
	/// Enumeration that defines the Windows message types that this application is concerned with.
	/// </summary>
	public enum WindowsMessageType
	{
		/// <summary>
		/// An unknown message type.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// A HotKey message.  Sent when the user presses a hot key combination that has been registered with this application.
		/// </summary>
		HotKey = Win32API.WM_HOTKEY,

		/// <summary>
		/// A Draw Clipboard message.  Sent when data on the system clipboard has changed.
		/// </summary>
		DrawClipboard = Win32API.WM_DRAWCLIPBOARD,

		/// <summary>
		/// A Change Clipboard Chain message.  Sent when an application is added or removed from the clipboard chain.
		/// </summary>
		ChangeClipboardChain = Win32API.WM_CHANGECBCHAIN
	}

	/// <summary>
	/// Class to represent a unique Hot Key message.
	/// </summary>
	public class WindowsMessage
	{
		private WindowsMessageType? messageType;

		public WindowsMessage()
		{
			this.MessageTime = DateTime.Now;
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
		/// Gets the type of this Windows message.
		/// </summary>
		public WindowsMessageType MessageType
		{
			get
			{
				if (!this.messageType.HasValue)
				{
					WindowsMessageType messageTypeForParse;
					if (!Enum.TryParse(this.Msg.ToString(), out messageTypeForParse))
					{
						messageTypeForParse = WindowsMessageType.Unknown;
					}
					this.messageType = messageTypeForParse;
				}
				return this.messageType.Value;
			}
		}

		/// <summary>
		/// Compares two WindowsMessage objects.
		/// </summary>
		/// <remarks>
		/// A WindowsMessage is equal to another if the message type and handle are identical and the message times are within 500 milliseconds of one another.
		/// </remarks>
		/// <param name="lhs">The WindowsMessage on the left side of the comparison.</param>
		/// <param name="rhs">The WindowsMessage on the right side of the comparison.</param>
		/// <returns>True if the two messages are equal, false if they are not.</returns>
		public static bool operator ==(WindowsMessage lhs, WindowsMessage rhs)
		{
			if (ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			else if ((object)lhs == null || (object)rhs == null)
			{
				return false;
			}

			return (lhs.Hwnd == rhs.Hwnd &&
			        lhs.Msg == rhs.Msg &&
			        lhs.MessageTime.Day == rhs.MessageTime.Day &&
			        lhs.MessageTime.Hour == rhs.MessageTime.Hour &&
			        lhs.MessageTime.Minute == rhs.MessageTime.Minute &&
			        lhs.MessageTime.Subtract(rhs.MessageTime).Duration().TotalMilliseconds <= 500);
		}

		/// <summary>
		/// Compares two WindowsMessage objects.
		/// </summary>
		/// <remarks>
		/// A WindowsMessage is equal to another if the message string is identical and the message time is the same, disregarding milliseconds.
		/// </remarks>
		/// <param name="lhs">The WindowsMessage on the left side of the comparison.</param>
		/// <param name="rhs">The WindowsMessage on the right side of the comparison.</param>
		/// <returns>True if the two messages are not equal, false if they are.</returns>
		public static bool operator !=(WindowsMessage lhs, WindowsMessage rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// Compares a WindowsMessage to another object.
		/// </summary>
		/// <param name="obj">The object to compare the WindowsMessage to.</param>
		/// <returns>True if the two objects are equal, false if they are not.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is WindowsMessage))
			{
				return false;
			}

			return this == (WindowsMessage)obj;
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
		/// Gets the string representation of this WindowsMessage object.
		/// </summary>
		/// <returns>The string representation of this WindowsMessage object.</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("Type: {0}\r\n", this.MessageType);
			builder.AppendFormat("Hwnd: 0x{0}\r\n", this.Hwnd.ToString("X8"));
			builder.AppendFormat("Msg: 0x{0}\r\n", this.Msg.ToString("X8"));
			builder.AppendFormat("WParam: 0x{0}\r\n", this.WParam.ToString("X8"));
			builder.AppendFormat("LParam: 0x{0}\r\n", this.LParam.ToString("X8"));
			builder.AppendFormat("Message Time: {0}", this.MessageTime.ToString("MM-dd-yyyy hh:mm:ss.fff"));
			return builder.ToString();
		}
	}
}
