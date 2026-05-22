using SubEventSystem.Events;

namespace SubEventSystemTests
{
    [TestFixture]
    public class SubEventTests
    {
        [Test]
        public void When_TokenIsInactive_ShouldNotInvokeHandler()
        {
            SubEvent<bool> _testEvent = new SubEvent<bool>();
            bool value = false;

            var token = _testEvent.Subscribe((bool newValue) => { value = newValue; });

            _testEvent.Invoke(true);
            Assert.That(value, Is.False);

            token.IsActive = true;
            _testEvent.Invoke(true);
            Assert.That(value, Is.True);
        }

        [Test]
        public void When_SubscribedWithInitialStateTrue_ShouldInvokeHandler()
        {
            SubEvent<bool> _testEvent = new SubEvent<bool>();
            bool value = false;

            _testEvent.Subscribe((bool newValue) => { value = newValue; }, initialState: true);

            _testEvent.Invoke(true);
            Assert.That(value, Is.True);
        }

        [Test]
        public void When_ConditionBecomesTrue_ShouldInvokeHandler()
        {
            SubEvent<bool> _testEvent = new SubEvent<bool>();
            bool value = false;

            bool shouldActivateEvent = false;

            _testEvent.Subscribe((bool newValue) => { value = newValue; }, activateCondition: () => shouldActivateEvent);

            _testEvent.Invoke(true);
            Assert.That(value, Is.False);

            shouldActivateEvent = true;
            _testEvent.Invoke(true);
            Assert.That(value, Is.True);
        }

        [Test]
        public void When_AllListenersUnsubscribed_ShouldFireDeactivationCallback()
        {
            bool isActive = false;

            SubEvent<bool> _testEvent = new SubEvent<bool>(
                OnFirstListenerActivation: () => { isActive = true; },
                OnLastListenerDeactivation: () => { isActive = false; });

            var first_token = _testEvent.Subscribe((bool newValue) => { }, initialState: true);
            Assert.That(isActive, Is.True);

            var second_token = _testEvent.Subscribe((bool newValue) => { }, initialState: true);

            _testEvent.UnSubscribe(first_token);
            Assert.That(isActive, Is.True);

            _testEvent.UnSubscribe(second_token);
            Assert.That(isActive, Is.False);
        }

        [Test]
        public void When_MultipleTokensActive_ShouldInvokeOnlyActiveOnes()
        {
            SubEvent<int> _testEvent = new SubEvent<int>();
            List<string> invokedTokens = new List<string>();

            var token1 = _testEvent.Subscribe((int v) => { invokedTokens.Add("token1"); }, initialState: true);
            var token2 = _testEvent.Subscribe((int v) => { invokedTokens.Add("token2"); }, initialState: false);
            var token3 = _testEvent.Subscribe((int v) => { invokedTokens.Add("token3"); }, initialState: true);

            _testEvent.Invoke(42);

            Assert.That(invokedTokens, Has.Count.EqualTo(2));
            Assert.That(invokedTokens, Does.Contain("token1"));
            Assert.That(invokedTokens, Does.Not.Contain("token2"));
            Assert.That(invokedTokens, Does.Contain("token3"));
        }

        [Test]
        public void When_UnsubscribeUnknownToken_ShouldThrowException()
        {
            SubEvent<bool> _testEvent = new SubEvent<bool>();
            var otherEvent = new SubEvent<bool>();
            var token = otherEvent.Subscribe((bool v) => { }, initialState: true);

            Assert.Throws<KeyNotFoundException>(() => _testEvent.UnSubscribe(token));
        }

        [Test]
        public void When_TokenDeactivatedAfterBeingActive_ShouldStopReceiving()
        {
            SubEvent<int> _testEvent = new SubEvent<int>();
            int callCount = 0;

            var token = _testEvent.Subscribe((int v) => { callCount++; }, initialState: true);

            _testEvent.Invoke(1);
            Assert.That(callCount, Is.EqualTo(1));

            token.IsActive = false;

            _testEvent.Invoke(2);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void When_TokenDisposed_ShouldNotInvokeHandler()
        {
            SubEvent<int> _testEvent = new SubEvent<int>();
            int callCount = 0;

            var token = _testEvent.Subscribe((int v) => { callCount++; }, initialState: true);

            _testEvent.Invoke(1);
            Assert.That(callCount, Is.EqualTo(1));

            _testEvent.UnSubscribe(token);

            _testEvent.Invoke(2);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void When_FirstTokenActivates_ShouldFireActivationCallback()
        {
            int activationCount = 0;

            SubEvent<bool> _testEvent = new SubEvent<bool>(
                OnFirstListenerActivation: () => { activationCount++; },
                OnLastListenerDeactivation: () => { });

            var token1 = _testEvent.Subscribe((bool v) => { }, initialState: false);
            Assert.That(activationCount, Is.EqualTo(0));

            token1.IsActive = true;
            Assert.That(activationCount, Is.EqualTo(1));

            // Il secondo token attivo NON deve riscatenare il callback
            var token2 = _testEvent.Subscribe((bool v) => { }, initialState: true);
            Assert.That(activationCount, Is.EqualTo(1));
        }

        [Test]
        public void When_ConditionReturnsFalseAgain_ShouldStopReceiving()
        {
            SubEvent<int> _testEvent = new SubEvent<int>();
            int callCount = 0;
            bool shouldActivate = false;

            var token = _testEvent.Subscribe((int v) => { callCount++; }, activateCondition: () => shouldActivate);

            _testEvent.Invoke(1);
            Assert.That(callCount, Is.EqualTo(0));

            shouldActivate = true;
            _testEvent.Invoke(2);
            Assert.That(callCount, Is.EqualTo(1));

            shouldActivate = false;
            _testEvent.Invoke(3);
            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void When_InvokedFromMultipleThreads_ShouldNotThrow()
        {
            SubEvent<int> _testEvent = new SubEvent<int>();
            int callCount = 0;

            _testEvent.Subscribe((int v) => { System.Threading.Interlocked.Increment(ref callCount); }, initialState: true);

            Assert.DoesNotThrow(() =>
            {
                Parallel.For(0, 1000, i =>
                {
                    _testEvent.Invoke(i);
                });
            });

            Assert.That(callCount, Is.EqualTo(1000));
        }
    }
}