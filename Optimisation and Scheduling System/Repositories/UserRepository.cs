using Npgsql;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Optimisation_and_Scheduling_System.Repositories
{
	public class UserRepository : IUserRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        public bool UserExists(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM UserModel WHERE UserName = @name", connection))
                {
                    cmd.Parameters.AddWithValue("name", username);
                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public void CreateUser(string username, string hashedPassword, string role)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO UserModel (UserName, HashedPassword, UserRole) VALUES (@name, @hashed, @role)", connection))
                {
                    cmd.Parameters.AddWithValue("name", username);
                    cmd.Parameters.AddWithValue("hashed", hashedPassword);
                    cmd.Parameters.AddWithValue("role", role);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ValidateUser(string username, string hashedPassword)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM UserModel WHERE UserName = @name AND HashedPassword = @hashed", connection))
                {
                    cmd.Parameters.AddWithValue("name", username);
                    cmd.Parameters.AddWithValue("hashed", hashedPassword);
                    return (long)cmd.ExecuteScalar() > 0;
                }
            }
        }
    }
}