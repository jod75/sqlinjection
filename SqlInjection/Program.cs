using System;
using System.Data.SqlClient;

namespace SqlInjection
{
    class Program
    {
        private static string cnxString = new SqlConnectionStringBuilder()
        {
            InitialCatalog = "SqlInjection",
            IntegratedSecurity = true,
            DataSource = @"(localdb)\MSSQLLocalDB"
        }.ConnectionString;

        private static bool IsTableNameValid(string tableName)
        {
            var result = false;
            using (var cnx = new SqlConnection(cnxString))
            {
                using (var cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = $"SELECT 1 FROM [sys].[tables] WHERE [name] = @tableName";
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter()
                    {
                        ParameterName = "@tableName",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Size = 100,
                        SqlValue = $"Client_{tableName}",
                        Direction = System.Data.ParameterDirection.Input
                    });

                    cnx.Open();
                    int rc = 0;
                    var obj = cmd.ExecuteScalar();
                    if (obj != DBNull.Value && obj != null)
                    {
                        rc = (int)obj;
                    }

                    result = rc == 1;
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            // user enters valid data
            var clientName = "A";

            // User enters a malicious command
            //var clientName = "A]; insert into [client_a] (id, category) values (7, 'magenta');";

            if (IsTableNameValid(clientName))
            {

                using (var cnx = new SqlConnection(cnxString))
                {
                    using (var cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = $"SELECT [id], [category] FROM [Client_{clientName}]";
                        cmd.CommandType = System.Data.CommandType.Text;

                        cnx.Open();
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                Console.WriteLine($"id = {rdr.GetInt32(rdr.GetOrdinal("id"))},\tcategory = {rdr.GetString(rdr.GetOrdinal("category"))}");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Ring alarm.... Alert! Alert!");
            }

            Console.WriteLine("Press ENTER to finish.");
            Console.ReadLine();
        }
    }
}
