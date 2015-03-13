//#define DEBUG
//#define PLACEMENT
//#define BITMAP

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
    public class Pool
    {
        public static Pool EmptyPool = new Pool(-1);
        public int Number;
        public int Capacity;

        public Pool(int number)
        {
            Number = number;
        }
    }

    public class Datacenter
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
            var s = new Server(1, -1, -1) {Pool = Pool.EmptyPool, Position = index};

            Cells[index] = s;
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

            if (counter != 0)
            {
                Chunks.Add(counterStart, counter);

                if (counterMax < counter)
                {
                    counterMax = counter;
                }
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

#if PLACEMENT
            ComputeChunks();
#endif
        }

        public void SetServer(Server s, int pos)
        {
            s.Datacenter = this;
            s.Position = pos;

            for (int i = 0; i < s.Size; i++)
            {
                Cells[pos + i] = s;
            }
            
#if PLACEMENT
            ComputeChunks();
#endif
        }
    }

    public class Server
    {
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
            Ratio = capacity/(double) size;
        }

        public static Brush GetColor(Server s)
        {
            if (s == null || s.Pool == null)
            {
                return Brushes.Black;
            }

            switch (s.Pool.Number)
            {
                case -1:
                    return Brushes.Red;
                case 0:
                    return Brushes.Yellow;
                case 1:
                    return Brushes.Aqua;
                case 2:
                    return Brushes.Aquamarine;
                case 3:
                    return Brushes.YellowGreen;
                case 4:
                    return Brushes.Violet;
                case 5:
                    return Brushes.Tomato;
                case 6:
                    return Brushes.Lime;
                case 7:
                    return Brushes.SlateBlue;
                case 8:
                    return Brushes.Blue;
                case 9:
                    return Brushes.BlueViolet;
                case 10:
                    return Brushes.Brown;
                case 11:
                    return Brushes.SkyBlue;
                case 12:
                    return Brushes.CadetBlue;
                case 13:
                    return Brushes.Chartreuse;
                case 14:
                    return Brushes.Chocolate;
                case 15:
                    return Brushes.Coral;
                case 16:
                    return Brushes.CornflowerBlue;
                case 17:
                    return Brushes.Plum;
                case 18:
                    return Brushes.Crimson;
                case 19:
                    return Brushes.Cyan;
                case 20:
                    return Brushes.DarkBlue;
                case 21:
                    return Brushes.DarkCyan;
                case 22:
                    return Brushes.DarkGoldenrod;
                case 23:
                    return Brushes.DarkGray;
                case 24:
                    return Brushes.DarkGreen;
                case 25:
                    return Brushes.DarkKhaki;
                case 26:
                    return Brushes.DarkMagenta;
                case 27:
                    return Brushes.DarkOliveGreen;
                case 28:
                    return Brushes.DarkOrange;
                case 29:
                    return Brushes.DarkOrchid;
                case 30:
                    return Brushes.DarkRed;
                case 31:
                    return Brushes.DarkSalmon;
                case 32:
                    return Brushes.DarkSeaGreen;
                case 33:
                    return Brushes.DarkSlateBlue;
                case 34:
                    return Brushes.DarkSlateGray;
                case 35:
                    return Brushes.DarkTurquoise;
                case 36:
                    return Brushes.DarkViolet;
                case 37:
                    return Brushes.DeepPink;
                case 38:
                    return Brushes.DeepSkyBlue;
                case 39:
                    return Brushes.DimGray;
                case 40:
                    return Brushes.DodgerBlue;
                case 41:
                    return Brushes.Firebrick;
                case 42:
                    return Brushes.Orange;
                case 43:
                    return Brushes.ForestGreen;
                default: // 44
                    return Brushes.Fuchsia;
            }
        }

        public new string ToString()
        {
            return Datacenter == null ? "x" : string.Format("{0} {1} {2}", Datacenter.Number, Position, Pool.Number);
        }
    }

    public class Program
    {
        private static void Main()
        {
            var sb2 = new StringBuilder();
            Tuple<Datacenter[], Server[], Pool[]> parsed = Parse();

            PlaceServers(parsed, sb2);

            for (int i = 0; i < 1000; i++)
            {
                ClearData(parsed);
                PlacePools(parsed, sb2);
                GetMinCap(parsed, true);
                //Console.ReadLine();
            }

#if DEBUG
            using (var sw2 = new StreamWriter("dc.debug"))
            {
                sw2.Write(sb2.ToString());
            }
#endif
        }

        public static void ClearData(Tuple<Datacenter[], Server[], Pool[]> parsed)
        {
            foreach (var pool in parsed.Item3)
            {
                pool.Capacity = 0;
            }

            foreach (var server in parsed.Item2)
            {
                server.Pool = null;
            }
        }

        public static void PlacePools(Tuple<Datacenter[], Server[], Pool[]> parsed, StringBuilder sb2)
        {
            // Place pools
            while (true)
            {
                var availableServers = parsed.Item2.Where(s => s != null && s.Pool == null && s.Datacenter != null);

                if (!availableServers.Any())
                {
                    break;
                }

                Tuple<Pool, Datacenter, Bitmap> res = GetMinCap(parsed);
                Pool selectedPool = res.Item1;
                Server server =
                    res.Item2.Cells.Where(s => s != null && s.Pool == null)
                        .Distinct()
                        .OrderBy(s => s.Capacity)
                        .Reverse()
                        .First();

                server.Pool = selectedPool;
                selectedPool.Capacity += server.Capacity;
#if DEBUG
                sb2.AppendFormat("Server {0} assigned to pool {1}\r\n", server.Number, selectedPool.Number);
#endif
            }
        }

        public static Tuple<Pool, Datacenter, Bitmap> GetMinCap(Tuple<Datacenter[], Server[], Pool[]> parsed,
            bool write = false)
        {
            Pool[] ps = parsed.Item3;
            Datacenter[] dcs = parsed.Item1;
            int globalminCap = int.MaxValue;
            Pool minPool = null;
            Bitmap resbm = null;
            //object locker = new object();

            Parallel.ForEach(ps, pool =>
            {
                int minCap = int.MaxValue;

                Parallel.ForEach(dcs, datacenter =>
                {
                    var cap = dcs.Where(dc => dc != datacenter).Sum(
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

            Dictionary<Datacenter, int> tmp =
                dcs.Where(dc => dc.Cells.Any(s => s != null && s.Pool == null))
                    .ToDictionary(dc => dc,
                        dc => dc.Cells.Where(s => s != null && s.Pool == minPool).Distinct().Sum(s => s.Capacity));
            Datacenter dcres;

            if (!tmp.Any())
            {
                dcres = null;
            }
            else
            {
                int min = tmp.Min(p => p.Value);

                dcres = tmp.Where(p => p.Value == min).Random().Key;
            }

            if (write)
            {
                Console.WriteLine(globalminCap);
                var sb = new StringBuilder();

                using (var sw = new StreamWriter("dc_" + globalminCap + ".out"))
                {
                    foreach (var server in parsed.Item2)
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

                        for (int j = 0; j < servers.Count; j++)
                        {
                            var server = servers[j];

                            graphics.FillRectangle(Server.GetColor(server), server.Position*10, i*10, 10*server.Size, 10);
                        }
                    }
                }

                resbm.Save("Visu_" + globalminCap + ".png", ImageFormat.Png);
#endif
            }

            return Tuple.Create(minPool, dcres /*dcMinPool*/, resbm);
        }

        public static Tuple<Datacenter[], Server[], Pool[]> Parse()
        {
            string[] inputs;

            using (var sr = new StreamReader("dc.in"))
            {
                inputs = sr.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }

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

            // Compute chunks
            for (int i = 0; i < numberOfDatacenter; i++)
            {
                datacenters[i].ComputeChunks();
            }

            for (int i = 0; i < numberOfServers; i++)
            {
                input = inputs[index++];
                data = input.Split();
                int x = int.Parse(data[0]);
                int y = int.Parse(data[1]);
                servers[i] = new Server(x, y, i);
            }

            return Tuple.Create(datacenters, servers, pools);
        }

        public static void PlaceServers(Tuple<Datacenter[], Server[], Pool[]> tuple, StringBuilder sb2)
        {
            // Place servers
#if PLACEMENT
            while (true)
            {
                var maxAvailableChunk = tuple.Item1.Select(d => d.ChunkMax.Value).Max();
                var datacenterWithMaxChunk = tuple.Item1.First(d => d.ChunkMax.Value == maxAvailableChunk);
                var availableServers = tuple.Item2.Where(s => s.Datacenter == null && s.Size <= maxAvailableChunk);

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
            }
            
            foreach (Datacenter t in tuple.Item1)
            {
                t.ComputeChunks();
            }
#else
            // Preparse
            string[] inputs;

            using (var sr2 = new StreamReader("placement.in"))
            {
                inputs = sr2.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }

            int index = 0;

            foreach (var server in tuple.Item2)
            {
                string input = inputs[index++];
                string[] data = input.Split();

                if (data.Length == 3)
                {
                    tuple.Item1[int.Parse(data[0])].SetServer(server, int.Parse(data[1]));
                }
            }
#endif

            // PKI
            int allCap = tuple.Item2.Sum(s => s.Capacity);
            int allSize = tuple.Item2.Sum(s => s.Size);

            Console.WriteLine("Global count: {0}", tuple.Item2.Length);
            Console.WriteLine("Global capacity: {0}", allCap);
            Console.WriteLine("Global size: {0}", allSize);
            Console.WriteLine("Global ratio: {0}", allCap / (double)allSize);


            var taken = tuple.Item2.Where(s => s.Datacenter != null).ToArray();
            int takenCap = taken.Sum(s => s.Capacity);
            int takenSize = taken.Sum(s => s.Size);

            Console.WriteLine("Taken count: {0}", taken.Length);
            Console.WriteLine("Taken capacity: {0}", takenCap);
            Console.WriteLine("Taken size: {0}", takenSize);
            Console.WriteLine("Taken ratio: {0}", takenCap / (double)takenSize);

            for (int i = 0; i < tuple.Item3.Length; i++)
            {
                tuple.Item3[i] = new Pool(i);
            }

            var ordered = tuple.Item2/*.Where(s => s.Capacity <= 90)*/.OrderBy(s => s.Ratio).Reverse().ToArray();
            var maxRatio = new List<Server>();
            int counter = takenSize;

            foreach (Server server in ordered)
            {
                if (counter == 0)
                {
                    break;
                }
                
                if (server.Size > counter)
                {
                    continue;
                }

                maxRatio.Add(server);
                counter -= server.Size;
            }

            int maxCap = maxRatio.Sum(s => s.Capacity);
            int maxSize = maxRatio.Sum(s => s.Size);

            Console.WriteLine("MaxRatio count: {0}", maxRatio.Count);
            Console.WriteLine("MaxRatio capacity: {0}", maxCap);
            Console.WriteLine("MaxRatio size: {0}", maxSize);
            Console.WriteLine("MaxRatio ratio: {0}", maxCap / (double)maxSize);
        }
    }

    public static class EnumExtensions
    {
        private static readonly Random Rnd = new Random();

        public static T Random<T>(this IEnumerable<T> list)
        {
            return list.ElementAt(Rnd.Next(list.Count()));
        }
    }
}
