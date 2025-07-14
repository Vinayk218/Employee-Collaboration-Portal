using EmployeeCollaborationConsole.Models;
using EmployeeCollaborationConsole.Services;
using EmployeeCollaborationConsole; // Added to import BusinessLogic
using System;

namespace EmployeeCollaborationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=Chandu;Database=EmployeeCollaboration;Trusted_Connection=True;";
            var businessLogic = new BusinessLogic();
            var userService = new UserService(connectionString, businessLogic);
            var requestService = new RequestService(connectionString, businessLogic);
            var taskService = new TaskService(connectionString, businessLogic);
            var announcementService = new AnnouncementService(connectionString, businessLogic);

            Console.WriteLine("Choose: 1) Register  2) Login");
            var initialChoice = Console.ReadLine();
            User? currentUser = null;

            if (initialChoice == "1")
            {
                currentUser = userService.Register();
                if (currentUser == null)
                    return;
                Console.WriteLine("Registered successfully! Please log in.\n");
            }

            while (currentUser == null)
            {
                currentUser = userService.Login();
                if (currentUser == null)
                    Console.WriteLine("Invalid name or password. Please try again.\n");
            }

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n--- Employee Portal ---");
                Console.WriteLine("1. Submit Request");
                Console.WriteLine("2. View My Department Requests");
                Console.WriteLine("3. View Announcements");
                Console.WriteLine("4. Update My Request");
                Console.WriteLine("5. Delete My Request");
                Console.WriteLine("6. Update My Profile");
                Console.WriteLine("7. Delete My Profile");
                Console.WriteLine("8. Add Task");
                Console.WriteLine("9. View My Tasks");

                bool canManageAnnouncements = businessLogic.CanManageAnnouncements(currentUser) ||
                                             currentUser.Role.Equals("manager", StringComparison.OrdinalIgnoreCase);
                if (canManageAnnouncements)
                {
                    Console.WriteLine("10. Add Announcement");
                    Console.WriteLine("11. Delete Announcement");
                }

                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        requestService.SubmitRequest(currentUser);
                        break;
                    case "2":
                        requestService.ViewRequests(currentUser);
                        break;
                    case "3":
                        announcementService.ViewAnnouncements();
                        break;
                    case "4":
                        requestService.UpdateRequest(currentUser);
                        break;
                    case "5":
                        requestService.DeleteRequest(currentUser);
                        break;
                    case "6":
                        userService.UpdateProfile(currentUser);
                        break;
                    case "7":
                        if (userService.DeleteProfile(currentUser))
                            running = false;
                        break;
                    case "8":
                        taskService.AddTask(currentUser);
                        break;
                    case "9":
                        taskService.ViewTasks(currentUser);
                        break;
                    case "10":
                        if (canManageAnnouncements)
                            announcementService.AddAnnouncement();
                        else
                            Console.WriteLine("Not authorized.");
                        break;
                    case "11":
                        if (canManageAnnouncements)
                            announcementService.DeleteAnnouncement();
                        else
                            Console.WriteLine("Not authorized.");
                        break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }
    }
}