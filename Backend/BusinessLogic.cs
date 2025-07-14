using EmployeeCollaborationConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EmployeeCollaborationConsole
{
    public class BusinessLogic
    {
        public bool CanManageAnnouncements(User user)
        {
            return user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanDeleteOwnProfile(User user)
        {
            return !user.Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanEditRequest(User user, Request request)
        {
            if (string.IsNullOrEmpty(request.Status))
                return false;

            var status = request.Status.ToLower();
            return status == "open" || status == "in progress";
        }

        


        public bool CanViewAllRequests(User user)
        {
            var role = user.Role.ToLower();
            return role == "admin" || role == "manager";
        }

        public bool CanSubmitMoreRequestsToday(User user, IEnumerable<Request> allRequests)
        {
            var today = DateTime.Today;

            int todayCount = allRequests
                .Where(r => r.UserId == user.UserId && r.CreatedAt.Date == today)
                .Count();

            return todayCount < 5;
        }

        public bool IsEmailValid(string email)
        {
            return !string.IsNullOrWhiteSpace(email)
                   && email.Contains("@")
                   && email.Contains(".");
        }
    }
}
