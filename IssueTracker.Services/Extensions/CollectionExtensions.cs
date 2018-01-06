using System;
using System.Collections.Generic;
using System.Linq;

namespace IssueTracker.Services.Extensions
{
    public static class CollectionExtensions
    {
        public static void ReplaceEntityCollection<T, K>(this ICollection<T> oldCollection,
            IEnumerable<T> newCollection, Func<T, K> selector)
        {
            var newItemKeys = new HashSet<K>(newCollection.Select(selector));
            var oldItemKeys = new HashSet<K>(oldCollection.Select(selector));

            var itemsToRemove = oldCollection
                .Where(item => !newItemKeys.Contains(selector(item)))
                .ToArray();
            var itemsToAdd = newCollection
                .Where(at => !oldItemKeys.Contains(selector(at)))
                .ToArray();

            foreach (var item in itemsToRemove)
            {
                oldCollection.Remove(item);
            }

            foreach (var item in itemsToAdd)
            {
                oldCollection.Add(item);
            }
        }
    }
}
