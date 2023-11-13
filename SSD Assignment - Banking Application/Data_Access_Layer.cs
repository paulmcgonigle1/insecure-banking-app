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
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            accounts = new List<Bank_Account>();
        }

        public static Data_Access_Layer getInstance()
        {
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
                    
                    while(dr.Read())
                    {


                        int accountType = dr.GetInt16(7);

                        // Decrypt each field
                        string accountNo = Encoding.UTF8.GetString(EncrpytionService.Decrypt(Convert.FromBase64String(dr.GetString(0))));
                        string name = Encoding.UTF8.GetString(EncrpytionService.Decrypt(Convert.FromBase64String(dr.GetString(1))));
                        string addressLine1 = Encoding.UTF8.GetString(EncrpytionService.Decrypt(Convert.FromBase64String(dr.GetString(2))));
                        string addressLine2 = Encoding.UTF8.GetString(EncrpytionService.Decrypt(Convert.FromBase64String(dr.GetString(3))));
                        string addressLine3 = Encoding.UTF8.GetString(EncrpytionService.Decrypt(Convert.FromBase64String(dr.GetString(4))));
                        string town = Encoding.UTF8.GetString(EncrpytionService.Decrypt(Convert.FromBase64String(dr.GetString(5))));
                        double balance = dr.GetDouble(6); 
                        double overdraftAmount = dr.GetDouble(8);
                        double interestRate = dr.GetDouble(9);
                       
                        if (accountType == Account_Type.Current_Account)
                        {
                            Current_Account ca = new Current_Account()
                            {
                                accountNo = accountNo,
                                name = name,
                                address_line_1 = addressLine1,
                                address_line_2 = addressLine2,
                                address_line_3 = addressLine3,
                                town = town,
                                balance = balance,
                                overdraftAmount = overdraftAmount
                            };
                            
                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account()
                            {
                                accountNo = accountNo,
                                name = name,
                                address_line_1 = addressLine1,
                                address_line_2 = addressLine2,
                                address_line_3 = addressLine3,
                                town = town,
                                balance = balance,
                                interestRate = interestRate
                            };
                            
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
            //encrpting senstive info
            byte[] encryptedName = EncrpytionService.Encrypt(Encoding.UTF8.GetBytes(ba.name));
            byte[] encryptedAccountNum = EncrpytionService.Encrypt(Encoding.UTF8.GetBytes(ba.accountNo));
            byte[] encryptedAddress1 = EncrpytionService.Encrypt(Encoding.UTF8.GetBytes(ba.address_line_1));
            byte[] encryptedAddress2 = EncrpytionService.Encrypt(Encoding.UTF8.GetBytes(ba.address_line_2));
            byte[] encryptedAddress3 = EncrpytionService.Encrypt(Encoding.UTF8.GetBytes(ba.address_line_3));
            byte[] encryptedTown = EncrpytionService.Encrypt(Encoding.UTF8.GetBytes(ba.town));
                

            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Bank_Accounts VALUES(" +
                    "'" + Convert.ToBase64String(encryptedAccountNum) + "', " +
                    "'" + Convert.ToBase64String(encryptedName) + "', " +
                    "'" + Convert.ToBase64String(encryptedAddress1) + "', " +
                    "'" + Convert.ToBase64String(encryptedAddress2) + "', " +
                    "'" + Convert.ToBase64String(encryptedAddress3) + "', " +
                    "'" + Convert.ToBase64String(encryptedTown) + "', " +
                    ba.balance + ", " +
                    (ba.GetType() == typeof(Current_Account) ? 1 : 2) + ", ";

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
