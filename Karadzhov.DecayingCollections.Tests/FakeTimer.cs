using System;

namespace Karadzhov.DecayingCollections.Tests
{
    public sealed class FakeTimer : ITimer
    {
        private Action _callback;

        public bool IsRunning { get; private set; }

        public void Dispose()
        {
            // Do nothing.
        }

        public void Pause()
        {
            if (!this.IsRunning)
                throw new InvalidOperationException("Timer is not running for it to be paused.");

            this.IsRunning = false;
        }

        public void Start(int periodInSeconds, Action callback)
        {
            if (this.IsRunning)
                throw new InvalidOperationException("Timer is already running.");

            this._callback = callback;
            this.IsRunning = true;
        }

        public void Execute() => this._callback?.Invoke();
    }
}
