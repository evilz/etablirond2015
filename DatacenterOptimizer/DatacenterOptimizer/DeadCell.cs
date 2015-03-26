namespace DatacenterOptimizer
{
	public class DeadCell : Server
	{
		public DeadCell(int position) : base(1, -1, -1)
		{
			Position = position;
		}

	}

	public static class DeadCellExtensions
	{
		public static bool IsDeadCell(this Server server)
		{
			if(server == null) return false;
			return server.GetType() == typeof(DeadCell);
		}
	}
}