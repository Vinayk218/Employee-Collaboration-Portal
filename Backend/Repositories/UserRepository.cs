using EmployeeCollaborationConsole.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sprint2.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Get the smallest unused UserId
                int newUserId;
                using (var getIdCmd = new SqlCommand(@"
                    SELECT TOP 1 number 
                    FROM master.dbo.spt_values 
                    WHERE type = 'P' AND number >= 1 AND number NOT IN (SELECT UserId FROM Users) 
                    ORDER BY number", connection))
                using (var reader = getIdCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        newUserId = reader.GetInt32(0);
                    }
                    else
                    {
                        reader.Close();
                        using var maxIdCmd = new SqlCommand("SELECT ISNULL(MAX(UserId), 0) + 1 FROM Users", connection);
                        newUserId = (int)maxIdCmd.ExecuteScalar();
                    }
                }

                user.UserId = newUserId;

                using (var enableCmd = new SqlCommand("SET IDENTITY_INSERT Users ON", connection))
                {
                    enableCmd.ExecuteNonQuery();
                }

                using (var insertCmd = new SqlCommand(@"
                    INSERT INTO Users (UserId, FullName, Email, Password, Department, Role, CreatedAt) 
                    VALUES (@UserId, @FullName, @Email, @Password, @Department, @Role, @CreatedAt)", connection))
                {
                    insertCmd.Parameters.AddWithValue("@UserId", user.UserId);
                    insertCmd.Parameters.AddWithValue("@FullName", user.FullName);
                    insertCmd.Parameters.AddWithValue("@Email", user.Email);
                    insertCmd.Parameters.AddWithValue("@Password", user.Password);
                    insertCmd.Parameters.AddWithValue("@Department", user.Department);
                    insertCmd.Parameters.AddWithValue("@Role", user.Role);
                    insertCmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                    insertCmd.ExecuteNonQuery();
                }

                using (var disableCmd = new SqlCommand("SET IDENTITY_INSERT Users OFF", connection))
                {
                    disableCmd.ExecuteNonQuery();
                }
            }
        }

        public User? GetUserByFullName(string fullName)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Users WHERE FullName = @FullName", conn);
            cmd.Parameters.AddWithValue("@FullName", fullName);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Password = reader["Password"]?.ToString() ?? "", // ← Added
                    Department = reader["Department"]?.ToString() ?? "",
                    Role = reader["Role"]?.ToString() ?? "",
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                };
            }
            return null;
        }

        public User? GetUserByEmail(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Users WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Password = reader["Password"]?.ToString() ?? "", // ← Added
                    Department = reader["Department"]?.ToString() ?? "",
                    Role = reader["Role"]?.ToString() ?? "",
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                };
            }
            return null;
        }

        public void UpdateUser(User user)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"UPDATE Users 
                                       SET FullName = @FullName, Department = @Department, Role = @Role 
                                       WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Department", user.Department);
            cmd.Parameters.AddWithValue("@Role", user.Role);
            cmd.Parameters.AddWithValue("@UserId", user.UserId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteUser(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM Users WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.ExecuteNonQuery();
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Users", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Password = reader["Password"]?.ToString() ?? "", // ← Added
                    Department = reader["Department"]?.ToString() ?? "",
                    Role = reader["Role"]?.ToString() ?? "",
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return users;
        }
    }
}