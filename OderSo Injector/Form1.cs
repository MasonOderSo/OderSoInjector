using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace OderSo_Injector
{
    public partial class Form1 : Form
    {
        private static string DLLP { get; set; }

        public Form1()
        {
            InitializeComponent();
            MakeRoundForm();

            Process minecraftProcess = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == "Minecraft.Windows");

            if (minecraftProcess != null)
            {
                DLLP = "Path to DLL";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Injector.Inject(DLLP);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.InitialDirectory = @"C:\";
                OFD.Title = "Locate DLL File";
                OFD.DefaultExt = "dll";
                OFD.Filter = "DLL Files (*.dll)|*.dll";
                OFD.CheckFileExists = true;
                OFD.CheckPathExists = true;
                OFD.ShowDialog();
                DLLP = OFD.FileName;
                label1.Text = DLLP;
            }
            catch (Exception ed)
            {
                MessageBox.Show(ed.Message);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        private void MakeRoundForm()
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 20;

            path.AddArc(new Rectangle(0, 0, radius * 2, radius * 2), 180, 90);
            path.AddArc(new Rectangle(Width - (radius * 2), 0, radius * 2, radius * 2), -90, 90);
            path.AddArc(new Rectangle(Width - (radius * 2), Height - (radius * 2), radius * 2, radius * 2), 0, 90);
            path.AddArc(new Rectangle(0, Height - (radius * 2), radius * 2, radius * 2), 90, 90);

            path.CloseFigure();

            Region region = new Region(path);

            Region = region;
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CAPTION = 0x2;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)(HT_CAPTION);
        }
    }
}