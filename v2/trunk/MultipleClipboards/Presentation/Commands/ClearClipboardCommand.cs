using System;
using System.Diagnostics;
using log4net;

namespace MultipleClipboards.Presentation.Commands
{
	public class ClearClipboardCommand : CommandExtension<ClearClipboardCommand>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ClearClipboardCommand));

		public ClearClipboardCommand()
		{
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public override void Execute(object parameter)
		{
			var arguments = parameter as ClearClipboardCommandArguments;

			if (arguments == null)
			{
				log.ErrorFormat(
					"Unable to clear the contents of a clipboard because the arguments given to the command were not of type ClearClipboardCommandArguments.{0}{1}",
					Environment.NewLine,
					new StackTrace(true));
				return;
			}

			AppController.ClipboardManager.ClearClipboardContents(arguments.ClipboardId, isCalledFromUi: true);

			if (arguments.Callback != null)
			{
				arguments.Callback();
			}
		}
	}

	public class ClearClipboardCommandArguments
	{
		public ClearClipboardCommandArguments(int clipboardId, Action callback)
		{
			this.ClipboardId = clipboardId;
			this.Callback = callback;
		}

		public int ClipboardId
		{
			get;
			set;
		}

		public Action Callback
		{
			get;
			set;
		}
	}
}
