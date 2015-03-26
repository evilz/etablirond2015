using System.Drawing;

namespace DatacenterOptimizer
{
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
}