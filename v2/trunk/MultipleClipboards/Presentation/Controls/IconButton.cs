using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MultipleClipboards.Presentation.Icons;

namespace MultipleClipboards.Presentation.Controls
{
	public class IconButton : Button
	{
		public IconButton()
		{
			this.IconSize = IconSize._16;
			this.Height = 25;
			this.UseLayoutRounding = true;
		}

		public IconType Icon
		{
			get;
			set;
		}

		public IconSize IconSize
		{
			get;
			set;
		}

		public string Text
		{
			get;
			set;
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			StackPanel stackPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(3, 1, 3, 1)
			};

			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.UriSource = new Uri(IconFactory.GetIconPath(this.Icon, this.IconSize), UriKind.Relative);
			bitmap.DecodePixelWidth = (int)this.IconSize;
			bitmap.EndInit();

			Image image = new Image
			{
				Width = (int)this.IconSize,
				Margin = new Thickness(0, 2, 0, 0),
				Source = bitmap,
				VerticalAlignment = VerticalAlignment.Center
			};

			TextBlock textBlock = new TextBlock
			{
				Text = this.Text,
				Margin = new Thickness(5, 1, 0, 1),
				VerticalAlignment = VerticalAlignment.Center
			};

			stackPanel.Children.Add(image);
			stackPanel.Children.Add(textBlock);
			this.Content = stackPanel;
		}
	}
}
