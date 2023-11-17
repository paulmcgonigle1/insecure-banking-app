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

        private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance;
        private readonly EncryptionService encryptionService;



        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
           
            accounts = new List<Bank_Account>();
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

        public void loadBankAccounts()
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts";
                    SqliteDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {

                        string decryptedName = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(1))));
                        string decryptedAddressLine1 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(2))));
                        string decryptedAddressLine2 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(3))));
                        string decryptedAddressLine3 = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(4))));
                        string decryptedTown = Encoding.UTF8.GetString(encryptionService.Decrypt(Convert.FromBase64String(dr.GetString(3))));

                        int accountType = dr.GetInt16(7);

                        if (accountType == Account_Type.Current_Account)
                        {
                            Current_Account ca = new Current_Account();
                            ca.accountNo = dr.GetString(0);
                            ca.name = decryptedName;
                            ca.address_line_1 = decryptedAddressLine1;
                            ca.address_line_2 = decryptedAddressLine2;
                            ca.address_line_3 = decryptedAddressLine3;
                            ca.town = decryptedTown;
                            ca.balance = dr.GetDouble(6);
                            ca.overdraftAmount = dr.GetDouble(8);
                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = dr.GetString(0);
                            sa.name = decryptedName;
                            sa.address_line_1 = decryptedAddressLine1;
                            sa.address_line_2 = decryptedAddressLine2;
                            sa.address_line_3 = decryptedAddressLine3;
                            sa.town = decryptedTown;
                            sa.balance = dr.GetDouble(6);
                            sa.interestRate = dr.GetDouble(9);
                            accounts.Add(sa);
                        }


                    }

                }

            }
        }

        public String addBankAccount(Bank_Account ba)
        {

            if (ba.GetType() == typeof(Current_Account))
                ba = (Current_Account)ba;
            else
                ba = (Savings_Account)ba;

            accounts.Add(ba);

            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                // Encrypt PPI data
                string encryptedName = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.name)));
                string encryptedAddressLine1 = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.address_line_1)));
                string encryptedAddressLine2 = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.address_line_2)));
                string encryptedAddressLine3 = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.address_line_3)));
                string encryptedTown = Convert.ToBase64String(encryptionService.Encrypt(Encoding.UTF8.GetBytes(ba.town)));
                command.CommandText = $@"
    INSERT INTO Bank_Accounts VALUES(
    '{ba.accountNo}', 
    '{encryptedName}', 
    '{encryptedAddressLine1}', 
    '{encryptedAddressLine2}', 
    '{encryptedAddressLine3}', 
    '{encryptedTown}', 
    {ba.balance}, 
    {(ba.GetType() == typeof(Current_Account) ? 1 : 2)}, ";

                if (ba.GetType() == typeof(Current_Account))
                {
                    Current_Account ca = (Current_Account)ba;
                    command.CommandText += ca.overdraftAmount + ", NULL)";
                }

                else
                {
                    Savings_Account sa = (Savings_Account)ba;
                    command.CommandText += "NULL," + sa.interestRate + ")";
                }

                command.ExecuteNonQuery();

            }

            return ba.accountNo;

        }

        public Bank_Account findBankAccountByAccNo(String accNo) 
        { 
        
            foreach(Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    return ba;
                }

            }

            return null; 
        }

        public bool closeBankAccount(String accNo) 
        {

            Bank_Account toRemove = null;
            
            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    toRemove = ba;
                    break;
                }

            }

            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = '" + toRemove.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {

            Bank_Account toLodgeTo = null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }

            }

            if (toLodgeTo == null)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = " + toLodgeTo.balance + " WHERE accountNo = '" + toLodgeTo.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {

            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }

            }

            if (toWithdrawFrom == null || result == false)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = " + toWithdrawFrom.balance + " WHERE accountNo = '" + toWithdrawFrom.accountNo + "'";
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

    }
}
