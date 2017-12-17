using System;
using System.Collections.Generic;
using System.Linq;

namespace IssueTracker.Services.Extensions
{
    public static class CollectionExtensions
    {
        public static void ReplaceEntityCollection<T, K>(this ICollection<T> oldCollection,
            ICollection<T> newCollection, Func<T, K> selector)
        {
            var newItemKeys = new HashSet<K>(newCollection.Select(selector));
            var oldItemKeys = new HashSet<K>(oldCollection.Select(selector));

            var itemsToRemove = oldCollection
                .Where(item => !newItemKeys.Contains(selector(item)))
                .ToArray();
            var itemsToAdd = newCollection
                .Where(at => !oldItemKeys.Contains(selector(at)))
                .ToArray();

            for (var i = 0; i < itemsToRemove.Length; i++)
            {
                oldCollection.Remove(itemsToRemove[i]);
            }

            for (var i = 0; i < itemsToAdd.Length; i++)
            {
                oldCollection.Add(itemsToAdd[i]);
            }


        }
    }
}
