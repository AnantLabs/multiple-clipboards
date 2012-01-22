using System;
using System.Globalization;
using System.Windows.Data;

namespace MultipleClipboards.Presentation.Converters
{
	[ValueConversion(typeof(Enum), typeof(String))]
	public class EnumToStringConverter : IValueConverter
	{
		private const string EnumValueNone = "None";

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value.ToString() == EnumValueNone ? string.Empty : value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}

			if (value.Equals(string.Empty))
			{
				value = EnumValueNone;
			}

			return Enum.Parse(targetType, value.ToString(), true);
		}
	}
}
