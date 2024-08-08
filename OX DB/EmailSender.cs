using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;

namespace OX_DB
{
    internal class EmailSender
    {
        private string refer = "https://github.com/MaxDreamGames/Ox_DB";
        private string reportMsg;
        private string fromAddress = "max.sender.code@yandex.ru";
        private string fromPass = "bjhqxzxtsgrrvsxr";
        private string toAddress = "maxdreamgames@gmail.com";
        private string header;
        private string body;
        private string sender;
        public static Dictionary<string, string> user = new Dictionary<string, string>();


        public EmailSender()
        {
            this.reportMsg = $"Сообщение об ошибке будет отправлено разработчику. Когда ошибка будет исправлена, вам на почту придет ссылка на скачивание исправленной версии. Либо можете ожидать обновления по ссылке {refer}\n" +
            $"Просим прощения за неудобства)";
        }
        public EmailSender(string header, string body, string sender)
        {
            this.header = header;
            this.sender = sender;
            this.body = $"Отправитель: {sender}\n\n" +
                $"{body}";
            this.reportMsg = $"Сообщение об ошибке будет отправлено разработчику. Когда ошибка будет исправлена, вам на почту придет ссылка на скачивание исправленной версии. Либо можете ожидать обновления по ссылке {refer}\n" +
            $"Просим прощения за неудобства)";
        }

        public string GetReportMsg() { return this.reportMsg; }
        public string GetRefer() { return this.refer; }

        public void SendEmail()
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(this.fromAddress);
            mail.To.Add(new MailAddress(this.toAddress));
            mail.Subject = this.header;
            mail.Body = this.body;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.yandex.ru";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(fromAddress, fromPass);
            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public int SendCode(string toAddress, string header, string body)
        {
            Random rand = new Random();
            int code = rand.Next(100000, 999999);
            body += $"\nВаш код: {code}";
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(this.fromAddress);
            mail.To.Add(new MailAddress(toAddress));
            mail.Subject = header;
            mail.Body = body;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.yandex.ru";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(fromAddress, fromPass);
            try
            {
                smtp.Send(mail);
                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return -1;
            }
        }

        public void SendReport(Exception ex, string message = "")
        {
            if (!string.IsNullOrEmpty(message))
                message += "\n\n";
            string msg = message + ex.Message;
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

        public void PrintException(Exception ex, string caption)
        {
            MessageBox.Show("Неудача!\n" + ex.Message + "\n" + this.GetReportMsg(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.SendReport(ex, $"Отправитель: {user["Name"]}, {user["Email"]}");
        }
    }
}
