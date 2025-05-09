using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Philosophers_CristianSalaF.PhilosophersDinner
{
    public class Philosopher : IDisposable
    {
        private readonly int id;
        private readonly object leftChopstick;
        private readonly object rightChopstick;
        private readonly Random random;
        private readonly ILogger logger;
        private readonly PhilosopherStatistics statistics;
        private long lastMealTime;
        private Thread thread;
        private volatile bool running = true;
        private object statisticsLock = new object();

        public Philosopher(int id, object leftChopstick, object rightChopstick, ILogger logger)
        {
            this.id = id;
            this.leftChopstick = leftChopstick;
            this.rightChopstick = rightChopstick;
            this.random = new Random(id + Environment.TickCount);
            this.logger = logger;
            this.statistics = new PhilosopherStatistics
            {
                PhilosopherNumber = id,
                MaxStarvationTime = 0,
                MealsCount = 0
            };
            this.lastMealTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            thread = new Thread(DoLifecycle);
            thread.Name = $"Philosopher-{id}";
        }

        public void Start()
        {
            thread.Start();
        }

        public void Stop()
        {
            running = false;
            // No need to join here, join in Dispose
        }

        public PhilosopherStatistics Statistics 
        {
            get
            {
                lock (statisticsLock)
                {
                    // Return a copy to avoid thread safety issues
                    return new PhilosopherStatistics
                    {
                        PhilosopherNumber = statistics.PhilosopherNumber,
                        MaxStarvationTime = statistics.MaxStarvationTime,
                        MealsCount = statistics.MealsCount
                    };
                }
            }
        }

        public long LastMealTime
        {
            get
            {
                lock (statisticsLock)
                {
                    return lastMealTime;
                }
            }
        }

        private void DoLifecycle()
        {
            try
            {
                while (running)
                {
                    Think();
                    if (!running) break;
                    TryEat();
                }
            }
            catch (ThreadInterruptedException)
            {
                // Expected during shutdown
                logger.Log(id, 0, "stopped due to interruption");
            }
        }

        private void Think()
        {
            logger.Log(id, 0, "is thinking");
            Thread.Sleep(random.Next(500, 2000));
        }

        private void TryEat()
        {
            // Even philosophers pick left first, odd pick right first
            if (id % 2 == 0)
            {
                PickupChopsticksInOrder(leftChopstick, rightChopstick);
            }
            else
            {
                PickupChopsticksInOrder(rightChopstick, leftChopstick);
            }

            if (!running) return; // Check if we should exit before eating

            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            long starvationTime = currentTime - lastMealTime;
            
            lock (statisticsLock)
            {
                statistics.MaxStarvationTime = Math.Max(statistics.MaxStarvationTime, starvationTime / 1000.0);
                lastMealTime = currentTime;
                statistics.MealsCount++;
            }

            logger.Log(id, 3, "is eating");
            Thread.Sleep(random.Next(500, 1000));

            // Release chopsticks in reverse order
            if (id % 2 == 0)
            {
                ReleaseChopsticksInOrder(rightChopstick, leftChopstick);
            }
            else
            {
                ReleaseChopsticksInOrder(leftChopstick, rightChopstick);
            }
        }

        private void PickupChopsticksInOrder(object first, object second)
        {
            
            logger.Log(id, 1, $"is trying to pick up first chopstick");
            lock (first)
            {
                logger.Log(id, 1, $"picked up first chopstick");

                logger.Log(id, 2, $"is trying to pick up second chopstick");
                lock (second)
                {
                    logger.Log(id, 2, $"picked up second chopstick");
                }
            }
        }

        private void ReleaseChopsticksInOrder(object first, object second)
        {
            logger.Log(id, 4, $"has released both chopsticks");
        }

        public void Dispose()
        {
            Stop();
            if (thread.IsAlive)
            {
                thread.Join(1000); // Wait up to 1 second for thread to finish
                if (thread.IsAlive)
                {
                    thread.Interrupt(); // Force interrupt if still running
                    thread.Join(1000);  // Give it another second to finish
                }
            }
        }
    }
}
