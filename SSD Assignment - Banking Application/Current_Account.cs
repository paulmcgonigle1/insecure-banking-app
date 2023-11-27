using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public sealed class Current_Account: Bank_Account
    {

        private double _overdraftAmount;

        public double OverdraftAmmount
        {
            get { return _overdraftAmount; }
            private set { _overdraftAmount = value; }
        }

        public Current_Account(): base()
        {

        }
        
        public Current_Account(String name, String address_line_1, String address_line_2, String address_line_3, String town, double balance, double overdraftAmount) : base(name, address_line_1, address_line_2, address_line_3, town, balance)
        {
            OverdraftAmmount = overdraftAmount;
        }

        public override bool withdraw(double amountToWithdraw)
        {
            double avFunds = getAvailableFunds();

            if (avFunds >= amountToWithdraw)
            {
                Balance -= amountToWithdraw;
                return true;
            }

            else
                return false;

        }

        public override double getAvailableFunds()
        {
            return (base.Balance + OverdraftAmmount);
        }

        public override String ToString()
        {

            return base.ToString() +
                "Account Type: Current Account\n" +
                "Overdraft Amount: " + OverdraftAmmount + "\n";

        }

    }
}
