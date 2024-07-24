using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace OX_DB
{
    public partial class MainMenu : Form
    {
        Color closeBtnColor;
        bool isDraging = false;
        Point startPoint = new Point(0, 0);


        public MainMenu()
        {
            InitializeComponent();
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            closeBtnColor = closeBtn.BackColor;
            label1.Text = Name;
        }

        private void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Maximized;
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void minimizeBtn_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void closeBtn_MouseEnter(object sender, EventArgs e)
        {
            closeBtn.BackColor = Color.Red;
        }

        private void closeBtn_MouseLeave(object sender, EventArgs e)
        {
            closeBtn.BackColor = closeBtnColor;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraging = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraging)
            {
                Point p = PointToScreen(e.Location);
                Point delta = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
                Location = delta;
                if ((delta.X != 0 || delta.Y != 0) && WindowState == FormWindowState.Maximized)
                    WindowState = FormWindowState.Normal;
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDraging = false;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_SIZEBOX = 0x40000;

                var cp = base.CreateParams;
                cp.Style |= WS_SIZEBOX;

                return cp;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection("jk");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Test");
                string msg = ex.Message;
                foreach (var err in ex.Data.Keys)
                {
                    Console.WriteLine(err + ": " + ex.Data[err]);
                }
                msg += "\nSource: " + (ex.Source);
                msg += "\n\nCall stack: " + ex.StackTrace;
                msg += "\n\nTarget Site: " + ex.TargetSite;
                msg += "\nHResult: " + ex.HResult;
                msg += "\nHlink: " + ex.HelpLink;
                EmailSender es = new EmailSender("Error", msg, "m@br.ru");
                es.SendEmail();
            }
        }
    }
}
// myDream-MaxDeep/agata0417
// max_senderCode228