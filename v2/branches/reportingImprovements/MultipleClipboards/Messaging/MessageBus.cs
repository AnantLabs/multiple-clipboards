using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;

namespace MultipleClipboards.Messaging
{
	public class MessageBus
	{
		private static MessageBus messageBusInstance;
		private static readonly ILog log = LogManager.GetLogger(typeof(MessageBus));
		private readonly ConcurrentDictionary<string, IList<Action<object>>> listenersByType;

		public static MessageBus Instance
		{
			get { return messageBusInstance ?? (messageBusInstance = new MessageBus()); }
		}

		public MessageBus()
		{
			this.listenersByType = new ConcurrentDictionary<string, IList<Action<object>>>();
		}

		public void Subscribe<TMessage>(Action<TMessage> listener)
			where TMessage : class
		{
			var listenerTypeName = typeof(TMessage).FullName;
			var originalCollection = this.listenersByType.GetOrAdd(listenerTypeName, new List<Action<object>>());

			IList<Action<object>> messageActions;
			if (!this.listenersByType.TryGetValue(listenerTypeName, out messageActions))
			{
				log.ErrorFormat("Error subscribing to messages of the type '{0}'.  Unable to get the value out of the concurrent dictionary.", listenerTypeName);
				return;
			}

			messageActions.Add(o => listener((TMessage)o));
			
			if (!this.listenersByType.TryUpdate(listenerTypeName, messageActions, originalCollection))
			{
				log.ErrorFormat("Error subscribing to messages of the type '{0}'.  Unable to update the concurrent dictionary with the new subscription method.", listenerTypeName);
			}
		}

		public void Publish<TMessage>(TMessage message)
			where TMessage : class
		{
			var listenerTypeName = typeof(TMessage).FullName;

			IList<Action<object>> listeners;
			if (!this.listenersByType.TryGetValue(listenerTypeName, out listeners))
			{
				log.ErrorFormat("Error publishing messages of the type '{0}'.  Unable to get the listeners collection out of the concurrent dictionary.", listenerTypeName);
				return;
			}

			if (listeners == null || listeners.Count == 0)
			{
				return;
			}

			foreach (var action in listeners)
			{
                action(message);
			}
		}
	}
}
