using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace pcb_monitoring_program.DatabaseManager
{
    public static class DB
    {
        private static readonly string _connStr =
            "Server=100.80.24.53;Port=3306;Database=pcb_inspection;Uid=pcb_admin;Pwd=1234;Charset=utf8mb4;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connStr);
        }
    }
}
