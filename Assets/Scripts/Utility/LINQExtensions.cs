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
		
		public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(
			this IEnumerable<TSource> source, int size)
		{
			TSource[] bucket = null;
			var count = 0;

			foreach (var item in source)
			{
				if (bucket == null)
					bucket = new TSource[size];

				bucket[count++] = item;
				if (count != size)
					continue;

				yield return bucket;

				bucket = null;
				count = 0;
			}

			if (bucket != null && count > 0)
				yield return bucket.Take(count);
		}
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			if (source == null || action == null) {
				throw new Exception ("Source or action is null.");
			}

			foreach (T element in source)
			{
				action(element);
			}
		}

	}

}
