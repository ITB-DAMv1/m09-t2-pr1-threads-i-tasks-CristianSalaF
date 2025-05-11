using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Ex2.AsteroidsGame
{
    public class GameController
    {
        private const int GameTickRate = 50; // Hz for game logic
        private const ConsoleKey MoveLeftKey = ConsoleKey.A;
        private const ConsoleKey MoveRightKey = ConsoleKey.D;
        private const ConsoleKey QuitKey = ConsoleKey.Q;

        public int PlayerX { get; private set; }
        public int PreviousPlayerX { get; set; }
        public MoveDirection PlayerDirection { get; set; } = MoveDirection.Static;
        public readonly List<Asteroid> Asteroids = new List<Asteroid>();
        public readonly List<Asteroid> PreviousAsteroids = new List<Asteroid>();
        public bool IsGameRunning { get; private set; } = true;
        public bool IsWebEvaluationRunning { get; set; } = true;
        public int Score { get; private set; } = 0;
        public int Lives { get; private set; } = 3;
        public DateTime GameStartTime { get; private set; }
        public readonly object LockObject = new object();
        private readonly Random random = new Random();
        
        public enum MoveDirection
        {
            Left,
            Right,
            Static
        }
        
        public void ResetGame()
        {
            IsGameRunning = true;
            Lives = 3;
            Score = 0;
            Asteroids.Clear();
            PreviousAsteroids.Clear();
        }

        public void SetupGame()
        {
            PlayerX = Console.WindowWidth / 2;
            PreviousPlayerX = PlayerX;
            PlayerDirection = MoveDirection.Static;
            GameStartTime = DateTime.Now;
            Score = 0;
        }

        public void HandleUserInput()
        {
            while (IsGameRunning)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    
                    lock (LockObject)
                    {
                        // Process only the main control keys to avoid queue buildup
                        if (key == MoveLeftKey && PlayerX > 1)
                        {
                            PlayerDirection = MoveDirection.Left;
                            PlayerX--;
                            // Only beep occasionally to avoid sound queuing
                            if (DateTime.Now.Millisecond % 100 == 0)  
                            {
                                try { Console.Beep(220, 10); } catch { Console.WriteLine("Left Beep!"); }
                            }
                        }
                        else if (key == MoveRightKey && PlayerX < Console.WindowWidth - 2)
                        {
                            PlayerDirection = MoveDirection.Right;
                            PlayerX++;
                            if (DateTime.Now.Millisecond % 100 == 0)
                            {
                                try { Console.Beep(440, 10); } catch { Console.WriteLine("Right Beep!"); }
                            }
                        }
                        else if (key == QuitKey)
                        {
                            IsGameRunning = false;
                        }
                        else
                        {
                            PlayerDirection = MoveDirection.Static;
                        }
                    }
                    
                    // Clear the input buffer to prevent queuing
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }
                }
                else
                {
                    // Reset direction to static when no key is pressed
                    lock (LockObject)
                    {
                        PlayerDirection = MoveDirection.Static;
                    }
                }
                
                // Small delay to prevent CPU hogging
                Thread.Sleep(5);
            }
        }

        public void UpdateGameState()
        {
            int frameCounter = 0;
            int asteroidGenerationRate = 5; // Generate asteroid every N frames
    
            while (IsGameRunning)
            {
                // Reset direction to static at the beginning of each frame
                // This ensures it only shows movement color when actually moving
                // TODO: Make it the static color only if stopped for more than 5 frames
                PlayerDirection = MoveDirection.Static;
                lock (LockObject)
                {
                    if (frameCounter % asteroidGenerationRate == 0)
                    {
                        GenerateAsteroid();
                    }
            
                    MoveAsteroids();
            
                    CheckCollisions();
                }
        
                frameCounter++;
        
                Thread.Sleep(1000 / GameTickRate);
            }
        }

        private void GenerateAsteroid()
        {
            // Generate asteroids at random positions at the top of the screen
            int x = random.Next(1, Console.WindowWidth - 1);
            Asteroids.Add(new Asteroid { X = x, Y = 1 });
        }

        private void MoveAsteroids()
        {
            // Move each asteroid down
            for (int i = Asteroids.Count - 1; i >= 0; i--)
            {
                Asteroids[i].Y++;
                
                // Remove asteroids that go off-screen and increase score
                if (Asteroids[i].Y >= Console.WindowHeight - 1)
                {
                    Asteroids.RemoveAt(i);
                    Score++;
                }
            }
        }

        private void CheckCollisions()
        {
            bool collisionDetected = false;
            int collidingAsteroidIndex = -1;

            for (int i = Asteroids.Count - 1; i >= 0; i--)
            {
                if (Asteroids[i].X == PlayerX && Asteroids[i].Y == Console.WindowHeight - 2)
                {
                    collisionDetected = true;
                    collidingAsteroidIndex = i;
                    break; // exits when it finds a collision
                }
            }

            if (collisionDetected)
            {
                ProcessPlayerCollision();
            }
        }

        private void ProcessPlayerCollision()
        {
            // Player hit by asteroid - play collision sound in a non-blocking way
            Task.Run(() => {
                try { Console.Beep(880, 100); } 
                catch { Console.WriteLine("Beep!"); }
            });
    
            // Player hit by asteroid
            Lives--;
    
            // Clear both current asteroids and their previous positions
            lock (LockObject)
            {
                Console.Clear();
        
                Asteroids.Clear();
                PreviousAsteroids.Clear();
            }
    
            IsGameRunning = Lives > 0;
    
            if (IsGameRunning)
            {
                ResetPlayerAfterCollision();
            }
        }

        private void ResetPlayerAfterCollision()
        {
            PlayerX = Console.WindowWidth / 2;
            PreviousPlayerX = PlayerX;
            PlayerDirection = MoveDirection.Static;
    
            Thread.Sleep(200);
        }

        public async Task SimulateWebEvaluation(CancellationToken ct)
        {
            try
            {
                // Simulate web evaluation that takes between 30 seconds and 1 minute
                int evaluationTimeMs = random.Next(30000, 60001);
                await Task.Delay(evaluationTimeMs, ct);
                IsWebEvaluationRunning = false;
            }
            catch (TaskCanceledException)
            {
                IsWebEvaluationRunning = false;
                Console.WriteLine("Evaluation cancelled.");
            }
        }
    }
}
