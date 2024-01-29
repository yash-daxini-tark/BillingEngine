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
        public static GenericList<T> LoadDataFromCsv(string filePath)
        {
            GenericList<T> csvData = new GenericList<T>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csvData.DataList = csv.GetRecords<T>().ToList();
            }

            return csvData;
        }
    }
}
