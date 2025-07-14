using EmployeeCollaborationConsole.Models;
using Sprint2.Repositories;
using System;
using System.Linq;

namespace EmployeeCollaborationConsole.Services
{
    public class RequestService
    {
        private readonly RequestRepository _requestRepo;
        private readonly BusinessLogic _businessLogic;

        public RequestService(string connectionString, BusinessLogic businessLogic)
        {
            _requestRepo = new RequestRepository(connectionString);
            _businessLogic = businessLogic;
        }

        public void SubmitRequest(User currentUser)
        {
            var allRequests = _requestRepo.GetAllRequests();
            if (!_businessLogic.CanSubmitMoreRequestsToday(currentUser, allRequests))
            {
                Console.WriteLine("You have reached the daily request limit (5). Try again tomorrow.");
                return;
            }

            var req = new Request
            {
                UserId = currentUser.UserId,
                Title = Ask("Title"),
                Description = Ask("Description"),
                Priority = Ask("Priority"),
                CreatedAt = DateTime.Now,
                Status = "Open"
            };
            _requestRepo.AddRequest(req);
            Console.WriteLine("Request submitted.");
        }

        public void ViewRequests(User currentUser)
        {
            if (_businessLogic.CanViewAllRequests(currentUser))
            {
                var requests = _requestRepo.GetAllRequests();
                if (!requests.Any())
                {
                    Console.WriteLine("No requests found.");
                    return;
                }

                foreach (var r in requests)
                    Console.WriteLine($"[{r.RequestId}] {r.Title} - {r.Description} | Priority: {r.Priority} | Status: {r.Status}");

                if (Ask("Do you want to resolve any request? (yes/no)").Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write("Enter Request ID to resolve: ");
                    if (int.TryParse(Console.ReadLine(), out int reqId))
                    {
                        var reqToClose = requests.FirstOrDefault(r => r.RequestId == reqId);
                        if (reqToClose != null && !reqToClose.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
                        {
                            reqToClose.Status = "Resolved";
                            _requestRepo.UpdateRequest(reqToClose);
                            Console.WriteLine("Request marked as Resolved.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid or already resolved request.");
                        }
                    }
                }
            }
            else
            {
                var myRequests = _requestRepo.GetRequestsByUser(currentUser.UserId);
                if (!myRequests.Any())
                {
                    Console.WriteLine("You have no requests.");
                    return;
                }

                foreach (var r in myRequests)
                    Console.WriteLine($"[{r.RequestId}] {r.Title} - {r.Description} | Priority: {r.Priority} | Status: {r.Status}");
            }
        }

        public void UpdateRequest(User currentUser)
        {
            var updatable = _requestRepo.GetAllRequests().Where(r => r.UserId == currentUser.UserId).ToList();
            if (!updatable.Any())
            {
                Console.WriteLine("No requests to update.");
                return;
            }

            for (int i = 0; i < updatable.Count; i++)
                Console.WriteLine($"{i + 1}. {updatable[i].Title} (Status: {updatable[i].Status})");

            if (int.TryParse(Console.ReadLine(), out int updateIndex) && updateIndex > 0 && updateIndex <= updatable.Count)
            {
                var reqToUpdate = updatable[updateIndex - 1];
                if (!_businessLogic.CanEditRequest(currentUser, reqToUpdate))
                {
                    Console.WriteLine("Cannot edit this request. Only 'Open' or 'In Progress' requests can be edited.");
                    return;
                }

                string? val = Ask($"New Title (blank to keep '{reqToUpdate.Title}')");
                if (!string.IsNullOrWhiteSpace(val)) reqToUpdate.Title = val;

                val = Ask("New Description (blank to keep current)");
                if (!string.IsNullOrWhiteSpace(val)) reqToUpdate.Description = val;

                val = Ask($"New Priority (blank to keep '{reqToUpdate.Priority}')");
                if (!string.IsNullOrWhiteSpace(val)) reqToUpdate.Priority = val;

                val = Ask($"New Status (blank to keep '{reqToUpdate.Status}')");
                if (!string.IsNullOrWhiteSpace(val)) reqToUpdate.Status = val;

                _requestRepo.UpdateRequest(reqToUpdate);
                Console.WriteLine("Request updated.");
            }
        }

        public void DeleteRequest(User currentUser)
        {
            var deletables = _requestRepo
                .GetAllRequests()
                .Where(r => r.UserId == currentUser.UserId && r.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!deletables.Any())
            {
                Console.WriteLine("You have no resolved requests to delete.");
                return;
            }

            for (int i = 0; i < deletables.Count; i++)
                Console.WriteLine($"{i + 1}. {deletables[i].Title} (Status: {deletables[i].Status})");

            if (int.TryParse(Console.ReadLine(), out int delIndex) && delIndex > 0 && delIndex <= deletables.Count)
            {
                _requestRepo.DeleteRequest(deletables[delIndex - 1].RequestId, currentUser.UserId);
                Console.WriteLine("Request deleted.");
            }
        }

        private static string Ask(string prompt)
        {
            Console.Write($"{prompt}: ");
            return Console.ReadLine() ?? "";
        }
    }
}