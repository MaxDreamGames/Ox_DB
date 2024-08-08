using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

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
            label1.Text = Name;
            textBoxes = new TextBox[3] { FIOText, phoneText, addressText };
            SelectData();
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
                try
                {
                    databaseManager.Request($"UPDATE Clients SET `{dataGridView1.SelectedCells[0].OwningColumn.Name}` = '{newStr}' WHERE ID = {e.RowIndex + 1};");

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Неверный тип данных!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dataGridView1.CancelEdit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Неверный тип данных!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dataGridView1.CancelEdit();
                }


            }
            else
            {
                try
                {
                    databaseManager.Request($"UPDATE Clients SET `{dataGridView1.SelectedCells[0].OwningColumn.Name}` = '{dataGridView1.SelectedCells[0].Value.ToString()}' WHERE ID = {e.RowIndex + 1};");

                }
                catch (Exception ex)
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
            ExerciseTherapy exerciseTherapy = new ExerciseTherapy();
            exerciseTherapy.ID = int.Parse(dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value.ToString());
            exerciseTherapy.ParentFm = this;
            exerciseTherapy.Show();
            Hide();
        }

        private void массажToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Massage massage = new Massage();
            massage.ID = int.Parse(dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value.ToString());
            massage.ParentFm = this;
            massage.Show();
            Hide();
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !dataGridView1.IsCurrentCellInEditMode)
            {
                var mb = MessageBox.Show("Вы действительно хотите удалить клиента?", "Подтвердите действие", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (mb == DialogResult.Cancel || mb == DialogResult.No)
                    return;
                if (databaseManager.Request($"SELECT * FROM Sewing WHERE `ФИО` = '{dataGridView1[1, dataGridView1.CurrentCell.RowIndex].Value.ToString()}';").Rows.Count > 0)
                    DeleteRow(dataGridView1[1, dataGridView1.CurrentCell.RowIndex].Value.ToString(), "Sewing");
                DeleteRow(dataGridView1[1, dataGridView1.CurrentCell.RowIndex].Value.ToString(), "Clients");
                SelectData();
            }
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
            try
            {
                string req = $"INSERT INTO Clients (ID, `ФИО`, `Телефон`, `Адрес`, `Дата рождения`, `Тип услуг`, `Уведомление`) VALUES ({currentID}, '{FIOText.Text}', '{phoneText.Text}', '{addressText.Text}', '{birthdayData.Value.Year}-{birthdayData.Value.Month}-{birthdayData.Value.Day}', '{serviceText.Text}', '{notifyData.Value.Year}-{notifyData.Value.Month}-{notifyData.Value.Day}');";
                Console.WriteLine(req);
                databaseManager.Request(req);
                SelectData();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Неудача!\n" + emailSender.GetReportMsg(), "Ошибка запроса БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                emailSender.SendReport(ex, $"Отправитель: {EmailSender.user["Name"]}, {EmailSender.user["Email"]}");
            }
            catch (Exception ex)
            {
                emailSender.PrintException(ex, "Ошибка запроса БД");
            }
        }

        void DeleteRow(string fio, string table)
        {

            try
            {
                int id = Convert.ToInt32(databaseManager.Request($"SELECT ID FROM {table} WHERE `ФИО` = '{fio}';").Rows[0][0]);
                databaseManager.Request($"DELETE FROM {table} WHERE `ФИО` = '{fio}'");
                for (global::System.Int32 i = id; i <= databaseManager.Request($"SELECT * FROM {table};").Rows.Count; i++)
                    databaseManager.Request($"UPDATE {table} SET ID = {i} WHERE ID = {i + 1};");
            }
            catch (Exception ex)
            {
                emailSender.PrintException(ex, "Ошибка запроса БД");
            }
        }


        public void SelectData()
        {
            dataGridView1.DataSource = databaseManager.Request("SELECT * FROM Clients;");
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && dataGridView1.CurrentCell != null)
            {
                if (!dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value.ToString().Contains("шв"))
                    швейкаToolStripMenuItem.Enabled = false;
                if (!dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value.ToString().Contains("ЛФК"))
                    лФКToolStripMenuItem.Enabled = false;
                if (!dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex].Cells[5].Value.ToString().Contains("мсж"))
                    массажToolStripMenuItem.Enabled = false;
                contextMenuStrip1.Show(dataGridView1, e.Location);
            }
        }
    }
}
