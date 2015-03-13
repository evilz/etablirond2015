﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

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

    public class Server
    {
        //public static Server Unavailable = new Server(1, -1, -1) { Pool = Pool.EmptyPool };

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
        private static void Main(string[] args)
        {
            var sb2 = new StringBuilder();
            Tuple<Datacenter[], Server[], Pool[]> parsed = Parse();

            PlaceServers(parsed);
            PlacePools(parsed, sb2);
            GetMinCap(parsed.Item1, parsed.Item3);

            /*
            var sw = new StreamWriter("res.out");
            var sw2 = new StreamWriter("res.debug");
            sw.Write(sb.ToString());
            sw2.Write(sb2.ToString());
            sw.Close();
            sw2.Close();*/

            Console.ReadLine();
        }


        public static void PlacePools(Tuple<Datacenter[], Server[], Pool[]> parsed, StringBuilder sb2)
        {
            // Place pools
            /*
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
            }*/

            /*
            int dataIndex = 0;
            foreach (var pool in parsed.Item3)
            {
                Server t = parsed.Item1[dataIndex++ % parsed.Item1.Length].Cells.Where(s => s != null && s != Server.Unavailable && s.Pool == null).OrderBy(s => s.Capacity).First();

                t.Pool = pool;
                pool.Capacity += t.Capacity;
                sb2.AppendFormat("Server {0} assigned to pool {1}\r\n", t.Number, pool.Number);
            }*/

            foreach (var datacenter in parsed.Item1)
            {
                foreach (
                    var server in datacenter.Cells.Where(s => s != null && s.Pool == null).Distinct()
                    )
                {
                    Pool selectedPool = GetMinCap(parsed.Item1, parsed.Item3);
                    server.Pool = selectedPool;
                    selectedPool.Capacity += server.Capacity;
                    sb2.AppendFormat("Server {0} assigned to pool {1}\r\n", server.Number, selectedPool.Number);
                }
            }

            var sb = new StringBuilder();
            foreach (var server in parsed.Item2)
            {
                sb.AppendLine(server.ToString());
            }
        }

        public static Pool GetMinCap(Datacenter[] dcs, Pool[] ps)
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
                            dc.Cells.Where(s => s != null && s.Pool == pool)
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

        public static Tuple<Datacenter[], Server[], Pool[]> Parse()
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
            sr.Close();
            return Tuple.Create(datacenters, servers, pools);
        }

        public static void PlaceServers(Tuple<Datacenter[], Server[], Pool[]> tuple)
        {
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

            // Preparse
            var sr2 = new StreamReader("output.txt");
            string[] inputs = sr2.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int index = 0;

            foreach (var server in tuple.Item2)
            {
                string input = inputs[index++];
                string[] data = input.Split();

                if (data.Length == 3)
                {
                    // "x" : string.Format("{0} {1} {2}", Datacenter.Number, Position, Pool.Number);
                    tuple.Item1[int.Parse(data[0])].SetServer(server, int.Parse(data[1]));
                }
            }

            for (int i = 0; i < tuple.Item1.Length; i++)
            {
                tuple.Item1[i].ComputeChunks();
            }

            for (int i = 0; i < tuple.Item3.Length; i++)
            {
                tuple.Item3[i] = new Pool(i);
            }

            sr2.Close();
        }
    }
}