using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingEngine
{
    internal class BillResourceInformation
    {
        public BillResourceInformation(string resourceType, int numberOfResources, string usedTime, string billedTime, string rate, double totalAmount)
        {
            this.resourceType = resourceType;
            this.numberOfResources = numberOfResources;
            this.usedTime = usedTime;
            this.billedTime = billedTime;
            this.rate = rate;
            this.totalAmount = totalAmount;
        }
        public string resourceType { get; set; }
        public int numberOfResources { get; set; }
        public string usedTime { get; set; }
        public string billedTime { get; set; }
        public string rate { get; set; }
        public double totalAmount { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4},${5}", resourceType, numberOfResources, usedTime, billedTime, rate, totalAmount);
        }

    }
}
