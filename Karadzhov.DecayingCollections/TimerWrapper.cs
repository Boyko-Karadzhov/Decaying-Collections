using System;
using System.Threading;

namespace Karadzhov.DecayingCollections
{
    /// <summary>
    /// Default implementation of ITimer.
    /// </summary>
    /// <seealso cref="Karadzhov.DecayingCollections.ITimer" />
    internal sealed class TimerWrapper : ITimer
    {
        private readonly Timer _timer;
        private Action _callback;
        private bool _isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerWrapper"/> class.
        /// </summary>
        public TimerWrapper()
        {
            this._timer = new Timer(this.TimerCallback, state: 0, dueTime: Timeout.Infinite, period: Timeout.Infinite);
        }

        /// <summary>
        /// Starts the timer with the specified period in milliseconds.
        /// </summary>
        /// <param name="periodMilliseconds">The period in milliseconds.</param>
        /// <param name="callback">The callback.</param>
        public void Start(int periodMilliseconds, Action callback)
        {
            this._isRunning = true;
            this._callback = callback;
            this._timer.Change(dueTime: 0, period: periodMilliseconds);
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        public void Pause()
        {
            this._isRunning = false;
            this._callback = null;
            this._timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning => this._isRunning;

        private void TimerCallback(object state)
        {
            lock (this)
            {
                this._callback?.Invoke();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => this._timer.Dispose();
    }
}