using Newtonsoft.Json;
using System;
using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VetClinicServer
{
    public static class DatabaseConnector
    {
        private const string DATETIME_FORMAT_STRING = "yyyy-MM-dd HH:mm:ss.fff";

        private static readonly string CONNECTION_STRING;

        static DatabaseConnector()
        {
            //CONNECTION_STRING = Environment.GetEnvironmentVariable("MSSQL_CONN");
            CONNECTION_STRING = @"Data Source= (localdb)\MSSQLLocalDB; Initial Catalog=VetClinicDB; User ID=Test;Password=test1;";
            //CONNECTION_STRING = @"Data Source=;Initial Catalog=ServerData;User ID=sa;Password=;";


            if (string.IsNullOrEmpty(CONNECTION_STRING))
            {
                Logger.Log("Database connection string is null", LogType.Error);
            }
        }

        internal static Response ProcessRequest(RequestData requestData)
        {
            if (string.IsNullOrEmpty(CONNECTION_STRING))
            {
                var response = Response.InternalError("OpenSession: Database connection string is null");
                Logger.Log(response.StatusString, LogType.Error);
                return response;
            }
            if (requestData.AdditionalData == "GetAllAnimals")
            {
                Response sessionIDTask = GetAllAnimals(requestData.Email, requestData.AadId);
                Logger.Log($"Result: {sessionIDTask.StatusCode}, {sessionIDTask.StatusString}");
                return sessionIDTask;
            }
            else
            {
                var response = Response.InternalError("Unrecognized request");
                Logger.Log(response.StatusString, LogType.Error);
                return response;
            }
        }

        internal static Response GetAllAnimals(string email, string aadId)
        {
            string[] tables = new string[] { "Animals", "AnimalImages" };
            string[] columns1 = new string[] { "id", "name", "type", "birthday", "owner" };
            string[] columns2 = new string[] { "animalID", "img" };
            string sqlExpression = $"SELECT " +
                $"{tables[0]}.{columns1[0]}, {tables[0]}.{columns1[1]}, {tables[0]}.{columns1[2]}, {tables[0]}.{columns1[3]}, " +
                $"{tables[0]}.{columns1[4]}, {tables[1]}.{columns2[1]} " +
                $"FROM {tables[0]} LEFT JOIN {tables[1]} " +
                $"on {tables[0]}.{columns1[0]}={tables[1]}.{columns2[0]}";
            try
            {
                using (SqlConnection con = new SqlConnection(CONNECTION_STRING))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand(sqlExpression, con))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        List<Animal> result=new List<Animal>();
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);    // ID int
                            string name = reader.GetString(1);  // Name string
                            string type = reader.GetString(2); // Type string
                            DateTime birthday = reader.GetDateTime(3); // Birthday date
                            int owner = reader.GetInt32(4); // Owner int
                            var tempImage = reader.GetValue(5);// Image string
                            Byte[] image;
                            if (tempImage.GetType() == typeof(DBNull))
                            {
                                image = null;
                            }
                            else
                            {
                                image = (byte[])tempImage; 
                            }
                            result.Add(new Animal(id, name, type, birthday, owner, image));
                        }
                        Logger.Log($"Got data: {result}");
                        return new Response(200, JsonConvert.SerializeObject(result));
                    }
                }
                
            }
            catch (Exception ex)
            {
                var response = Response.InternalError($"GetSessionID: SQL exception. Reason: {ex.Message}{Environment.NewLine}SQL expression: {sqlExpression}");
                Logger.Log(response.StatusString, LogType.Error);
                return response;
            }
        }
        internal static string GetErrorString()
        {
            return string.IsNullOrEmpty(CONNECTION_STRING) ? "CONNECTION STRING ERROR" : null;
        }
    }
}
