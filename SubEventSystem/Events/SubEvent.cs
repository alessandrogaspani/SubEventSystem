using System.Collections;

namespace SubEventSystem.Events
{
    public delegate void ActiveListenersStateChangedEventHandler<T>(SubEvent<T> sender, bool activeListeners);

    public delegate void PubSubEventHandler<T>(T args);

    public class SubEvent<T> : IDisposable
    {
        private readonly List<SubEventToken<T>> _subscriptions = new();
        private readonly object _lock = new();

        public ActiveListenersStateChangedEventHandler<T> ActiveListenersStateChangedEvent { get; set; }

        private bool _hasActiveListeners;

        public bool HasActiveListeners
        {
            get => _hasActiveListeners;
            private set
            {
                if (_hasActiveListeners == value) return;
                _hasActiveListeners = value;
                ActiveListenersStateChangedEvent?.Invoke(this, value);
            }
        }

        public SubEvent()
        {
        }

        public SubEvent(Action OnFirstListenerActivation, Action OnLastListenerDeactivation)
        {
            ActiveListenersStateChangedEvent = (sender, activeListeners) =>
            {
                if (activeListeners)
                    OnFirstListenerActivation?.Invoke();
                else
                    OnLastListenerDeactivation?.Invoke();
            };
        }

        public SubEventToken<T> Subscribe(PubSubEventHandler<T> handler, Func<bool> activateCondition)
            => SubscribeToEvent(handler, activateCondition, activateCondition.Invoke());

        public SubEventToken<T> Subscribe(PubSubEventHandler<T> handler, bool initialState)
            => SubscribeToEvent(handler, null, initialState);

        public SubEventToken<T> Subscribe(PubSubEventHandler<T> handler)
            => SubscribeToEvent(handler, null, false);

        private SubEventToken<T> SubscribeToEvent(PubSubEventHandler<T> handler, Func<bool> activateCondition, bool initialState)
        {
            lock (_lock)
            {
                var token = new SubEventToken<T>()
                {
                    Handler = handler,
                    ShouldActivate = activateCondition,
                    ActivationStateChangedEvent = OnTokenActivationStateChanged
                };

                _subscriptions.Add(token);
                token.IsActive = initialState;

                return token;
            }
        }

        private void OnTokenActivationStateChanged(SubEventToken<T> sender)
        {
            lock (_lock)
            {
                HasActiveListeners = _subscriptions.Exists(token => token.IsActive);
            }
        }

        public void UnSubscribe(SubEventToken<T> token)
        {
            lock (_lock)
            {
                if (!_subscriptions.Remove(token))
                    throw new KeyNotFoundException($"Could not unsubscribe {token.GetType().Name} from {this.GetType().Name}");

                token.IsActive = false;
                token.Dispose();
            }
        }

        public void Invoke(T value)
        {
            SubEventToken<T>[] snapshot;
            lock (_lock) { snapshot = _subscriptions.ToArray(); }

            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i].IsActive)
                    snapshot[i].Handler?.Invoke(value);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var token in _subscriptions)
                    token.Dispose();
                _subscriptions.Clear();
                HasActiveListeners = false;
            }
        }
    }

    // --- Versione non generica ---

    public delegate void ActiveListenersStateChangedEventHandler(SubEvent sender, bool activeListeners);

    public delegate void PubSubEventHandler();

    public class SubEvent : IDisposable
    {
        private readonly List<SubEventToken> _subscriptions = new();
        private readonly object _lock = new();

        public event ActiveListenersStateChangedEventHandler ActiveListenersStateChangedEvent;

        private bool _hasActiveListeners;

        public bool HasActiveListeners
        {
            get => _hasActiveListeners;
            private set
            {
                if (_hasActiveListeners == value) return;
                _hasActiveListeners = value;
                ActiveListenersStateChangedEvent?.Invoke(this, value);
            }
        }

        public SubEvent()
        {
        }

        public SubEvent(Action OnFirstListenerActivation, Action OnLastListenerDeactivation)
        {
            ActiveListenersStateChangedEvent += (sender, activeListeners) =>
            {
                if (activeListeners)
                    OnFirstListenerActivation?.Invoke();
                else
                    OnLastListenerDeactivation?.Invoke();
            };
        }

        public SubEventToken Subscribe(PubSubEventHandler handler, Func<bool> activateCondition)
            => SubscribeToEvent(handler, activateCondition, activateCondition.Invoke());

        public SubEventToken Subscribe(PubSubEventHandler handler, bool initialState)
            => SubscribeToEvent(handler, null, initialState);

        public SubEventToken Subscribe(PubSubEventHandler handler)
            => SubscribeToEvent(handler, null, false);

        private SubEventToken SubscribeToEvent(PubSubEventHandler handler, Func<bool> activateCondition, bool initialState)
        {
            lock (_lock)
            {
                var token = new SubEventToken()
                {
                    Handler = handler,
                    ShouldActivate = activateCondition,
                    ActivationStateChangedEvent = OnTokenActivationStateChanged
                };

                _subscriptions.Add(token);
                token.IsActive = initialState;

                return token;
            }
        }

        private void OnTokenActivationStateChanged(SubEventToken sender)
        {
            lock (_lock)
            {
                HasActiveListeners = _subscriptions.Exists(token => token.IsActive);
            }
        }

        public void UnSubscribe(SubEventToken token)
        {
            lock (_lock)
            {
                if (!_subscriptions.Remove(token))
                    throw new KeyNotFoundException($"Could not unsubscribe {token.GetType().Name} from {this.GetType().Name}");

                token.IsActive = false;
                token.Dispose();
            }
        }

        public void Invoke()
        {
            SubEventToken[] snapshot;
            lock (_lock) { snapshot = [.. _subscriptions]; }

            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i].IsActive)
                    snapshot[i].Handler?.Invoke();
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (var token in _subscriptions)
                    token.Dispose();
                _subscriptions.Clear();
                HasActiveListeners = false;
            }
        }
    }
}