using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DatacenterOptimizer
{
    class Pool
    {
        public int Number;
        public int Capacity;

        public Pool(int number)
        {
            Number = number;
        }
    }

    class Datacenter
    {
        public static KeyValuePair<int, int> EmptyChunk = new KeyValuePair<int, int>(-1, -1);
        public readonly Server[] Cells;
        public Dictionary<int, int> Chunks;
        public Pool Pool;
        public KeyValuePair<int, int> ChunkMax;
        public int Number;

        public Datacenter(int size, int number)
        {
            Cells = new Server[size];
            Chunks = new Dictionary<int, int>();
            Number = number;
        }

        public void SetUnavailable(int index)
        {
            Cells[index] = Server.Unavailable;
        }

        public void ComputeChunks()
        {
            Chunks.Clear();

            int counter = 0;
            int counterStart = 0;
            int counterMax = 0;
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

                        if (counterMax < counter)
                        {
                            counterMax = counter;
                        }
                    }
                    counterStart = index + 1;
                    counter = 0;
                }
                index++;
            }

            ChunkMax = counterMax == 0 ? EmptyChunk : Chunks.First(p => p.Value == counterMax);
        }

        public void SetServer(Server s)
        {
            s.Datacenter = this;
            s.Position = ChunkMax.Key;

            for (int i = 0; i < s.Size; i++)
            {
                Cells[ChunkMax.Key + i] = s;
            }

            //ComputeChunks();
        }

        public void SetServer(Server s, int pos)
        {
            s.Datacenter = this;
            s.Position = pos;

            for (int i = 0; i < s.Size; i++)
            {
                Cells[pos + i] = s;
            }

            //ComputeChunks();
        }
    }

    class Server
    {
        public static Server Unavailable = new Server(-1, -1, -1);

        public int Number;
        public int Capacity;
        public int Size;
        public Pool Pool;
        public Datacenter Datacenter;
        public int Position;
        public double Ratio;

        public Server(int size, int capacity, int number)
        {
            Number = number;
            Capacity = capacity;
            Size = size;
            Ratio = capacity/(double)size;
        }

        public new string ToString()
        {
            return Datacenter == null ? "x" : string.Format("{0} {1} {2}", Datacenter.Number, Position, Pool.Number);
        }
    }

    class Program
    {
        private static void Main(string[] args)
        {
            Tuple<Datacenter[], Server[], Pool[]> parsed = Parse();
            var sb2 = new StringBuilder();

            // Place servers
            /*
            while (true)
            {
                var maxAvailableChunk = parsed.Item1.Select(d => d.ChunkMax.Value).Max();
                var datacenterWithMaxChunk = parsed.Item1.First(d => d.ChunkMax.Value == maxAvailableChunk);
                var availableServers = parsed.Item2.Where(s => s.Datacenter == null && s.Size <= maxAvailableChunk);

                if (!availableServers.Any())
                {
                    break;
                }

                var maxAvailableCapacity = availableServers.Max(s => s.Ratio);
                Server server = availableServers.FirstOrDefault(s => s.Ratio == maxAvailableCapacity);

                if (server == null)
                {
                    break;
                }

                datacenterWithMaxChunk.SetServer(server);
                sb2.AppendFormat("Server {0} placed\r\n", server.Number);
            }*/

            // Place pools
            var servers = parsed.Item2.Where(s => s.Datacenter != null).OrderBy(s => s.Capacity).Reverse();
            
            foreach (var server in servers)
            {
                //int minCap = parsed.Item3.Min(p => p.Capacity);
                //Pool selectedPool = parsed.Item3.First(p => p.Capacity == minCap);
                
                //Pool selectedPool = GetMinCapPool(server.Datacenter, parsed.Item3);
                Pool selectedPool = GetMinCap(parsed.Item1, parsed.Item3);

                server.Pool = selectedPool;
                selectedPool.Capacity += server.Capacity;
                sb2.AppendFormat("Server {0} assigned to pool {1}\r\n", server.Number, selectedPool.Number);
            }

            var sb = new StringBuilder();
            foreach (var server in parsed.Item2)
            {
                sb.AppendLine(server.ToString());
            }

            var sw = new StreamWriter("res.out");
            var sw2 = new StreamWriter("res.debug");
            sw.Write(sb.ToString());
            sw2.Write(sb2.ToString());
            sw.Close();
            sw2.Close();

            GetMinCap(parsed.Item1, parsed.Item3);
            Console.ReadLine();
        }

        private static Pool GetMinCap(Datacenter[] dcs, Pool[] ps)
        {
            int globalminCap = int.MaxValue;
            Pool minPool = null;
            foreach (var pool in ps)
            {
                int minCap = int.MaxValue;
                foreach (var datacenter in dcs)
                {
                    var cap = dcs.Where(dc => dc != datacenter).Sum(
                        dc =>
                            dc.Cells.Where(s => s != null && s != Server.Unavailable && s.Pool == pool)
                                .Distinct()
                                .Sum(s => s.Capacity));
                    if (minCap > cap)
                    {
                        minCap = cap;
                    }
                }
                if (globalminCap > minCap)
                {
                    globalminCap = minCap;
                    minPool = pool;
                }
            }

            Console.WriteLine(globalminCap);
            if (globalminCap == 0)
            {
                int minCap = ps.Min(p => p.Capacity);
                return ps.First(p => p.Capacity == minCap);
            }
            else
            {
                return minPool;
            }
        }

        private static Pool GetMinCapPool(Datacenter dc, Pool[] pools)
        {
            Pool minPool = null;
            int minCap = -1;
            var servers = dc.Cells.Distinct().Where(s => s != null && s.Pool != null);

            foreach (var pool in pools)
            {
                var curPoolCap = servers.Where(s => s.Pool == pool);
                int cap = curPoolCap.Sum(s => s.Capacity);
                if (cap > minCap)
                {
                    minCap = cap;
                    minPool = pool;
                }
            }
            return minPool;
        }

        private static Tuple<Datacenter[], Server[], Pool[]> Parse()
        {
            var sr = new StreamReader("dc.in");
            string[] inputs = sr.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            string input = inputs[index++];
            string[] data = input.Split();
            int numberOfDatacenter = int.Parse(data[0]);
            int datacenterSize = int.Parse(data[1]);
            int unavailableSize = int.Parse(data[2]);
            int poolSize = int.Parse(data[3]);
            int numberOfServers = int.Parse(data[4]);

            var datacenters = new Datacenter[numberOfDatacenter];
            var servers = new Server[numberOfServers];
            var pools = new Pool[poolSize];

            for (int i = 0; i < numberOfDatacenter; i++)
            {
                datacenters[i] = new Datacenter(datacenterSize, i);
            }

            for (int i = 0; i < unavailableSize; i++)
            {
                input = inputs[index++];
                data = input.Split();
                int x = int.Parse(data[0]);
                int y = int.Parse(data[1]);

                datacenters[x].SetUnavailable(y);
            }

            /*
            // Compute chunks
            for (int i = 0; i < numberOfDatacenter; i++)
            {
                datacenters[i].ComputeChunks();
            }*/

            for (int i = 0; i < numberOfServers; i++)
            {
                input = inputs[index++];
                data = input.Split();
                int x = int.Parse(data[0]);
                int y = int.Parse(data[1]);
                servers[i] = new Server(x, y, i);
            }

            // Preparse
            var sr2 = new StreamReader("output.txt");
            inputs = sr2.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            index = 0;

            foreach (var server in servers)
            {
                input = inputs[index++];
                data = input.Split();

                if (data.Length == 3)
                {
                    // "x" : string.Format("{0} {1} {2}", Datacenter.Number, Position, Pool.Number);
                    datacenters[int.Parse(data[0])].SetServer(server, int.Parse(data[1]));
                }
            }

            for (int i = 0; i < numberOfDatacenter; i++)
            {
                datacenters[i].ComputeChunks();
            }

            for (int i = 0; i < poolSize; i++)
            {
                pools[i] = new Pool(i);
            }

            return Tuple.Create(datacenters, servers, pools);
        }
    }
}
