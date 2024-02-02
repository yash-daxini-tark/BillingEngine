using System;
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

        public AWSResourceUsage() { }
        public AWSResourceUsage(string customerID, string eC2InstanceID, string eC2InstanceType, DateTime usedFrom, DateTime usedUntil)
        {
            CustomerID = customerID;
            EC2InstanceID = eC2InstanceID;
            EC2InstanceType = eC2InstanceType;
            UsedFrom = usedFrom;
            UsedUntil = usedUntil;
            totalCost = 0;
        }
        public AWSResourceUsage(AWSResourceUsage aWSResourceUsage)
        {
            CustomerID = aWSResourceUsage.CustomerID;
            EC2InstanceID = aWSResourceUsage.EC2InstanceID;
            EC2InstanceType = aWSResourceUsage.EC2InstanceType;
            UsedFrom = aWSResourceUsage.UsedFrom;
            UsedUntil = aWSResourceUsage.UsedUntil;
            totalCost = 0;
        }
        public List<AWSResourceUsage> splitDateBasedOnItsMonth()
        {
            List<AWSResourceUsage> updatedResources = new List<AWSResourceUsage>();
            var totalDifferenceInSeconds = (UsedUntil - UsedFrom).TotalSeconds;
            DateTime startDateOfService = UsedFrom;
            while (totalDifferenceInSeconds > 0)
            {
                DateTime endDateOfMonth = new DateTime(startDateOfService.Year, startDateOfService.Month, DateTime.DaysInMonth(startDateOfService.Year, startDateOfService.Month), 23, 59, 59);
                double minimumSeconds = Math.Min((endDateOfMonth - startDateOfService).TotalSeconds, totalDifferenceInSeconds);
                totalDifferenceInSeconds -= minimumSeconds + 1;
                updatedResources.Add(new AWSResourceUsage(CustomerID, EC2InstanceID, EC2InstanceType, startDateOfService, startDateOfService.AddSeconds(minimumSeconds)));
                startDateOfService += new TimeSpan(0, 0, 0, (int)Math.Ceiling(minimumSeconds + 1));
            }
            return updatedResources;
        }
        public double calculateCost(double charge)
        {
            totalCost = Math.Ceiling((UsedUntil - UsedFrom).TotalHours) * charge;
            return totalCost;
        }
        override
        public string ToString()
        {
            return CustomerID + " " + EC2InstanceID + " " + EC2InstanceType + " " + UsedFrom.ToString() + " " + UsedUntil + " " + totalCost;
        }
    }
}
