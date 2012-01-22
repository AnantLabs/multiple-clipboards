using System.Windows.Controls.Primitives;

namespace MultipleClipboards.Presentation.Controls
{
    public class HighlightButton : ToggleButton
    {
		/// <summary>
		/// Gets or sets the corner radius of the button.
		/// </summary>
    	public string CornerRadius
    	{
    		get;
    		set;
    	}

		/// <summary>
		/// Gets or sets the color of the background highlight for this button.
		/// </summary>
    	public string HighlightBackground
    	{
    		get;
    		set;
    	}

		/// <summary>
		/// Gets or sets the color applied to the background when the button is pressed.
		/// </summary>
    	public string PressedBackground
    	{
    		get;
    		set;
    	}
    }
}
