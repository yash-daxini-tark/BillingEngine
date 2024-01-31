using System.Globalization;
using System.Linq;
using System.Text;

namespace BillingEngine;
class Program
{
    #region arrangeDatesByItsMonth
    public static List<AWSResourceUsage> arrangeDatesByItsMonth(List<AWSResourceUsage> resourceUsages)
    {
        List<AWSResourceUsage> updatedResources = new List<AWSResourceUsage>();
        for (int i = 0; i < resourceUsages.Count(); i++)
        {
            AWSResourceUsage usage = resourceUsages[i];
            var differenceOfDates = usage.UsedUntil - usage.UsedFrom;
            var lastDayOfMonth = new DateTime(usage.UsedFrom.Year, usage.UsedFrom.Month, DateTime.DaysInMonth(usage.UsedFrom.Year, usage.UsedFrom.Month), 23, 59, 59);
            var remainingDaysInCurrentMonth = lastDayOfMonth - usage.UsedFrom;
            int curMonth = usage.UsedFrom.Month;
            int curYear = usage.UsedFrom.Year;
            var totalDifferenceInSeconds = differenceOfDates.TotalSeconds - 1;
            if (remainingDaysInCurrentMonth.TotalSeconds <= differenceOfDates.TotalSeconds)
            {
                var minimumSeconds = Math.Min(remainingDaysInCurrentMonth.TotalSeconds, totalDifferenceInSeconds);
                if (lastDayOfMonth == usage.UsedFrom)
                {
                    usage.UsedUntil = lastDayOfMonth.Add(new TimeSpan(0, 0, 0, 1));
                }
                else usage.UsedUntil = usage.UsedFrom.AddSeconds(minimumSeconds);
                updatedResources.Add(new AWSResourceUsage(usage));
                totalDifferenceInSeconds -= minimumSeconds;
                if (curMonth == 12)
                {
                    curMonth = 1;
                    curYear++;
                }
                else curMonth++;
                while (totalDifferenceInSeconds > 0)
                {
                    DateTime from = new DateTime(curYear, curMonth, 1);
                    DateTime tillLastDate = new DateTime(curYear, curMonth, DateTime.DaysInMonth(curYear, curMonth), 23, 59, 59);
                    var totalDaysInCurMonth = (tillLastDate - from).TotalSeconds;
                    minimumSeconds = Math.Min(totalDaysInCurMonth, totalDifferenceInSeconds);
                    totalDifferenceInSeconds -= minimumSeconds + 1;
                    DateTime till = from.AddSeconds(minimumSeconds);
                    AWSResourceUsage tempObj = new AWSResourceUsage(usage.CustomerID, usage.EC2InstanceID, usage.EC2InstanceType, from, till);
                    updatedResources.Add(tempObj);
                    if (curMonth == 12)
                    {
                        curMonth = 1;
                        curYear++;
                    }
                    else curMonth++;
                }
            }
            else
            {
                updatedResources.Add(new AWSResourceUsage(usage));
            }
        }
        return updatedResources;
    }

    #endregion

    #region calculate cost

