using System;

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
            get
            {
                return GetActivationValue();
            }

            set
            {
                SetActivationValue(value);
            }
        }

        private void SetActivationValue(bool value)
        {
            if (_isActive == value) return;
            _isActive = value;
            ActivationStateChangedEvent?.Invoke(this);
        }

        public bool GetActivationValue()
        {
            if (ShouldActivate != null)
            {
                var newValue = ShouldActivate.Invoke();

                SetActivationValue(newValue);
            }

            return _isActive;
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
            get
            {
                if (ShouldActivate != null)
                {
                    var newValue = ShouldActivate.Invoke();
                    SetActivationValue(newValue);
                }
                return _isActive;
            }
            set => SetActivationValue(value);
        }

        private void SetActivationValue(bool value)
        {
            if (_isActive == value) return;
            _isActive = value;
            ActivationStateChangedEvent?.Invoke(this);
        }

        public void Dispose()
        {
            Handler = null;
            ActivationStateChangedEvent = null;
            ShouldActivate = null;
        }
    }
}
