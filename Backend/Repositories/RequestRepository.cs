using System.Collections.Generic;
using System.Data.SqlClient;
using EmployeeCollaborationConsole.Models;

namespace Sprint2.Repositories
{
    public class RequestRepository
    {
        private readonly string _connectionString;

        public RequestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddRequest(Request request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"INSERT INTO Requests (UserId, Title, Description, Priority, Status)
                           VALUES (@UserId, @Title, @Description, @Priority, @Status)", conn);

            cmd.Parameters.AddWithValue("@UserId", request.UserId);
            cmd.Parameters.AddWithValue("@Title", request.Title);
            cmd.Parameters.AddWithValue("@Description", request.Description);
            cmd.Parameters.AddWithValue("@Priority", request.Priority);
            cmd.Parameters.AddWithValue("@Status", request.Status);
            cmd.ExecuteNonQuery();
        }

        public List<Request> GetAllRequests()
        {
            var requests = new List<Request>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Requests", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                requests.Add(new Request
                {
                    RequestId = Convert.ToInt32(reader["RequestId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Title = reader["Title"]?.ToString() ?? "",
                    Description = reader["Description"]?.ToString() ?? "",
                    Priority = reader["Priority"]?.ToString() ?? "",
                    Status = reader["Status"]?.ToString() ?? ""
                });
            }
            return requests;
        }

        public void UpdateRequest(Request request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"UPDATE Requests 
                                       SET Title = @Title, Description = @Description, 
                                           Priority = @Priority, Status = @Status 
                                       WHERE RequestId = @RequestId", conn);
            cmd.Parameters.AddWithValue("@Title", request.Title);
            cmd.Parameters.AddWithValue("@Description", request.Description);
            cmd.Parameters.AddWithValue("@Priority", request.Priority);
            cmd.Parameters.AddWithValue("@Status", request.Status);
            cmd.Parameters.AddWithValue("@RequestId", request.RequestId);
            cmd.ExecuteNonQuery();
        }

        public void DeleteRequest(int requestId, int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM Requests WHERE RequestId = @RequestId AND UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@RequestId", requestId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.ExecuteNonQuery();
        }

        // Optional: Get requests by user
        public List<Request> GetRequestsByUser(int userId)
        {
            var requests = new List<Request>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Requests WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                requests.Add(new Request
                {
                    RequestId = Convert.ToInt32(reader["RequestId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    Title = reader["Title"]?.ToString() ?? "",
                    Description = reader["Description"]?.ToString() ?? "",
                    Priority = reader["Priority"]?.ToString() ?? "",
                    Status = reader["Status"]?.ToString() ?? ""
                });
            }
            return requests;
        }
    }
}
