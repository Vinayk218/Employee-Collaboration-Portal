using EmployeeCollaborationConsole.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Sprint2.Repositories
{
    public class AnnouncementRepository
    {
        private readonly string _connectionString;

        public AnnouncementRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddAnnouncement(Announcement ann)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Announcements (Title, Message, CreatedAt) VALUES (@Title, @Message, @CreatedAt)", conn);
            cmd.Parameters.AddWithValue("@Title", ann.Title);
            cmd.Parameters.AddWithValue("@Message", ann.Message);
            cmd.Parameters.AddWithValue("@CreatedAt", ann.CreatedAt);
            cmd.ExecuteNonQuery();
        }

        public List<Announcement> GetAllAnnouncements()
        {
            var announcements = new List<Announcement>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Announcements", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                announcements.Add(new Announcement
                {
                    AnnouncementId = Convert.ToInt32(reader["AnnouncementId"]),
                    Title = reader["Title"].ToString()!,
                    Message = reader["Message"].ToString()!,
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }
            return announcements;
        }

        public void DeleteAnnouncement(int annId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM Announcements WHERE AnnouncementId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", annId);
            cmd.ExecuteNonQuery();
        }
    }
}
