using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace OX_DB
{
    public partial class LogUp : Form
    {
        Color closeBtnColor;
        bool isDraging = false;
        Point startPoint = new Point(0, 0);
        string name;
        string email;
        int code;
        string msgBirthdayToday = "Сегодня день рождения у: ";
        string msgBirthdayTomorrow = "Завтра день рождения у: ";
        string msgNotify = "Сегодня встреча с: ";


        public LogUp()
        {
            InitializeComponent();
        }

        private void LogUp_Load(object sender, EventArgs e)
        {
            Console.WriteLine(SetAutoRunValue(true, Environment.CurrentDirectory + "\\OX DB.exe"));
            WindowState = FormWindowState.Minimized;
            closeBtnColor = closeBtn.BackColor;
            label1.Text = Name;
            logUpPanel.Hide();
            logInPanel.Hide();
            codePanel.Hide();
            successRegPanel.Hide();
            CheckDate();
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

            Main main = new Main();
            EmailSender.user["Name"] = usersList.SelectedItem.ToString();
            DatabaseManager databaseManager = new DatabaseManager();
            EmailSender.user.Add("Email", databaseManager.Request($"SELECT Email FROM Users WHERE Name = '{usersList.SelectedItem.ToString()}'").Rows[0][0].ToString());
            main.Show();
            this.Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Focus();
            WindowState = FormWindowState.Normal;
        }


        static string GetProjectName()
        {
            return Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
        }

        void SuccessfulRegistr()
        {
            DatabaseManager dm = new DatabaseManager();
            int currentID = Convert.ToInt32(dm.Request("SELECT COUNT(ID) FROM Users").Rows[0][0]);
            dm.Request($"INSERT INTO Users (ID, Name, Email) VALUES ({currentID}, '{name}', '{email}');");
        }

        void CheckDate()
        {

            string currentDate = $"{DateTime.Now.Day.ToString("00")}.{DateTime.Now.Month.ToString("00")}";
            DateTime tomorrow = DateTime.Now.AddDays(1);
            string tomorrowDate = $"{tomorrow.Day.ToString("00")}.{tomorrow.Month.ToString("00")}";
            DatabaseManager dm = new DatabaseManager();
            DataTable dt = dm.Request("SELECT `ФИО`, `Дата рождения`, `Уведомление` FROM Clients");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];
                string[] birthdaySplited = row[1].ToString().Split('.');
                string[] notifySplited = row[2].ToString().Split('.');
                string birthdayDate = $"{birthdaySplited[0]}.{birthdaySplited[1]}";
                string notifyDate = $"{notifySplited[0]}.{notifySplited[1]}";
                if (notifyDate == currentDate)
                {
                    msgNotify += $"{row[0]}, ";
                }
                if (birthdayDate == currentDate)
                {
                    msgBirthdayToday += $"{row[0]} - {(DateTime.Now.Year - int.Parse(birthdaySplited[2].Split(' ')[0]))}, ";
                }
                if (birthdayDate == tomorrowDate)
                {
                    msgBirthdayTomorrow += $"{row[0]} - {(DateTime.Now.Year - int.Parse(birthdaySplited[2].Split(' ')[0]))}, ";
                }
            }

            if (msgBirthdayToday != "Сегодня день рождения у: ")
            {
                notifyIcon1.BalloonTipText = msgBirthdayToday;
                notifyIcon1.ShowBalloonTip(6000);
            }
            if (msgBirthdayTomorrow != "Завтра день рождения у: ")
            {
                notifyIcon1.BalloonTipText = msgBirthdayTomorrow;
                notifyIcon1.ShowBalloonTip(6000);
            }
            if (msgNotify != "Сегодня встреча с: ")
            {
                notifyIcon1.BalloonTipText = msgNotify;
                notifyIcon1.ShowBalloonTip(6000);
            }
        }

        bool SetAutoRunValue(bool autoRun, string path)
        {
            const string name = "OX DB";
            string exePath = path;
            RegistryKey reg;

            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");

            try
            {
                if (autoRun)
                    reg.SetValue(name, exePath);
                else
                    reg.DeleteValue(name);

                reg.Flush();
                reg.Close();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
