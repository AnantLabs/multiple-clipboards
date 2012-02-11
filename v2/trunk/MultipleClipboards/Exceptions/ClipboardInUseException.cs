using System;

namespace MultipleClipboards.Exceptions
{
	public class ClipboardInUseException : Exception
	{
		public ClipboardInUseException()
		{
		}

		public ClipboardInUseException(string message)
			: base(message)
		{
		}

		public ClipboardInUseException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
