using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
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
        private static DatabaseAccess myDB;

        static void Main(string[] args)
        {
            myDB = new DatabaseAccess();
            DataBaseType dataBaseType = DataBaseType.SQLite;
            ConnectionStringSettings connectionStringSettings = GetConnectionStringSettings(dataBaseType);
            initSQLParameter(dataBaseType);

            try
            {
                myDB.CreateDbConnection(connectionStringSettings);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ReadKey();
                Environment.Exit(0);
            }
            myDB.OpenDatabase();
            if (dataBaseType == DataBaseType.SQLite)
            {
                myDB.BeginTransaction();
                CreateDatabaseSQLite();
                myDB.CommitTransaction();
            }
            if (dataBaseType == DataBaseType.MSSQL)
            {
                myDB.BeginTransaction();
                CreateDatabaseMSSQL();
                myDB.CommitTransaction();
            }
            if (dataBaseType == DataBaseType.Oracle)
            {
                myDB.BeginTransaction();
                CreateDatabaseOracle();
                myDB.CommitTransaction();
            }

            int nb = myDB.ExecuteScalar<int>("SELECT COUNT(*) FROM CONTACTS");
            Console.WriteLine($"nb lines in contacts : {nb}");

            Console.WriteLine();
            Console.WriteLine("Tous les contacts");
            DbDataReader reader = myDB.ExecuteReader("SELECT * FROM CONTACTS");
            while (reader.Read())
            {
                Console.WriteLine($"Id={reader.Get<int>("Id", 0)}, Name={reader.Get<string>("Name")}, FirstName={reader.Get<string>("FirstName","")}, Actif={reader.Get<bool>("Actif")}");
                Console.WriteLine("Avec index de colonne");
                Console.WriteLine($"Id={reader.Get<int>(0, 0)}, Name={reader.Get<string>(1)}, FirstName={reader.Get<string>(2)}, Actif={reader.Get<bool>(3)}");
            }
            reader.Close();

            Console.WriteLine();
            Console.WriteLine("Tous les contacts qui ont pour prénom John et qui sont actifs");
            List<DbParameter> parameters = new List<DbParameter>
            {
                myDB.CreateParameter($"{sqlParameterFormat}FirstName", "John"),
                myDB.CreateParameter($"{sqlParameterFormat}Actif", 1)
            };
            reader = myDB.ExecuteReader($"SELECT * FROM CONTACTS WHERE FirstName = {sqlParameterFormat}FirstName AND Actif = {sqlParameterFormat}Actif", parameters);
            while (reader.Read())
            {
                Console.WriteLine($"Id={reader.Get<int>("Id", 0)}, Name={reader.Get<string>("Name")}, Actif={reader.Get<bool>("Actif")}");
                DbDataReader reader2 = myDB.ExecuteReader($"SELECT * FROM CONTACTS WHERE Id = {reader.Get<int>("Id", 0)}");
                if (reader2.Read()) {
                    Console.WriteLine($"Reader2 Id={reader2.Get<int>("Id", 0)}, Name={reader2.Get<string>("Name")}, Actif={reader2.Get<bool>("Actif")}");
                }
                reader2.Close();
            }
            reader.Close();

            Console.WriteLine();
            Console.WriteLine("Datatable");
            parameters = new List<DbParameter>
            {
                myDB.CreateParameter($"{sqlParameterFormat}FirstName", "John"),
                myDB.CreateParameter($"{sqlParameterFormat}Actif", 1)
            };
            DataTable dataTable = myDB.GetDataTable($"SELECT * FROM CONTACTS WHERE FirstName = {sqlParameterFormat}FirstName AND Actif = {sqlParameterFormat}Actif", parameters);
            foreach (DataRow row in dataTable.Rows)
            {
                Console.WriteLine($"Id={row["Id"]}, Name={row["Name"]}, Actif={row["Actif"]}");
            }

            if (dataBaseType != DataBaseType.SQLite)
            {
                myDB.ExecuteNonQuery("DROP TABLE CONTACTS");
            }
            myDB.CloseDatabase();

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
            myDB.ExecuteNonQuery(sql);

            sql = "INSERT INTO CONTACTS (ID, NAME, FIRSTNAME, ACTIF) VALUES (@Id, @Name, @FirstName, @Actif)";

            parameters.Add(myDB.CreateParameter("@Id", 1));
            parameters.Add(myDB.CreateParameter("@Name", "Wayne"));
            parameters.Add(myDB.CreateParameter("@FirstName", "John"));
            parameters.Add(myDB.CreateParameter("@Actif", 0));
            myDB.ExecuteNonQuery(sql, parameters);

            parameters.Clear();
            parameters.Add(myDB.CreateParameter("@Id", 2));
            parameters.Add(myDB.CreateParameter("@Name", "Doe"));
            parameters.Add(myDB.CreateParameter("@FirstName", "John"));
            parameters.Add(myDB.CreateParameter("@Actif", 1));
            myDB.ExecuteNonQuery(sql, parameters);

            parameters.Clear();
            parameters.Add(myDB.CreateParameter("@Id", 3));
            parameters.Add(myDB.CreateParameter("@Name", "Clint"));
            parameters.Add(myDB.CreateParameter("@FirstName", null));
            parameters.Add(myDB.CreateParameter("@Actif", 1));
            myDB.ExecuteNonQuery(sql, parameters);
        }

        private static void CreateDatabaseMSSQL()
        {
            List<DbParameter> parameters = new List<DbParameter>();
            //*** Create table and data
            string sql = "CREATE TABLE CONTACTS (ID int, NAME varchar(50), FIRSTNAME varchar(50), ACTIF int)";
            myDB.ExecuteNonQuery(sql);

            sql = "INSERT INTO CONTACTS (ID, NAME, FIRSTNAME, ACTIF) VALUES (@Id, @Name, @FirstName, @Actif)";

            parameters.Add(myDB.CreateParameter("@Id", 1));
            parameters.Add(myDB.CreateParameter("@Name", "Wayne"));
            parameters.Add(myDB.CreateParameter("@FirstName", "John"));
            parameters.Add(myDB.CreateParameter("@Actif", 0));
            myDB.ExecuteNonQuery(sql, parameters);

            parameters.Clear();
            parameters.Add(myDB.CreateParameter("@Id", 2));
            parameters.Add(myDB.CreateParameter("@Name", "Doe"));
            parameters.Add(myDB.CreateParameter("@FirstName", "John"));
            parameters.Add(myDB.CreateParameter("@Actif", 1));
            myDB.ExecuteNonQuery(sql, parameters);
        }

        private static void CreateDatabaseOracle()
        {
            if (myDB.ExecuteScalar<int>("SELECT count(*) FROM user_tables WHERE table_name = 'CONTACTS'") == 1) return;
            List<DbParameter> parameters = new List<DbParameter>();
            //*** Create table and data
            string sql = "CREATE TABLE CONTACTS (ID number(10), NAME varchar2(50), FIRSTNAME varchar2(50), ACTIF number(1))";
            myDB.ExecuteNonQuery(sql);

            sql = "INSERT INTO CONTACTS (ID, NAME, FIRSTNAME, ACTIF) VALUES (:Id, :Name, :FirstName, :Actif)";

            parameters.Add(myDB.CreateParameter(":Id", 1));
            parameters.Add(myDB.CreateParameter(":Name", "Wayne"));
            parameters.Add(myDB.CreateParameter(":FirstName", "John"));
            parameters.Add(myDB.CreateParameter(":Actif", 0));
            myDB.ExecuteNonQuery(sql, parameters);

            parameters.Clear();
            parameters.Add(myDB.CreateParameter(":Id", 2));
            parameters.Add(myDB.CreateParameter(":Name", "Doe"));
            parameters.Add(myDB.CreateParameter(":FirstName", "John"));
            parameters.Add(myDB.CreateParameter(":Actif", 1));
            myDB.ExecuteNonQuery(sql, parameters);
        }

    }

}
