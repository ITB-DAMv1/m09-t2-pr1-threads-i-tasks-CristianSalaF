using CsvHelper;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Philosophers_CristianSalaF.PhilosophersDinner
{
    public class StatisticsWriter
    {
        private readonly string filePath;

        public StatisticsWriter(string filePath = "philosopher_statistics.csv")
        {
            this.filePath = filePath;
        }

        public void SaveStatistics(List<PhilosopherStatistics> statistics)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(statistics);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving CSV: {ex.Message}");
            }
        }
    }
}
