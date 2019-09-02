using System;
using System.Collections.Generic;
using System.Linq;
using EventFeed;

namespace ShoppingCart.ShoppingCart
{
    public class ShoppingCart
    {
        public void AddItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IEventStore eventStore)
        {
            foreach(var items in shoppingCartItems)
                if (this.items.Add(item))
                    eventStore.Raise(
                        "ShoppingCartItemAdded",
                        new {UserId, item}
                    );
        }

        public void RemoveItems(int[] productCatalogueIds, IEventStore eventStore)
        {
            items.RemoveWhere(i => productCatalogueIds.Contains(i.ProductCatalogueId));
        }
    }
}