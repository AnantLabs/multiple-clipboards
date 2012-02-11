using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultipleClipboards.Presentation.Controls
{
	/// <summary>
	/// Provides masking behavior for any <see cref="TextBox"/>.
	/// </summary>
	/// <remarks>
	/// Code courtesy of Kent Boogaart.
	/// http://stackoverflow.com/a/1103822/1048767
	/// </remarks>
	public static class Masking
	{
		private static readonly DependencyPropertyKey maskExpressionPropertyKey = DependencyProperty.RegisterAttachedReadOnly("MaskExpression",
				typeof(Regex),
				typeof(Masking),
				new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the Mask dependency property.
		/// </summary>
		public static readonly DependencyProperty MaskProperty = DependencyProperty.RegisterAttached("Mask",
				typeof(string),
				typeof(Masking),
				new FrameworkPropertyMetadata(OnMaskChanged));

		/// <summary>
		/// Identifies the MaskExpression dependency property.
		/// </summary>
		public static readonly DependencyProperty MaskExpressionProperty = maskExpressionPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the mask for a given <see cref="TextBox"/>.
		/// </summary>
		/// <param name="textBox">
		/// The <see cref="TextBox"/> whose mask is to be retrieved.
		/// </param>
		/// <returns>
		/// The mask, or <see langword="null"/> if no mask has been set.
		/// </returns>
		public static string GetMask(TextBox textBox)
		{
			if (textBox == null)
			{
				throw new ArgumentNullException("textBox");
			}

			return textBox.GetValue(MaskProperty) as string;
		}

		/// <summary>
		/// Sets the mask for a given <see cref="TextBox"/>.
		/// </summary>
		/// <param name="textBox">
		/// The <see cref="TextBox"/> whose mask is to be set.
		/// </param>
		/// <param name="mask">
		/// The mask to set, or <see langword="null"/> to remove any existing mask from <paramref name="textBox"/>.
		/// </param>
		public static void SetMask(TextBox textBox, string mask)
		{
			if (textBox == null)
			{
				throw new ArgumentNullException("textBox");
			}

			textBox.SetValue(MaskProperty, mask);
		}

		/// <summary>
		/// Gets the mask expression for the <see cref="TextBox"/>.
		/// </summary>
		/// <remarks>
		/// This method can be used to retrieve the actual <see cref="Regex"/> instance created as a result of setting the mask on a <see cref="TextBox"/>.
		/// </remarks>
		/// <param name="textBox">
		/// The <see cref="TextBox"/> whose mask expression is to be retrieved.
		/// </param>
		/// <returns>
		/// The mask expression as an instance of <see cref="Regex"/>, or <see langword="null"/> if no mask has been applied to <paramref name="textBox"/>.
		/// </returns>
		public static Regex GetMaskExpression(TextBox textBox)
		{
			if (textBox == null)
			{
				throw new ArgumentNullException("textBox");
			}

			return textBox.GetValue(MaskExpressionProperty) as Regex;
		}

		private static void SetMaskExpression(TextBox textBox, Regex regex)
		{
			textBox.SetValue(maskExpressionPropertyKey, regex);
		}

		private static void OnMaskChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var textBox = dependencyObject as TextBox;
			var mask = e.NewValue as string;
			textBox.PreviewTextInput -= TextBoxPreviewTextInput;
			textBox.PreviewKeyDown -= TextBoxPreviewKeyDown;
			DataObject.RemovePastingHandler(textBox, Pasting);

			if (mask == null)
			{
				textBox.ClearValue(MaskProperty);
				textBox.ClearValue(MaskExpressionProperty);
			}
			else
			{
				textBox.SetValue(MaskProperty, mask);
				SetMaskExpression(textBox, new Regex(mask, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace));
				textBox.PreviewTextInput += TextBoxPreviewTextInput;
				textBox.PreviewKeyDown += TextBoxPreviewKeyDown;
				DataObject.AddPastingHandler(textBox, Pasting);
			}
		}

		private static void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			var textBox = sender as TextBox;
			var maskExpression = GetMaskExpression(textBox);

			if (maskExpression == null)
			{
				return;
			}

			var proposedText = GetProposedText(textBox, e.Text);

			if (!maskExpression.IsMatch(proposedText))
			{
				e.Handled = true;
			}
		}

		private static void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
		{
			var textBox = sender as TextBox;
			var maskExpression = GetMaskExpression(textBox);

			if (maskExpression == null)
			{
				return;
			}

			// Pressing space doesn't raise PreviewTextInput - no idea why, but we need to handle explicitly here.
			if (e.Key == Key.Space)
			{
				var proposedText = GetProposedText(textBox, " ");

				if (!maskExpression.IsMatch(proposedText))
				{
					e.Handled = true;
				}
			}
		}

		private static void Pasting(object sender, DataObjectPastingEventArgs e)
		{
			var textBox = sender as TextBox;
			var maskExpression = GetMaskExpression(textBox);

			if (maskExpression == null)
			{
				return;
			}

			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				var pastedText = e.DataObject.GetData(typeof(string)) as string;
				var proposedText = GetProposedText(textBox, pastedText);

				if (!maskExpression.IsMatch(proposedText))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}

		private static string GetProposedText(TextBox textBox, string newText)
		{
			var text = textBox.Text;

			if (textBox.SelectionStart != -1)
			{
				text = text.Remove(textBox.SelectionStart, textBox.SelectionLength);
			}

			text = text.Insert(textBox.CaretIndex, newText);

			return text;
		}
	}
}
