using System;
using System.Windows;
using System.Windows.Data;

namespace MultipleClipboards.Presentation.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				return (bool)value ? Visibility.Visible : Visibility.Collapsed;
			}
			catch
			{
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				return (Visibility)value == Visibility.Visible;
			}
			catch
			{
				return false;
			}
		}
	}
}
