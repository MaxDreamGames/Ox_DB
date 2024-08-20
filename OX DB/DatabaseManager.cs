using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace OX_DB
{
    internal class DatabaseManager
    {
        EmailSender emailSender = new EmailSender();
        public MySqlConnection GetConnection(string host, int port, string database, string username, string password)
        {
            string connectionString = $"Server={host};Database={database};port={port};User Id={username};password={password};charset=utf8";
            MySqlConnection connection = new MySqlConnection(connectionString);
            return connection;
        }

        public MySqlConnection Connect()
        {
            MySqlConnection sqlConnection = GetConnection("localhost", 3306, "ox_db", "root", "0122");
            sqlConnection.Open();
            return sqlConnection;
        }

        public DataTable Request(string request, bool isSend = true)
        {
            try
            {
                MySqlConnection connection = Connect();
                MySqlCommand cmd = new MySqlCommand(request, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                connection.Close();
                return dt;
            }
            catch (Exception ex)
            {
                if (isSend)
                    emailSender.PrintException(ex, "Ошибка запроса к БД");
                return null;
            }
        }

        public bool CheckExistingOfThisPersonInTable(string FIO, string table) // func: check existing of the person in table by his fio
        {
            if (this.Request($"SELECT * FROM {table} WHERE `ФИО` = '{FIO}'").Rows.Count > 0)
                return true;
            return false;
        }
    }
}
