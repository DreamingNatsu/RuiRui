using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuiRuiBot.ExtensionMethods
{
    public static class EnumerableOperatorExtensions
        {
            public static IEnumerable<T> Addition<T>(this IEnumerable<T> left, IEnumerable<T> right)
            {
                return left.Concat(right);
            }
        }
    public static class ListTools
    {
        
        private static readonly Random Random = new Random();
        public static T GetRandomEntry<T>(this IEnumerable<T> list)
        {
            var enumerable = list as IList<T> ?? list.ToList();
            return enumerable[Random.Next(enumerable.Count)];
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action){
            foreach (var element in list) {
                    action(element);
            }
        }
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> list, Func<T,T> action){
            return list.Select(action);
        }
        public static Task ForEachAsync<TSource, TResult>(this IEnumerable<TSource> source,Func<TSource, Task<TResult>> taskSelector, Action<TSource, TResult> resultProcessor)
        {
            var oneAtATime = new SemaphoreSlim(initialCount: 1, maxCount: 1);
            return Task.WhenAll(
                from item in source
                select ProcessAsync(item, taskSelector, resultProcessor, oneAtATime));
        }
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            return Task.WhenAll(
                from item in source
                select Task.Run(() => body(item)));
        }
        private static async Task ProcessAsync<TSource, TResult>(
            TSource item,
            Func<TSource, Task<TResult>> taskSelector, Action<TSource, TResult> resultProcessor,
            SemaphoreSlim oneAtATime)
        {
            var result = await taskSelector(item);
            await oneAtATime.WaitAsync();
            try { resultProcessor(item, result); }
            finally { oneAtATime.Release(); }
        }
    }
}
