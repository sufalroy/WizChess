using System;
using System.Collections.Generic;

namespace WizChess.Events
{
	public static class EventHooks
	{
		private static Dictionary<Type, EventHandler<EventArgs>> s_RegisteredEvents = new Dictionary<Type, EventHandler<EventArgs>>();

		public static void RegisterHook<T>(EventHandler<EventArgs> hook) where T : EventArgs
		{
			if (!s_RegisteredEvents.ContainsKey(typeof(T)))
				s_RegisteredEvents.Add(typeof(T), null);

			s_RegisteredEvents[typeof(T)] += hook;
		}

		public static void CallHooks<T>(T e) where T : EventArgs
		{
			s_RegisteredEvents[typeof(T)]?.Invoke(null, e);
		}
	}
}
