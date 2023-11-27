using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public abstract class Bank_Account
    {
        //Restricting access modifers to fields to encapsulate them
        private String _accountNo;

        public String AccountNo
        {
            get { return _accountNo; }
            private set { _accountNo = value; }
        }

        private String _name;

        public String Name
        {
            get { return _name; }
            private set { _name = value; }  
        }

        private String _address_line_1;
        public String Address_Line_1
        {
            get { return _address_line_1; }
            private set { _address_line_1 = value;}
        }
        private String _address_line_2;
        public String Address_Line_2
        {
            get { return _address_line_2; }
            private set { _address_line_2 = value; }
        }
        private String _address_line_3;

        public String Address_Line_3
        {
            get { return _address_line_3; }
            private set { _address_line_3 = value;}
        }
        private String _town;
        public String Town
        {
            get { return _town; }
            private set { _town = value; }
        }

        private double _balance;

        public double Balance
        {
            get { return _balance; }
            protected set { _balance = value; }
        }

        public Bank_Account()
        {

        }
        
        public Bank_Account(String name, String address_line_1, String address_line_2, String address_line_3, String town, double balance)
        {
            _accountNo = System.Guid.NewGuid().ToString();
            Name = name;
            Address_Line_1 = address_line_1;
            Address_Line_2 = address_line_2;
            Address_Line_3 = address_line_3;
            Town = town;
            Balance = balance;
        }

        public void lodge(double amountIn)
        {

            Balance += amountIn;

        }

        public abstract bool withdraw(double amountToWithdraw);

        public abstract double getAvailableFunds();

        public override String ToString()
        {

            return "\nAccount No: " + _accountNo + "\n" +
            "Name: " + Name + "\n" +
            "Address Line 1: " + Address_Line_1 + "\n" +
            "Address Line 2: " + Address_Line_2 + "\n" +
            "Address Line 3: " + Address_Line_3 + "\n" +
            "Town: " + Town + "\n" +
            "Balance: " + Balance + "\n";

    }

    }
}
