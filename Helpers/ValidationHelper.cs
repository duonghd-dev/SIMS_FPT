using System;
using System.Collections.Generic;
using System.Linq;

namespace SIMS_FPT.Helpers
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validate email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate phone number (10-11 digits)
        /// </summary>
        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Phone is optional

            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{10,11}$");
        }

        /// <summary>
        /// Check if date of birth is valid (not in future, at least 5 years old)
        /// </summary>
        public static bool IsValidDateOfBirth(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return true; // Optional field

            if (dateOfBirth.Value > DateTime.Now)
                return false; // Cannot be in future

            // Check if at least 5 years old
            var age = DateTime.Now.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > DateTime.Now.AddYears(-age))
                age--;

            return age >= 5; // Minimum age 5
        }

        /// <summary>
        /// Get age from date of birth
        /// </summary>
        public static int GetAge(DateTime dateOfBirth)
        {
            var age = DateTime.Now.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Now.AddYears(-age))
                age--;
            return age;
        }

        /// <summary>
        /// Validate ID format (alphanumeric, 3-20 characters)
        /// </summary>
        public static bool IsValidId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9]{3,20}$");
        }

        /// <summary>
        /// Validate numeric field (non-negative)
        /// </summary>
        public static bool IsValidPositiveNumber(int? value, int minValue = 0)
        {
            return !value.HasValue || value.Value >= minValue;
        }

        /// <summary>
        /// Validate credit range (1-10)
        /// </summary>
        public static bool IsValidCredits(int? credits)
        {
            return !credits.HasValue || (credits.Value >= 1 && credits.Value <= 10);
        }

        /// <summary>
        /// Validate joining date relative to date of birth.
        /// Rules:
        /// - If both provided: JoiningDate must be >= DateOfBirth
        /// - Must be at least <paramref name="minAgeAtJoin"/> years after DateOfBirth
        /// - If either is missing, treat as valid (handled elsewhere if required)
        /// </summary>
        public static bool IsValidJoiningDate(DateTime? dateOfBirth, DateTime? joiningDate, int minAgeAtJoin = 21)
        {
            if (!joiningDate.HasValue || !dateOfBirth.HasValue)
                return true;

            var dob = dateOfBirth.Value.Date;
            var join = joiningDate.Value.Date;

            if (join < dob)
                return false;

            var minJoinDate = dob.AddYears(minAgeAtJoin);
            return join >= minJoinDate;
        }
    }
}
