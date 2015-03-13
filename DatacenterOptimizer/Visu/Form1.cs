using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatacenterOptimizer;

namespace Visu
{
    public partial class Form1 : Form
    {
        private Tuple<Datacenter[], Server[], Pool[]> _data;
        private Graphics _flagGraphics;
        private Bitmap _flag;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _data = DatacenterOptimizer.Program.Parse();
            int width = _data.Item1[0].Cells.Length * 10, height = _data.Item1.Length * 10;
            pictureBox1.Size = new Size(width, height);
            Controls.Add(pictureBox1);
            _flag = new Bitmap(width, height);
            _flagGraphics = Graphics.FromImage(_flag);
            _flagGraphics.FillRectangle(Brushes.Black, 0, 0, width, height);
            pictureBox1.Image = _flag;

            var sb2 = new StringBuilder();

            DatacenterOptimizer.Program.PlaceServers(_data, sb2);
            DatacenterOptimizer.Program.PlacePools(_data, sb2);
            int counter = DatacenterOptimizer.Program.GetMinCap(_data, true).Item3;

            for (int i = 0; i < _data.Item1.Length; i++)
            {
                var datacenter = _data.Item1[i];
                var servers = datacenter.Cells.Distinct().ToList();

                for (int j = 0; j < servers.Count; j++)
                {
                    var server = servers[j];

                    try
                    {
                        _flagGraphics.FillRectangle(Server.GetColor(server), server.Position * 10, i * 10, 10 * server.Size, 10);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            try
            {
                pictureBox1.Image = _flag;
                _flag.Save("Visu_" + counter + ".png", ImageFormat.Png);
            }
            catch (Exception)
            {

            }
        }
    }
}
