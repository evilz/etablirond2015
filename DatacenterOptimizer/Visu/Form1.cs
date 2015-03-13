using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
            var sb2 = new StringBuilder();

            _data = DatacenterOptimizer.Program.Parse();
            DatacenterOptimizer.Program.PlaceServers(_data);
            DatacenterOptimizer.Program.PlacePools(_data, sb2);
            DatacenterOptimizer.Program.GetMinCap(_data.Item1, _data.Item3);
            int width = _data.Item1[0].Cells.Length * 10, height = _data.Item1.Length * 10;
            pictureBox1.Size = new Size(width, height);
            Controls.Add(pictureBox1);
            _flag = new Bitmap(width, height);
            _flagGraphics = Graphics.FromImage(_flag);

            new Thread(() =>
            {
                for (int i = 0; i < _data.Item1.Length; i++)
                {
                    var datacenter = _data.Item1[i];
                    var servers = datacenter.Cells.Distinct().ToList();
                    for (int j = 0; j < servers.Count; j++)
                    {
                        var server = servers[j];
                        _flagGraphics.FillRectangle(Server.GetColor(server), server.Position * 10, i * 10, 10 * server.Size, 10);
                        try
                        {
                            pictureBox1.Image = _flag;
                        }
                        catch (Exception)
                        {

                        }
                        Thread.Sleep(1);
                    }
                }
            }).Start();
        }
    }
}
