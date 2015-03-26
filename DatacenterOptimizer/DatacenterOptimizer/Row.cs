using System.Collections.Generic;

namespace DatacenterOptimizer
{
	public class Row
	{
		public Server[] Cells { get; }
		public Dictionary<int, int> Chunks { get; }
		public int Number { get; }
		public int Capacity { get; set; }

		public Row(int size, int number)
		{
			Cells = new Server[size];
			Chunks = new Dictionary<int, int>();
			Number = number;
		}

		public void SetUnavailable(int index)
		{
			Cells[index] = new Server(1, -1, -1) { Pool = Pool.EmptyPool, Position = index };
		}

		public void ComputeChunks()
		{
			Chunks.Clear();

			int counter = 0;
			int counterStart = 0;
			int index = 0;

			foreach (var server in Cells)
			{
				if (server == null)
				{
					counter++;
				}
				else
				{
					if (counter != 0)
					{
						Chunks.Add(counterStart, counter);
					}

					counterStart = index + 1;
					counter = 0;
				}

				index++;
			}

			if (counter != 0)
			{
				Chunks.Add(counterStart, counter);
			}
		}

		public void SetServer(Server s, int pos)
		{
			s.Row = this;
			s.Position = pos;
			Capacity += s.Capacity;

			for (int i = 0; i < s.Size; i++)
			{
				Cells[pos + i] = s;
			}
		}
	}
}