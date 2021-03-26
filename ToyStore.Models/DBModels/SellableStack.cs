using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ToyStore.Models.Abstracts;

namespace ToyStore.Models.DBModels
{
    /// <summary>
    /// This method is a solution for stacking multiple items. <br/>
    /// Whenever a customer adds an item into his order cart, it is stored as a SellableStack. <br/>
    /// Or for the Location an inventory is made up of SellableStacks. <br/>
    /// And a cart or an inventory would not have the same Sellable Stack more than once.
    /// </summary>
    public class SellableStack
    {
        /// <summary>
        /// The Id of the Sellable stack that uniquely identifies it.
        /// </summary>
        /// <returns></returns>
        [Key]
        public Guid SellableStackId { get; set; } = new Guid();

        /// <summary>
        /// The Item type that this stack consists of.
        /// </summary>
        /// <value></value>
        public Sellable Item { get; set; }

        /// <summary>
        /// The number of the same Sellable items in this stack.
        /// </summary>
        /// <value></value>
        public int Count { get; set; }

        public List<Tag> GetTags()
        {
            return Item.Tags;
        }
    }
}