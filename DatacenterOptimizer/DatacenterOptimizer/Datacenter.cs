using System.Collections.Generic;

namespace DatacenterOptimizer
{
	public class Datacenter
	{
		public readonly Server[] Cells;
		public Dictionary<int, int> Chunks;
		public Pool Pool;
		public int Number;
		public int Capacity;
		public int AvailableSlot;

		public Datacenter(int size, int number)
		{
			Cells = new Server[size];
			Chunks = new Dictionary<int, int>();
			Number = number;
			AvailableSlot = size;
		}

		public void SetUnavailable(int index)
		{
			var s = new Server(1, -1, -1) {Pool = Pool.EmptyPool, Position = index};

			Cells[index] = s;
			AvailableSlot--;
		}

		public void ComputeChunks()
		{
			Chunks.Clear();

			int counter = 0;
			int counterStart = 0;
			int index = 0;

			foreach (Server server in Cells)
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
			s.Datacenter = this;
			s.Position = pos;
			Capacity += s.Capacity;
			AvailableSlot -= s.Size;

			for (int i = 0; i < s.Size; i++)
			{
				Cells[pos + i] = s;
			}
		}
	}
}