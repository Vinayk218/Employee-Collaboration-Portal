using EmployeeCollaborationConsole.Models;
using Sprint2.Repositories;
using System;
using System.Linq;

namespace EmployeeCollaborationConsole.Services
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepo;
        private readonly BusinessLogic _businessLogic;

        public TaskService(string connectionString, BusinessLogic businessLogic)
        {
            _taskRepo = new TaskRepository(connectionString);
            _businessLogic = businessLogic;
        }

        public void AddTask(User currentUser)
        {
            var task = new UserTask
            {
                UserId = currentUser.UserId,
                Title = Ask("Task Title"),
                Description = Ask("Task Description")
            };

            if (DateTime.TryParse(Ask("Due Date (yyyy-MM-dd)"), out DateTime due))
            {
                task.DueDate = due;
                _taskRepo.AddTask(task);
                Console.WriteLine("Task added.");
            }
            else
            {
                Console.WriteLine("Invalid date.");
            }
        }

        public void ViewTasks(User currentUser)
        {
            var myTasks = _taskRepo.GetAllTasks().Where(t => t.UserId == currentUser.UserId);
            if (!myTasks.Any())
            {
                Console.WriteLine("You have no tasks.");
                return;
            }

            foreach (var t in myTasks)
                Console.WriteLine($"{t.Title} - Due: {t.DueDate.ToShortDateString()}");
        }

        private static string Ask(string prompt)
        {
            Console.Write($"{prompt}: ");
            return Console.ReadLine() ?? "";
        }
    }
}