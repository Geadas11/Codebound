using System;
using System.Collections.Generic;

namespace Codebound.Core
{
    /// <summary>
    /// Synchronous, type-keyed publish/subscribe bus for decoupled cross-module communication.
    /// Handlers that throw are logged and do not stop delivery to other handlers.
    /// </summary>
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (!_handlers.TryGetValue(typeof(T), out var list))
            {
                list = new List<Delegate>();
                _handlers[typeof(T)] = list;
            }
            if (!list.Contains(handler)) list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler != null && _handlers.TryGetValue(typeof(T), out var list)) list.Remove(handler);
        }

        public void Publish<T>(T evt)
        {
            if (!_handlers.TryGetValue(typeof(T), out var list) || list.Count == 0) return;
            // Copy so handlers may (un)subscribe during delivery.
            var snapshot = list.ToArray();
            foreach (var d in snapshot)
            {
                try { ((Action<T>)d)(evt); }
                catch (Exception e) { CbLog.Error("Core", $"Event handler for {typeof(T).Name} threw: {e}"); }
            }
        }

        public int HandlerCount<T>() => _handlers.TryGetValue(typeof(T), out var list) ? list.Count : 0;
    }
}
