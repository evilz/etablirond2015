using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DatacenterOptimizer;

namespace Visu
{
    public partial class Form1 : Form
    {
        private Tuple<Datacenter[], Server[], Pool[]> _data;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var sb2 = new StringBuilder();

            _data = DatacenterOptimizer.Program.Parse();
            DatacenterOptimizer.Program.PlaceServers(_data, sb2);
            DatacenterOptimizer.Program.PlacePools(_data, sb2);
            Bitmap image = DatacenterOptimizer.Program.GetMinCap(_data, true).Item3;
            pictureBox1.Size = new Size(image.Width, image.Height);
            pictureBox1.Image = image;
        }
    }
}
