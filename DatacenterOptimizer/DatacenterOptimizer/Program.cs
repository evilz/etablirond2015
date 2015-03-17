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
        public static void Main()
        {
            var sb2 = new StringBuilder();
            Tuple<Datacenter[], Server[], Pool[]> parsed = Parse();

            for (int i = 0; i < 3; i++)
            {
#if PLACEMENT
                ClearData(parsed, true);
#endif
                PlaceServers(parsed, sb2);

                for (int j = 420; j < 422; j++)
                {
                    //for (int k = 400; k < 401; k++)
                    //{
                    //Console.Write("j = {0}, k = {1}: ", j, k);
                    Console.Write("j = {0}: ", j);
                    for (int l = 0; l < 30; l++)
                    {
                        ClearData(parsed);
                        PlacePools(parsed, sb2, j); //, k, 3);
                        GetMinCap(parsed, true);
                        //Console.ReadLine();
                    }
                    //}
                }
            }

#if DEBUG
            using (var sw2 = new StreamWriter("dc.debug"))
            {
                sw2.Write(sb2.ToString());
            }
#endif
        }

        public static void ClearData(Tuple<Datacenter[], Server[], Pool[]> parsed, bool cleanPlacement = false)
        {
            foreach (var pool in parsed.Item3)
            {
                pool.Capacity = 0;
                pool.Delta = null;
                pool.Path.Clear();
            }

            foreach (var server in parsed.Item2)
            {
                server.Pool = null;
#if PLACEMENT
                if (cleanPlacement)
                {
                    server.Datacenter = null;
                }
#endif
            }

#if PLACEMENT
            if (cleanPlacement)
            {
                foreach (var dc in parsed.Item1)
                {
                    dc.Capacity = 0;

                    for (int index = 0; index < dc.Cells.Length; index++)
                    {
                        var server = dc.Cells[index];

                        if (server != null && server.Pool != Pool.EmptyPool)
                        {
                            dc.Cells[index] = null;
                        }
                    }
                }
            }
#endif
        }

        public static void PlacePools(Tuple<Datacenter[], Server[], Pool[]> parsed, StringBuilder sb2, int limit1)//, int limit2, int pivot)
        {
            // Place pools
#if NEWALGO
            try
            {
                foreach (var pool in parsed.Item3)
                {
                    //Console.Write("Pool n°{0}: ", pool.Number);

                    // Delta selection
                    Server delta =
                        parsed.Item2.Where(s => s != null && s.Pool == null && s.Datacenter != null)
                            .OrderByDescending(s => s.Capacity)
                            .First();

                    pool.AddServer(delta, true);

                    List<Server> servers = FindPath(parsed, limit1, pool, 25);//null; // To benchmark
                    /*
                    int currentDelta = 1000;

                    for (int i = 10; i < 20; i++)
                    {
                        var tmp = FindPath(parsed, limit1, pool, i);
                        int newDelta = Math.Abs(tmp.Sum(s => s.Capacity) - pool.Delta.Capacity - limit1);

                        if (newDelta < currentDelta)
                        {
                            servers = tmp;
                            currentDelta = newDelta;
                        }
                    }*/

                    foreach (var server in servers)
                    {
                        server.Pool = pool;
                        pool.Capacity += server.Capacity;
                    }

                    //Console.WriteLine("done.");
                }
            }
            catch (Exception)
            {
                // Skip
            }
#endif

            for (int i = 0; i < parsed.Item3.Length; i++)
            {
                Pool pool = parsed.Item3[i];
                int limit = limit1;//i < pivot ? limit1 : limit2;

                while (pool.Capacity < limit)
                {
                    Server server =
                        parsed.Item2.Where(
                            s =>
                                s != null && s.Pool == null && s.Datacenter != null &&
                                s.Datacenter.Cells.All(s2 => s2 != null && s2.Pool != pool))
                            .OrderByDescending(s => s.Capacity)
                            .FirstOrDefault();

                    if (server == null)
                    {
                        break;
                    }

                    server.Pool = pool;
                    pool.Capacity += server.Capacity;

                    //Console.WriteLine("Server {0} assigned to pool {1}", server.Number, pool.Number);
                }
            }

            while (true)
            {
                var availableServers = parsed.Item2.Where(s => s != null && s.Pool == null && s.Datacenter != null);

                if (!availableServers.Any())
                {
                    break;
                }

                Tuple<Pool, Server, Bitmap> res = GetMinCap(parsed);
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

        private static List<Server> FindPath(Tuple<Datacenter[], Server[], Pool[]> parsed, int limit1, Pool pool, int tries)
        {
            var poolPaths = new List<Pool> {pool};
            var scoringDico = new Dictionary<Pool, int>();
            var toAdd = new List<Pool>();

            while (tries > 0)
            {
                toAdd.Clear();

                foreach (var poolPath in poolPaths)
                {
                    IEnumerable<Server> servers =
                        parsed.Item2.Where(
                            s =>
                                s != null && s.Pool == null && s.Datacenter != null && !poolPath.Path.Contains(s) && s.Datacenter != poolPath.Delta.Datacenter &&
                                poolPath.Path.Where(s2 => s2.Datacenter == s.Datacenter).Sum(s3 => s3.Capacity) + s.Capacity <= poolPath.Delta.Capacity).OrderByDescending(s => s.Capacity);
                    
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

        public static void DumpPoolStatus(Tuple<Datacenter[], Server[], Pool[]> parsed)
        {
            foreach (var pool in parsed.Item3)
            {
                Console.WriteLine("Pool {0}: {1}", pool.Number, pool.Capacity);
            }

            foreach (var datacenter in parsed.Item1)
            {
                Console.WriteLine("Rangée {0}: {1}", datacenter.Number, datacenter.Cells.Where(s => s != null && s.Datacenter != null).Distinct().Sum(s => s.Capacity));
                /*
                foreach (var pool in parsed.Item3)
                {
                    Console.WriteLine("\tPool {0}: {1}", pool.Number, datacenter.Cells.Where(s=>s.Pool == pool).Distinct().Sum(s => s.Capacity));
                }

                Console.ReadLine();*/
            }
        }

        public static Tuple<Pool, Server, Bitmap> GetMinCap(Tuple<Datacenter[], Server[], Pool[]> parsed,
            bool write = false)
        {
            Pool[] ps = parsed.Item3;
            Datacenter[] dcs = parsed.Item1;
            int globalminCap = int.MaxValue;
            Pool minPool = null;
            Bitmap resbm = null;
            int sum = 0;
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

            Dictionary<Datacenter, int> tmp =
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

                Datacenter dcres = tmp.Where(p => p.Value == min).Random().Key;
                res =
                    dcres.Cells.Where(s => s != null && s.Pool == null)
                        .Distinct()
                        .OrderByDescending(s => s.Capacity)
                        .First();
            }

            if (write)
            {
                double maxTheorical = Math.Truncate(sum/45f);
                Console.WriteLine("{0} {1}", globalminCap, maxTheorical);

                if (globalminCap > 414)
                {
                    var sb = new StringBuilder();

                    using (var sw = new StreamWriter(string.Format("dc_{0}_{1}.out", globalminCap, maxTheorical)))
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

            return Tuple.Create(minPool, res /*dcMinPool*/, resbm);
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

            for (int i = 0; i < pools.Length; i++)
            {
                pools[i] = new Pool(i);
            }

            return Tuple.Create(datacenters, servers, pools);
        }

        public static void PlaceServers(Tuple<Datacenter[], Server[], Pool[]> parsed, StringBuilder sb2)
        {
            // Place servers
#if PLACEMENT
            // Best Fit
            List<Tuple<Datacenter, int, int>> chunks =
                (parsed.Item1.SelectMany(datacenter => datacenter.Chunks,
                    (datacenter, chunk) => Tuple.Create(datacenter, chunk.Key, chunk.Value))).ToList();
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
            var serversByRatio = parsed.Item2.OrderByDescending(s => s.Ratio).ThenByDescending(s => s.Size);

            foreach (var server in serversByRatio)
            {
                IEnumerable<Tuple<Datacenter, int, int>> available = chunks.Where(c => c.Item3 >= server.Size).OrderByDescending(c => c.Item3);

                if (!available.Any())
                {
                    continue;
                }

#if EQUILIBRATE
                List<Datacenter> dcAvailable = available.Select(c => c.Item1).ToList();
                int dcMinCap = dcAvailable.Min(dc => dc.Capacity);
                Datacenter dcToUse = dcAvailable.Where(dc => dc.Capacity == dcMinCap).Random();
                available = available.Where(c => c.Item1 == dcToUse);
#endif

                IEnumerable<Tuple<Datacenter, int, int>> perfectFit = available.Where(c => c.Item3 == server.Size);
                Tuple<Datacenter, int, int> chunk;

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
            int allCap = parsed.Item2.Sum(s => s.Capacity);
            int allSize = parsed.Item2.Sum(s => s.Size);

            Console.WriteLine("Global count: {0}", parsed.Item2.Length);
            Console.WriteLine("Global capacity: {0}", allCap);
            Console.WriteLine("Global size: {0}", allSize);
            Console.WriteLine("Global ratio: {0}", allCap / (double)allSize);


            var taken = parsed.Item2.Where(s => s.Datacenter != null).ToArray();
            int takenCap = taken.Sum(s => s.Capacity);
            int takenSize = taken.Sum(s => s.Size);

            Console.WriteLine("Taken count: {0}", taken.Length);
            Console.WriteLine("Taken capacity: {0}", takenCap);
            Console.WriteLine("Taken size: {0}", takenSize);
            Console.WriteLine("Taken ratio: {0}", takenCap / (double)takenSize);
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
