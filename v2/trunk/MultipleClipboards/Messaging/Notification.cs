using System.Windows.Media;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.Messaging
{
	public class Notification
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
}
