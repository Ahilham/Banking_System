using Assignment2.Models;

namespace Assignment2.Data
{
    public class Generation
    {

        public static void Generate()
        {
            ClearDatabase();

            for (int i = 0; i < 10; i++)
            {
                UserProfile user = DataGenerator.GenerateRandomUserProfile();
                DBmanager.CreateUserProfile(user);


                BankAccount userBankAcc = DataGenerator.GenerateRandomBankAccount(i+1);
                DBmanager.CreateBankAccount(userBankAcc);

                

                Transactions transactions = DataGenerator.GenerateRandomTransaction(i+1);
                if (transactions.TransactionType.Equals("Deposit"))
                {
                    DBmanager.Deposit(transactions.AccountNumber, transactions.Amount);
                    
                }
                else if (transactions.TransactionType.Equals("Withdrawal"))
                {
                    DBmanager.Withdrawal(transactions.AccountNumber, transactions.Amount);
                }
            }

            for (int i = 10; i < 13; i++)
            {
                UserProfile Admin = DataGenerator.GenerateRandomAdminProfile();
                DBmanager.CreateUserProfile(Admin);
            }
        }

        private static void ClearDatabase()
        {
            try
            {
                // Assumes you have methods in DBmanager to clear each table
                DBmanager.ClearTransactionsTable();  // Clears all transactions
                DBmanager.ClearBankAccountTable();   // Clears all bank accounts
                DBmanager.ClearUserProfileTable();   // Clears all user profiles

                Console.WriteLine("Database cleared successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error clearing the database: " + ex.Message);
            }
        }
    }
}
