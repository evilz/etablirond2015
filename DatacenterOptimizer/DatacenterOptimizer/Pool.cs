using System;
using System.Collections.Generic;

namespace DatacenterOptimizer
{
	public class Pool
	{
		public enum Status
		{
			Completing, // To fill
			Completed,  // IsTarget
			Overload    // Overloaded
		}

		public static Pool EmptyPool = new Pool(-1);
		public int Number;
		public int Capacity;
		public Server Delta;
		public List<Server> Path; 

		public Pool(int number)
		{
			Number = number;
			Path = new List<Server>();
		}

		public Pool(Pool p) : this(p.Number)
		{
			Capacity = p.Capacity;
			Path.AddRange(p.Path);
			Delta = p.Delta;
		}

		public void AddServer(Server s, bool isDelta = false)
		{
			Path.Add(s);
			Capacity += s.Capacity;

			if (isDelta)
			{
				Delta = s;
			}
		}

		public Tuple<Status, int> Score(int target)
		{
			int score = (Capacity - Delta.Capacity) - target;
			var status = score < 0 ? Status.Completing : (score == 0 ? Status.Completed : Status.Overload);

			if (score < 0 && score > -6)
			{
				score -= target;
			}
			else if (score > 0)
			{
				score -= 10; // Malus
			}

			return Tuple.Create(status, score);
		}

		public bool IsTarget(int target)
		{
			return (Capacity - Delta.Capacity) == target;
		}
	}
}