using CsvHelper.Configuration.Attributes;
using T2_PR1_Ex1.PhilosophersDinner;

namespace T2_PR1_Ex1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string txtExit = "Press any key to exit...";
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Ensure ANSI codes work properly
            
            ILogger logger = new AnsiConsoleLogger();
            
            using (DiningTable diningTable = new DiningTable(logger))
            {
                diningTable.StartSimulation();
                
                Console.WriteLine(txtExit);
                Console.ReadKey();
            }
        }
    }
}
