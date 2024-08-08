using System;
using System.IO;

namespace OX_DB
{
    internal class FileManager
    {

        static public string CopyFile(string source, string destination) // func: copy file to local folder
        {
            EmailSender sender = new EmailSender();
            string destinationFilePath = Path.Combine(destination, Path.GetFileName(source));
            if (File.Exists(destinationFilePath)) // if file already exists return its path
                return @destinationFilePath.Replace(@"\", @"\\");
            try // copy file and return its path
            {
                File.Copy(source, destinationFilePath, true);
                return @destinationFilePath.Replace(@"\", @"\\");
            }
            catch (Exception ex)
            {
                sender.PrintException(ex, "Ошибка копирования");
                return string.Empty;
            }
        }

        static public void DeleteFile(string path)
        {
            if (File.Exists(@path))
                File.Delete(@path);
           
        }

    }
}
