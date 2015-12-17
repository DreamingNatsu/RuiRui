using System;
using System.Collections.Generic;
using System.Linq;

namespace RuiRuiBot.ExtensionMethods
{
    public static class ListTools
    {
        private static readonly Random Random = new Random();
        public static T GetRandomEntry<T>(this IEnumerable<T> list)
        {
            var enumerable = list as IList<T> ?? list.ToList();
            return enumerable[Random.Next(enumerable.Count)];
        }
    }
}
