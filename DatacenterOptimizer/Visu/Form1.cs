using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Visu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var sb2 = new StringBuilder();
            var data = DatacenterOptimizer.Program.Parse();

            DatacenterOptimizer.Program.PlaceServers(data, sb2);
            DatacenterOptimizer.Program.PlacePools(data, sb2);
            Bitmap image = DatacenterOptimizer.Program.GetMinCap(data, true).Item3;
            pictureBox1.Size = new Size(image.Width, image.Height);
            pictureBox1.Image = image;
        }
    }
}
