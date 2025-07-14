using EmployeeCollaborationConsole.Models;
using Sprint2.Repositories;
using System;
using System.Text.RegularExpressions;

namespace EmployeeCollaborationConsole.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepo;
        private readonly BusinessLogic _businessLogic;

        public UserService(string connectionString, BusinessLogic businessLogic)
        {
            _userRepo = new UserRepository(connectionString);
            _businessLogic = businessLogic;
        }

        public User? Register()
        {
            string fullName;
            do
            {
                fullName = Ask("Full Name").Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                    Console.WriteLine("Your name is required.");
            } while (string.IsNullOrWhiteSpace(fullName));

            string email;
            do
            {
                email = Ask("Email");
                if (!_businessLogic.IsEmailValid(email))
                    Console.WriteLine("Invalid email format. Try again.");
            } while (!_businessLogic.IsEmailValid(email));

            string password;
            do
            {
                password = Ask("Password (min 6 chars, must include uppercase, lowercase, digit)");
                if (!IsValidPassword(password))
                    Console.WriteLine("Password does not meet complexity requirements. Try again.");
            } while (!IsValidPassword(password));

            string confirmPassword = Ask("Confirm Password");
            if (password != confirmPassword)
            {
                Console.WriteLine("Passwords do not match. Registration failed.");
                return null;
            }

            string department = Ask("Department (IT/HR/Finance)");
            string role = Ask("Role (Employee/Manager/Admin)");

            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                Department = department,
                Role = role,
                CreatedAt = DateTime.Now,
                Password = password
            };

            _userRepo.AddUser(newUser);
            return newUser;
        }

        public User? Login()
        {
            string loginName;
            do
            {
                loginName = Ask("Enter your Full Name").Trim();
                if (string.IsNullOrWhiteSpace(loginName))
                    Console.WriteLine("Your name is required.");
            } while (string.IsNullOrWhiteSpace(loginName));

            var passwordInput = Ask("Enter your Password");
            var user = _userRepo.GetUserByFullName(loginName);

            if (user != null && user.Password == passwordInput)
                return user;

            return null;
        }

        public void UpdateProfile(User currentUser)
        {
            bool updated = false;
            string? newName = Ask("New Full Name (blank to keep)");
            if (!string.IsNullOrWhiteSpace(newName))
            {
                currentUser.FullName = newName;
                updated = true;
            }

            string? newDept = Ask("New Department (blank to keep)");
            if (!string.IsNullOrWhiteSpace(newDept))
            {
                currentUser.Department = newDept;
                updated = true;
            }

            string? newRole = Ask("New Role (blank to keep)");
            if (!string.IsNullOrWhiteSpace(newRole))
            {
                currentUser.Role = newRole;
                updated = true;
            }

            if (updated)
            {
                _userRepo.UpdateUser(currentUser);
                Console.WriteLine("Profile updated.");
            }
        }

        public bool DeleteProfile(User currentUser)
        {
            if (!_businessLogic.CanDeleteOwnProfile(currentUser))
            {
                Console.WriteLine("Admins cannot delete their profiles.");
                return false;
            }

            if (Ask("Are you sure you want to delete your profile? (yes/no)").Trim().Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                _userRepo.DeleteUser(currentUser.Email);
                Console.WriteLine("Profile deleted. Goodbye!");
                return true;
            }
            return false;
        }

        private static string Ask(string prompt)
        {
            Console.Write($"{prompt}: ");
            return Console.ReadLine() ?? "";
        }

        private static bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$");
        }
    }
}