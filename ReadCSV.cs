using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace BillingEngine
{
    internal class ReadCSV<T>
    {
        public static List<T> LoadDataFromCsv(string filePath)
        {
            List<T> csvData = new List<T>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csvData = csv.GetRecords<T>().ToList();
            }

            return csvData;
        }
    }
}
