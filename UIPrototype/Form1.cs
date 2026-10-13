using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIPrototype
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        //switch from void to Task
        private async void button1_Click(object sender, EventArgs e)
        {

            //MessageBox.Show("button1 clicked");

            try
            {
                //MessageBox.Show("button1 clicked");

                var bridge = new PythonBridge();

                string response = await bridge.TestConnectionAsync("https://www.ncei.noaa.gov/data/oceans/woa/WOA23/");

                MessageBox.Show(response, "Python IPC Test");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "IPC Test Failed");
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
