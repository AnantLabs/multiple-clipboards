using System;
using log4net;

namespace MultipleClipboards.Presentation.Commands
{
	public class ApplicationCommand : CommandExtension<ApplicationCommand>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ApplicationCommand));

		public ApplicationCommand()
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
				log.Error("An ApplicationCommand was invoked with a null parameter.  This application should not allow that.");
				return;
			}

			switch (parameter.ToString())
			{
				case "Open":
					AppController.ShowMainWindow();
					break;

				case "Shutdown":
					AppController.Shutdown();
					break;

				default:
					throw new ArgumentException(string.Format("Invalid application command recived: '{0}'", parameter));
			}
		}
	}
}
