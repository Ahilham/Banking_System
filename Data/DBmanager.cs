using Assignment2.Models;
using System.Data.SQLite;
using System.Xml.Linq;
using static System.Data.Entity.Infrastructure.Design.Executor;


/*******************************************************************************
 
// Improvement on Withdrawal class: Refactor this section to handle insufficient funds error handling (try to make it serialize so that the error would be carried forward to the business layer)

*******************************************************************************/

namespace Assignment2.Data
{
    public class DBmanager 
    {

        private static string connectionString = "Data Source=assignment2DB.db;Version=3;";


        // Bank Account database side 
        //       |
        //       |
        //     \   /
        //      \ /
        //       v

        public static bool CreateBankAccount(BankAccount bankAccount)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO \"Bank Account\" (Balance, UserId) VALUES (@Balance, @UserId);";
                        command.Parameters.AddWithValue("@Balance", bankAccount.Balance);
                        command.Parameters.AddWithValue("@UserId", bankAccount.UserId);

                        int rowsinserted = command.ExecuteNonQuery();
                        connection.Close();
                        if (rowsinserted > 0)
                        {
                            return true;
                        }
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }


        public static BankAccount GetAccountByAccNum(int AccountNum)
        {
            BankAccount bankAccount = null;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM \"Bank Account\" WHERE AccountNumber = @AccountNumber";
                        command.Parameters.AddWithValue("@AccountNumber", AccountNum);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bankAccount = new BankAccount();
                                bankAccount.AccountNumber = Convert.ToInt32(reader["AccountNumber"]);
                                bankAccount.Balance = Convert.ToInt32(reader["Balance"]);
                                bankAccount.UserId = Convert.ToInt32(reader["UserId"]);
                            }
                        }
                    }
                    
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return bankAccount;
        }

        public static bool UpdateBankAccount(BankAccount bankAccount)
        {
            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Create a new SQLite command to execute SQL
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        // Build the SQL command to update data by ID
                        command.CommandText = $"UPDATE \"Bank Account\" SET UserId = @UserId WHERE AccountNumber = @AccountNumber;";
                        command.Parameters.AddWithValue("@UserId", bankAccount.UserId);
                        command.Parameters.AddWithValue("@AccountNumber", bankAccount.AccountNumber);

                        // Execute the SQL command to update data
                        int rowsUpdated = command.ExecuteNonQuery();
                        connection.Close();

                        // Check if any rows were updated
                        if (rowsUpdated > 0)
                        {
                            return true; // Update was successful
                        }
                    }
                }
                return false; // No rows were updated
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false; // Update failed
            }
        }

        public static bool DeleteBankAccount(int AccountNumber)
        {
            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Create a new SQLite command to execute SQL
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        // Build the SQL command to delete data by ID
                        command.CommandText = $"DELETE FROM \"Bank Account\" WHERE AccountNumber = @AccountNumber";
                        command.Parameters.AddWithValue("@AccountNumber", AccountNumber);

                        // Execute the SQL command to delete data
                        int rowsDeleted = command.ExecuteNonQuery();

                        // Check if any rows were deleted
                        if (rowsDeleted > 0)
                        {
                            return true; // Deletion was successful
                        }
                    }
                }
                return false; // No rows were deleted
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false; // Deletion failed
            }
        }

        public static void ClearBankAccountTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM \"Bank Account\"";
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand resetSequenceCommand = connection.CreateCommand())
                {
                    resetSequenceCommand.CommandText = "DELETE FROM sqlite_sequence WHERE name = 'Bank Account'";
                    resetSequenceCommand.ExecuteNonQuery();
                }
            }
        }


        // transaction database side 
        //       |
        //       |
        //     \   /
        //      \ /
        //       v



        public static List<Transactions> GetTransactionHistoryByBankAcc(int AccountNumber)
        {
            List<Transactions> transactionsList = null;
            Transactions transactions = null;

            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Create a new SQLite command to execute SQL
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        // Build the SQL command to select a student by ID
                        command.CommandText = "SELECT * FROM Transactions WHERE AccountNumber = @AccountNumber";
                        command.Parameters.AddWithValue("@AccountNumber", AccountNumber);

                        // Execute the SQL command and retrieve data
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                transactions = new Transactions();
                                transactions.TransactionId = Convert.ToInt32(reader["TransactionId"]);
                                transactions.AccountNumber = Convert.ToInt32(reader["AccountNumber"]);
                                transactions.TransactionType = reader["TransactionType"].ToString();
                                transactions.Amount = (decimal)(reader["Amount"]);
                                transactions.TransactionDate = (DateTime)reader["TransactionDate"];
                                transactions.RelatedAccount = Convert.ToInt32(reader["RelatedAccount"]);
                                transactions.Description = reader["Description"].ToString();

                                transactionsList.Add(transactions);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return transactionsList;
        }


        public static bool Deposit(int AccountNumber, decimal amount)
        {
            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Start a transaction to ensure atomicity
                    using (var transaction = connection.BeginTransaction())
                    {
                        decimal ori_balance = 0;

                        // Create a new SQLite command to execute SQL
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            // Fetch the original balance for the account
                            command.CommandText = "SELECT Balance FROM \"Bank Account\" WHERE AccountNumber = @AccountNumber;";
                            command.Parameters.AddWithValue("@AccountNumber", AccountNumber);

                            // Execute the SQL command and retrieve data
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader["Balance"] is long)
                                    {
                                        ori_balance = Convert.ToDecimal((long)reader["Balance"]);
                                    }
                                    else if (reader["Balance"] is decimal)
                                    {
                                        ori_balance = (decimal)reader["Balance"];
                                        
                                    }
                                    
                                }
                                else
                                {
                                    Console.WriteLine("Account not found.");
                                    return false;
                                }
                            }

                            

                            // Clear the previous parameters before reusing the command
                            command.Parameters.Clear();

                            // Update the balance in the bank account
                            command.CommandText = "UPDATE \"Bank Account\" SET Balance = @Balance WHERE AccountNumber = @AccountNumber;";
                            command.Parameters.AddWithValue("@Balance", Math.Round(ori_balance + amount, 2));
                            command.Parameters.AddWithValue("@AccountNumber", AccountNumber);

                            int rowsUpdated = command.ExecuteNonQuery();

                            if (rowsUpdated > 0)
                            {
                                // Clear parameters before reusing the command again
                                command.Parameters.Clear();

                                // Insert the transaction into the Transactions table
                                command.CommandText = "INSERT INTO Transactions (AccountNumber, TransactionType, Amount, RelatedAccount) VALUES (@AccountNumber, @TransactionType, @Amount, @RelatedAccount);";
                                command.Parameters.AddWithValue("@AccountNumber", AccountNumber);
                                command.Parameters.AddWithValue("@TransactionType", "Deposit");
                                command.Parameters.AddWithValue("@Amount", amount);
                                command.Parameters.AddWithValue("@RelatedAccount", AccountNumber);

                                int rowsInserted = command.ExecuteNonQuery();

                                if (rowsInserted > 0)
                                {
                                    // Commit the transaction to ensure both update and insert succeed
                                    transaction.Commit();
                                    return true; // Deposit and transaction logging were successful
                                }
                                else
                                {
                                    // Rollback transaction if insert fails
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                            else
                            {
                                // Rollback transaction if balance update fails
                                transaction.Rollback();
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }

        public static bool Withdrawal(int AccountNumber, decimal amount)
        {
            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Start a transaction to ensure atomicity
                    using (var transaction = connection.BeginTransaction())
                    {
                        decimal ori_balance = 0;
                        decimal result;

                        // Create a new SQLite command to execute SQL
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            // Fetch the original balance for the account
                            command.CommandText = "SELECT Balance FROM \"Bank Account\" WHERE AccountNumber = @AccountNumber;";
                            command.Parameters.AddWithValue("@AccountNumber", AccountNumber);

                            // Execute the SQL command and retrieve data
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader["Balance"] is long)
                                    {
                                        ori_balance = Convert.ToDecimal((long)reader["Balance"]);
                                    }
                                    else if (reader["Balance"] is decimal)
                                    {
                                        ori_balance = (decimal)reader["Balance"];
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Account not found.");
                                    return false;
                                }
                            }

                            result = Math.Round(ori_balance - amount, 2);

                            if (result >= 0)
                            {
                                
                                command.Parameters.Clear();

                                
                                command.CommandText = "UPDATE \"Bank Account\" SET Balance = @Balance WHERE AccountNumber = @AccountNumber;";
                                command.Parameters.AddWithValue("@Balance", result);
                                command.Parameters.AddWithValue("@AccountNumber", AccountNumber);

                                int rowsUpdated = command.ExecuteNonQuery();

                                if (rowsUpdated > 0)
                                {
                                    
                                    command.Parameters.Clear();

                                    
                                    command.CommandText = "INSERT INTO Transactions (AccountNumber, TransactionType, Amount, RelatedAccount) VALUES (@AccountNumber, @TransactionType, @Amount, @RelatedAccount);";
                                    command.Parameters.AddWithValue("@AccountNumber", AccountNumber);
                                    command.Parameters.AddWithValue("@TransactionType", "Withdrawal");
                                    command.Parameters.AddWithValue("@Amount", amount);
                                    command.Parameters.AddWithValue("@RelatedAccount", AccountNumber);

                                    int rowsInserted = command.ExecuteNonQuery();

                                    if (rowsInserted > 0)
                                    {
                                        
                                        transaction.Commit();
                                        return true; 
                                    }
                                    else
                                    {
                                        
                                        transaction.Rollback();
                                        return false;
                                    }
                                }
                                else
                                {
                                    
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                            else
                            {

                                Console.WriteLine("Error: Insufficient funds."); // IMPORTANT TODO: Improvement on this line - Refactor the DBManager class
                                return false;

                            }

                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }


        public static bool Transfer(int sender, int recipient, decimal amount, string description)
        {
            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Start a transaction to ensure atomicity
                    using (var transaction = connection.BeginTransaction())
                    {
                        decimal senderBalance = 0;

                        // Create a new SQLite command to execute SQL
                        using (SQLiteCommand command = connection.CreateCommand())
                        {
                            // Fetch the original balance for the sender's account
                            command.CommandText = "SELECT Balance FROM \"Bank Account\" WHERE AccountNumber = @SenderAccount;";
                            command.Parameters.AddWithValue("@SenderAccount", sender);

                            // Execute the SQL command and retrieve data
                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Convert the balance correctly
                                    if (reader["Balance"] is long)
                                    {
                                        senderBalance = Convert.ToDecimal((long)reader["Balance"]);
                                    }
                                    else if (reader["Balance"] is decimal)
                                    {
                                        senderBalance = (decimal)reader["Balance"];
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Sender account not found.");
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            // Check if the sender has enough balance
                            if (senderBalance < amount)
                            {
                                Console.WriteLine("Insufficient funds.");
                                transaction.Rollback();
                                return false;
                            }

                            // Clear the previous parameters before reusing the command
                            command.Parameters.Clear();

                            // Debit the sender's account
                            command.CommandText = "UPDATE \"Bank Account\" SET Balance = @NewSenderBalance WHERE AccountNumber = @SenderAccount;";
                            command.Parameters.AddWithValue("@NewSenderBalance", Math.Round(senderBalance - amount, 2));
                            command.Parameters.AddWithValue("@SenderAccount", sender);

                            int rowsUpdated = command.ExecuteNonQuery();

                            if (rowsUpdated == 0)
                            {
                                Console.WriteLine("Failed to update sender's balance.");
                                transaction.Rollback();
                                return false;
                            }

                            // Clear parameters before reusing the command again
                            command.Parameters.Clear();

                            // Fetch the recipient's balance
                            decimal recipientBalance = 0;
                            command.CommandText = "SELECT Balance FROM \"Bank Account\" WHERE AccountNumber = @RecipientAccount;";
                            command.Parameters.AddWithValue("@RecipientAccount", recipient);

                            using (SQLiteDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Convert the balance correctly
                                    if (reader["Balance"] is long)
                                    {
                                        recipientBalance = Convert.ToDecimal((long)reader["Balance"]);
                                    }
                                    else if (reader["Balance"] is decimal)
                                    {
                                        recipientBalance = (decimal)reader["Balance"];
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Recipient account not found.");
                                    transaction.Rollback();
                                    return false;
                                }
                            }

                            // Clear parameters before reusing the command again
                            command.Parameters.Clear();

                            // Credit the recipient's account
                            command.CommandText = "UPDATE \"Bank Account\" SET Balance = @NewRecipientBalance WHERE AccountNumber = @RecipientAccount;";
                            command.Parameters.AddWithValue("@NewRecipientBalance", Math.Round(recipientBalance + amount, 2));
                            command.Parameters.AddWithValue("@RecipientAccount", recipient);

                            rowsUpdated = command.ExecuteNonQuery();

                            if (rowsUpdated == 0)
                            {
                                Console.WriteLine("Failed to update recipient's balance.");
                                transaction.Rollback();
                                return false;
                            }

                            // Clear parameters before reusing the command again
                            command.Parameters.Clear();

                            // Log the sender's transaction (withdrawal)
                            command.CommandText = "INSERT INTO Transactions (AccountNumber, TransactionType, Amount, RelatedAccount, Description) VALUES (@SenderAccount, @TransactionType, @Amount, @RelatedAccount, @Description);";
                            command.Parameters.AddWithValue("@SenderAccount", sender);
                            command.Parameters.AddWithValue("@TransactionType", "Transfer");
                            command.Parameters.AddWithValue("@Amount", Math.Round(amount, 2));
                            command.Parameters.AddWithValue("@RelatedAccount", recipient);
                            command.Parameters.AddWithValue("@Description", description);


                            int rowsInserted = command.ExecuteNonQuery();

                            if (rowsInserted == 0)
                            {
                                Console.WriteLine("Failed to log sender's transaction.");
                                transaction.Rollback();
                                return false;
                            }

                            // Clear parameters before reusing the command again
                            command.Parameters.Clear();

                            // Log the recipient's transaction (deposit)
                            command.CommandText = "INSERT INTO Transactions (AccountNumber, TransactionType, Amount, RelatedAccount, Description) VALUES (@RecipientAccount, @TransactionType, @Amount, @RelatedAccount, @Description);";
                            command.Parameters.AddWithValue("@RecipientAccount", recipient);
                            command.Parameters.AddWithValue("@TransactionType", "Receive");
                            command.Parameters.AddWithValue("@Amount", Math.Round(amount, 2));
                            command.Parameters.AddWithValue("@RelatedAccount", sender);
                            command.Parameters.AddWithValue("@Description", description);

                            rowsInserted = command.ExecuteNonQuery();

                            if (rowsInserted == 0)
                            {
                                Console.WriteLine("Failed to log recipient's transaction.");
                                transaction.Rollback();
                                return false;
                            }

                            // Commit the transaction to ensure all updates and inserts succeed
                            transaction.Commit();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return false;
            }
        }

        public static void ClearTransactionsTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Transactions" ;
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand resetSequenceCommand = connection.CreateCommand())
                {
                    resetSequenceCommand.CommandText = "DELETE FROM sqlite_sequence WHERE name = 'Transactions'";
                    resetSequenceCommand.ExecuteNonQuery();
                }
            }
        }

        // user database side 
        //       |
        //       |
        //     \   /
        //      \ /
        //       v

        public static bool CreateUserProfile(UserProfile userAccount)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = $"INSERT INTO UserProfile (Name, UserName, Email, Address, PhoneNumber, Picture, password, Role) VALUES (@Name, @UserName, @Email, @Address, @PhoneNumber, @Picture, @password, @Role);";
                        command.Parameters.AddWithValue("@Name", userAccount.Name);
                        command.Parameters.AddWithValue("@UserName", userAccount.UserName);
                        command.Parameters.AddWithValue("@Email", userAccount.Email);
                        command.Parameters.AddWithValue("@Address", userAccount.Address);
                        command.Parameters.AddWithValue("@PhoneNumber", userAccount.PhoneNumber);
                        command.Parameters.AddWithValue("@Picture", userAccount.Picture);
                        command.Parameters.AddWithValue("@password", userAccount.Password);
                        command.Parameters.AddWithValue("@Role", userAccount.Role);
                               

                        int rowsinserted = command.ExecuteNonQuery();
                        connection.Close();
                        if (rowsinserted > 0)
                        {
                            return true;
                        }
                    }
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public static UserProfile GetProfileByUserName(string userName)
        {
            UserProfile userProfile = null;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM UserProfile WHERE UserName = @UserName";
                        command.Parameters.AddWithValue("@UserName", userName);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userProfile = new UserProfile();
                                userProfile.Name = (string)reader["Name"];
                                userProfile.Email = (string)reader["Email"];
                                userProfile.Address = (string)reader["Address"];
                                userProfile.PhoneNumber = (string)reader["PhoneNumber"];
                                userProfile.Picture = (byte[])reader["Picture"];
                                userProfile.Password = (string)reader["password"];
                                userProfile.UserName = (string)reader["UserName"];
                                
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return userProfile;
        }

        public static UserProfile GetProfileByEmail(string email)
        {
            UserProfile userProfile = null;

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT * FROM UserProfile WHERE Email = @Email";
                        command.Parameters.AddWithValue("@Email", email);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userProfile = new UserProfile();
                                userProfile.Name = (string)reader["Name"];
                                userProfile.Email = (string)reader["Email"];
                                userProfile.Address = (string)reader["Address"];
                                userProfile.PhoneNumber = (string)reader["PhoneNumber"];
                                userProfile.Picture = (byte[])reader["Picture"];
                                userProfile.Password = (string)reader["password"];
                                userProfile.UserName = (string)reader["UserName"];

                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return userProfile;
        }

        public static bool UpdateUserProfile(UserProfile userProfile, int UserId)
        {
            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Create a new SQLite command to execute SQL
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        // Build the SQL command to update data by ID
                        command.CommandText = $"UPDATE UserProfile SET Name = @Name, Email = @Email, Address = @Address, PhoneNumber = @PhoneNumber, Picture = @Picture, password = @password WHERE UserId = @UserId;";
                        command.Parameters.AddWithValue("@Name", userProfile.Name);
                        command.Parameters.AddWithValue("@UserName", userProfile.UserName);
                        command.Parameters.AddWithValue("@Email", userProfile.Email);
                        command.Parameters.AddWithValue("@Address", userProfile.Address);
                        command.Parameters.AddWithValue("@PhoneNumber", userProfile.PhoneNumber);
                        command.Parameters.AddWithValue("@Picture", userProfile.Picture);
                        command.Parameters.AddWithValue("@password", userProfile.Password);
                        command.Parameters.AddWithValue("@UserId", UserId);

                        // Execute the SQL command to update data
                        int rowsUpdated = command.ExecuteNonQuery();
                        connection.Close();

                        // Check if any rows were updated
                        if (rowsUpdated > 0)
                        {
                            return true; // Update was successful
                        }
                    }
                }
                return false; // No rows were updated
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user profile: {ex.Message}");
                return false; // Update failed
            }
        }

        public static bool DeleteUserProfile(int UserId)
        {
            try
            {
                // Create a new SQLite connection
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Create a new SQLite command to execute SQL
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        // Build the SQL command to delete data by ID
                        command.CommandText = $"DELETE FROM UserProfile WHERE UserId = @UserId";
                        command.Parameters.AddWithValue("@UserId", UserId);

                        // Execute the SQL command to delete data
                        int rowsDeleted = command.ExecuteNonQuery();

                        // Check if any rows were deleted
                        if (rowsDeleted > 0)
                        {
                            return true; // Deletion was successful
                        }
                    }
                }
                return false; // No rows were deleted
            }
            catch (Exception ex)
            {
                Console.WriteLine("User profile deletion Error: " + ex.Message);
                return false; // Deletion failed
            }
        }

        public static void ClearUserProfileTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM UserProfile";
                    command.ExecuteNonQuery();
                }

                using (SQLiteCommand resetSequenceCommand = connection.CreateCommand())
                {
                    resetSequenceCommand.CommandText = "DELETE FROM sqlite_sequence WHERE name = 'UserProfile'";
                    resetSequenceCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
