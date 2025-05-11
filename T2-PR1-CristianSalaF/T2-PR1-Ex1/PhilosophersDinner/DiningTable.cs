using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Ex1.PhilosophersDinner
{
    public class DiningTable : IDisposable
    {
        private const int NUM_PHILOSOPHERS = 5;
        private const int SIMULATION_TIME_MS = 30000; // 30 seconds
        private const int MAX_STARVATION_TIME_MS = 15000; // 15 seconds

        private readonly object[] chopsticks;
        private readonly Philosopher[] philosophers;
        private readonly ILogger logger;
        private readonly Thread starvationCheckerThread;
        private volatile bool running = true;
        private readonly StatisticsWriter statisticsWriter;
        private readonly ManualResetEvent simulationComplete = new ManualResetEvent(false);

        public DiningTable(ILogger logger)
        {
            this.logger = logger;
            this.statisticsWriter = new StatisticsWriter();

            chopsticks = new object[NUM_PHILOSOPHERS];
            for (int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                chopsticks[i] = new object();
            }

            philosophers = new Philosopher[NUM_PHILOSOPHERS];
            for (int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                int leftChopstickIndex = i;
                int rightChopstickIndex = (i + 1) % NUM_PHILOSOPHERS;
                philosophers[i] = new Philosopher(i, chopsticks[leftChopstickIndex], chopsticks[rightChopstickIndex], logger);
            }
            
            starvationCheckerThread = new Thread(CheckForStarvation);
            starvationCheckerThread.Name = "StarvationChecker";
            starvationCheckerThread.IsBackground = true;
        }

        public void StartSimulation()
        {
            logger.LogInfo("Starting Dining Philosophers Simulation...");
            
            starvationCheckerThread.Start();
            
            foreach (var philosopher in philosophers)
            {
                philosopher.Start();
            }
            
            Thread timeoutThread = new Thread(() => 
            {
                Thread.Sleep(SIMULATION_TIME_MS);
                StopSimulation();
            });
            timeoutThread.Name = "TimeoutThread";
            timeoutThread.IsBackground = true;
            timeoutThread.Start();
            
            simulationComplete.WaitOne();
            
            PrintStatistics();
            
            SaveStatisticsToCsv();

            logger.LogInfo("Simulation completed.");
        }

        private void StopSimulation()
        {
            if (!running) return; // Prevent multiple stops
            
            running = false;
            
            foreach (var philosopher in philosophers)
            {
                philosopher.Stop();
            }
            
            simulationComplete.Set();
        }

        private void CheckForStarvation()
        {
            try
            {
                while (running)
                {
                    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    
                    for (int i = 0; i < NUM_PHILOSOPHERS; i++)
                    {
                        long timeSinceLastMeal = currentTime - philosophers[i].LastMealTime;
                        
                        if (timeSinceLastMeal > MAX_STARVATION_TIME_MS)
                        {
                            logger.LogError($"\nSTARVATION DETECTED! Philosopher {i} has been starving for {timeSinceLastMeal/1000.0:F1} seconds!");
                            StopSimulation();
                            return;
                        }
                    }
                    
                    Thread.Sleep(100); // Check every 100ms
                }
            }
            catch (ThreadInterruptedException)
            {
                logger.LogInfo("Starvation checker interrupted.");
            }
        }

        private void PrintStatistics()
        {
            logger.LogInfo("\n--- FINAL STATISTICS ---");
            logger.LogInfo("Philosopher\tMax Starvation Time (s)\tMeals Count");
            
            for (int i = 0; i < NUM_PHILOSOPHERS; i++)
            {
                var stats = philosophers[i].Statistics;
                logger.LogInfo($"{i}\t\t{stats.MaxStarvationTime:F3}\t\t\t{stats.MealsCount}");
            }
        }

        private void SaveStatisticsToCsv()
        {
            List<PhilosopherStatistics> statistics = philosophers.Select(p => p.Statistics).ToList();
            statisticsWriter.SaveStatistics(statistics);
            logger.LogInfo("\nStatistics saved to philosopher_statistics.csv");
        }

        public void Dispose()
        {
            StopSimulation();
            
            foreach (var philosopher in philosophers)
            {
                philosopher.Dispose();
            }
            
            if (starvationCheckerThread.IsAlive)
            {
                starvationCheckerThread.Interrupt();
                starvationCheckerThread.Join(1000);
            }
            
            simulationComplete.Dispose();
        }
    }
}
