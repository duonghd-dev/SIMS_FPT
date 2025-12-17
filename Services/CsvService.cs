using CsvHelper;
using CsvHelper.Configuration;
using SIMS_FPT.Services.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SIMS_FPT.Services
{
    public class CsvService : ICsvService
    {
        public List<T> ReadCsv<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<T>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            return csv.GetRecords<T>().ToList();
        }
    }
}
