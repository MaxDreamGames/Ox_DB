using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OX_DB
{
    public partial class LogUp : Form
    {
        Color closeBtnColor;
        bool isDraging = false;
        Point startPoint = new Point(0, 0);
        ControlCollection codeCtrls;
        string name;
        string email;
        int code;


        public LogUp()
        {
            InitializeComponent();
        }

        private void LogUp_Load(object sender, EventArgs e)
        {
            closeBtnColor = closeBtn.BackColor;
            label1.Text = Name;
            logUpPanel.Hide();
            logInPanel.Hide();
            codePanel.Hide();
            successRegPanel.Hide();
            /* codeCtrls.Add(Next2Btn);
             codeCtrls.Add(codeText);
             codeCtrls.Add(label4);
             Controls = new Control.ControlCollection();*/
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
        private void logUpBtn_Click(object sender, EventArgs e)
        {
            logUpPanel.Show();
            startPanel.Hide();
        }

        private void LogInBtn_Click(object sender, EventArgs e)
        {
            logInPanel.Show();
            startPanel.Hide();
            DatabaseManager dm = new DatabaseManager();
            DataTable users = dm.Request("SELECT Name FROM Users");
            List<string> list = new List<string>();
            foreach (DataRow el in users.Rows)
                list.Add(el[0].ToString());
            usersList.DataSource = list;
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

        private void nameText_Enter(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(nameText.Text) || nameText.Text == "Имя")
                nameText.Text = "";
        }

        private void nameText_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(nameText.Text))
                nameText.Text = "Имя";
        }

        private void emailText_Enter(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(emailText.Text) || emailText.Text == "Почта")
                emailText.Text = "";
        }

        private void emailText_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(emailText.Text))
                emailText.Text = "Почта";
        }
        private void codeText_Enter(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(codeText.Text) || codeText.Text == "XXXXXX")
                codeText.Text = "";
        }

        private void codeText_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(codeText.Text))
                codeText.Text = "XXXXXX";
        }

        private void backBtn1_Click(object sender, EventArgs e)
        {
            logInPanel.Hide();
            startPanel.Show();
        }

        private void back2Btn_Click(object sender, EventArgs e)
        {
            logUpPanel.Hide();
            startPanel.Show();
        }

        private void back3Btn_Click(object sender, EventArgs e)
        {
            codePanel.Hide();
            logUpPanel.Show();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            successRegPanel.Hide();
            startPanel.Show();
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(nameText.Text) || string.IsNullOrEmpty(emailText.Text) || nameText.Text == "Имя" || emailText.Text == "Почта")
            {
                MessageBox.Show("Данные введены неверно. \n" +
                    "Проверьте, заполнены ли все поля.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            EmailSender emailSender = new EmailSender();
            string body = $"Здравствуйте, {nameText.Text}!\n" +
                $"Добро пожаловать в {GetProjectName()}!\n" +
                $"Мы рады, что вы выбрали наш сервис!\n";
            code = emailSender.SendCode(emailText.Text, $"Код регистрации {GetProjectName()}", body);
            if (code == -1)
            {
                MessageBox.Show("Неверно введена почта или ошибка в отправке письма!\n" +
                    "Попробуйте еще раз.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            name = nameText.Text;
            email = emailText.Text;
            logUpPanel.Hide();
            codePanel.Show();
        }

        private void Next2Btn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(codeText.Text) || codeText.Text == "XXXXXX")
            {
                MessageBox.Show("Введите код!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (codeText.Text == code.ToString())
            {
                SuccessfulRegistr();
                codePanel.Hide();
                successRegPanel.Show();
            }
            else
            {
                MessageBox.Show("Неверный код!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void finalyLogInBtn_Click(object sender, EventArgs e)
        {
            if (usersList.SelectedItem == null)
            {
                MessageBox.Show("Нет пользователя!\nПожалуйста, зарегестрируйтесь!", "Отсутствие пользователя", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

        }


        static string GetProjectName()
        {
            return Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
        }

        void SuccessfulRegistr()
        {
            DatabaseManager dm = new DatabaseManager();
            int currentID = Convert.ToInt32(dm.Request("SELECT COUNT(ID) FROM Users").Rows[0][0]);
            Console.WriteLine(currentID);
            dm.Request($"INSERT INTO Users (ID, Name, Email) VALUES ({currentID}, '{name}', '{email}');");
            Console.WriteLine(dm.Request("SELECT * FROM Users").ToString());
        }

    }
}