    public static void calculateCost(List<AWSResourceTypes> awsResourceTypes, List<AWSResourceUsage> awsResourceUsage, List<Customer> customers, int testcase)
    {
        List<AWSResourceUsage> updatedResources = arrangeDatesByItsMonth(awsResourceUsage);

        var grouped = updatedResources.OrderBy(resource => resource.CustomerID)
                                      .ThenBy(resource => resource.UsedFrom)
                                      .GroupBy(resource => new { resource.CustomerID, resource.EC2InstanceType, resource.UsedFrom.Year, resource.UsedFrom.Month })
                                      .Select(resource => new { key = resource.Key, list = resource.Select(resource => resource).ToList() });

        foreach (var item in grouped)
        {
            double cost = Convert.ToDouble(awsResourceTypes.Where(type => type.InstanceType.Equals(item.key.EC2InstanceType))
                                 .Select(type => type.Charge).ToList()[0].ToString().Substring(1));

            foreach (var item1 in item.list)
            {
                var diff = item1.UsedUntil - item1.UsedFrom;
                item1.totalCost += Convert.ToDouble(Math.Ceiling(diff.TotalHours) * cost);
            }
        }

        var customerWise = updatedResources.GroupBy(resource => new { resource.CustomerID, resource.UsedFrom.Month, resource.UsedFrom.Year })
                                           .Select(resource => new { Key = resource.Key, list = resource.Select(resource => resource).ToList() });

        foreach (var item in customerWise)
        {
            var customerName = customers.Where(customer => customer.CustomerID.Replace("-", "") == item.Key.CustomerID)
                                        .Select(Customer => Customer.CustomerName).ToList()[0];
            var month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Key.Month);
            string fileName = item.Key.CustomerID.Substring(0, 4) + "-" + item.Key.CustomerID.Substring(4) + "_" + month.Substring(0, 3).ToUpper() + "-" + item.Key.Year;
            double totalAmount = item.list.Sum((a) => a.totalCost);
            double totalDiff = item.list.Sum((a) => (a.UsedUntil - a.UsedFrom).TotalHours);
            StringBuilder str = new StringBuilder();
            totalAmount = Math.Round(totalAmount, 4);
            str.AppendLine(customerName);
            str.AppendLine("Bill for month of " + month + " " + item.Key.Year);
            str.AppendLine("Total Amount: " + totalAmount);
            str.AppendLine("Resource Type,Total Resources,Total Used Time (HH:mm:ss),Total Billed Time (HH:mm:ss),Rate (per hour),Total Amount");
            var grp = awsResourceUsage.GroupBy(res => new { res.EC2InstanceType, res.EC2InstanceID, res.CustomerID, res.UsedFrom.Month, res.UsedFrom.Year });
            Dictionary<string, TimeSpan> usedTimeForeachInstanceType = new Dictionary<string, TimeSpan>();
            Dictionary<string, int> countOfEachInstanceType = new Dictionary<string, int>();
            Dictionary<string, double> totalCost = new Dictionary<string, double>();
            HashSet<string> ec2InstanceIDs = new HashSet<string>();
            foreach (var item2 in item.list)
            {
                TimeSpan t = (item2.UsedUntil - item2.UsedFrom);
                if (usedTimeForeachInstanceType.ContainsKey(item2.EC2InstanceType))
                {
                    totalCost[item2.EC2InstanceType] += item2.totalCost;
                    usedTimeForeachInstanceType[item2.EC2InstanceType] += t;
                    if (ec2InstanceIDs.Contains(item2.EC2InstanceID)) continue;
                    countOfEachInstanceType[item2.EC2InstanceType]++;
                    ec2InstanceIDs.Add(item2.EC2InstanceID);
                }
                else
                {
                    totalCost.Add(item2.EC2InstanceType, item2.totalCost);
                    usedTimeForeachInstanceType.Add(item2.EC2InstanceType, t);
                    if (ec2InstanceIDs.Contains(item2.EC2InstanceID)) continue;
                    countOfEachInstanceType.Add(item2.EC2InstanceType, 1);
                    ec2InstanceIDs.Add(item2.EC2InstanceID);
                }
            }
            double finalAmountForCurrentFile = 0;
            foreach (var key in usedTimeForeachInstanceType.Keys)
            {
                var cost = awsResourceTypes.Where(type => type.InstanceType.Equals(key)).Select(type => type.Charge).FirstOrDefault();
                TimeSpan totalTimeForCurrentInstanceType = usedTimeForeachInstanceType.ContainsKey(key) ? usedTimeForeachInstanceType[key] : new TimeSpan();
                if (totalTimeForCurrentInstanceType.Minutes == 59 && totalTimeForCurrentInstanceType.Seconds == 59)
                {
                    str.AppendLine(key + "," + countOfEachInstanceType[key] + "," + Math.Floor(totalTimeForCurrentInstanceType.TotalHours + 1) + ":0:0" + "," + Math.Ceiling(totalTimeForCurrentInstanceType.TotalHours) + ":00:00" + "," + cost + "," + "$" + Math.Round(totalCost[key], 4));
                }
                else if (totalTimeForCurrentInstanceType.Seconds == 59)
                {
                    str.AppendLine(key + "," + countOfEachInstanceType[key] + "," + Math.Floor(totalTimeForCurrentInstanceType.TotalHours) + ":" + (totalTimeForCurrentInstanceType.Minutes + 1) + ":0" + "," + Math.Ceiling(totalTimeForCurrentInstanceType.TotalHours) + ":00:00" + "," + cost + "," + "$" + Math.Round(totalCost[key], 4));
                }
                else str.AppendLine(key + "," + countOfEachInstanceType[key] + "," + Math.Floor(totalTimeForCurrentInstanceType.TotalHours) + ":" + totalTimeForCurrentInstanceType.Minutes + ":" + totalTimeForCurrentInstanceType.Seconds + "," + Math.Ceiling(totalTimeForCurrentInstanceType.TotalHours) + ":00:00" + "," + cost + "," + "$" + Math.Round(totalCost[key], 4));
                finalAmountForCurrentFile = totalCost[key];
            }
            if (finalAmountForCurrentFile == 0) continue;
            writeFile(fileName, str.ToString(), testcase);
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
    }

    #endregion

    #region Write Into File

    public static void writeFile(string fileName, string content, int testcase)
    {
        string directory = "../../../Output/" + "Testcase" + testcase;

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        File.WriteAllText(directory + "/" + fileName + ".csv", content);
    }
    #endregion

    public static void Main(string[] args)
    {

        for (int i = 1; i < 5; i++)
        {
            string pathOfAWSResourceUsage = "../../../TestCases/TestCases/Case" + i + "/Input/AWSCustomerUsage.csv";
            string pathOfAWSResourceTypes = "../../../TestCases/TestCases/Case" + i + "/Input/AWSResourceTypes.csv";
            string pathOfCustomer = "../../../TestCases/TestCases/Case" + i + "/Input/Customer.csv";

            List<Customer> customers = new List<Customer>();
            List<AWSResourceUsage> awsResourceUsage = new List<AWSResourceUsage>();
            List<AWSResourceTypes> awsResourceTypes = new List<AWSResourceTypes>();

            takeInput(ref customers, ref awsResourceUsage, ref awsResourceTypes, pathOfCustomer, pathOfAWSResourceTypes, pathOfAWSResourceUsage);

            calculateCost(awsResourceTypes, awsResourceUsage, customers, i);

        }
    }
}