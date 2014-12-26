using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MuteFm
{
    public partial class TopForm : Form
    {
        private static volatile TopForm _instance;
        private static object syncRoot = new Object();
        private Timer timer = null;

        private TopForm()
        {
            InitializeComponent();
        }

        private Image _favIconImage = null;

        public static TopForm Instance
        {             
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new TopForm();
                            _instance._favIconImage = _instance.mIconPictureBox.Image;
                        }
                    }
                }
                return _instance;
            }
        }

        private void TopForm_Load(object sender, EventArgs e)
        {
//            this.TopMost = true;

            this.panel1.Top = this.Height - (this.panel1.Height * 2);
            this.mLabel.Text = "";
            panel1.Width = this.Width - panel1.Left;
            mLabel.Click += new EventHandler(mLabel_Click);
            mLabel.Cursor = Cursors.Hand;

            this.mIconPictureBox.Visible = false;
            this.mIconPictureBox.Click += new EventHandler(mIconPictureBox_Click);
            this.mIconPictureBox.Cursor = Cursors.Hand;
        }

        void mLabel_Click(object sender, EventArgs e)
        {
            UiPackage.UiCommands.OnOperation(Operation.Show);
        }

        void mIconPictureBox_Click(object sender, EventArgs e)
        {
            UiPackage.UiCommands.OnOperation(Operation.Show);
        }

        public void SetText(string text, bool useBgMusicIcon)
        {
            if (this.mLabel.Text == text)
                return;


            if (timer != null)
            {
                timer.Enabled = false;
                timer.Stop();
                Instance.mLabel.Text = "";
                this.mIconPictureBox.Visible = false;
                this.Visible = false;
                System.Threading.Thread.Sleep(10);
            }

            this.mLabel.Text = text;
            this.mIconPictureBox.Visible = (text != "");
            if (text != "")
            {
                if (useBgMusicIcon)
                    this.mIconPictureBox.Image = WebServer.GetBitmapFromWebServer(@"playericon\" + SmartVolManagerPackage.BgMusicManager.ActiveBgMusic.Id + ".png");
                else
                    this.mIconPictureBox.Image = _favIconImage;
            }
            // Size and center the panel so that it is centered and fits the text
            Graphics g = mLabel.CreateGraphics();
            int textWidth = (int)g.MeasureString(text, mLabel.Font).Width;
            mLabel.Width = textWidth;
            mIconPictureBox.Left = 13;
            mLabel.Left = 75;
            panel1.Width = (mLabel.Width + mLabel.Left);
            mLabel.Left = 75;
            panel1.Left = (Screen.PrimaryScreen.WorkingArea.Width - panel1.Width) / 2;
            //panel1.BackColor = Color.Green;
            //mLabel.BackColor = Color.Orange;
            //TODO: this isn't working conistently (but does sometimes)

            this.TopMost = true;
            this.Show();

            if (timer != null)
                timer.Stop();
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 5000;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Enabled = true;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (timer.Enabled == true)
            {
                Instance.mLabel.Text = "";
                this.mIconPictureBox.Visible = false;
                this.Visible = false;
            }
            timer.Enabled = false;
            timer.Stop();
        }

        // Ignore mouse clicks
        // http://stackoverflow.com/questions/112224/click-through-transparency-for-visual-c-sharp-window-forms
        protected override void WndProc(ref Message m)
        {
            int WM_NCHITTEST = 0x84;
            int HTTRANSPARENT = -1;
            
            if (m.Msg == (int)WM_NCHITTEST)
                m.Result = (IntPtr)HTTRANSPARENT;
            else
                base.WndProc(ref m);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
