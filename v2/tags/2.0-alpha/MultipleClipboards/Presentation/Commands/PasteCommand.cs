using MultipleClipboards.Entities;
using MultipleClipboards.Persistence;

namespace MultipleClipboards.Presentation.Commands
{
	public class PasteCommand : CommandExtension<PasteCommand>
	{
		public PasteCommand()
		{
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public override void Execute(object parameter)
		{
			if (parameter == null)
			{
				LogManager.Error("A PasteCommand was invoked with a null parameter.  This application should not allow that.");
				return;
			}

			ulong clipboardDataId;
			if (ulong.TryParse(parameter.ToString(), out clipboardDataId))
			{
				AppController.ClipboardManager.PlaceHistoricalEntryOnClipboard(ClipboardDefinition.SystemClipboardId, clipboardDataId);
			}
			else
			{
				LogManager.ErrorFormat("A PasteCommand was invoked with a parameter that was not a valid clipboard data ID.  The given parameter was: '{0}'.", parameter);
			}
		}
	}
}
