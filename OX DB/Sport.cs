using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OX_DB
{
    public partial class Sport : Form
    {
        public int ID { get; set; }
        public Main ParentFm { get; set; }
        Color closeBtnColor;
        bool isDraging = false;
        Point startPoint = new Point(0, 0);
        Panel[] panels;
        public string name;
        public string mainTable;
        public string addTable;

        DatabaseManager databaseManager = new DatabaseManager();
        EmailSender emailSender = new EmailSender();


        public Sport()
        {
            InitializeComponent();
        }

        private void Sport_Load(object sender, EventArgs e)
        {
            tableName.Text = name;
            closeBtnColor = closeBtn.BackColor;
            label1.Text = Text;

            panels = new Panel[3] { dataPanel, anamnesPanel, programPanel };

            // check existing of the person in sewing table
            DataRow rowOfMainInf = databaseManager.Request($"SELECT `ФИО`, `Уведомление` FROM Clients WHERE ID = {this.ID}")?.Rows[0];
            string fio = rowOfMainInf.ItemArray[0].ToString();
            DateTime dateTime = (DateTime)rowOfMainInf[1];
            DataTable neededTable = databaseManager.Request($"SELECT * FROM {mainTable} WHERE `ФИО` = '{fio}';");
            FIO.Text = fio;
            if (neededTable.Rows.Count == 0) // if the person doesn't exist set dates from main table
            {
                date.Value = dateTime;
            }
            else // else fill all fields with data
            {
                FillData();
            }
            ActivePanel(dataPanel, panels);
        }

        protected void panel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Maximized;
        }

        protected void closeBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected void minimizeBtn_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        protected void closeBtn_MouseEnter(object sender, EventArgs e)
        {
            closeBtn.BackColor = Color.Red;
        }

        protected void closeBtn_MouseLeave(object sender, EventArgs e)
        {
            closeBtn.BackColor = closeBtnColor;
        }

        protected void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraging = true;
            startPoint = new Point(e.X, e.Y);
        }

        protected void panel1_MouseMove(object sender, MouseEventArgs e)
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

        protected void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDraging = false;
        }

        protected void backBtn_Click(object sender, EventArgs e)
        {
            ParentFm.Show();
            Close();
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

        protected void data_Click(object sender, EventArgs e)
        {
            ActivePanel(dataPanel, panels);
        }

        protected void anamnes_Click(object sender, EventArgs e)
        {
            if (!databaseManager.CheckExistingOfThisPersonInTable(FIO.Text, mainTable))
            {

                MessageBox.Show("Сначала заполните данный о клиенте!", "Клиент не найден", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ActivePanel(dataPanel, panels);
                return;
            }
            ActivePanel(anamnesPanel, panels);
        }

        public void program_Click(object sender, EventArgs e)
        {
            if (!databaseManager.CheckExistingOfThisPersonInTable(FIO.Text, mainTable))
            {

                MessageBox.Show("Сначала заполните данный о клиенте!", "Клиент не найден", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ActivePanel(dataPanel, panels);
                return;
            }
            ActivePanel(programPanel, panels);
        }

        public void mCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            wCheckBox.Checked = false;
        }

        public void wCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            mCheckBox.Checked = false;
        }

        public void saveBtnInDataPanel_Click(object sender, EventArgs e)
        {
            if (!CheckInputData())
                return;

            if (databaseManager.CheckExistingOfThisPersonInTable(FIO.Text, mainTable))
            {
                databaseManager.Request($"UPDATE {mainTable} SET `Дата` = '{date.Value:yyyy-MM-dd}', `Пол` = '{(mCheckBox.Checked ? 'М' : 'Ж')}', `Рост` = {tall.Text} WHERE `ФИО` = '{FIO.Text}';");
            }
            else
            {
                AddRowToPTTable();
            }
            SaveTable();
        }

        public void addRowBtnInData_Click(object sender, EventArgs e)
        {
            AddRowToTable();
        }

        public void lableTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetTextBoxToLabel(sender as TextBox);
            }
        }

        public void removeRowBtnInData_Click(object sender, EventArgs e)
        {
            RemoveRowFromTable();
        }


        public void anamnesSaveBtn_Click(object sender, EventArgs e)
        {
            if (databaseManager.Request($"UPDATE {mainTable} SET `Анамнез` = '{anamnesTB.Text}' WHERE `ФИО` = '{FIO.Text}';") != null)
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void programSaveBtn_Click(object sender, EventArgs e)
        {
            if (databaseManager.Request($"UPDATE {mainTable} SET `Программа` = '{programTB.Text}' WHERE `ФИО` = '{FIO.Text}';") != null)
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        public void ActivePanel(Panel panel, Panel[] panels)
        {
            foreach (Panel p in panels)
            {
                if (p == panel)
                {
                    p.Show();
                    p.Focus();
                }
                else
                    p.Hide();
            }
        }

        public bool CheckInputData()
        {
            if (date.Value.Year < 1900)
            {
                MessageBox.Show("Неверная дата!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!mCheckBox.Checked && !wCheckBox.Checked || mCheckBox.Checked && wCheckBox.Checked)
            {
                MessageBox.Show("Неверный пол!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(tall.Text, out int value))
            {
                MessageBox.Show("Неверный рост!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        public void SetTextBoxToLabel(TextBox tb)
        {
            if (tb.Text.Contains("~"))
            {
                MessageBox.Show("Присустствует недопустимый символ '~'!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Label lbl = new Label
            {
                Text = tb.Text,
                Font = new Font(tb.Font.FontFamily, 13, FontStyle.Bold),
                Margin = tb.Margin,
                Width = tb.Width,
                Location = tb.Location,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tb.Parent?.Controls.Add(lbl);
            tb.Parent?.Controls.Remove(tb);
        }

        public void AddRowToTable()
        {
            int heightOfRow = PTTable.Height / PTTable.RowCount;
            PTTable.Height += heightOfRow;
            PTTable.RowCount++;
            Console.WriteLine(PTTable.RowCount);
            PTTable.RowStyles.Add(new RowStyle(SizeType.Absolute, PTTable.Height / (PTTable.RowCount - 1)));
            // Перебираем количество столбцов и добавляем в каждый TextBox
            for (int col = 0; col < PTTable.ColumnCount; col++)
            {
                // Создаем новый TextBox
                TextBox textBox = new TextBox
                {
                    Name = "textBox_" + PTTable.RowCount + "_" + col, // Задаем уникальное имя
                    Width = 90,
                    Font = new Font("Microsoft Sans Serif", 12),
                    TextAlign = HorizontalAlignment.Center,
                    Margin = new Padding(20)
                };
                // Добавляем TextBox в ячейку новой строки
                PTTable.Controls.Add(textBox, col, PTTable.RowCount - 1);
            }

            (PTTable.GetControlFromPosition(0, PTTable.RowCount - 1) as TextBox).KeyDown += lableTextBox_KeyDown;
            addRowBtnInData.Location = new Point(addRowBtnInData.Location.X, addRowBtnInData.Location.Y + heightOfRow);
            removeRowBtnInData.Location = new Point(removeRowBtnInData.Location.X, removeRowBtnInData.Location.Y + heightOfRow);
        }

        public void RemoveRowFromTable()
        {
            if (PTTable.RowCount <= 4)
                return;
            int heightOfRow = PTTable.Height / PTTable.RowCount;
            PTTable.RowCount--;
            PTTable.Height -= heightOfRow;
            PTTable.RowStyles.RemoveAt(PTTable.RowCount);
            addRowBtnInData.Location = new Point(addRowBtnInData.Location.X, addRowBtnInData.Location.Y - heightOfRow);
            removeRowBtnInData.Location = new Point(removeRowBtnInData.Location.X, removeRowBtnInData.Location.Y - heightOfRow);
        }

        public void AddRowToPTTable()
        {
            int currentID = Convert.ToInt32(databaseManager.Request($"SELECT COUNT(ID) FROM {mainTable};").Rows[0][0]) + 1; // get id of the new row
            string req = $"INSERT INTO {mainTable} (`ID`, `ФИО`, `Дата`, `Пол`, `Рост`, `Анамнез`, `Программа`) VALUES" +
                $" ({currentID}, '{FIO.Text}', '{date.Value:yyyy-MM-dd}', '{(mCheckBox.Checked ? mCheckBox.Text : wCheckBox.Text)}', {int.Parse(tall.Text)}, '', '');";
            databaseManager.Request(req);
        }

        public void FillData()
        {
            DataRow neccessaryRow = databaseManager.Request($"SELECT * FROM {mainTable} WHERE `ФИО` = '{FIO.Text}';").Rows[0];
            date.Value = (DateTime)neccessaryRow[2];
            if (neccessaryRow[3].ToString() == "М")
                mCheckBox.Checked = true;
            else
                wCheckBox.Checked = true;
            tall.Text = neccessaryRow[4].ToString();
            anamnesTB.Text = neccessaryRow[5].ToString();
            programTB.Text = neccessaryRow[6].ToString();
            if (Convert.ToInt32(databaseManager.Request($"SELECT COUNT(ID) FROM {addTable} WHERE `userId` = {neccessaryRow[0]};").Rows[0][0]) > 0)
                FillTable();
        }

        public void FillTable()
        {
            int userId = Convert.ToInt32(databaseManager.Request($"SELECT ID FROM {mainTable} WHERE `ФИО` = '{FIO.Text}';").Rows[0][0]);

            string[] theRest = databaseManager.Request($"SELECT `Остальное` FROM {addTable} WHERE `userId` = {userId};").Rows[0][0].ToString().Split('\n');
            for (global::System.Int32 j = 0; j < theRest.Length; j++)
            {
                Console.WriteLine(j + ": " + theRest[j]);
                if (string.IsNullOrEmpty(theRest[j].ToString()) || theRest[j] == " " || theRest[j] == "\n")
                    break;
                AddRowToTable();
                TextBox tb = PTTable.GetControlFromPosition(0, j + 4) as TextBox;
                string[] pair = theRest[j].Split('~');

                tb.Text = pair[0];
                SetTextBoxToLabel(tb);
            }
            for (int i = 1; i < PTTable.ColumnCount; i++)
            {
                DataRow currentRow = databaseManager.Request($"SELECT * FROM {addTable} WHERE `userId` = {userId} AND `Номер` = {i};").Rows[0];
                List<string> theRest1 = currentRow[6].ToString().Split('\n').ToList();
                //theRest.RemoveAt(theRest.Count - 1);
                for (global::System.Int32 j = 1; j < PTTable.RowCount; j++)
                {
                    if (j < 4)
                    {
                        (PTTable.GetControlFromPosition(i, j) as TextBox).Text = currentRow[j + 2].ToString() == "0" ? "" : currentRow[j + 2].ToString();
                    }
                    else
                    {
                        if (theRest[j - 4].Split('~').Length > 1)
                        {
                            string s = theRest1[j - 4].Split('~')[1].Replace('.', ',');
                            (PTTable.GetControlFromPosition(i, j) as TextBox).Text = s == "0" ? "" : s;
                        }
                    }
                }

            }
        }

        public void SaveTable()
        {
            int userId = Convert.ToInt32(databaseManager.Request($"SELECT ID FROM {mainTable} WHERE `ФИО` = '{FIO.Text}';").Rows[0][0]);
            int id = Convert.ToInt32(databaseManager.Request($"SELECT COUNT(ID) FROM {addTable} WHERE `userId` = {userId};").Rows[0][0]) + 1;
            if (id > 1)
            {
                for (global::System.Int32 i = 1; i < PTTable.ColumnCount; i++)
                {
                    string requestStr = $"UPDATE {addTable} SET ";
                    for (global::System.Int32 j = 1; j < 4; j++)
                    {
                        TextBox currentTB = (PTTable.GetControlFromPosition(i, j) as TextBox);
                        if (string.IsNullOrEmpty(currentTB.Text))
                            currentTB.Text = "0";

                        if (!float.TryParse(currentTB.Text, out float value) && j != 2)
                        {
                            MessageBox.Show("Неверное значение!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (j == 2)
                            requestStr += $"`{(PTTable.GetControlFromPosition(0, j) as Label).Text}` = '{currentTB.Text}', ";
                        else
                            requestStr += $"`{(PTTable.GetControlFromPosition(0, j) as Label).Text}` = {currentTB.Text.Replace(',', '.')}, ";
                        if (currentTB.Text == "0")
                            currentTB.Text = "";
                    }
                    requestStr += "`Остальное` = '";
                    for (global::System.Int32 j = 4; j < PTTable.RowCount; j++)
                    {
                        requestStr += $"{(PTTable.GetControlFromPosition(0, j) as Label).Text}~{(PTTable.GetControlFromPosition(i, j) as TextBox).Text.Replace(',', '.')}\n";
                    }
                    requestStr += $"' WHERE `userId` = {userId} AND `Номер` = {i};";
                    Console.WriteLine(requestStr);
                    if (databaseManager.Request(requestStr) != null)
                    {
                        if (i == PTTable.ColumnCount - 1)
                            MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                        return;
                }
            }
            else
            {
                for (global::System.Int32 i = 1; i < PTTable.ColumnCount; i++)
                {
                    if (Convert.ToInt32(databaseManager.Request($"SELECT COUNT(ID) FROM {addTable};").Rows[0][0]) == 0)
                        id = 1;
                    else
                        id = Convert.ToInt32(databaseManager.Request($"SELECT MAX(ID) FROM {addTable};").Rows[0][0]) + 1;
                    string requestStr = $"INSERT INTO {addTable} (ID, `userID`, `Номер`, `Вес`, `Давление`, `ЧСС`, `Остальное`) VALUES " +
                        $"({id}, {userId}, {i}";
                    string theRest = "";
                    for (global::System.Int32 j = 1; j < PTTable.RowCount; j++)
                    {
                        TextBox currentTB = PTTable.GetControlFromPosition(i, j) as TextBox;
                        if (j <= 3)
                        {
                            if (string.IsNullOrEmpty(currentTB.Text))
                            {
                                requestStr += ", 0";
                                continue;
                            }

                            if (!float.TryParse(currentTB.Text, out float value) && j != 2)
                            {
                                MessageBox.Show("Неверное значение!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            if (j == 2)
                                requestStr += $", '{currentTB.Text}'";
                            else
                                requestStr += $", {currentTB.Text.Replace(',', '.')}";
                        }
                        if (j > 3)
                        {
                            Label rowName = PTTable.GetControlFromPosition(0, j) as Label;
                            theRest += $"{rowName.Text}~{(string.IsNullOrEmpty(currentTB.Text) ? "0" : currentTB.Text)}\n";
                        }
                    }
                    theRest.TrimEnd('\n');
                    requestStr += $", '{theRest}');";
                    if (databaseManager.Request(requestStr) != null)
                    {
                        if (i == PTTable.ColumnCount - 1)
                            MessageBox.Show("Данные успешно сохранены", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                        return;
                }
            }
        }

    }
}
