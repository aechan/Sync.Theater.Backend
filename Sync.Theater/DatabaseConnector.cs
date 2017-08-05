using Sync.Theater.Models;
using Sync.Theater.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Theater
{

    /// <summary>
    /// Connects to SQL database using ADO.NET
    /// </summary>
    class DatabaseConnector
    {
        // build connection string based off our config.json variables
        private static string connectionString;

        private static SyncLogger Logger;


        static DatabaseConnector()
        {
            Logger = new SyncLogger("DatabaseConnector", ConsoleColor.Magenta);

            connectionString = ConfigManager.Config.SQLConnectionString.Replace("{userid}", ConfigManager.Config.DBUsername).Replace("{pwd}", ConfigManager.Config.DBPassword);
        }
        /// <summary>
        /// Given the PasswordHash, Username and or Email of the user, returns a SyncUser created from database data.
        /// If an error occurs or no user is found, returns null
        /// </summary>
        /// <param name="PasswordHash"></param>
        /// <param name="Username"></param>
        /// <param name="Email"></param>
        /// <returns></returns>
        public static SyncUser ValidateAndGetUser(string RawPassword, string Username = "", string Email = "" )
        {
            // exit early if no username or email is provided
            if ((string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email)) || (string.IsNullOrWhiteSpace(RawPassword))) { return null; }
           
            string queryString;

            // search for user by either Username or Email
            if (Username == "")
            {
                queryString = string.Format("SELECT Id, Username, Email, PasswordHash FROM Users WHERE Email = '{0}'", Email);
            }
            else
            {
                queryString = string.Format("SELECT Id, Username, Email, PasswordHash FROM Users WHERE Username = '{0}'", Username);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);

                // Open the connection in a try/catch block. 
                // Create and execute the DataReader, writing the result
                // set to the console window.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (UserAuth.VerifyPassword(RawPassword, (string)reader[3]))
                        {
                            return new SyncUser((int)reader[0], (string)reader[2], (string)reader[1]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    return null;
                }

                return null;
            }
        }


        public static bool AddUserToDB(string Username, string Email, string PasswordHash)
        {
            // exit early if no username or email is provided
            if ((string.IsNullOrWhiteSpace(Username) && string.IsNullOrWhiteSpace(Email)) || (string.IsNullOrWhiteSpace(PasswordHash))) { return false; }

            Random rand = new Random();
            // insert the user into table
            string queryString = string.Format("INSERT INTO Users VALUES ({0}, '{1}', '{2}', '{3}');", rand.Next(0, int.MaxValue), Username, Email, PasswordHash );
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);

                // Open the connection in a try/catch block. 
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    return false;
                }
            }

        }
    }
}
