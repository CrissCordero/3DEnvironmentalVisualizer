using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using System.Linq;

namespace UIPrototype
{
    public partial class Form1 : Form
    {
        private PythonBridge? _bridge;
        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        //switch from void to Task
        private async void button1_Click(object sender, EventArgs e)
        {

            //MessageBox.Show("button1 clicked");

            try
            {
                //details button
                MessageBox.Show("button1 clicked");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "IPC Test Failed");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private async void btnLoadModel_Click(object sender, EventArgs e)
        {
            btnLoadModel.Enabled = false;

            try
            {
                _bridge ??= new PythonBridge();
                await webView21.EnsureCoreWebView2Async();

                string csv = Path.Combine(PythonBridge.ProjectPythonFolder(), "woa23_95A4_t01an01.csv");
                double[] depths = { 0, 25, 50, 100, 200, 400, 700, 1000 };

                //csv, depths, lomin, lomax, lamin, lamax, stride, nColors
                var result = await _bridge.GenerateGraphAsync(csv, depths, -80, -40, 20, 50, 2, 8);

                webView21.CoreWebView2.Navigate(new Uri(result.OutputFile!).AbsoluteUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Graph error");
            }
            finally
            {
                btnLoadModel.Enabled = true;
            }
        }
    }
}
