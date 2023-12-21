using System;
using System.Collections.Generic;
using System.Linq;

namespace Banking_Application
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Data_Access_Layer dal = Data_Access_Layer.getInstance();
            //dal.loadBankAccounts();
            bool running = true;
            int maxNameLength = 20; 
            do
            {

                Console.WriteLine("");
                Console.WriteLine("***Banking Application Menu***");
                Console.WriteLine("1. Add Bank Account");
                Console.WriteLine("2. Close Bank Account");
                Console.WriteLine("3. View Account Information");
                Console.WriteLine("4. Make Lodgement");
                Console.WriteLine("5. Make Withdrawal");
                Console.WriteLine("6. Exit");
                Console.WriteLine("CHOOSE OPTION:");
                String option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        String accountType = "";
                        int loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");

                            Console.WriteLine("");
                            Console.WriteLine("***Account Types***:");
                            Console.WriteLine("1. Current Account.");
                            Console.WriteLine("2. Savings Account.");
                            Console.WriteLine("CHOOSE OPTION:");
                            accountType = Console.ReadLine();

                            loopCount++;

                        } while (!(accountType.Equals("1") || accountType.Equals("2")));

                        String name = "";
                        loopCount = 0;
                        bool containsNumber = name.Any(char.IsDigit);

                        do
                        {

                            Console.WriteLine("Enter Name: ");
                            name = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(name))
                            {
                                Console.WriteLine("Name cannot be empty. Please try again.");
                            }
                            else if (name.Length > maxNameLength) // maxlength is defined
                            {
                                Console.WriteLine($"Name cannot be longer than {maxNameLength} characters. Please try again.");
                            }
                            else if (containsNumber)
                            {
                                Console.WriteLine("Name cannot contain numbers. Please try again.");
                            }

                        } while (string.IsNullOrWhiteSpace(name) || name.Length > maxNameLength || name.Any(char.IsDigit));

                        String addressLine1 = "";
                        loopCount = 0;
                        int maxAddressLength = 30;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID ADDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Address Line 1: ");
                            addressLine1 = Console.ReadLine();

                            loopCount++;

                        } while (string.IsNullOrWhiteSpace(addressLine1) || addressLine1.Length > maxAddressLength);

                        Console.WriteLine("Enter Address Line 2: ");
                        String addressLine2 = Console.ReadLine();

                        if (addressLine2.Length > maxAddressLength) 
                        {
                            Console.WriteLine($"Address Line 2 cannot be longer than {maxAddressLength} characters.");
                            
                        }

                        Console.WriteLine("Enter Address Line 3: ");
                        String addressLine3 = Console.ReadLine();

                        if (addressLine3.Length > maxAddressLength) 
                        {
                            Console.WriteLine($"Address Line 3 cannot be longer than {maxAddressLength} characters.");
                            
                        }

                        String town = "";
                        loopCount = 0;
                        int maxTownLength = 30;
                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Town: ");
                            town = Console.ReadLine();

                            loopCount++;

                        } while (string.IsNullOrWhiteSpace(town) || town.Length > maxTownLength); 

                        double balance = -1;
                        loopCount = 0;
                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Opening Balance: ");
                            String balanceString = Console.ReadLine();

                            // check if the input can be parsed to a double and if it's non-negative
                            if (!double.TryParse(balanceString, out balance) || balance < 0)
                            {
                                Console.WriteLine("Invalid opening balance. Please enter a non-negative number.");
                                loopCount++;
                            }
                            else
                            {
                            
                                break;
                            }

                        } while (true); 

                        Bank_Account ba;

                        if (Convert.ToInt32(accountType) == Account_Type.Current_Account)
                        {
                            double overdraftAmount = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Overdraft Amount: ");
                                String overdraftAmountString = Console.ReadLine();

                                // to check if the input can be parsed to a double and if it's non-negative
                                if (!double.TryParse(overdraftAmountString, out overdraftAmount) || overdraftAmount < 0)
                                {
                                    Console.WriteLine("Please enter a valid, non-negative overdraft amount.");
                                    loopCount++;
                                }
                                else
                                {
                                   
                                    break;
                                }

                            } while (true); // looping  until valid input is provided

                            ba = new Current_Account(name, addressLine1, addressLine2, addressLine3, town, balance, overdraftAmount);
                        }

                        else
                        {

                            double interestRate = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Interest Rate: ");
                                String interestRateString = Console.ReadLine();

                                if (!double.TryParse(interestRateString, out interestRate) || interestRate < 0)
                                {
                                    Console.WriteLine("Please enter a valid, non-negative interest rate.");
                                    loopCount++;
                                }

                            } while (interestRate < 0);

                            ba = new Savings_Account(name, addressLine1, addressLine2, addressLine3, town, balance, interestRate);
                        }

                        String accNo = dal.addBankAccount(ba);

                        if (!string.IsNullOrEmpty(accNo))
                        {
                            
                            Console.WriteLine("New Account Number Is: " + accNo);
                        }
                        else
                        {
                            
                            Console.WriteLine("Failed to add a new account.");
                        }
                        break;
                    case "2":
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(accNo))
                        {
                            Console.WriteLine("Account number cannot be empty.");
                            break; 
                        }

                        ba = dal.findBankAccountByAccNo(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());

                            String ans = "";

                            do
                            {

                                Console.WriteLine("Proceed With Delection (Y/N)?");
                                ans = Console.ReadLine();

                                switch (ans)
                                {
                                    case "Y":
                                    case "y":
                                        dal.closeBankAccount(accNo);
                                        break;
                                    case "N":
                                    case "n":
                                        break;
                                    default:
                                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                                        break;
                                }
                            } while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n")));
                        }

                        break;
                    case "3":
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(accNo))
                        {
                            Console.WriteLine("Account number cannot be empty.");
                            break; 
                        }
                        ba = dal.loadBankAccount(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());
                        }

                        break;
                    case "4": //Lodge
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(accNo))
                        {
                            Console.WriteLine("Account number cannot be empty.");
                            break; 
                        }
                        ba = dal.loadBankAccount(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToLodge = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Lodge: ");
                                String amountToLodgeString = Console.ReadLine();

                                if (!double.TryParse(amountToLodgeString, out amountToLodge) || amountToLodge < 0)
                                {
                                    Console.WriteLine("Invalid amount. Please enter a non-negative number.");
                                    loopCount++;
                                }

                            } while (amountToLodge < 0);
                            dal.lodge(accNo, amountToLodge);
                        }
                        break;
                    case "5": //Withdraw
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.loadBankAccount(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToWithdraw = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Withdraw (€" + ba.getAvailableFunds() + " Available): ");
                                String amountToWithdrawString = Console.ReadLine();

                                if (!double.TryParse(amountToWithdrawString, out amountToWithdraw) || amountToWithdraw < 0)
                                {
                                    Console.WriteLine("Invalid amount. Please enter a non-negative number.");
                                    loopCount++;
                                }
                                else if (amountToWithdraw > ba.getAvailableFunds())
                                {
                                    Console.WriteLine("Insufficient funds. Please enter an amount up to €" + ba.getAvailableFunds());
                                    amountToWithdraw = -1; // Reset the amount to force re-validation
                                    loopCount++;
                                }
                            } while (amountToWithdraw < 0);

                            bool withdrawalOK = dal.withdraw(accNo, amountToWithdraw);

                            if (withdrawalOK == false)
                            {

                                Console.WriteLine("Insufficient Funds Available.");
                            }
                        }
                        break;
                    case "6":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                        break;
                }


            } while (running != false);

        }

    }
}