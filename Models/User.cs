using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanToPlate.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public static bool validEmail(string email)
        {
            string trimmedEmail = email.Trim();
            try
            {
                var addr = new System.Net.Mail.MailAddress(trimmedEmail);
                string domainPart = addr.Host;
                return addr.Address == trimmedEmail && domainPart.Contains('.') && domainPart.IndexOf('.') != domainPart.Length - 1;
            }
            catch
            {
                return false;
            }
        }

        public static bool validPassword(string password, string confirmPassword)
        {
            if (password != confirmPassword)
                return false;
            if (password.Length < 8)
                return false;
            bool hasUpperChar = false;
            bool hasLowerChar = false;
            bool hasNumber = false;
            bool hasSymbols = false;
            foreach (char c in password)
            {
                if (char.IsUpper(c))
                    hasUpperChar = true;
                else if (char.IsLower(c))
                    hasLowerChar = true;
                else if (char.IsDigit(c))
                    hasNumber = true;
                else
                    hasSymbols = true;
            }
            if (hasUpperChar == false || hasLowerChar == false || hasNumber == false || hasSymbols == false) { return false; }

            return true;
        }

    }
}
