using EmployeeCollaborationConsole.Models;
using Sprint2.Repositories;
using System;
using System.Linq;

namespace EmployeeCollaborationConsole.Services
{
    public class AnnouncementService
    {
        private readonly AnnouncementRepository _announcementRepo;
        private readonly BusinessLogic _businessLogic;

        public AnnouncementService(string connectionString, BusinessLogic businessLogic)
        {
            _announcementRepo = new AnnouncementRepository(connectionString);
            _businessLogic = businessLogic;
        }

        public void AddAnnouncement()
        {
            var ann = new Announcement
            {
                Title = Ask("Announcement Title"),
                Message = Ask("Message"),
                CreatedAt = DateTime.Now
            };
            _announcementRepo.AddAnnouncement(ann);
            Console.WriteLine("Announcement added.");
        }

        public void ViewAnnouncements()
        {
            var announcements = _announcementRepo.GetAllAnnouncements();
            if (!announcements.Any())
            {
                Console.WriteLine("No announcements found.");
                return;
            }

            foreach (var a in announcements)
                Console.WriteLine($"{a.Title} - {a.Message}");
        }

        public void DeleteAnnouncement()
        {
            var allAnns = _announcementRepo.GetAllAnnouncements();
            if (!allAnns.Any())
            {
                Console.WriteLine("No announcements to delete.");
                return;
            }

            for (int i = 0; i < allAnns.Count; i++)
                Console.WriteLine($"{i + 1}. {allAnns[i].Title}");

            if (int.TryParse(Console.ReadLine(), out int annIdx) && annIdx > 0 && annIdx <= allAnns.Count)
            {
                _announcementRepo.DeleteAnnouncement(allAnns[annIdx - 1].AnnouncementId);
                Console.WriteLine("Announcement deleted.");
            }
        }

        private static string Ask(string prompt)
        {
            Console.Write($"{prompt}: ");
            return Console.ReadLine() ?? "";
        }
    }
}