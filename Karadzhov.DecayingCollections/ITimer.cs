using System;

namespace Karadzhov.DecayingCollections
{
    /// <summary>
    /// Interface defining what is required for a Timer used in DecayingCollections.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// Starts the timer with the specified period in seconds.
        /// </summary>
        /// <param name="periodInSeconds">The period in seconds.</param>
        /// <param name="callback">The callback.</param>
        void Start(int periodInSeconds, Action callback);

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        void Pause();

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning { get; }
    }
}