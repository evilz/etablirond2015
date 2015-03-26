using System;
using System.Collections.Generic;
using System.Linq;

namespace DatacenterOptimizer
{
	public static class EnumerableExtensions
	{
		private static readonly Random _rnd = new Random();

		public static T Random<T>(this IEnumerable<T> list)
		{
			var enumerable = list as T[] ?? list.ToArray();
			return enumerable.ElementAt(_rnd.Next(enumerable.Count()));
		}
	}
}