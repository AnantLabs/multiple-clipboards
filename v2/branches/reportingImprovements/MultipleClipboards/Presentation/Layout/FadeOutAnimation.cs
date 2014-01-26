using System;
using System.Collections;
using System.Windows;
using System.Windows.Media.Animation;

namespace MultipleClipboards.Presentation.Layout
{
	public class FadeOutAnimation : DependencyObject
	{
		private const int DurationMs = 500;

		private static readonly Hashtable hookedElements = new Hashtable();

		public static readonly DependencyProperty IsActiveProperty =
		  DependencyProperty.RegisterAttached("IsActive",
		  typeof(bool),
		  typeof(FadeOutAnimation),
		  new FrameworkPropertyMetadata(false, OnIsActivePropertyChanged));

		public static bool GetIsActive(UIElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}

			return (bool)element.GetValue(IsActiveProperty);
		}

		public static void SetIsActive(UIElement element, bool value)
		{
			if (element == null)
			{
				throw new ArgumentNullException("element");
			}
			element.SetValue(IsActiveProperty, value);
		}

		static FadeOutAnimation()
		{
			UIElement.VisibilityProperty.AddOwner(typeof(FrameworkElement), new FrameworkPropertyMetadata(Visibility.Visible, VisibilityChanged, CoerceVisibility));
		}

		private static void VisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		private static void OnIsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var fe = d as FrameworkElement;
			if (fe == null)
			{
				return;
			}
			if (GetIsActive(fe))
			{
				HookVisibilityChanges(fe);
			}
			else
			{
				UnHookVisibilityChanges(fe);
			}
		}

		private static void UnHookVisibilityChanges(FrameworkElement fe)
		{
			if (hookedElements.Contains(fe))
			{
				hookedElements.Remove(fe);
			}
		}

		private static void HookVisibilityChanges(FrameworkElement fe)
		{
			hookedElements.Add(fe, false);
		}

		private static object CoerceVisibility(DependencyObject d, object baseValue)
		{
			var fe = d as FrameworkElement;
			if (fe == null)
			{
				return baseValue;
			}

			if (CheckAndUpdateAnimationStartedFlag(fe))
			{
				return baseValue;
			}
			// If we get here, it means we have to start fade in or fade out
			// animation. In any case return value of this method will be
			// Visibility.Visible. 

			var visibility = (Visibility)baseValue;

			var da = new DoubleAnimation
			{
				Duration = new Duration(TimeSpan.FromMilliseconds(DurationMs))
			};

			da.Completed += (o, e) =>
			{
				// This will trigger value coercion again
				// but CheckAndUpdateAnimationStartedFlag() function will reture true
				// this time, and animation will not be triggered.
				fe.Visibility = visibility;
				// NB: Small problem here. This may and probably will brake 
				// binding to visibility property.
			};

			if (visibility == Visibility.Collapsed || visibility == Visibility.Hidden)
			{
				da.From = 0.9;
				da.To = 0.0;
			}
			else
			{
				da.From = 0.0;
				da.To = 0.9;
			}

			fe.BeginAnimation(UIElement.OpacityProperty, da);
			return Visibility.Visible;
		}

		private static bool CheckAndUpdateAnimationStartedFlag(FrameworkElement fe)
		{
			var hookedElement = hookedElements.Contains(fe);
			if (!hookedElement)
			{
				return true; // don't need to animate unhooked elements.
			}

			var animationStarted = (bool)hookedElements[fe];
			hookedElements[fe] = !animationStarted;

			return animationStarted;
		}
	}
}
