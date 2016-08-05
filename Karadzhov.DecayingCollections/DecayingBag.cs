using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Karadzhov.DecayingCollections
{
    /// <summary>
    /// Represents a strongly typed unordered collection. Its items have a specified lifetime and will be removed automatically when they expire.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="DecayingCollections.DecayingCollection{TItem, List{TItem}}" />
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Bag is ok.")]
    public sealed class DecayingBag<TItem> : DecayingCollection<TItem, List<TItem>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingBag{TItem}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        public DecayingBag(int lifespanInSeconds) : base(lifespanInSeconds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecayingBag{TItem}"/> class.
        /// </summary>
        /// <param name="lifespanInSeconds">The lifespan of an item in seconds.</param>
        /// <param name="steps">The number of steps that the lifetime is divided into.</param>
        public DecayingBag(int lifespanInSeconds, int steps) : base(lifespanInSeconds, steps)
        {
        }
    }
}
