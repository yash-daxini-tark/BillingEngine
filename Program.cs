using System.Security.Cryptography.X509Certificates;
using BillingEngine;
using CsvHelper.Configuration.Attributes;

namespace BillingEngine;
class Program
{
    public static void arrangeDate(List<AWSResourceUsage> l)
    {
        for (int i = 0; i < l.Count(); i++)
        {
            AWSResourceUsage usage = l[i];
            var differenceOfDates = usage.UsedUntil - usage.UsedFrom;
            var totalDays = DateTime.DaysInMonth(usage.UsedFrom.Year, usage.UsedFrom.Month);
            //var totalSeconds = totalDays * 24 * 60 * 60;
            var myDate = usage.UsedFrom;
            var lastDayOfMonth = new DateTime(myDate.Year, myDate.Month, DateTime.DaysInMonth(myDate.Year, myDate.Month), 23, 59, 59);
            var remainingSecondsInCurrentMonth = lastDayOfMonth - usage.UsedFrom;
            //Console.WriteLine(usage.UsedFrom + " " + usage.UsedUntil + " " + remainingSecondsInCurrentMonth + " " + differenceOfDates + " " + remainingSecondsInCurrentMonth.TotalSeconds + " " + differenceOfDates.TotalSeconds + " " + lastDayOfMonth);
            int curMonth = usage.UsedFrom.Month;
            int curYear = usage.UsedFrom.Year;
            var totalDifferenceInSeconds = differenceOfDates.TotalSeconds;
            if (remainingSecondsInCurrentMonth.TotalSeconds < differenceOfDates.TotalSeconds)
            {
                var min = Math.Min(remainingSecondsInCurrentMonth.TotalSeconds, totalDifferenceInSeconds);
                usage.UsedUntil = usage.UsedFrom.AddSeconds(min);
                totalDifferenceInSeconds -= min;
                //Console.WriteLine(usage.UsedFrom+ " " + usage.UsedUntil);
                //Console.WriteLine(usage.CustomerID);
                curMonth++;
                while (totalDifferenceInSeconds > 0)
                {
                    var totalDaysInCurMonth = DateTime.DaysInMonth(curYear, curMonth) * 24 * 60 * 60;
                    min = Math.Min(totalDaysInCurMonth, totalDifferenceInSeconds);
                    totalDifferenceInSeconds -= min;
                    DateTime from = new DateTime(curYear, curMonth, 1);
                    DateTime till = from.AddSeconds(min);
                    AWSResourceUsage tempObj = new AWSResourceUsage(usage.CustomerID, usage.EC2InstanceID, usage.EC2InstanceType, from, till);
                    l.Add(tempObj);
                    curMonth++;
                    if (curMonth == 13)
                    {
                        curYear++;
                        curMonth = 1;
                    }
                    //Console.WriteLine(curMonth + " " + tempObj.CustomerID + " " + tempObj.UsedFrom + " " + tempObj.UsedUntil + " " + totalDifferenceInSeconds);
                }
            }
        }
    }
    public static void Main(string[] args)
    {
        #region Path of files

        string pathOfAWSResourceUsage = "C:/Users/YashDaxini/Downloads/TestCases/TestCases/Case1/Input/AWSCustomerUsage.csv";
        string pathOfAWSResourceTypes = "C:/Users/YashDaxini/Downloads/TestCases/TestCases/Case1/Input/AWSResourceTypes.csv";
        string pathOfCustomer = "C:/Users/YashDaxini/Downloads/TestCases/TestCases/Case1/Input/Customer.csv";

        #endregion

        #region Get Input

        GenericList<Customer> customerObj = ReadCSV<Customer>.LoadDataFromCsv(pathOfCustomer);
        List<Customer> customers = customerObj.DataList;

        GenericList<AWSResourceTypes> awsResourceTypesObj = ReadCSV<AWSResourceTypes>.LoadDataFromCsv(pathOfAWSResourceTypes);
        List<AWSResourceTypes> awsResourceTypes = awsResourceTypesObj.DataList;

        GenericList<AWSResourceUsage> awsResourceUsageObj = ReadCSV<AWSResourceUsage>.LoadDataFromCsv(pathOfAWSResourceUsage);
        List<AWSResourceUsage> awsResourceUsage = awsResourceUsageObj.DataList;

        //foreach (var item in customers)
        //{
        //    Console.WriteLine(item.ToString());
        //}
        //foreach (var item in awsResourceTypes)
        //{
        //    Console.WriteLine(item.ToString());
        //}
        //foreach (var item in awsResourceUsage)
        //{
        //    Console.WriteLine(item.ToString());
        //}

        #endregion

        arrangeDate(awsResourceUsage);

        var grouped = awsResourceUsage.OrderBy(resource => resource.CustomerID).ThenBy(resource => resource.UsedFrom).GroupBy(resource => new { resource.CustomerID, resource.EC2InstanceType, resource.UsedFrom.Year, resource.UsedFrom.Month }).
            Select(resource => new { key = resource.Key, list = resource.Select(resource => resource).ToList() });



        foreach (var item in grouped)
        {
            var customerName = customers.Where(customer => customer.CustomerID.Replace("-", "") == item.key.CustomerID).Select(Customer => Customer.CustomerName).ToList()[0];
            //Console.WriteLine(item.key + " " + string.Format(" ", item.list));
            Console.WriteLine(customerName + " " + item.key.Month + " " + item.key.Year);
            double amount = 0;
            double cost = Convert.ToDouble(awsResourceTypes.Where(type => type.InstanceType.Equals(item.key.EC2InstanceType)).Select(type => type.Charge).ToList()[0].ToString().Substring(1));
            //Console.WriteLine(cost);
            foreach (var item1 in item.list)
            {
                var diff = item1.UsedUntil - item1.UsedFrom;
                amount += (diff.TotalHours * cost);
            }
            Console.WriteLine(amount);
        }
    }
}