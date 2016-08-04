using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Karadzhov.DecayingCollections
{
    /// <summary>
    /// Base class for decaying collections.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <seealso cref="System.Collections.Generic.ICollection{TItem}" />
    public abstract class DecayingCollection<TItem, TCollection> : ICollection<TItem>, IDisposable
        where TCollection : ICollection<TItem>, new()
    {
        private readonly int _lifespan;
        private readonly TCollection[] _ring;

        private volatile int _count;
        private volatile int _cursor;

        private ITimer _timer;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingCollection{TItem, TCollection}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        protected DecayingCollection(int lifespanInSeconds)
            : this(lifespanInSeconds, lifespanInSeconds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingCollection{TItem, TCollection}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        /// <param name="steps">The number of steps that the lifetime is divided into.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "It's set to a private field and is disposed on this object's Dispose.")]
        protected DecayingCollection(int lifespanInSeconds, int steps)
            : this(new TimerWrapper(), lifespanInSeconds, steps)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingCollection{TItem, TCollection}"/> class.
        /// </summary>
        /// <param name="timer">A timer instance used by this collection to measure time.</param>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        /// <param name="steps">The number of steps that the lifetime is divided into.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected DecayingCollection(ITimer timer, int lifespanInSeconds, int steps)
        {
            if (null == timer)
                throw new ArgumentNullException(nameof(timer));

            this._lifespan = lifespanInSeconds;
            this._ring = new TCollection[steps];
            for (var i = 0; i < steps; i++)
                this._ring[i] = new TCollection();

            this._timer = timer;
        }


        /// <summary>
        /// Occurs when an item has decayed.
        /// </summary>
        public event EventHandler<ItemDecayedEventArgs<TItem>> ItemDecayed;

        /// <summary>
        /// Called when an item has decayed.
        /// </summary>
        /// <param name="item">The item.</param>
        protected virtual void OnItemDecayed(TItem item) => this.ItemDecayed?.Invoke(this, new ItemDecayedEventArgs<TItem>(item));

        private void TimerElapsed()
        {
            if (null == this._ring)
                return;

            var decayedSegment = this.Step();
            foreach (var item in decayedSegment)
                this.OnItemDecayed(item);
        }

        private TCollection Step()
        {
            // While doing Step logic other threads can safely act on the current cursor which is not changed until the end of the method.
            var newCursor = (this._cursor + 1) % this._ring.Length;
            var collectionToRemove = this._ring[newCursor];
            this._ring[newCursor] = new TCollection();
            this._count -= collectionToRemove.Count;
            this.SetupTimer();

            // The segment at the new cursor is ready for use.
            this._cursor = newCursor;

            return collectionToRemove;
        }

        private void SetupTimer()
        {
            if (0 == this._count && this._timer.IsRunning)
                this._timer.Pause();
            else if (this._count > 0 && !this._timer.IsRunning)
                this._timer.Start(this._lifespan, this.TimerElapsed);
        }

        #region ICollection<T>

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => this._count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(TItem item)
        {
            this._ring[this._cursor].Add(item);
            this._count++;
            this.SetupTimer();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < this._ring.Length; i++)
                this.Step();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(TItem item) => this._ring.Any(s => s.Contains(item));

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            if (null == array)
                throw new ArgumentNullException(nameof(array));

            if (array.Length < arrayIndex + this.Count)
                throw new ArgumentException("The array is not big enough for this collection.", nameof(array));

            var i = arrayIndex;
            foreach (var item in this)
                array[i++] = item;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TItem> GetEnumerator() => (from segment in this._ring
                                                      from item in segment
                                                      select item).GetEnumerator();

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(TItem item)
        {
            if (this._ring.Any(s => s.Remove(item)))
            {
                this._count--;
                this.SetupTimer();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #endregion

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (null != this._timer)
                    {
                        this._timer.Dispose();
                        this._timer = null;
                    }
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
