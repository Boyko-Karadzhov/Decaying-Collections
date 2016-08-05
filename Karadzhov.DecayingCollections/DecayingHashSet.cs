using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Karadzhov.DecayingCollections
{
    /// <summary>
    /// Represents a strongly typed hash set. Its items have a specified lifetime and will be removed automatically when they expire. Adding an element wich is already in the set will reset its lifecycle.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="DecayingCollections.DecayingCollection{TItem, List{TItem}}" />
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "HashSet is an ok suffix.")]
    public sealed class DecayingHashSet<TItem> : DecayingCollection<TItem, HashSet<TItem>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingBag{TItem}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        public DecayingHashSet(int lifespanInSeconds) : base(lifespanInSeconds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingBag{TItem}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        /// <param name="steps">The number of steps that the lifetime is divided into.</param>
        public DecayingHashSet(int lifespanInSeconds, int steps) : base(lifespanInSeconds, steps)
        {
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public override void Add(TItem item)
        {
            this.Remove(item);
            base.Add(item);
        }
    }
}
