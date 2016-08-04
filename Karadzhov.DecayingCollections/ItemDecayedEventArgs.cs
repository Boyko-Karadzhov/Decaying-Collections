using System;

namespace Karadzhov.DecayingCollections
{
    /// <summary>
    /// Event arguments for the ItemDecayed event of DecayingCollection instances.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="System.EventArgs" />
    public sealed class ItemDecayedEventArgs<TItem> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemDecayedEventArgs{TItem}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public ItemDecayedEventArgs(TItem item)
        {
            this.Item = item;
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public TItem Item { get; }
    }
}
