using System.Windows.Media;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.Messaging
{
	public abstract class NotificationBase
	{
		public string MessageBody
		{
			get;
			set;
		}

		public IconType IconType
		{
			get;
			set;
		}

		public Brush BorderBrush
		{
			get;
			set;
		}
	}

	public class MainWindowNotification : NotificationBase
	{
	}

	public class TrayNotification : NotificationBase
	{
	}
}
