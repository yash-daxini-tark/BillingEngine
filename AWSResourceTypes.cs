using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace BillingEngine
{
    internal class AWSResourceTypes
    {
        [Name("Instance Type")]
        public string InstanceType { get; set; }
        [Name("Charge/Hour")]
        //[System.ComponentModel.TypeConverter(typeof(DoubleConverter))]
        public string Charge { get; set; }
        public AWSResourceTypes(string instanceType, string charge)
        {
            InstanceType = instanceType;
            Charge = charge;
        }
        public AWSResourceTypes() { }
        override
        public string ToString()
        {
            return InstanceType + " " + Charge;
        }
    }
}
