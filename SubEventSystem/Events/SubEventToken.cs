namespace SubEventSystem.Events
{
    public delegate void ActivationStateChangedEventHandler<T>(SubEventToken<T> sender);

    public class SubEventToken<T> : IDisposable
    {
        public Func<bool> ShouldActivate { get; set; }
        public PubSubEventHandler<T> Handler { get; set; }
        internal ActivationStateChangedEventHandler<T> ActivationStateChangedEvent { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                ActivationStateChangedEvent?.Invoke(this);
            }
        }

        /// <summary>
        /// Re-evaluates activation based on ShouldActivate condition.
        /// Call this explicitly when external state changes.
        /// </summary>
        public void RefreshActivation()
        {
            if (ShouldActivate != null)
                IsActive = ShouldActivate.Invoke();
        }

        public void Dispose()
        {
            Handler = null;
            ActivationStateChangedEvent = null;
            ShouldActivate = null;
        }
    }

    // --- Versione non generica ---

    public delegate void ActivationStateChangedEventHandler(SubEventToken sender);

    public class SubEventToken : IDisposable
    {
        public Func<bool> ShouldActivate { get; set; }
        public PubSubEventHandler Handler { get; set; }
        internal ActivationStateChangedEventHandler ActivationStateChangedEvent { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                ActivationStateChangedEvent?.Invoke(this);
            }
        }

        /// <summary>
        /// Re-evaluates activation based on ShouldActivate condition.
        /// Call this explicitly when external state changes.
        /// </summary>
        public void RefreshActivation()
        {
            if (ShouldActivate != null)
                IsActive = ShouldActivate.Invoke();
        }

        public void Dispose()
        {
            Handler = null;
            ActivationStateChangedEvent = null;
            ShouldActivate = null;
        }
    }
}
