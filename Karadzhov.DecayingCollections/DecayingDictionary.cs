using System;
using System.Collections.Generic;

namespace Karadzhov.DecayingCollections
{
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
                if (null != key)
                    throw new ArgumentNullException(nameof(key));

                TValue value;
                if (this.TryGetValue(key, out value))
                    return value;
                else
                    throw new KeyNotFoundException();
            }
            set
            {
                if (null != key)
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
                    if (i != this.Cursor)
                    {
                        keys.AddRange(this.Ring[i].Keys);
                    }
                    else
                    {
                        this.Lock.EnterReadLock();
                        try
                        {
                            keys.AddRange(this.Ring[i].Keys);
                        }
                        finally
                        {
                            this.Lock.ExitReadLock();
                        }
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
                    if (i != this.Cursor)
                    {
                        values.AddRange(this.Ring[i].Values);
                    }
                    else
                    {
                        this.Lock.EnterReadLock();
                        try
                        {
                            values.AddRange(this.Ring[i].Values);
                        }
                        finally
                        {
                            this.Lock.ExitReadLock();
                        }
                    }
                }

                return values;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return this.Keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return this.Values;
            }
        }

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
                if (i != this.Cursor)
                {
                    isFound = this.Ring[i].ContainsKey(key);
                }
                else
                {
                    this.Lock.EnterReadLock();
                    try
                    {
                        isFound = this.Ring[i].ContainsKey(key);
                    }
                    finally
                    {
                        this.Lock.ExitReadLock();
                    }
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
                if (i != this.Cursor)
                {
                    isFound = this.Ring[i].Remove(key);
                }
                else
                {
                    this.Lock.EnterUpgradeableReadLock();
                    try
                    {
                        isFound = this.Ring[i].ContainsKey(key);
                        if (isFound)
                        {
                            this.Lock.EnterWriteLock();
                            try
                            {
                                this.Ring[i].Remove(key);
                            }
                            finally
                            {
                                this.Lock.ExitWriteLock();
                            }
                        }
                    }
                    finally
                    {
                        this.Lock.ExitUpgradeableReadLock();
                    }
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
                if (i != this.Cursor)
                {
                    isFound = this.Ring[i].TryGetValue(key, out value);
                }
                else
                {
                    this.Lock.EnterReadLock();
                    try
                    {
                        isFound = this.Ring[i].TryGetValue(key, out value);
                    }
                    finally
                    {
                        this.Lock.ExitReadLock();
                    }
                }

                if (isFound)
                    return true;
            }

            value = default(TValue);
            return false;
        }
    }
}
