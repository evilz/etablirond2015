namespace DatacenterOptimizer
{
	public class Datacenter
	{
		public Row[] Rows { get; }
		public Server[] Servers { get; }
		public Pool[] Pools { get; }

		public Datacenter(Row[] rows, Server[] servers, Pool[] pools)
		{
			Rows = rows;
			Servers = servers;
			Pools = pools;
		}

		public void ClearData(bool cleanPlacement = false)
		{
			foreach (var pool in Pools)
			{
				pool.Clean();
			}

			foreach (var server in Servers)
			{
				server.Clean(cleanPlacement);
			}

			if (cleanPlacement)
			{
				foreach (var row in Rows)
				{
					row.Clean();
				}
			}
		}

	}
}
