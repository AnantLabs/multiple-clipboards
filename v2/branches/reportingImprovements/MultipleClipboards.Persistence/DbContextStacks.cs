using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace MultipleClipboards.Persistence
{
    internal static class DbContextStacks
    {
        private const string storageKey = "dbContextTracker";

        public static void Push<T>(T genericInstance)
        {
            ICollection stackObject;
            Stack<T> typedStack;
            if (StoredDictionary.TryGetValue(typeof(T), out stackObject))
            {
                typedStack = (Stack<T>)stackObject;
            }
            else
            {
                typedStack = new Stack<T>();
                if (!StoredDictionary.TryAdd(typeof(T), typedStack))
                {
                    if (StoredDictionary.TryGetValue(typeof(T), out stackObject))
                    {
                        typedStack = (Stack<T>)stackObject;
                    }
                }
            }

            typedStack.Push(genericInstance);
        }

        public static T Pop<T>()
        {
            ICollection stackObject;
            if (StoredDictionary.TryGetValue(typeof(T), out stackObject))
            {
                var typedStack = (Stack<T>)stackObject;
                return typedStack.Pop();
            }

            return default(T);
        }

        public static T Peek<T>()
        {
            ICollection stackObject;
            if (StoredDictionary.TryGetValue(typeof(T), out stackObject))
            {
                var typedStack = (Stack<T>)stackObject;
                return typedStack.Peek();
            }

            return default(T);
        }

        public static bool Any<T>()
        {
            ICollection stackObject;
            if (StoredDictionary.TryGetValue(typeof(T), out stackObject))
            {
                var typedStack = (Stack<T>)stackObject;
                return typedStack.Count > 0;
            }

            return false;
        }

        private static ConcurrentDictionary<Type, ICollection> StoredDictionary
        {
            get
            {
                var result = (ConcurrentDictionary<Type, ICollection>)CallContext.GetData(storageKey);
                if (result == null)
                {
                    result = new ConcurrentDictionary<Type, ICollection>();
                    CallContext.SetData(storageKey, result);
                }
                return result;
            }
        }
    }
}