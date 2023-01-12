using System;
using System.Collections.Generic;

namespace Uno.PackageDiff
{
	internal static class EnumerableExtensions
	{
		public static TSource FirstOrDefault<TSource, TArg>(this IEnumerable<TSource> source, Func<TSource, TArg, bool> predicate, TArg arg)
		{
			foreach(var item in source)
			{
				if(predicate(item, arg))
				{
					return item;
				}
			}
			return default;
		}
	}
}
