using System;
using MultipleClipboards.Entities;

namespace MultipleClipboards.ClipboardManagement
{
	public class AsyncClipboardOperationArguments
	{
		public Action Callback
		{
			get;
			set;
		}
	}

	public class ProcessHotKeyArguments : AsyncClipboardOperationArguments
	{
		public HotKey HotKey
		{
			get;
			set;
		}
	}
}
