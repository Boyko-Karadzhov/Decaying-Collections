using System;
using System.Collections.Generic;

namespace Karadzhov.DecayingCollections
{
    /// <summary>
    /// Represents a dictionary with decaying elements.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="Karadzhov.DecayingCollections.DecayingCollection{System.Collections.Generic.KeyValuePair{TKey, TValue}, System.Collections.Generic.Dictionary{TKey, TValue}}" />
    /// <seealso cref="System.Collections.Generic.IDictionary{TKey, TValue}" />
    /// <seealso cref="System.Collections.Generic.IReadOnlyDictionary{TKey, TValue}" />
    public sealed class DecayingDictionary<TKey, TValue> : DecayingCollection<KeyValuePair<TKey, TValue>, Dictionary<TKey, TValue>>, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        public DecayingDictionary(int lifespanInSeconds) : base(lifespanInSeconds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        /// <param name="steps">The number of steps that the lifespan is divided into.</param>
        public DecayingDictionary(int lifespanInSeconds, int steps) : base(lifespanInSeconds, steps)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="timer">A timer instance used by this collection to measure time.</param>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        /// <param name="steps">The number of steps that the lifespan is divided into.</param>
        public DecayingDictionary(ITimer timer, int lifespanInSeconds, int steps) : base(timer, lifespanInSeconds, steps)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
        public TValue this[TKey key]
        {
            get
            {
                if (null == key)
                    throw new ArgumentNullException(nameof(key));

                TValue value;
                if (this.TryGetValue(key, out value))
                    return value;
                else
                    throw new KeyNotFoundException();
            }
            set
            {
                if (null == key)
                    throw new ArgumentNullException(nameof(key));

                if (this.ContainsKey(key))
                    this.Remove(key);

                this.Add(key, value);
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new List<TKey>(this.Count);
                for (int i = 0; i < this.Ring.Count; i++)
                {
                    this.Locks[i].EnterReadLock();
                    try
                    {
                        keys.AddRange(this.Ring[i].Keys);
                    }
                    finally
                    {
                        this.Locks[i].ExitReadLock();
                    }
                }

                return keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>(this.Count);
                for (int i = 0; i < this.Ring.Count; i++)
                {
                    this.Locks[i].EnterReadLock();
                    try
                    {
                        values.AddRange(this.Ring[i].Values);
                    }
                    finally
                    {
                        this.Locks[i].ExitReadLock();
                    }
                }

                return values;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        /// <exception cref="ArgumentException">An item with the same key already exists.</exception>
        public void Add(TKey key, TValue value)
        {
            if (null == key)
                throw new ArgumentNullException(nameof(key));

            if (this.ContainsKey(key))
                throw new ArgumentException("An item with the same key already exists.", nameof(key));

            base.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            if (null == key)
                throw new ArgumentNullException(nameof(key));

            bool isFound;
            for (var i = 0; i < this.Ring.Count; i++)
            {
                this.Locks[i].EnterReadLock();
                try
                {
                    isFound = this.Ring[i].ContainsKey(key);
                }
                finally
                {
                    this.Locks[i].ExitReadLock();
                }

                if (isFound)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool Remove(TKey key)
        {
            if (null == key)
                throw new ArgumentNullException(nameof(key));

            bool isFound;
            for (var i = 0; i < this.Ring.Count; i++)
            {
                this.Locks[i].EnterUpgradeableReadLock();
                try
                {
                    isFound = this.Ring[i].ContainsKey(key);
                    if (isFound)
                    {
                        this.Locks[i].EnterWriteLock();
                        try
                        {
                            this.Ring[i].Remove(key);
                        }
                        finally
                        {
                            this.Locks[i].ExitWriteLock();
                        }
                    }
                }
                finally
                {
                    this.Locks[i].ExitUpgradeableReadLock();
                }

                if (isFound)
                {
                    this.Count = this.Count - 1;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">key is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (null == key)
                throw new ArgumentNullException(nameof(key));

            bool isFound;
            for (var i = 0; i < this.Ring.Count; i++)
            {
                this.Locks[i].EnterReadLock();
                try
                {
                    isFound = this.Ring[i].TryGetValue(key, out value);
                }
                finally
                {
                    this.Locks[i].ExitReadLock();
                }

                if (isFound)
                    return true;
            }

            value = default(TValue);
            return false;
        }
    }
}
