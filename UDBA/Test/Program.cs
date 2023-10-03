using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDBA;

namespace Test
{
    class Program
    {
        private enum DataBaseType
        {
            Oracle,
            MSSQL,
            SQLite
        };

        private static string sqlParameterFormat;

        static void Main(string[] args)
        {
            DataBaseType dataBaseType = DataBaseType.SQLite;
            ConnectionStringSettings connectionStringSettings = GetConnectionStringSettings(dataBaseType);
            initSQLParameter(dataBaseType);

            try
            {
                DatabaseAccess.CreateDbConnection(connectionStringSettings);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ReadKey();
                Environment.Exit(0);
            }
            DatabaseAccess.OpenDatabase();
            if (dataBaseType == DataBaseType.SQLite)
            {
                DatabaseAccess.BeginTransaction();
                CreateDatabaseSQLite();
                DatabaseAccess.CommitTransaction();
            }
            if (dataBaseType == DataBaseType.MSSQL)
            {
                DatabaseAccess.BeginTransaction();
                CreateDatabaseMSSQL();
                DatabaseAccess.CommitTransaction();
            }
            if (dataBaseType == DataBaseType.Oracle)
            {
                DatabaseAccess.BeginTransaction();
                CreateDatabaseOracle();
                DatabaseAccess.CommitTransaction();
            }

            int nb = DatabaseAccess.ExecuteScalar<int>("SELECT COUNT(*) FROM CONTACTS");
            Console.WriteLine($"nb lines in contacts : {nb}");

            Console.WriteLine();
            Console.WriteLine("Tous les contacts");
            DbDataReader reader = DatabaseAccess.ExecuteReader("SELECT * FROM CONTACTS");
            while (reader.Read())
            {
                Console.WriteLine($"Id={reader.Get<int>("Id", 0)}, Name={reader.Get<string>("Name")}, FirstName={reader.Get<string>("FirstName")}, Actif={reader.Get<bool>("Actif")}");
                Console.WriteLine("Avec index de colonne");
                Console.WriteLine($"Id={reader.Get<int>(0, 0)}, Name={reader.Get<string>(1)}, FirstName={reader.Get<string>(2)}, Actif={reader.Get<bool>(3)}");
            }
            reader.Close();

            Console.WriteLine();
            Console.WriteLine("Tous les contacts qui ont pour prénom John et qui sont actifs");
            List<DbParameter> parameters = new List<DbParameter>
            {
                DatabaseAccess.CreateParameter($"{sqlParameterFormat}FirstName", "John"),
                DatabaseAccess.CreateParameter($"{sqlParameterFormat}Actif", 1)
            };
            reader = DatabaseAccess.ExecuteReader($"SELECT * FROM CONTACTS WHERE FirstName = {sqlParameterFormat}FirstName AND Actif = {sqlParameterFormat}Actif", parameters);
            while (reader.Read())
            {
                Console.WriteLine($"Id={reader.Get<int>("Id", 0)}, Name={reader.Get<string>("Name")}, Actif={reader.Get<bool>("Actif")}");
            }
            reader.Close();

            Console.WriteLine();
            Console.WriteLine("Datatable");
            parameters = new List<DbParameter>
            {
                DatabaseAccess.CreateParameter($"{sqlParameterFormat}FirstName", "John"),
                DatabaseAccess.CreateParameter($"{sqlParameterFormat}Actif", 1)
            };
            DataTable dataTable = DatabaseAccess.GetDataTable($"SELECT * FROM CONTACTS WHERE FirstName = {sqlParameterFormat}FirstName AND Actif = {sqlParameterFormat}Actif", parameters);
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine($"Id={row["Id"]}, Name={row["Name"]}, Actif={row["Actif"]}");
            }

            if (dataBaseType != DataBaseType.SQLite)
            {
                DatabaseAccess.ExecuteNonQuery("DROP TABLE CONTACTS");
            }
            DatabaseAccess.CloseDatabase();

            Console.ReadKey();
        }

        /// <summary>
        /// Initialise le format du paramètre SQL en fonction du type de DB
        /// </summary>
        private static void initSQLParameter(DataBaseType dataBaseType)
        {
            switch (dataBaseType)
            {
                case DataBaseType.Oracle:
                    sqlParameterFormat = ":";
                    break;
                default:
                    sqlParameterFormat = "@";
                    break;
            }
        }


