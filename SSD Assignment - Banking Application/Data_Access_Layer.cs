using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SSD_Assignment___Banking_Application;

namespace Banking_Application
{
    public class Data_Access_Layer
    {
        //nowow need to inject key and encryption services
        //2. Modify Data storage and retrieval for 'addbankaccount' and 'loadbankaccount'
        //3. Secure key and iv storage
        //4. Error handling
        //5. Logging operations

        //private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance;
        private readonly EncryptionService encryptionService;



        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
           
            //accounts = new List<Bank_Account>();
            KeyManagementService keyManagementService = new KeyManagementService(/* any required parameters */);
            encryptionService = new EncryptionService(keyManagementService); // Properly initialized here
        }

        public static Data_Access_Layer getInstance()
        {
            if (instance == null)
            {
                instance = new Data_Access_Layer(); // Instance is created here
            }
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount REAL,
                        interestRate REAL
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();
                
            }
        }

        public Bank_Account loadBankAccount(string accountNo)
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts WHERE AccountNo = @accountNo";
                    command.Parameters.AddWithValue("@accountNo", accountNo);

                    using (SqliteDataReader dr = command.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string decryptedName = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(1))));
                            string decryptedAddressLine1 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(2))));
                            string decryptedAddressLine2 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(3))));
                            string decryptedAddressLine3 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(4))));
                            string decryptedTown = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(5))));
                            double balance = dr.GetDouble(6);

                            int accountType = dr.GetInt16(7);

                            if (accountType == Account_Type.Current_Account)
                            {
                                double overdraftAmount = dr.GetDouble(8);
                                return new Current_Account(decryptedName, decryptedAddressLine1, decryptedAddressLine2, decryptedAddressLine3, decryptedTown, balance, overdraftAmount);
                            }
                            else
                            {
                                double interestRate = dr.GetDouble(9);
                                return new Savings_Account(decryptedName, decryptedAddressLine1, decryptedAddressLine2, decryptedAddressLine3, decryptedTown, balance, interestRate);
                            }
                        }
                    } 

           
                }
            }
            return null;

        }
        

        public String addBankAccount(Bank_Account ba)
        {

            if (ba.GetType() == typeof(Current_Account))
                ba = (Current_Account)ba;
            else
                ba = (Savings_Account)ba;

            //accounts.Add(ba);

            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                
                // Encrypt PPI data
                string encryptedName = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.Name)));
                string encryptedAddressLine1 = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.Address_Line_1)));
                string encryptedAddressLine2 = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.Address_Line_2)));
                string encryptedAddressLine3 = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.Address_Line_3)));
                string encryptedTown = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.Town)));
               
                command.CommandText = @"
                INSERT INTO Bank_Accounts (accountNo, name, address_line_1, address_line_2, address_line_3, town, balance, accountType, overdraftAmount, interestRate)
                VALUES (@AccountNo, @Name, @AddressLine1, @AddressLine2, @AddressLine3, @Town, @Balance, @AccountType, @OverdraftAmount, @InterestRate)
                ";
                command.Parameters.AddWithValue("@AccountType", ba is Current_Account ? 1 : 2);
                command.Parameters.AddWithValue("@AccountNo", ba.AccountNo);
                command.Parameters.AddWithValue("@Name", encryptedName);
                command.Parameters.AddWithValue("@AddressLine1", encryptedAddressLine1);
                command.Parameters.AddWithValue("@AddressLine2", encryptedAddressLine2);
                command.Parameters.AddWithValue("@AddressLine3", encryptedAddressLine3);
                command.Parameters.AddWithValue("@Town", encryptedTown);
                command.Parameters.AddWithValue("@Balance", ba.Balance);

                if (ba.GetType() == typeof(Current_Account))
                {
                    Current_Account ca = (Current_Account)ba;
                    command.Parameters.AddWithValue("@OverdraftAmount", ca.OverdraftAmmount);
                    command.Parameters.AddWithValue("@InterestRate", DBNull.Value);
                }
                else
                {
                    Savings_Account sa = (Savings_Account)ba;
                    command.Parameters.AddWithValue("@InterestRate", sa.InterestRate);
                    command.Parameters.AddWithValue("@OverdraftAmount", DBNull.Value);
                }


                command.ExecuteNonQuery();

            }

            return ba.AccountNo;

        }

        public Bank_Account findBankAccountByAccNo(String accNo) 
        { 
        
            return loadBankAccount(accNo);
        }

        public bool closeBankAccount(String accNo) 
        {

            Bank_Account toRemove = loadBankAccount(accNo);

            if (toRemove == null)
            {
                // Account not found in the database
                return false;
            }
            else
            {
                // Account found, proceed with deletion
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@accountNo", accNo); // Use parameterized query for security
                    command.ExecuteNonQuery();
                }

                

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {

            Bank_Account toLodgeTo = loadBankAccount(accNo);

            if (toLodgeTo == null)
            {
                return false; // Account not found
            }
            else
            {
                // Perform the lodgment
                toLodgeTo.lodge(amountToLodge);

                // Update the account balance in the database
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@balance", toLodgeTo.Balance);
                    command.Parameters.AddWithValue("@accountNo", accNo); // Use parameterized query for security
                    command.ExecuteNonQuery();
                }

                

                return true;
            }
        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {

            Bank_Account toWithdrawFrom = loadBankAccount(accNo); ;
        
            if (toWithdrawFrom == null)
            {
                return false;
            }
            bool result = toWithdrawFrom.withdraw(amountToWithdraw);
            if (!result)
            {
                return false; // Withdrawal failed (e.g., insufficient funds)
            }
            else
            {
                // Update the account balance in the database
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@balance", toWithdrawFrom.Balance);
                    command.Parameters.AddWithValue("@accountNo", accNo); // Use parameterized query for security
                    command.ExecuteNonQuery();
                }


                return true;
            }
            }

    }
}
