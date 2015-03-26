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
	}
}
