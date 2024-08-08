using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OX_DB
{
    public partial class Sewing : Form
    {
        public int ID { get; set; }
        public Main ParentFm { get; set; }
        Color closeBtnColor;
        bool isDraging = false;
        Point startPoint = new Point(0, 0);
        DatabaseManager databaseManager = new DatabaseManager();
        EmailSender emailSender = new EmailSender();
        Dictionary<string, string> dataDict = new Dictionary<string, string>();
        bool isExist = false;
        Panel[] panels;
        TextBox[] textBoxesOfFirstMeeting;
        string[] dataString;
        string imageOfTechnicalDrowingPath;
        string imageOfAvatar;
        string imageOfReadyProduct;

        public Sewing()
        {
            InitializeComponent();
        }

        private void Sewing_Load(object sender, EventArgs e)
        {
            closeBtnColor = closeBtn.BackColor;
            label1.Text = Name;

            panels = new Panel[3] { firstMeettingPanel, fittingPanel, readyPanel };
            textBoxesOfFirstMeeting = new TextBox[25] { OSh, DP, OG, OT, OB, DI, DPT, VGr, VGK, CGr, Shgrm, Shgrb, DST, VPrZ, DSK, ShS, DB, Rzv, Rbv, Rzn, Rbn, Rpn, SBZ, L_Ya, G_Zh };

            // check existing of the person in sewing table
            DataRow rowOfMainInf = databaseManager.Request($"SELECT `ФИО`, `Уведомление` FROM Clients WHERE ID = {this.ID}").Rows[0];
            string fio = rowOfMainInf.ItemArray[0].ToString();
            DateTime dateTime = (DateTime)rowOfMainInf[1];
            DataTable neededTable = databaseManager.Request($"SELECT * FROM Sewing WHERE `ФИО` = '{fio}';");
            FIO.Text = fio;
            if (neededTable.Rows.Count == 0) // if the person doesn't exist set dates from main table
            {
                date.Value = dateTime;
                firstMeetingDate.Value = dateTime;
            }
            else // else fill all fields with data
            {
                FillData(neededTable.Rows[0]);
                isExist = true;
                DeleteUselessImages();
            }
            // open first meeting panel
            firstMeettingPanel.Show();
            fittingPanel.Hide();
            readyPanel.Hide();
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
        private void backBtn_Click(object sender, EventArgs e)
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

        private void chooseBtn_Click(object sender, EventArgs e)
        {
            imageOfTechnicalDrowingPath = ChooseImagesFromFileSystem(openFileDialog1, technicalPicture);
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (!CheckForCorrectInput()) // check format of inputs
                return;

            imageOfTechnicalDrowingPath = FileManager.CopyFile(imageOfTechnicalDrowingPath, $"{Environment.CurrentDirectory}\\img"); // save image path after copying
            Console.WriteLine(imageOfTechnicalDrowingPath);
            if (CheckExistingOfThisPersonInSewing()) // check existing of the person in sewing table before saving
            {
                // update data in the table
                string updateQuery = GenerateSewingUpdateQuery(date.Value, OSh.Text.Replace(',', '.'), DP.Text.Replace(',', '.'), OG.Text.Replace(',', '.'), OT.Text.Replace(',', '.'), OB.Text.Replace(',', '.'), DI.Text.Replace(',', '.'),
                    DPT.Text.Replace(',', '.'), VGr.Text.Replace(',', '.'), VGK.Text.Replace(',', '.'), CGr.Text.Replace(',', '.'), Shgrm.Text.Replace(',', '.'), Shgrb.Text.Replace(',', '.'), DST.Text.Replace(',', '.'), VPrZ.Text.Replace(',', '.'),
                    DSK.Text.Replace(',', '.'), ShS.Text.Replace(',', '.'), DB.Text.Replace(',', '.'), Rzv.Text.Replace(',', '.'), Rbv.Text.Replace(',', '.'), Rzn.Text.Replace(',', '.'), Rbn.Text.Replace(',', '.'), Rpn.Text.Replace(',', '.'),
                    SBZ.Text.Replace(',', '.'), L_Ya.Text.Replace(',', '.'), G_Zh.Text.Replace(',', '.'), firstMeetingDate.Value, imageOfTechnicalDrowingPath, description.Text, null, null, null, null);
                Console.WriteLine(updateQuery);
                databaseManager.Request(updateQuery);
                databaseManager.Request($"UPDATE Clients SET `Уведомление` = '{date.Value:yyyy-MM-dd}' WHERE `ФИО` = '{FIO.Text}';");

                if (!CheckDataUpdating())
                {
                    MessageBox.Show("Не удалось сохранить данные!", "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                // add row in sewing
                AddNewRowToSewing();
            }
            MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ParentFm.SelectData();
        }

        private void firstMeeting_Click(object sender, EventArgs e)
        {
            ActivePanel(firstMeettingPanel);
        }

        private void fitting_Click(object sender, EventArgs e)
        {
            if (!CheckExistingOfThisPersonInSewing())
            {

                MessageBox.Show("Сначала заполните данный о клиенте!", "Клиент не найден", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ActivePanel(firstMeettingPanel);
                return;
            }
            ActivePanel(fittingPanel);
            FIOInFitting.Text = FIO.Text;
            dateInFitting.Value = date.Value;
        }

        private void ready_Click(object sender, EventArgs e)
        {
            if (!CheckExistingOfThisPersonInSewing())
            {

                MessageBox.Show("Сначала заполните данный о клиенте!", "Клиент не найден", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ActivePanel(firstMeettingPanel);
                return;
            }
            ActivePanel(readyPanel);
            FIOInReadyPanel.Text = FIO.Text;
        }

        private void choosingBtnInFitting_Click(object sender, EventArgs e)
        {
            imageOfAvatar = ChooseImagesFromFileSystem(openFileDialog1, avatar); // choose avatar from file system and save its path
        }

        private void saveBtnInFitting_Click(object sender, EventArgs e)
        {
            if (avatar.BackgroundImage == null) // check filling of necessary
            {
                MessageBox.Show("Вставте изображение аватара!", "Ошибка вставки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            imageOfAvatar = FileManager.CopyFile(imageOfAvatar, $"{Environment.CurrentDirectory}\\img"); // save image path after copying
            Console.WriteLine(imageOfAvatar);
            databaseManager.Request(GenerateSewingUpdateQuery(new DateTime(1, 1, 1), null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                new DateTime(1, 1, 1), null, null, fitDescription.Text, @imageOfAvatar, null, null));
            MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void fittingPanel_Enter(object sender, EventArgs e)
        {

        }

        private void chooseBtnInReadyPanel_Click(object sender, EventArgs e)
        {
            imageOfReadyProduct = ChooseImagesFromFileSystem(openFileDialog1, readyProductPicture);
        }

        private void saveBtnInReadyPanel_Click(object sender, EventArgs e)
        {
            if (readyProductPicture.BackgroundImage == null) // check filling of necessary
            {
                MessageBox.Show("Вставте изображение аватара!", "Ошибка вставки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            imageOfReadyProduct = FileManager.CopyFile(imageOfReadyProduct, $"{Environment.CurrentDirectory}\\img"); // save image path after copying
            databaseManager.Request(GenerateSewingUpdateQuery(new DateTime(1, 1, 1), null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                new DateTime(1, 1, 1), null, null, null, null, review.Text, @imageOfReadyProduct));
            MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void печатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printDocument1.DefaultPageSettings.Landscape = true;
            printDocument1.DefaultPageSettings.Color = true;
            printPreviewDialog1.ShowDialog();
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawString(label31.Text, label31.Font, Brushes.Black, e.PageBounds.Width / 2 - label31.Width / 2, label31.Location.Y);
            e.Graphics.DrawString(label34.Text, label34.Font, Brushes.Black, label34.Location);
            e.Graphics.DrawString(FIO.Text, FIO.Font, Brushes.Black, label34.Location.X + label34.Width + 15, label34.Location.Y);
            e.Graphics.DrawString(label35.Text, label35.Font, Brushes.Black, label35.Location);
            e.Graphics.DrawString($"{dateInFitting.Value:dd.MM.yyyy}", FIO.Font, Brushes.Black, dateInFitting.Location);
            e.Graphics.DrawString(label33.Text, label33.Font, Brushes.Black, e.PageBounds.Width / 4 - label33.Width / 2, label33.Location.Y);
            SizeF size = new SizeF(e.PageBounds.Width / 2 - 50, e.PageBounds.Height / 2 + e.PageBounds.Height / 4);
            StringFormat format = new StringFormat();
            format.FormatFlags = StringFormatFlags.FitBlackBox;

            SizeF strSize = new SizeF();
            strSize = e.Graphics.MeasureString(fitDescription.Text, fitDescription.Font, size, format);
            // Draw rectangle representing size of string.
            e.Graphics.DrawRectangle(new Pen(Color.White, 1), 0.0F, fitDescription.Location.Y, strSize.Width, strSize.Height);

            PointF currentLinePos = new PointF(10, fitDescription.Location.Y);
            // Draw string to screen.
            Bitmap bm = new Bitmap(fitDescription.Width, fitDescription.Height);
            fitDescription.DrawToBitmap(bm, new Rectangle(0, 0, fitDescription.Width, fitDescription.Height));
            Bitmap bm2 = new Bitmap(bm, Convert.ToInt32(fitDescription.Width * 0.8f), Convert.ToInt32(fitDescription.Height * 0.8f));
            e.Graphics.DrawImage(bm2, currentLinePos);
            float w = e.PageBounds.Width / 2 - 50;
            int h = e.PageBounds.Height - 90;
            double scale = Math.Min(Convert.ToDouble(w) / Convert.ToDouble(avatar.BackgroundImage.Width), Convert.ToDouble(h) / Convert.ToDouble(avatar.BackgroundImage.Height));
            Console.WriteLine(w / avatar.BackgroundImage.Width);
            Console.WriteLine((Convert.ToDouble(h) / Convert.ToDouble(avatar.BackgroundImage.Height)));
            Console.WriteLine(scale);
            Bitmap bitmap = new Bitmap(avatar.BackgroundImage, Convert.ToInt32(avatar.BackgroundImage.Width * scale), Convert.ToInt32(avatar.BackgroundImage.Height * scale));

            e.Graphics.DrawImage(bitmap, (e.PageBounds.Width / 4) * 3 - bitmap.Width / 2, e.PageBounds.Height / 2 - bitmap.Height / 2); // TODO: Image


        }


        private void fittingPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                fittingMenuStrip.Show(fittingPanel, e.Location);
        }


        void ActivePanel(Panel panel)
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

        void FillData(DataRow dataTable) // fill all fields with data from sewing table
        {
            date.Value = (DateTime)dataTable[2];
            for (int i = 3; i <= 27; i++)
                textBoxesOfFirstMeeting[i - 3].Text = dataTable[i].ToString();
            firstMeetingDate.Value = (DateTime)dataTable[28];
            imageOfTechnicalDrowingPath = @dataTable[29].ToString();
            if (imageOfTechnicalDrowingPath.Length > 0)
                technicalPicture.BackgroundImage = new Bitmap(imageOfTechnicalDrowingPath);
            description.Text = dataTable[30].ToString();
            fitDescription.Text = dataTable[31].ToString();
            imageOfAvatar = @dataTable[32].ToString();
            if (imageOfAvatar.Length > 0)
                avatar.BackgroundImage = new Bitmap(imageOfAvatar);
            review.Text = dataTable[33].ToString();
            imageOfReadyProduct = @dataTable[34].ToString();
            if (imageOfReadyProduct.Length > 0)
                readyProductPicture.BackgroundImage = new Bitmap(imageOfReadyProduct);
        }

        bool CheckExistingOfThisPersonInSewing() // func: check existing of the person in sewing table
        {
            if (databaseManager.Request($"SELECT * FROM Sewing WHERE `ФИО` = '{FIO.Text}'").Rows.Count > 0)
                return true;
            return false;
        }

        bool CheckDataUpdating() // check the succes of the updating
        {
            if (!CheckExistingOfThisPersonInSewing())
                return false;

            DataRow row = databaseManager.Request($"SELECT * FROM Sewing WHERE `ФИО` = '{FIO.Text}'").Rows[0];
            for (int i = 2; i < row.ItemArray.Length; i++)
            {
                Console.WriteLine(i + " " + row[i]);
                if (string.IsNullOrEmpty(row[i].ToString()))
                    continue;
                if (i == 2)
                {
                    if (row[i].ToString() != date.Value.ToString()) return false;
                    else continue;
                }
                if (i - 3 < textBoxesOfFirstMeeting.Length)
                {
                    Console.WriteLine(i + " " + textBoxesOfFirstMeeting[i - 3].Text.Replace('.', ','));
                    if (row[i].ToString() != textBoxesOfFirstMeeting[i - 3].Text.Replace('.', ',')) return false;
                    else continue;
                }
                if (i == 28)
                {
                    if (row[i].ToString() != firstMeetingDate.Value.ToString()) return false;
                    else continue;
                }
                if (i == 29)
                {
                    if (!CompareImages(Image.FromFile(@row[i].ToString()), technicalPicture.BackgroundImage)) return false;
                    else continue;
                }
                if (i == 30)
                {
                    if (row[i].ToString() != description.Text) return false;
                    else continue;
                }
                if (i == 31)
                {
                    if (row[i].ToString() != fitDescription.Text) return false;
                    else continue;
                }
                if (i == 32)
                {
                    if (!CompareImages(Image.FromFile(@row[i].ToString()), avatar.BackgroundImage)) return false;
                    else continue;
                }
                if (i == 33)
                {
                    if (row[i].ToString() != review.Text) return false;
                    else continue;
                }
                if (i == 34)
                {
                    if (!CompareImages(Image.FromFile(@row[i].ToString()), readyProductPicture.BackgroundImage)) return false;
                    else continue;
                }

            }
            return true;
        }

        bool CheckForCorrectInput() // func: check format of inputs
        {
            if (date.Value < firstMeetingDate.Value || technicalPicture.BackgroundImage == null)
            {
                MessageBox.Show("Неверная дата либо отсутствует рисунок!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            foreach (var item in textBoxesOfFirstMeeting)
            {
                if (string.IsNullOrEmpty(item.Text))
                {
                    MessageBox.Show("Не все поля заполнены!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!float.TryParse(item.Text, out float value))
                {
                    MessageBox.Show("Поля не соответствуют требуемуму типу!\nПишите десятичные числа через запятую.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }

        void AddNewRowToSewing() // func: add a new row in sewing table
        {
            int currentID = Convert.ToInt32(databaseManager.Request("SELECT COUNT(ID) FROM Sewing;").Rows[0][0]) + 1; // get id of the new row
            // init request
            string requestStr = $"INSERT INTO Sewing (`ID`, `ФИО`, `Дата`, `ОШ`, `ДП`, `ОГ`, `ОТ`, `ОБ`, `ДИ`, `ДПТ`, `ВГр`, `ВГК`, `ЦГр`, `Шгрм`, `Шгрб`, `ДСТ`, `ВПрЗ`, `ДСК`, `ШС`, `ДБ`, `Рзв`, `Рбв`, `Рзн`, " +
                $"`Рбн`, `Рпн`, `СБЗ`, `Л-Я`, `Г-Ж`, `1 встреча`, `Тех. рисунок`, `Описание`, `Описание посадки`, `Аватар`, `Отзыв`, `Готовое изделие`) " +
                $"VALUES ({currentID}, '{FIO.Text}', '{date.Value.Year}-{date.Value.Month}-{date.Value.Day}', ";
            foreach (var item in textBoxesOfFirstMeeting)
                requestStr += item.Text + ", ";
            requestStr += $"'{firstMeetingDate.Value.Year}-{firstMeetingDate.Value.Month}-{firstMeetingDate.Value.Day}', '{@imageOfTechnicalDrowingPath}', '{description.Text}', '', '', '', '');";
            Console.WriteLine(requestStr);
            // request to db
            try
            {
                databaseManager.Request(requestStr);
            }
            catch (Exception ex)
            {
                emailSender.PrintException(ex, "Ошибка запроса к БД");
            }
        }

        void DeleteUselessImages() // delete the images which aren't used from a local folder
        {
            DataTable imgs = databaseManager.Request($"SELECT `Тех. рисунок`, `Аватар`, `Готовое изделие` FROM Sewing;");
            string[] paths = Directory.GetFiles(Environment.CurrentDirectory + "\\img");
            Dictionary<string, int> map = new Dictionary<string, int>();
            for (int i = 0; i < paths.Length; i++)
            {
                map[paths[i]] = 0;
                foreach (DataRow row in imgs.Rows)
                {
                    for (int j = 0; j < imgs.Columns.Count; j++)
                    {
                        if (paths[i] == row[j].ToString())
                            map[paths[i]]++;
                    }
                    Console.WriteLine(paths[i] + ": " + map[paths[i]]);
                }
                if (map[paths[i]] == 0)
                {
                    try
                    {
                        FileManager.DeleteFile(paths[i]);
                        map.Remove(paths[i]);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка очистки", "Ошибка очистки", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }
            }
        }

        string ChooseImagesFromFileSystem(OpenFileDialog openFileDialog, PictureBox pictureBox) // user chooses image file from his files and this func returns its path
        {
            using (OpenFileDialog fileDialog = openFileDialog) // choosing image file
            {
                fileDialog.InitialDirectory = "c:\\";
                fileDialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
                fileDialog.FilterIndex = 0;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (pictureBox.BackgroundImage != null)
                    {
                        pictureBox.BackgroundImage.Dispose();
                        pictureBox.BackgroundImage = null;
                    }
                    using (FileStream fs = new FileStream(fileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {

                        pictureBox.BackgroundImage = Image.FromStream(fs);
                    }
                    return fileDialog.FileName; // save image path to global var
                }
                return null;
            }
        }

        string GenerateSewingUpdateQuery(DateTime date, string osh, string dp, string og, string ot,
         string ob, string di, string dpt, string vgr, string vgk, string cgr, string shgrm, string shgrb, string dst,
         string vprz, string dsk, string shs, string db, string rzv, string rbv, string rzn, string rbn, string rpn,
         string sbz, string lya, string gzh, DateTime firstMeeting, string techDrawing, string description,
         string fitDescription, string avatar, string review, string finishedProduct) // generate the query for updating sewing table
        {
            string query = "UPDATE Sewing SET ";
            bool first = true;

            if (date != new DateTime(1, 1, 1))
            {
                query += first ? "" : ", ";
                query += $"`Дата` = '{date:yyyy-MM-dd}'";
                first = false;
            }
            if (osh != null)
            {
                query += first ? "" : ", ";
                query += $"`ОШ` = {osh}";
                first = false;
            }
            if (dp != null)
            {
                query += first ? "" : ", ";
                query += $"`ДП` = {dp}";
                first = false;
            }
            if (og != null)
            {
                query += first ? "" : ", ";
                query += $"`ОГ` = {og}";
                first = false;
            }
            if (ot != null)
            {
                query += first ? "" : ", ";
                query += $"`ОТ` = {ot}";
                first = false;
            }
            if (ob != null)
            {
                query += first ? "" : ", ";
                query += $"`ОБ` = {ob}";
                first = false;
            }
            if (di != null)
            {
                query += first ? "" : ", ";
                query += $"`ДИ` = {di}";
                first = false;
            }
            if (dpt != null)
            {
                query += first ? "" : ", ";
                query += $"`ДПТ` = {dpt}";
                first = false;
            }
            if (vgr != null)
            {
                query += first ? "" : ", ";
                query += $"`ВГр` = {vgr}";
                first = false;
            }
            if (vgk != null)
            {
                query += first ? "" : ", ";
                query += $"`ВГК` = {vgk}";
                first = false;
            }
            if (cgr != null)
            {
                query += first ? "" : ", ";
                query += $"`ЦГр` = {cgr}";
                first = false;
            }
            if (shgrm != null)
            {
                query += first ? "" : ", ";
                query += $"`Шгрм` = {shgrm}";
                first = false;
            }
            if (shgrb != null)
            {
                query += first ? "" : ", ";
                query += $"`Шгрб` = {shgrb}";
                first = false;
            }
            if (dst != null)
            {
                query += first ? "" : ", ";
                query += $"`ДСТ` = {dst}";
                first = false;
            }
            if (vprz != null)
            {
                query += first ? "" : ", ";
                query += $"`ВПрЗ` = {vprz}";
                first = false;
            }
            if (dsk != null)
            {
                query += first ? "" : ", ";
                query += $"`ДСК` = {dsk}";
                first = false;
            }
            if (shs != null)
            {
                query += first ? "" : ", ";
                query += $"`ШС` = {shs}";
                first = false;
            }
            if (db != null)
            {
                query += first ? "" : ", ";
                query += $"`ДБ` = {db}";
                first = false;
            }
            if (rzv != null)
            {
                query += first ? "" : ", ";
                query += $"`Рзв` = {rzv}";
                first = false;
            }
            if (rbv != null)
            {
                query += first ? "" : ", ";
                query += $"`Рбв` = {rbv}";
                first = false;
            }
            if (rzn != null)
            {
                query += first ? "" : ", ";
                query += $"`Рзн` = {rzn}";
                first = false;
            }
            if (rbn != null)
            {
                query += first ? "" : ", ";
                query += $"`Рбн` = {rbn}";
                first = false;
            }
            if (rpn != null)
            {
                query += first ? "" : ", ";
                query += $"`Рпн` = {rpn}";
                first = false;
            }
            if (sbz != null)
            {
                query += first ? "" : ", ";
                query += $"`СБЗ` = {sbz}";
                first = false;
            }
            if (lya != null)
            {
                query += first ? "" : ", ";
                query += $"`Л-Я` = {lya}";
                first = false;
            }
            if (gzh != null)
            {
                query += first ? "" : ", ";
                query += $"`Г-Ж` = {gzh}";
                first = false;
            }
            if (firstMeeting != new DateTime(1, 1, 1))
            {
                query += first ? "" : ", ";
                query += $"`1 встреча` = '{firstMeeting:yyyy-MM-dd}'";
                first = false;
            }
            if (techDrawing != null)
            {
                query += first ? "" : ", ";
                query += $"`Тех. рисунок` = '{techDrawing}'";
                first = false;
            }
            if (description != null)
            {
                query += first ? "" : ", ";
                query += $"`Описание` = '{description}'";
                first = false;
            }
            if (fitDescription != null)
            {
                query += first ? "" : ", ";
                query += $"`Описание посадки` = '{fitDescription}'";
                first = false;
            }
            if (avatar != null)
            {
                query += first ? "" : ", ";
                query += $"`Аватар` = '{avatar}'";
                first = false;
            }
            if (review != null)
            {
                query += first ? "" : ", ";
                query += $"`Отзыв` = '{review}'";
                first = false;
            }
            if (finishedProduct != null)
            {
                query += first ? "" : ", ";
                query += $"`Готовое изделие` = '{finishedProduct}'";
                first = false;
            }

            query += $" WHERE `ФИО` = '{FIO.Text}'";

            return query;
        }

        private bool CompareImages(Image img1, Image img2)
        {
            if (img1 == null || img2 == null)
            {
                return false;
            }

            // Convert images to byte arrays
            byte[] img1Bytes = ImageToByteArray(img1);
            byte[] img2Bytes = ImageToByteArray(img2);

            // Compare byte arrays
            return StructuralComparisons.StructuralEqualityComparer.Equals(img1Bytes, img2Bytes);
        }
        private byte[] ImageToByteArray(Image img)
        {
            MemoryStream ms;
            using (ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
            }
            return ms.ToArray();
        }

    }
}
