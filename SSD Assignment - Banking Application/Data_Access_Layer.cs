using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SSD_Assignment___Banking_Application;
using System.Diagnostics;
using System.Threading;
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

        //setup for the eventLogger
        public static void SetupEventLogSource()
        {
            if (!EventLog.SourceExists("MyBankApp"))
            {
                EventLog.CreateEventSource("MyBankApp", "Application");
            }
        }
        //method for logging with ease
        //private void Log(string message, EventLogEntryType type = EventLogEntryType.Information)
        //{
        //    using (EventLog eventLog = new EventLog("Application"))
        //    {
        //        eventLog.Source = "MyBankApp";
        //        eventLog.WriteEntry(message, type);
        //    }
        //}

        //ddos checking if there is enough disk space
        public static bool IsEnoughDiskSpace(long minRequiredSpace)
        {
            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory));

                //// Check if the drive is ready to ensure it is accessible
                //if (drive.IsReady)
                //{
                //    Console.WriteLine($"Drive {drive.Name}");
                //    Console.WriteLine($"Available free space: {drive.AvailableFreeSpace} bytes");
                //}
                //else
                //{
                //    Console.WriteLine($"Drive {drive.Name} is not ready.");
                //}
                return drive.IsReady && drive.AvailableFreeSpace > minRequiredSpace;


                
            }
            catch (Exception ex)
            {                
                return false;
            }
        }
        public static bool IsMemoryUsageAcceptable(long maxAllowedMemory)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                // Refresh the process info to get updated values
                currentProcess.Refresh();

                //Console.WriteLine($"Current memory usage: {currentProcess.PrivateMemorySize64} bytes");

                if (currentProcess.PrivateMemorySize64 > maxAllowedMemory)
                {
                    //Console.WriteLine($"Memory usage exceeds the acceptable limit of {maxAllowedMemory} bytes.");
                    return false;
                }
                else
                {
                    //Console.WriteLine($"Memory usage is within the acceptable limit of {maxAllowedMemory} bytes.");
                    return true;
                }
            }
        }

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            initialiseDatabase(); // Ensure the database and table are initialized

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
                        interestRate REAL,
                        dataHash TEXT
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
                    // Read encrypted data directly
                    string encryptedName = dr.GetString(1);
                    string encryptedAddressLine1 = dr.GetString(2);
                    string encryptedAddressLine2 = dr.GetString(3);
                    string encryptedAddressLine3 = dr.GetString(4);
                    string encryptedTown = dr.GetString(5);
                    double balance = dr.GetDouble(6);

                    // Use encrypted data for hash generation
                    string loadedData = $"{encryptedName}{encryptedAddressLine1}{encryptedAddressLine2}{encryptedAddressLine3}{encryptedTown}{balance}";
                    string regeneratedHash = HashUtility.GenerateHash(loadedData);

                   

                    // Compare with the stored hash
                    string storedHash = dr.GetString(10);
                    Console.WriteLine($"Debug - Stored Hash: {storedHash}");

                    if (regeneratedHash != storedHash)
                    {
                        throw new InvalidOperationException("Data integrity check failed for account " + accountNo);
                    }

                    // Decrypt the data after hash verification
                    string decryptedName = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(encryptedName)));
                    string decryptedAddressLine1 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(encryptedAddressLine1)));
                    string decryptedAddressLine2 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(encryptedAddressLine2)));
                    string decryptedAddressLine3 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(encryptedAddressLine3)));
                    string decryptedTown = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(encryptedTown)));

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
            const long requiredDiskSpace = 500; // bytes
            const long maxAllowedMemory = 1000000000; // Example: 1 GB

            //checking disk space
            if (!IsEnoughDiskSpace(requiredDiskSpace))
            {
                Console.WriteLine("There is not sufficient Disk Space to run operation");
                //Log($"Insufficient disk space to add new bank account {ba.AccountNo} at {DateTime.Now}", EventLogEntryType.Warning);
                return null; // or throw an exception
            }

            // Check memory usage
            if (!IsMemoryUsageAcceptable(maxAllowedMemory))
            {
                Console.WriteLine("Insufficient Memory to add a new bank account.");
                //Log($"Insufficient memory space to add new bank account {ba.AccountNo} at {DateTime.Now}", EventLogEntryType.Warning);

                return null; // or throw an exception
            }


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

                // Generate a hash of the account data
                string accountData = $"{encryptedName}{encryptedAddressLine1}{encryptedAddressLine2}{encryptedAddressLine3}{encryptedTown}{ba.Balance}";
                string dataHash = HashUtility.GenerateHash(accountData);
               
                command.CommandText = @"
                INSERT INTO Bank_Accounts (accountNo, name, address_line_1, address_line_2, address_line_3, town, balance, accountType, overdraftAmount, interestRate, dataHash)
                VALUES (@AccountNo, @Name, @AddressLine1, @AddressLine2, @AddressLine3, @Town, @Balance, @AccountType, @OverdraftAmount, @InterestRate, @DataHash)
                ";
                command.Parameters.AddWithValue("@AccountType", ba is Current_Account ? 1 : 2);
                command.Parameters.AddWithValue("@AccountNo", ba.AccountNo);
                command.Parameters.AddWithValue("@Name", encryptedName);
                command.Parameters.AddWithValue("@AddressLine1", encryptedAddressLine1);
                command.Parameters.AddWithValue("@AddressLine2", encryptedAddressLine2);
                command.Parameters.AddWithValue("@AddressLine3", encryptedAddressLine3);
                command.Parameters.AddWithValue("@Town", encryptedTown);
                command.Parameters.AddWithValue("@Balance", ba.Balance);
                command.Parameters.AddWithValue("@DataHash", dataHash); // Add the hash as a parameter

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
                // Log the successful addition of a bank account
               

            }
            //Log($"Bank account {ba.AccountNo} added successfully at {DateTime.Now}.", EventLogEntryType.SuccessAudit);
            return ba.AccountNo;

        }

        public Bank_Account findBankAccountByAccNo(String accNo) 
        { 
        
            return loadBankAccount(accNo);
        }

        public bool closeBankAccount(String accNo) 
        {
            // Disk space check
            long requiredSpace = 500;
            const long maxAllowedMemory = 1000000000; // Example: 1 GB

            if (!IsEnoughDiskSpace(requiredSpace))
            {
                //Log($"Insufficient disk space to close bank account at {DateTime.Now}", EventLogEntryType.Warning);
                return false; // or throw an exception
            }

            // Check memory usage
            if (!IsMemoryUsageAcceptable(maxAllowedMemory))
            {
                Console.WriteLine("Insufficient Memory to add a new bank account.");
                //Log($"Insufficient memory space to add new bank account {ba.AccountNo} at {DateTime.Now}", EventLogEntryType.Warning);

                return false; // or throw an exception
            }


            try
            {

                Bank_Account toRemove = loadBankAccount(accNo);

                if (toRemove == null)
                {
                    // Account not found in the database
                    //Log($"Attempted to close non-existing account {accNo} at {DateTime.Now}", EventLogEntryType.Warning);
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


                    //Log($"Bank account {accNo} closed successfully at {DateTime.Now}.");

                    return true;
                }
            }
            catch (Exception ex)
            {
                //Log($"Error closing account {accNo}: {ex.Message} at {DateTime.Now}", EventLogEntryType.Error);
                return false;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {
            // Disk space check
            long requiredSpace = 500;
            const long maxAllowedMemory = 1000000000; // Example: 1 GB

            if (!IsEnoughDiskSpace(requiredSpace))
            {
                //Log($"Insufficient disk space to lodge to bank account {accNo} at {DateTime.Now}", EventLogEntryType.Warning);
                return false; // or throw an exception
            }

            // Check memory usage
            if (!IsMemoryUsageAcceptable(maxAllowedMemory))
            {
                Console.WriteLine("Insufficient Memory to add a new bank account.");
                //Log($"Insufficient memory space to add new bank account {ba.AccountNo} at {DateTime.Now}", EventLogEntryType.Warning);

                return false; // or throw an exception
            }


            Bank_Account toLodgeTo = loadBankAccount(accNo);

            if (toLodgeTo == null)
            {
                //Log($"Failed to lodge into account {accNo}: ammount - {amountToLodge} at {DateTime.Now}", EventLogEntryType.Error);
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


                //Log($"Succesfully lodged into account {accNo}: ammount - {amountToLodge} at {DateTime.Now}", EventLogEntryType.Error);

                return true;
            }
        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {

            // Disk space check
            long requiredSpace = 500;
            const long maxAllowedMemory = 1000000000; // Example: 1 GB

            if (!IsEnoughDiskSpace(requiredSpace))
            {
                //Log($"Insufficient disk space to withdraw from bank account {accNo} at {DateTime.Now}", EventLogEntryType.Warning);
                return false; // or throw an exception
            }

            // Check memory usage
            if (!IsMemoryUsageAcceptable(maxAllowedMemory))
            {
                Console.WriteLine("Insufficient Memory to add a new bank account.");
                //Log($"Insufficient memory space to add new bank account {ba.AccountNo} at {DateTime.Now}", EventLogEntryType.Warning);

                return false; // or throw an exception
            }


            Bank_Account toWithdrawFrom = loadBankAccount(accNo); ;
        
            if (toWithdrawFrom == null)
            {
                //Log($"Failed to withdraw from account {accNo}: ammount - {amountToWithdraw} at {DateTime.Now}", EventLogEntryType.Error);

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


                //Log($"Succesfully withdrew from account account {accNo}: ammount - {amountToWithdraw} at {DateTime.Now}", EventLogEntryType.Error);

                return true;
            }
            }

    }
}
