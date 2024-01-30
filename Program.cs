using System.Globalization;
using System.Linq;

namespace BillingEngine;
class Program
{
    #region arrangeDatesByItsMonth
    public static void arrangeDatesByItsMonth(List<AWSResourceUsage> resourceUsages)
    {
        for (int i = 0; i < resourceUsages.Count(); i++)
        {
            AWSResourceUsage usage = resourceUsages[i];
            var differenceOfDates = usage.UsedUntil - usage.UsedFrom;
            var lastDayOfMonth = new DateTime(usage.UsedFrom.Year, usage.UsedFrom.Month, DateTime.DaysInMonth(usage.UsedFrom.Year, usage.UsedFrom.Month), 23, 59, 59);
            var remainingDaysInCurrentMonth = lastDayOfMonth - usage.UsedFrom;
            int curMonth = usage.UsedFrom.Month;
            int curYear = usage.UsedFrom.Year;
            var totalDifferenceInSeconds = differenceOfDates.TotalSeconds-1;
            if (remainingDaysInCurrentMonth.TotalSeconds < differenceOfDates.TotalSeconds)
            {
                var minimumSeconds = Math.Min(remainingDaysInCurrentMonth.TotalSeconds, totalDifferenceInSeconds);
                usage.UsedUntil = usage.UsedFrom.AddSeconds(minimumSeconds);
                totalDifferenceInSeconds -= minimumSeconds;
                curMonth = curMonth == 12 ? 1 : curMonth + 1;
                while (totalDifferenceInSeconds > 0)
                {
                    DateTime from = new DateTime(curYear, curMonth, 1);
                    DateTime tillLastDate = new DateTime(curYear, curMonth, DateTime.DaysInMonth(curYear,curMonth),23,59,59);
                    var totalDaysInCurMonth = (tillLastDate-from).TotalSeconds;
                    minimumSeconds = Math.Min(totalDaysInCurMonth, totalDifferenceInSeconds);
                    totalDifferenceInSeconds -= minimumSeconds+1;
                    DateTime till = from.AddSeconds(minimumSeconds);
                    AWSResourceUsage tempObj = new AWSResourceUsage(usage.CustomerID, usage.EC2InstanceID, usage.EC2InstanceType, from, till);
                    resourceUsages.Add(tempObj);
                    curMonth = curMonth == 12 ? 1 : curMonth + 1;
                }
            }
        }
    }

    #endregion

    #region calculate cost

    public static void calculateCost(List<AWSResourceTypes> awsResourceTypes, List<AWSResourceUsage> awsResourceUsage, List<Customer> customers)
    {
        arrangeDatesByItsMonth(awsResourceUsage);

        Console.WriteLine(string.Format(" ", awsResourceUsage));

        var grouped = awsResourceUsage.OrderBy(resource => resource.CustomerID)
                                      .ThenBy(resource => resource.UsedFrom)
                                      .GroupBy(resource => new { resource.CustomerID, resource.EC2InstanceType, resource.UsedFrom.Year, resource.UsedFrom.Month })
                                      .Select(resource => new { key = resource.Key, list = resource.Select(resource => resource).ToList() });

        foreach (var item in grouped)
        {

            double amount = 0;
            double cost = Convert.ToDouble(awsResourceTypes.Where(type => type.InstanceType.Equals(item.key.EC2InstanceType))
                                 .Select(type => type.Charge).ToList()[0].ToString().Substring(1));

            foreach (var item1 in item.list)
            {
                var diff = item1.UsedUntil - item1.UsedFrom;
                item1.totalCost += Convert.ToDouble(Math.Ceiling(diff.TotalHours) * cost);
            }
        }

        var customerWise = awsResourceUsage.GroupBy(resource => new { resource.CustomerID, resource.UsedFrom.Month, resource.UsedFrom.Year })
                                           .Select(resource => new { Key = resource.Key, list = resource.Select(resource => resource).ToList() });

        foreach (var item in customerWise)
        {
            var customerName = customers.Where(customer => customer.CustomerID.Replace("-", "") == item.Key.CustomerID)
                                        .Select(Customer => Customer.CustomerName).ToList()[0];
            var month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Key.Month);
            double totalAmount = item.list.Sum((a) => a.totalCost);
            double totalDiff = item.list.Sum((a) => (a.UsedUntil - a.UsedFrom).TotalHours);
            totalAmount = Math.Round(totalAmount, 4);
            Console.WriteLine(customerName);
            Console.WriteLine("Bill for month of " + month + " " + item.Key.Year);
            Console.WriteLine("Total Cost = " + totalAmount);
            Console.WriteLine();
        }
    }
    #endregion

    #region Take Input

    public static void takeInput(ref List<Customer> customers, ref List<AWSResourceUsage> awsResourceUsage, ref List<AWSResourceTypes> awsResourceTypes, string pathOfCustomer, string pathOfAWSResourceTypes, string pathOfAWSResourceUsage)
    {
        GenericList<Customer> customerObj = ReadCSV<Customer>.LoadDataFromCsv(pathOfCustomer);
        customers = customerObj.DataList;

        GenericList<AWSResourceTypes> awsResourceTypesObj = ReadCSV<AWSResourceTypes>.LoadDataFromCsv(pathOfAWSResourceTypes);
        awsResourceTypes = awsResourceTypesObj.DataList;

        GenericList<AWSResourceUsage> awsResourceUsageObj = ReadCSV<AWSResourceUsage>.LoadDataFromCsv(pathOfAWSResourceUsage);
        awsResourceUsage = awsResourceUsageObj.DataList;

        Console.WriteLine();
    }

    #endregion

    public static void Main(string[] args)
    {


        for (int i = 1; i < 5; i++)
        {
            string pathOfAWSResourceUsage = "C:/Users/YashDaxini/Downloads/TestCases/TestCases/Case" + i + "/Input/AWSCustomerUsage.csv";
            string pathOfAWSResourceTypes = "C:/Users/YashDaxini/Downloads/TestCases/TestCases/Case" + i + "/Input/AWSResourceTypes.csv";
            string pathOfCustomer = "C:/Users/YashDaxini/Downloads/TestCases/TestCases/Case" + i + "/Input/Customer.csv";

            List<Customer> customers = new List<Customer>();
            List<AWSResourceUsage> awsResourceUsage = new List<AWSResourceUsage>();
            List<AWSResourceTypes> awsResourceTypes = new List<AWSResourceTypes>();

            takeInput(ref customers, ref awsResourceUsage, ref awsResourceTypes, pathOfCustomer, pathOfAWSResourceTypes, pathOfAWSResourceUsage);

            calculateCost(awsResourceTypes, awsResourceUsage, customers);

        }
    }
}