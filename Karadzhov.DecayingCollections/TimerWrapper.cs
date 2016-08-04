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
        private Timer _timer;
        private Action _callback;

        /// <summary>
        /// Starts the timer with the specified period in seconds.
        /// </summary>
        /// <param name="periodInSeconds">The period in seconds.</param>
        /// <param name="callback">The callback.</param>
        /// <exception cref="InvalidOperationException">A timer is already started. Call Stop first.</exception>
        public void Start(int periodInSeconds, Action callback)
        {
            if (null != this._timer)
                throw new InvalidOperationException("A timer is already started. Call Stop first.");

            this._callback = callback;
            this._timer = new Timer(this.TimerCallback, null, 0, periodInSeconds * 1000);
        }

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">The timer is not started so there is nothing to stop.</exception>
        public void Pause()
        {
            if (null == this._timer)
                throw new InvalidOperationException("The timer is not started so there is nothing to stop.");

            this._callback = null;
            this._timer.Dispose();
            this._timer = null;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsRunning => null != this._timer;

        private void TimerCallback(object state) => this._callback?.Invoke();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (null != this._timer)
            {
                this.Pause();
            }
        }
    }
}