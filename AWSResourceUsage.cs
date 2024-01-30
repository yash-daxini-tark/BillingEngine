﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace BillingEngine
{
    internal class AWSResourceUsage
    {
        [Name("Customer ID")]
        public string CustomerID { get; set; }
        [Name("EC2 Instance ID")]
        public string EC2InstanceID { get; set; }
        [Name("EC2 Instance Type")]
        public string EC2InstanceType { get; set; }
        [Name("Used From")]
        public DateTime UsedFrom { get; set; }
        [Name("Used Until")]
        public DateTime UsedUntil { get; set; }

        [Ignore]
        public double totalCost { get; set; }   

        public AWSResourceUsage(string customerID, string eC2InstanceID, string eC2InstanceType, DateTime usedFrom, DateTime usedUntil)
        {
            CustomerID = customerID;
            EC2InstanceID = eC2InstanceID;
            EC2InstanceType = eC2InstanceType;
            UsedFrom = usedFrom;
            UsedUntil = usedUntil;
            totalCost = 0;
        }
        public AWSResourceUsage()
        {

        }
        override
            public string ToString()
        {
            return CustomerID + " " + EC2InstanceID + " " + EC2InstanceType + " " + UsedFrom.ToString() + " " + UsedUntil + " " + totalCost;
        }
    }
}
