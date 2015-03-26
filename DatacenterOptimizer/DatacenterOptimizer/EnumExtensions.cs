using System;
using System.Collections.Generic;
using System.Linq;

namespace DatacenterOptimizer
{
	public static class EnumExtensions
	{
		private static readonly Random Rnd = new Random();

		public static T Random<T>(this IEnumerable<T> list)
		{
			var enumerable = list as T[] ?? list.ToArray();
			return enumerable.ElementAt(Rnd.Next(enumerable.Count()));
		}
	}
}