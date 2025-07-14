using EmployeeCollaborationConsole.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sprint2.Repositories
{
    public class TaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddTask(UserTask task)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(@"
    INSERT INTO dbo.Tasks (UserId, Title, Description, DueDate)
    VALUES (@UserId, @Title, @Description, @DueDate)", conn);


            cmd.Parameters.AddWithValue("@UserId", task.UserId);
            cmd.Parameters.AddWithValue("@Title", task.Title);
            cmd.Parameters.AddWithValue("@Description", task.Description);
            cmd.Parameters.AddWithValue("@DueDate", task.DueDate);

            cmd.ExecuteNonQuery();
        }

        public List<UserTask> GetAllTasks()
        {
            var tasks = new List<UserTask>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT * FROM Tasks", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new UserTask
                {
                    TaskId = Convert.ToInt32(reader["TaskId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Title = reader["Title"].ToString()!,
                    Description = reader["Description"].ToString()!,
                    DueDate = Convert.ToDateTime(reader["DueDate"])
                });
            }

            return tasks;
        }
    }
}
