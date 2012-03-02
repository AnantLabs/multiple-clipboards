using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultipleClipboards.Messaging
{
	public class MessageBus
	{
		private static MessageBus _messageBus;
		private static readonly object listenersLock = new object();
		private readonly IDictionary<string, IList<MessageAction>> listenersByType;

		public static MessageBus Instance
		{
			get
			{
				return _messageBus ?? (_messageBus = new MessageBus());
			}
		}

		public MessageBus()
		{
			lock (listenersLock)
			{
				this.listenersByType = new Dictionary<string, IList<MessageAction>>();
			}
		}

		public void Subscribe<TMessage>(Action<TMessage> listener)
			where TMessage : class
		{
			Subscribe(listener, true);
		}

		public void Subscribe<TMessage>(Action<TMessage> listener, bool executeOnMainThread)
			where TMessage : class
		{
			string listenerTypeName = typeof(TMessage).FullName ?? "unknown";

			lock (listenersLock)
			{
				if (!this.listenersByType.ContainsKey(listenerTypeName))
				{
					this.listenersByType.Add(listenerTypeName, new List<MessageAction>());
				}

				var messageAction = new MessageAction
				{
					Action = o => listener((TMessage)o),
					ExecuteOnMainThread = executeOnMainThread
				};
				this.listenersByType[listenerTypeName].Add(messageAction);
			}
		}

		public void Publish<TMessage>(TMessage message)
			where TMessage : class
		{
			string listenerTypeName = typeof(TMessage).FullName ?? "unknown";
			IList<MessageAction> listeners;

			lock (listenersLock)
			{
				if (!this.listenersByType.ContainsKey(listenerTypeName))
				{
					return;
				}

				listeners = this.listenersByType[listenerTypeName];
			}

			Parallel.ForEach(listeners.Where(o => !o.ExecuteOnMainThread), action => action.Action(message));

			foreach (var action in listeners.Where(o => o.ExecuteOnMainThread))
			{
				action.Action(message);
			}
		}

		private class MessageAction
		{
			public Action<object> Action
			{
				get;
				set;
			}

			public bool ExecuteOnMainThread
			{
				get;
				set;
			}
		}
	}
}
