using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Ex2.AsteroidsGame
{
    public class UIController
    {
        private const int RenderTickRate = 20; // Hz for rendering
        private const char PlayerChar = '^';
        private const char AsteroidChar = '*';
        private const char BackgroundChar = ' ';
        
        private readonly string playerColorStatic = "\u001b[38;2;0;255;255m"; // Cyan
        private readonly string playerColorLeft = "\u001b[38;2;0;255;0m"; // Green
        private readonly string playerColorRight = "\u001b[38;2;255;255;0m"; // Yellow
        private readonly string asteroidColor = "\u001b[38;2;255;255;0m"; // Yellow
        private readonly string borderColor = "\u001b[38;2;255;0;0m"; // Red
        private readonly string scoreColor = "\u001b[38;2;0;255;0m"; // Green
        private readonly string resetColor = "\u001b[0m"; // Reset
        
        private GameController gameController;
        
        public void Initialize(GameController controller)
        {
            gameController = controller;
            Console.Clear();
            DrawBorder();
        }
        
        public void RenderGame()
        {
            DrawBorder();
            
            while (gameController.IsGameRunning)
            {
                try
                {
                    lock (gameController.LockObject)
                    {
                        ClearGameArea();
                        
                        Console.SetCursorPosition(gameController.PlayerX, Console.WindowHeight - 2);
                        string currentPlayerColor = gameController.PlayerDirection switch
                        {
                            GameController.MoveDirection.Left => playerColorLeft,
                            GameController.MoveDirection.Right => playerColorRight,
                            _ => playerColorStatic
                        };
                        Console.Write($"{currentPlayerColor}{PlayerChar}{resetColor}");
                        
                        foreach (var asteroid in gameController.Asteroids)
                        {
                            if (asteroid.Y < Console.WindowHeight - 1 && asteroid.Y > 0)
                            {
                                Console.SetCursorPosition(asteroid.X, asteroid.Y);
                                Console.Write($"{asteroidColor}{AsteroidChar}{resetColor}");
                            }
                        }
                        
                        UpdateUI();
                    }
                }
                catch (Exception ex)
                {
                    Console.Clear();
                    DrawBorder();
                }
                
                Thread.Sleep(1000 / RenderTickRate);
            }
        }

        public void DrawBorder()
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;
            
            Console.SetCursorPosition(0, 0);
            Console.Write($"{borderColor}{"".PadRight(width, '=')}{resetColor}");
            
            Console.SetCursorPosition(0, height - 1);
            Console.Write($"{borderColor}{"".PadRight(width, '=')}{resetColor}");
            
            for (int i = 1; i < height - 1; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write($"{borderColor}|{resetColor}");
                
                Console.SetCursorPosition(width - 1, i);
                Console.Write($"{borderColor}|{resetColor}");
            }
        }

        private void ClearGameArea()
        {
            Console.SetCursorPosition(gameController.PreviousPlayerX, Console.WindowHeight - 2);
            Console.Write(BackgroundChar);
            
            foreach (var asteroid in gameController.PreviousAsteroids)
            {
                if (asteroid.Y < Console.WindowHeight - 1 && asteroid.Y > 0)
                {
                    Console.SetCursorPosition(asteroid.X, asteroid.Y);
                    Console.Write(BackgroundChar);
                }
            }
            
            // Update tracking variables in game controller for the next frame
            gameController.PreviousPlayerX = gameController.PlayerX;
            
            // Copy current asteroids to previous asteroids list for next frame
            gameController.PreviousAsteroids.Clear();
            foreach (var asteroid in gameController.Asteroids)
            {
                gameController.PreviousAsteroids.Add(new Asteroid { X = asteroid.X, Y = asteroid.Y });
            }
        }

        private void UpdateUI()
        {
            string scoreText = $"Score: {gameController.Score} | Lives: {gameController.Lives} | Press Q to quit";
            Console.SetCursorPosition(2, 0);
            Console.Write($"{scoreColor}{scoreText}{resetColor}");
        }

        public bool ShowFinalScoreWithRestart(int score, TimeSpan gameTime, int livesUsed, bool isWebEvaluationRunning)
        {
            Console.Clear();
    
            // Play game over sound in a separate task to avoid blocking
            Task.Run(() => {
                try {
                    for (int i = 0; i < 3; i++)
                    {
                        Console.Beep(660, 100);
                        Thread.Sleep(50);
                    }
                    Console.Beep(440, 300);
                } catch { Console.WriteLine("GameOver Beep!"); }
            });
    
            Console.WriteLine("\n\n");
            Console.WriteLine($"{scoreColor}===== GAME OVER ====={resetColor}");
            Console.WriteLine($"Total score: {score} asteroids avoided");
            Console.WriteLine($"Time played: {gameTime.Minutes:00}:{gameTime.Seconds:00}");
            Console.WriteLine($"Lives used: {livesUsed}");
            Console.WriteLine("\nGame data saved to game_history.csv");
    
            // Only offer restart option if web evaluation is still running
            if (isWebEvaluationRunning)
            {
                Console.WriteLine($"\n{scoreColor}Press R to restart the game or any other key to exit...{resetColor}");
                var key = Console.ReadKey(true).Key;
                Console.Clear();
                return key == ConsoleKey.R;
            }
            else
            {
                Console.WriteLine($"\n{scoreColor}Evaluation complete. Press any key to exit...{resetColor}");
                Console.ReadKey(true);
                Console.Clear();
                return false;
            }
        }
    }
}
