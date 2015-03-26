//#define DEBUG
#define PLACEMENT
//#define BITMAP
#define EQUILIBRATE
//#define DESEQUILIBRATE
#define NEWALGO // Pool selection

using System;
using System.Collections.Generic;
using System.Drawing;
#if BITMAP
using System.Drawing.Imaging;
#endif
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatacenterOptimizer
{
	public class Program
	{
		public static void Main()
		{
			var sb2 = new StringBuilder();
			var datacenter = Parse();

			{
#if PLACEMENT
				datacenter.ClearData(true);
#endif
				PlaceServers(datacenter, sb2);

				{
					for (int l = 0; l < 10; l++)
					{
						datacenter.ClearData();
						PlacePools(datacenter, sb2, 420);
						GetMinCap(datacenter, true);
					}

				}
				Console.ReadLine();
			}

#if DEBUG
			using (var sw2 = new StreamWriter("dc.debug"))
			{
				sw2.Write(sb2.ToString());
			}
#endif
		}

		
		public static void PlacePools(Datacenter datacenter, StringBuilder sb2, int limit1)//, int limit2, int pivot)
		{
			// Place pools
#if NEWALGO // Try to integrate this part after preselection
			try
			{
				foreach (var pool in datacenter.Pools)
				{
					// Delta selection
					Server delta =
						datacenter.Servers.Where(s => s != null && s.Pool == null && s.Row != null)
							.OrderByDescending(s => s.Capacity)
							.First();

					pool.AddServer(delta, true);

					List<Server> servers = FindPath(datacenter, limit1, pool, 25);

					foreach (var server in servers)
					{
						server.Pool = pool;
						pool.Capacity += server.Capacity;
					}
				}
			}
			catch (Exception)
			{
				// Skip
			}
#endif

			for (int i = 0; i < datacenter.Pools.Length; i++)
			{
				Pool pool = datacenter.Pools[i];
				int limit = limit1;

				while (pool.Capacity < limit)
				{
					Server server =
						datacenter.Servers.Where(
							s =>
								s != null && s.Pool == null && s.Row != null &&
								s.Row.Cells.All(s2 => s2 != null && s2.Pool != pool))
							.OrderByDescending(s => s.Capacity)
							.FirstOrDefault();

					if (server == null)
					{
						break;
					}

					server.Pool = pool;
					pool.Capacity += server.Capacity;
				}
			}

			while (true)
			{
				var availableServers = datacenter.Servers.Where(s => s != null && s.Pool == null && s.Row != null);

				if (!availableServers.Any())
				{
					break;
				}

				Tuple<Pool, Server, Bitmap> res = GetMinCap(datacenter);
				Pool selectedPool = res.Item1;
				Server server = res.Item2;

				server.Pool = selectedPool;
				selectedPool.Capacity += server.Capacity;
				//Console.WriteLine("Server {0} assigned to pool {1}", server.Number, selectedPool.Number);
#if DEBUG
				sb2.AppendFormat("Server {0} assigned to pool {1}\r\n", server.Number, selectedPool.Number);
#endif
			}

			//DumpPoolStatus(parsed);
			//Console.ReadLine();
		}

		private static List<Server> FindPath(Datacenter datacenter, int limit1, Pool pool, int tries)
		{
			var poolPaths = new List<Pool> { pool };
			var scoringDico = new Dictionary<Pool, int>();
			var toAdd = new List<Pool>();

			while (tries > 0)
			{
				toAdd.Clear();

				foreach (var poolPath in poolPaths)
				{
					var path = poolPath;
					IEnumerable<Server> servers =
						datacenter.Servers.Where(
							s =>
								s != null && s.Pool == null && s.Row != null && !path.Path.Contains(s) && s.Row != path.Delta.Row &&
								path.Path.Where(s2 => s2.Row == s.Row).Sum(s3 => s3.Capacity) + s.Capacity <= path.Delta.Capacity).OrderByDescending(s => s.Capacity);

					Server first = servers.FirstOrDefault();

					if (first == null)
					{
						break;
					}

					var rndServer = servers.Where(s => s.Capacity == first.Capacity).Random();

					if (limit1 - pool.Capacity >= pool.Delta.Capacity && first.Capacity <= pool.Delta.Capacity)
					{
						pool.AddServer(rndServer);
						toAdd.Add(pool);
						continue;
					}

					foreach (var server in servers)
					{
						var poolToAdd = new Pool(poolPath);

						poolToAdd.AddServer(server);

						if (poolToAdd.IsTarget(limit1))
						{
							return poolToAdd.Path;
						}

						Tuple<Pool.Status, int> score = poolToAdd.Score(limit1);

						if (score.Item1 == Pool.Status.Overload)
						{
							scoringDico.Add(poolToAdd, score.Item2);
							tries--;
						}

						toAdd.Add(poolToAdd);
					}
				}

				tries--;
				poolPaths.Clear();
				poolPaths.AddRange(toAdd);
			}

			if (!scoringDico.Any())
			{
				throw new Exception("Skip");
			}

			int maxScore = scoringDico.Max(p => p.Value);

			return scoringDico.Where(p => p.Value == maxScore).Random().Key.Path;
		}

		public static void DumpPoolStatus(Tuple<Row[], Server[], Pool[]> parsed)
		{
			foreach (var pool in parsed.Item3)
			{
				Console.WriteLine("Pool {0}: {1}", pool.Number, pool.Capacity);
			}

			foreach (var datacenter in parsed.Item1)
			{
				Console.WriteLine("Rangée {0}: {1}", datacenter.Number, datacenter.Cells.Where(s => s != null && s.Row != null).Distinct().Sum(s => s.Capacity));
				/*
                foreach (var pool in parsed.Item3)
                {
                    Console.WriteLine("\tPool {0}: {1}", pool.Number, datacenter.Cells.Where(s=>s.Pool == pool).Distinct().Sum(s => s.Capacity));
                }

                Console.ReadLine();*/
			}
		}

		public static Tuple<Pool, Server, Bitmap> GetMinCap(Datacenter datacenter, bool write = false)
		{
			Pool[] ps = datacenter.Pools;
			Row[] dcs = datacenter.Rows;
			int globalminCap = int.MaxValue;
			Pool minPool = null;
			Bitmap resbm = null;
			int sum = 0;
			//object locker = new object();

			Parallel.ForEach(ps, pool =>
			{
				int minCap = int.MaxValue;

				Parallel.ForEach(dcs, row =>
				{
					var cap = dcs.Where(dc => dc != row).Sum(
						dc =>
							dc.Cells.Where(s => s != null && s.Pool == pool)
								.Distinct()
								.Sum(s => s.Capacity));
					//lock (locker)
					{
						if (minCap > cap)
						{
							minCap = cap;
						}
					}
				});

				//lock (locker)
				{
					sum += minCap;
					if (globalminCap > minCap)
					{
						globalminCap = minCap;
						minPool = pool;
					}
				}
			});

			if (globalminCap == 0)
			{
				int minCap = ps.Min(p => p.Capacity);

				minPool = ps.First(p => p.Capacity == minCap);
			}

			Dictionary<Row, int> tmp =
				dcs.Where(dc => dc.Cells.Any(s => s != null && s.Pool == null))
					.ToDictionary(dc => dc,
						dc => dc.Cells.Where(s => s != null && s.Pool == minPool).Distinct().Sum(s => s.Capacity));
			Server res;

			if (!tmp.Any())
			{
				res = null;
			}
			else
			{
				int min = tmp.Min(p => p.Value);

				Row dcres = tmp.Where(p => p.Value == min).Random().Key;
				res =
					dcres.Cells.Where(s => s != null && s.Pool == null)
						.Distinct()
						.OrderByDescending(s => s.Capacity)
						.First();
			}

			if (write)
			{
				double maxTheorical = Math.Truncate(sum / 45f);
				Console.WriteLine("Garanteed capacity: {0}, theorical maximum {1}", globalminCap, maxTheorical);

				if (globalminCap > 414)
				{
					var sb = new StringBuilder();

					using (var sw = new StreamWriter(string.Format("dc_{0}_{1}.out", globalminCap, maxTheorical)))
					{
						foreach (var server in datacenter.Servers)
						{
							sb.AppendLine(server.ToString());
						}

						sw.Write(sb.ToString());
					}

#if BITMAP
                    int width = dcs[0].Cells.Length*10, height = dcs.Length*10;

                    resbm = new Bitmap(width, height);

                    using (Graphics graphics = Graphics.FromImage(resbm))
                    {
                        graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

                        for (int i = 0; i < parsed.Item1.Length; i++)
                        {
                            Datacenter datacenter = parsed.Item1[i];
                            List<Server> servers = datacenter.Cells.Distinct().ToList();

                            foreach (var server in servers)
                            {
                                if (server != null)
                                {
                                    graphics.FillRectangle(Server.GetColor(server), server.Position * 10, i * 10, 10 * server.Size, 10);
                                }
                            }
                        }
                    }

                    resbm.Save(string.Format("Visu_{0}_{1}.png", globalminCap, maxTheorical), ImageFormat.Png);
#endif
				}
			}

			return Tuple.Create(minPool, res, resbm);
		}

		public static Datacenter Parse()
		{
			string[] inputs;

			using (var sr = new StreamReader("dc.in"))
			{
				inputs = sr.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			}

			int index = 0;
			string input = inputs[index++];
			string[] data = input.Split();
			int numberOfRows = int.Parse(data[0]);
			int datacenterSize = int.Parse(data[1]);
			int unavailableSize = int.Parse(data[2]);
			int poolSize = int.Parse(data[3]);
			int numberOfServers = int.Parse(data[4]);
			var rows = new Row[numberOfRows];
			var servers = new Server[numberOfServers];
			var pools = new Pool[poolSize];

			for (int i = 0; i < numberOfRows; i++)
			{
				rows[i] = new Row(datacenterSize, i);
			}

			for (int i = 0; i < unavailableSize; i++)
			{
				input = inputs[index++];
				data = input.Split();
				int x = int.Parse(data[0]);
				int y = int.Parse(data[1]);

				rows[x].SetUnavailable(y);
			}

			// Compute chunks
			for (int i = 0; i < numberOfRows; i++)
			{
				rows[i].ComputeChunks();
			}

			for (int i = 0; i < numberOfServers; i++)
			{
				input = inputs[index++];
				data = input.Split();
				int x = int.Parse(data[0]);
				int y = int.Parse(data[1]);
				servers[i] = new Server(x, y, i);
			}

			for (int i = 0; i < pools.Length; i++)
			{
				pools[i] = new Pool(i);
			}

			return new Datacenter(rows, servers, pools);
		}

		public static void PlaceServers(Datacenter datacenter, StringBuilder sb2)
		{
			// Place servers
#if PLACEMENT
			// Best Fit
			List<Tuple<Row, int, int>> chunks =
				(datacenter.Rows.SelectMany(r => r.Chunks,
					(r, chunk) => Tuple.Create(r, chunk.Key, chunk.Value))).ToList();
#if DESEQUILIBRATE
            List<Server> serverList = parsed.Item2.OrderByDescending(s => s.Ratio).ToList();
            
            foreach (Datacenter dc in parsed.Item1)
            {
                while (dc.AvailableSlot > 0)
                {
                    Tuple<Datacenter, int, int> dcChunk = chunks.Where(c => c.Item1 == dc).Random();
                    Server server;

                    if (dcChunk.Item3 > 5)
                    {
                        server = serverList.First();

                        chunks.Remove(dcChunk);
                        dcChunk.Item1.SetServer(server, dcChunk.Item2);
                        chunks.Add(Tuple.Create(dcChunk.Item1, dcChunk.Item2 + server.Size, dcChunk.Item3 - server.Size));
                    }
                    else
                    {
                        server = serverList.FirstOrDefault(s => s.Size == dcChunk.Item3);
                        
                        if (server == null)
                        {
                            server = serverList.First(q => server.Size < dcChunk.Item3);
                            chunks.Add(Tuple.Create(dcChunk.Item1, dcChunk.Item2 + server.Size, dcChunk.Item3 - server.Size));
                        }

                        chunks.Remove(dcChunk);
                        dcChunk.Item1.SetServer(server, dcChunk.Item2);
                    }

                    serverList.Remove(server);
                }
            }
#else
			var serversByRatio = datacenter.Servers.OrderByDescending(s => s.Ratio).ThenByDescending(s => s.Size);

			foreach (var server in serversByRatio)
			{
				var server1 = server;
				IEnumerable<Tuple<Row, int, int>> available = chunks.Where(c => c.Item3 >= server1.Size).OrderByDescending(c => c.Item3);

				if (!available.Any())
				{
					continue;
				}

#if EQUILIBRATE
				var dcAvailable = available.Select(c => c.Item1).ToList();
				int dcMinCap = dcAvailable.Min(dc => dc.Capacity);
				var dcToUse = dcAvailable.Where(dc => dc.Capacity == dcMinCap).Random();
				available = available.Where(c => c.Item1 == dcToUse);
#endif

				var server2 = server;
				IEnumerable<Tuple<Row, int, int>> perfectFit = available.Where(c => c.Item3 == server2.Size);
				Tuple<Row, int, int> chunk;

				if (perfectFit.Any())
				{
					chunk = perfectFit.Random();
				}
				else
				{
					int maxSize = available.First().Item3;

					chunk = available.Where(c => c.Item3 == maxSize).Random();
					chunks.Add(Tuple.Create(chunk.Item1, chunk.Item2 + server.Size, chunk.Item3 - server.Size));
				}

				chunks.Remove(chunk);
				chunk.Item1.SetServer(server, chunk.Item2);
			}
#endif
#else
            // Preparse
            string[] inputs;

            using (var sr2 = new StreamReader("placement.in"))
            {
                inputs = sr2.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }

            int index = 0;

            foreach (var server in parsed.Item2)
            {
                string input = inputs[index++];
                string[] data = input.Split();

                if (data.Length == 3)
                {
                    parsed.Item1[int.Parse(data[0])].SetServer(server, int.Parse(data[1]));
                }
            }
#endif

			// PKI
			int allCap = datacenter.Servers.Sum(s => s.Capacity);
			int allSize = datacenter.Servers.Sum(s => s.Size);

			Console.WriteLine("Global count: {0}", datacenter.Servers.Length);
			Console.WriteLine("Global capacity: {0}", allCap);
			Console.WriteLine("Global size: {0}", allSize);
			Console.WriteLine("Global ratio: {0}", allCap / (double)allSize);


			var taken = datacenter.Servers.Where(s => s.Row != null).ToArray();
			int takenCap = taken.Sum(s => s.Capacity);
			int takenSize = taken.Sum(s => s.Size);

			Console.WriteLine("Taken count: {0}", taken.Length);
			Console.WriteLine("Taken capacity: {0}", takenCap);
			Console.WriteLine("Taken size: {0}", takenSize);
			Console.WriteLine("Taken ratio: {0}", takenCap / (double)takenSize);
		}
	}
}
