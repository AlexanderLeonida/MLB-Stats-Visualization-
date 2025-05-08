using System;

namespace DbConnection
{
    public static class DbConnection
    {
        public static string GetConnectionString()
        {
            // Get the environment variables or use defaults
            string server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
            string user = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
            string pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
            string db = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "";

            // Return the connection string
            return $"server={server};port={port};user={user};password={pass};database={db};";
        }
    }
}