        private static ConnectionStringSettings GetConnectionStringSettings(DataBaseType dataBaseType)
        {
            ConnectionStringSettings connectionStringSettings;

            switch (dataBaseType)
            {
                case DataBaseType.Oracle:
                    connectionStringSettings = new ConnectionStringSettings("Oracle.ManagedDataAccess.Client", "Data Source=mydb;User Id=myuser;Password=mypassword;")
                    {
                        ProviderName = "Oracle.ManagedDataAccess.Client"
                    };
                    break;
                case DataBaseType.MSSQL:
                    connectionStringSettings = new ConnectionStringSettings("System.Data.SqlClient", "Server=localhost;Database=mydb;User Id=myuser;Password=mypassword;MultipleActiveResultSets=False;")
                    {
                        ProviderName = "System.Data.SqlClient"
                    };
                    break;
                default:
                    connectionStringSettings = new ConnectionStringSettings("System.Data.SqLite", "Data Source=:memory:;Version=3;New=True;")
                    {
                        ProviderName = "System.Data.SqLite"
                    };
                    break;

            }
            return connectionStringSettings;
        }

        private static void CreateDatabaseSQLite()
        {
            List<DbParameter> parameters = new List<DbParameter>();
            //*** Create table and data
            string sql = "CREATE TABLE CONTACTS (ID int, NAME char(50), FIRSTNAME char(50), ACTIF BOOLEAN)";
            DatabaseAccess.ExecuteNonQuery(sql);

            sql = "INSERT INTO CONTACTS (ID, NAME, FIRSTNAME, ACTIF) VALUES (@Id, @Name, @FirstName, @Actif)";

            parameters.Add(DatabaseAccess.CreateParameter("@Id", 1));
            parameters.Add(DatabaseAccess.CreateParameter("@Name", "Wayne"));
            parameters.Add(DatabaseAccess.CreateParameter("@FirstName", "John"));
            parameters.Add(DatabaseAccess.CreateParameter("@Actif", 0));
            DatabaseAccess.ExecuteNonQuery(sql, parameters);

            parameters.Clear();
            parameters.Add(DatabaseAccess.CreateParameter("@Id", 2));
            parameters.Add(DatabaseAccess.CreateParameter("@Name", "Doe"));
            parameters.Add(DatabaseAccess.CreateParameter("@FirstName", "John"));
            parameters.Add(DatabaseAccess.CreateParameter("@Actif", 1));
            DatabaseAccess.ExecuteNonQuery(sql, parameters);
        }

        private static void CreateDatabaseMSSQL()
        {
            List<DbParameter> parameters = new List<DbParameter>();
            //*** Create table and data
            string sql = "CREATE TABLE CONTACTS (ID int, NAME varchar(50), FIRSTNAME varchar(50), ACTIF int)";
            DatabaseAccess.ExecuteNonQuery(sql);

            sql = "INSERT INTO CONTACTS (ID, NAME, FIRSTNAME, ACTIF) VALUES (@Id, @Name, @FirstName, @Actif)";

            parameters.Add(DatabaseAccess.CreateParameter("@Id", 1));
            parameters.Add(DatabaseAccess.CreateParameter("@Name", "Wayne"));
            parameters.Add(DatabaseAccess.CreateParameter("@FirstName", "John"));
            parameters.Add(DatabaseAccess.CreateParameter("@Actif", 0));
            DatabaseAccess.ExecuteNonQuery(sql, parameters);

            parameters.Clear();
            parameters.Add(DatabaseAccess.CreateParameter("@Id", 2));
            parameters.Add(DatabaseAccess.CreateParameter("@Name", "Doe"));
            parameters.Add(DatabaseAccess.CreateParameter("@FirstName", "John"));
            parameters.Add(DatabaseAccess.CreateParameter("@Actif", 1));
            DatabaseAccess.ExecuteNonQuery(sql, parameters);
        }

        private static void CreateDatabaseOracle()
        {
            if (DatabaseAccess.ExecuteScalar<int>("SELECT count(*) FROM user_tables WHERE table_name = 'CONTACTS'") == 1) return;
            List<DbParameter> parameters = new List<DbParameter>();
            //*** Create table and data
            string sql = "CREATE TABLE CONTACTS (ID number(10), NAME varchar2(50), FIRSTNAME varchar2(50), ACTIF number(1))";
            DatabaseAccess.ExecuteNonQuery(sql);

            sql = "INSERT INTO CONTACTS (ID, NAME, FIRSTNAME, ACTIF) VALUES (:Id, :Name, :FirstName, :Actif)";

            parameters.Add(DatabaseAccess.CreateParameter(":Id", 1));
            parameters.Add(DatabaseAccess.CreateParameter(":Name", "Wayne"));
            parameters.Add(DatabaseAccess.CreateParameter(":FirstName", "John"));
            parameters.Add(DatabaseAccess.CreateParameter(":Actif", 0));
            DatabaseAccess.ExecuteNonQuery(sql, parameters);

            parameters.Clear();
            parameters.Add(DatabaseAccess.CreateParameter(":Id", 2));
            parameters.Add(DatabaseAccess.CreateParameter(":Name", "Doe"));
            parameters.Add(DatabaseAccess.CreateParameter(":FirstName", "John"));
            parameters.Add(DatabaseAccess.CreateParameter(":Actif", 1));
            DatabaseAccess.ExecuteNonQuery(sql, parameters);
        }

    }

}
