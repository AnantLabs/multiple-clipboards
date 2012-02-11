using System;
using MultipleClipboards.Entities;

namespace MultipleClipboards.ClipboardManagement
{
	public class ProcessHotKeyArguments
	{
		public HotKey HotKey
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
