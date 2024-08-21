using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OX_DB
{
    public partial class Main : Form
    {

        Color closeBtnColor;
        bool isDraging = false;
        Point startPoint = new Point(0, 0);
        DatabaseManager databaseManager = new DatabaseManager();
        EmailSender emailSender = new EmailSender();
        TextBox[] textBoxes;

        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            closeBtnColor = closeBtn.BackColor;
            label1.Text = Text;
            textBoxes = new TextBox[3] { FIOText, phoneText, addressText };
            SelectData();
            SetNotifyDate();
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
            Application.Exit();
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


        private void addBtn_Click(object sender, EventArgs e)
        {
            AddRow();
        }

        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            Console.WriteLine("Add");
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                MessageBox.Show("Этот столбец не редактируем!", "Отказ в доступе", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4 || e.ColumnIndex == 6)
            {
                string newStr = $"{dataGridView1.SelectedCells[0].Value.ToString().Split('.')[2]}-{dataGridView1.SelectedCells[0].Value.ToString().Split('.')[1]}-{dataGridView1.SelectedCells[0].Value.ToString().Split('.')[0]}";
                newStr = newStr.Replace(" 0:00:00", "");
                Console.WriteLine(newStr);
                if (DateTime.TryParse(newStr, out DateTime dt))
                    databaseManager.Request($"UPDATE Clients SET `{dataGridView1.SelectedCells[0].OwningColumn.Name}` = '{newStr}' WHERE ID = {e.RowIndex + 1};");
                else
                {
                    MessageBox.Show("Неверный тип данных!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dataGridView1.CancelEdit();
                }

            }
            else
            {
                if (string.IsNullOrEmpty(dataGridView1.SelectedCells[0].Value.ToString()) || databaseManager.Request($"UPDATE Clients SET `{dataGridView1.SelectedCells[0].OwningColumn.Name}` = '{dataGridView1.SelectedCells[0].Value.ToString()}' WHERE ID = {e.RowIndex + 1};", false) == null)
                {
                    MessageBox.Show("Неверный ввод!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dataGridView1.CancelEdit();
                }
            }

            SelectData();
        }

        private void швейкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Sewing sewing = new Sewing();
            sewing.ID = int.Parse(dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value.ToString());
            sewing.ParentFm = this;
            sewing.Show();
            Hide();
        }

        private void ЛФКToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Sport Sport = new Sport();
            Sport.ID = int.Parse(dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value.ToString());
            Sport.ParentFm = this;
            Sport.Text = "Physical therapy";
            Sport.name = "Тренировки";
            Sport.mainTable = "Physical_Therapy";
            Sport.addTable = "Trainings";
            Sport.Show();
            Hide();
        }

        private void массажToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Sport Sport = new Sport();
            Sport.ID = int.Parse(dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value.ToString());
            Sport.ParentFm = this;
            Sport.Text = "Massage";
            Sport.name = "Массаж";
            Sport.mainTable = "Massage";
            Sport.addTable = "Massage_sessions";
            Sport.Show();
            Hide();
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !dataGridView1.IsCurrentCellInEditMode)
            {
                var mb = MessageBox.Show("Вы действительно хотите удалить клиента?", "Подтвердите действие", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (mb == DialogResult.Cancel || mb == DialogResult.No)
                    return;
                string currentFIO = dataGridView1[1, dataGridView1.CurrentCell.RowIndex].Value.ToString();
                if (databaseManager.CheckExistingOfThisPersonInTable(currentFIO, "Sewing"))
                    DeleteRow(currentFIO, "Sewing");
                if (databaseManager.CheckExistingOfThisPersonInTable(currentFIO, "Physical_Therapy"))
                    DeleteRow(currentFIO, "Physical_Therapy");
                if (databaseManager.CheckExistingOfThisPersonInTable(currentFIO, "Massage"))
                    DeleteRow(currentFIO, "Massage");
                DeleteRow(currentFIO, "Clients");
                SelectData();
            }
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridView1.CurrentCell != null)
            {
                string currentServices = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value.ToString().ToLower();
                if (!currentServices.Contains("шв"))
                    швейкаToolStripMenuItem.Enabled = false;
                else
                    швейкаToolStripMenuItem.Enabled = true;
                if (!currentServices.Contains("лфк"))
                    лФКToolStripMenuItem.Enabled = false;
                else
                    лФКToolStripMenuItem.Enabled = true;
                if (!currentServices.Contains("мсж"))
                    массажToolStripMenuItem.Enabled = false;
                else
                    массажToolStripMenuItem.Enabled = true;
                contextMenuStrip1.Show(dataGridView1, e.Location);
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show("Неверный ввод!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            dataGridView1.CancelEdit();
        }

        private void обновитToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectData();
        }


        void AddRow()
        {
            foreach (var item in textBoxes)
            {
                if (item.Text.Length == 0)
                    break;
                while (item.Text[0] == ' ' || item.Text[0] == '\t' || item.Text[item.Text.Length - 1] == ' ' || item.Text[item.Text.Length - 1] == '\t')
                    item.Text = item.Text.Trim(new char[] { ' ', '\t' });
            }
            if (string.IsNullOrEmpty(FIOText.Text) || string.IsNullOrEmpty(phoneText.Text) || string.IsNullOrEmpty(addressText.Text) || serviceText.Text == "None")
            {
                MessageBox.Show("Не все поля заполнены!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (birthdayData.Value.Year < 1900 || notifyData.Value.Year < 1900)
            {
                MessageBox.Show("Неверно заполнены дат(а/ы)!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;

            }
            if (databaseManager.Request($"SELECT ID FROM Clients WHERE 'ФИО' = '{FIOText.Text}';").Rows.Count > 0)
            {
                MessageBox.Show("Человек с таким именем уже существует!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int currentID = Convert.ToInt32(databaseManager.Request("SELECT COUNT(ID) FROM Clients;").Rows[0][0]) + 1;
            string req = $"INSERT INTO Clients (ID, `ФИО`, `Телефон`, `Адрес`, `Дата рождения`, `Тип услуг`, `Уведомление`) VALUES ({currentID}, '{FIOText.Text}', '{phoneText.Text}', '{addressText.Text}', '{birthdayData.Value.Year}-{birthdayData.Value.Month}-{birthdayData.Value.Day}', '{serviceText.Text}', '{notifyData.Value.Year}-{notifyData.Value.Month}-{notifyData.Value.Day}');";
            Console.WriteLine(req);
            databaseManager.Request(req);
            SelectData();
            ClearInputs();
        }

        void DeleteRow(string fio, string table)
        {
            int id = Convert.ToInt32(databaseManager.Request($"SELECT ID FROM {table} WHERE `ФИО` = '{fio}';").Rows[0][0]);
            databaseManager.Request($"DELETE FROM {table} WHERE `ФИО` = '{fio}' AND ID = {id}");
            for (global::System.Int32 i = id; i <= databaseManager.Request($"SELECT * FROM {table};").Rows.Count; i++)
                databaseManager.Request($"UPDATE {table} SET ID = {i} WHERE ID = {i + 1};");
        }


        public void SelectData()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = databaseManager.Request("SELECT * FROM Clients;");
        }

        void SetNotifyDate()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                List<DateTime> dateTimes = new List<DateTime>();
                string currentFIO = dataGridView1.Rows[i].Cells[1].Value.ToString();
                if (databaseManager.CheckExistingOfThisPersonInTable(currentFIO, "Sewing"))
                    dateTimes.Add((DateTime)databaseManager.Request($"SELECT `Дата` FROM Sewing WHERE `ФИО` = '{currentFIO}';").Rows[0][0]);
                if (databaseManager.CheckExistingOfThisPersonInTable(currentFIO, "Physical_Therapy"))
                    dateTimes.Add((DateTime)databaseManager.Request($"SELECT `Дата` FROM Physical_Therapy WHERE `ФИО` = '{currentFIO}';").Rows[0][0]);
                if (databaseManager.CheckExistingOfThisPersonInTable(currentFIO, "Massage"))
                    dateTimes.Add((DateTime)databaseManager.Request($"SELECT `Дата` FROM Massage WHERE `ФИО` = '{currentFIO}';").Rows[0][0]);
                if (dateTimes.Count == 0)
                    dateTimes.Add((DateTime)databaseManager.Request($"SELECT `Уведомление` FROM Clients WHERE `ФИО` = '{currentFIO}';").Rows[0][0]);

                dateTimes.Sort();
                foreach (DateTime date in dateTimes)
                {
                    if (date >= DateTime.Today)
                    {
                        databaseManager.Request($"UPDATE Clients SET `Уведомление` = '{date:yyyy-MM-dd}' WHERE `ФИО` = '{currentFIO}';");
                        break;
                    }
                }
            }
            SelectData();
        }

        void ClearInputs()
        {
            FIOText.Clear();
            phoneText.Clear();
            addressText.Clear();
            birthdayData.Value = birthdayData.MinDate;
            serviceText.Text = "None";
            notifyData.Value = notifyData.MinDate;
        }
    }
}
