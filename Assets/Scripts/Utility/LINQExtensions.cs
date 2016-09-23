using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Specialized;

namespace Utility
{
    public static class LINQExtensions
    {
        // public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
        //     this IEnumerable<TSource> source, int size)
        // {
        //     TSource[] bucket = null;
        //     var count = 0;

        //     foreach (var item in source)
        //     {
        //         if (bucket == null)
        //             bucket = new TSource[size];

        //         bucket[count++] = item;
        //         if (count != size)
        //             continue;

        //         yield return bucket;

        //         bucket = null;
        //         count = 0;
        //     }

        //     if (bucket != null && count > 0)
        //         yield return bucket.Take(count);
        // }
    //     public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
    //    this IEnumerable<TFirst> first,
    //    IEnumerable<TSecond> second,
    //    Func<TFirst, TSecond, TResult> func)
    //     {
    //         using (var enumeratorA = first.GetEnumerator())
    //         using (var enumeratorB = second.GetEnumerator())
    //         {
    //             while (enumeratorA.MoveNext())
    //             {
    //                 enumeratorB.MoveNext();
    //                 yield return func(enumeratorA.Current, enumeratorB.Current);
    //             }
    //         }
        // }
    //     public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    // (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    //     {
    //         HashSet<TKey> seenKeys = new HashSet<TKey>();
    //         foreach (TSource element in source)
    //         {
    //             if (seenKeys.Add(keySelector(element)))
    //             {
    //                 yield return element;
    //             }
    //         }
    //     }
    //     public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    //     {
    //         if (source == null || action == null)
    //         {
    //             throw new Exception("Source or action is null.");
    //         }

    //         foreach (T element in source)
    //         {
    //             action(element);
    //         }
    //     }

    }

}
