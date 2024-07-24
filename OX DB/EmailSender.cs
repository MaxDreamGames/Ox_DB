using System;
using System.Net;
using System.Net.Mail;

namespace OX_DB
{
    internal class EmailSender
    {
        private string fromAddress = "max.sender.code@yandex.ru";
        private string fromPass = "bjhqxzxtsgrrvsxr";
        private string toAddress = "maxdreamgames@gmail.com";
        private string header;
        private string body;
        private string sender;

        public EmailSender() { }
        public EmailSender(string header, string body, string sender)
        {
            this.header = header;
            this.sender = sender;
            this.body = $"Отправитель: {sender}\n\n" +
                $"{body}";
        }

        public void SendEmail()
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(this.fromAddress);
            mail.To.Add(new MailAddress(this.toAddress));
            mail.Subject = this.header;
            mail.Body = this.body;
            Console.WriteLine(mail.Body);
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
            Console.WriteLine(mail.Body);
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
    }
}
