using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace BillingEngine
{
    internal class Customer
    {
        [Name("Customer ID")]
        public string CustomerID { get; set; }
        [Name("Customer Name")]
        public string CustomerName { get; set; }

        public Customer(string customerID,string customerName)
        {
            CustomerID = customerID;
            CustomerName = customerName;

        }
        public Customer() { }   
        override
        public string ToString()
        {
            return CustomerID + " " + CustomerName;
        }
    }
}
