using Assignment2.Models;
using System;

namespace Assignment2.Data
{
    public class DataGenerator
    {
        private static Random random = new Random();

        private static string GenerateRandomName()
        {
            string[] firstNames = { "John", "Jane", "Michael", "Sarah", "Robert", "Emily" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis" };

            string firstName = firstNames[random.Next(firstNames.Length)];
            string lastName = lastNames[random.Next(lastNames.Length)];

            return $"{firstName} {lastName}";
        }

        public static byte[] GenerateProfilePic()
        {
            string[] filePaths = { "profilePicture/profile1.png", "profilePicture/profile2.png", "profilePicture/profile3.jpeg" };

            string selectedFilePath = filePaths[random.Next(filePaths.Length)];

            // Read the file from disk and convert it to a byte array
            if (File.Exists(selectedFilePath))
            {
                return File.ReadAllBytes(selectedFilePath);
            }

            // Return null or throw an exception if the file does not exist
            throw new FileNotFoundException($"File not found: {selectedFilePath}");
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static UserProfile GenerateRandomUserProfile()
        {
            var userProfile = new UserProfile
            {
                UserName = GenerateRandomString(8),  // Random username
                Name = GenerateRandomName(),
                Email = $"{GenerateRandomString(5)}@example.com",  // Random email
                Address = $"{random.Next(1, 1000)} Main St, City {GenerateRandomString(5)}",
                PhoneNumber = $"555-{random.Next(100, 999)}-{random.Next(1000, 9999)}",  // Random phone number
                Password = GenerateRandomString(10),  // Random password
                Picture = GenerateProfilePic(),  
                Role = "User"
            };

            return userProfile;
        }

        public static UserProfile GenerateRandomAdminProfile()
        {
            var userProfile = new UserProfile
            {
                UserName = GenerateRandomString(8),  // Random username
                Name = GenerateRandomName(),
                Email = $"{GenerateRandomString(5)}@example.com",  // Random email
                Address = $"{random.Next(1, 1000)} Main St, City {GenerateRandomString(5)}",
                PhoneNumber = $"555-{random.Next(100, 999)}-{random.Next(1000, 9999)}",  // Random phone number
                Password = GenerateRandomString(10),  // Random password
                Picture = GenerateProfilePic(),
                Role = "Admin"
            };

            return userProfile;
        }


        public static BankAccount GenerateRandomBankAccount(int userId)
        {
            return new BankAccount
            {
                Balance = Math.Round((decimal)(random.NextDouble() * 5000 + 5000), 2),  // Random balance between 5000 and 10000
                UserId = userId
            };
        }

        public static Transactions GenerateRandomTransaction(int accountNumber)
        {
            string[] transactionTypes = { "Deposit", "Withdrawal" };
            string transactionType = transactionTypes[random.Next(transactionTypes.Length)];

            return new Transactions
            {// Math.Round(ori_balance + amount, 2)
                AccountNumber = accountNumber,
                TransactionType = transactionType,
                Amount = Math.Round((decimal)(random.NextDouble() * 5000), 2),  // Random amount between 0 and 5000
                TransactionDate = DateTime.Now.AddDays(-random.Next(0, 365)),  // Random date within the last year
                RelatedAccount = accountNumber  // Some random related account (dummy data)
            };
        }
    }
}
