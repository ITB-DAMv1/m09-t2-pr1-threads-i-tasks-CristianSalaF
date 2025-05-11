using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Ex2.AsteroidsGame
{
    public class StatsManager
    {
        public StatsManager()
        {
            if (!File.Exists("game_history.csv"))
            {
                using (StreamWriter sw = File.CreateText("game_history.csv"))
                {
                    sw.WriteLine("Date,Score,TimePlayed,LivesUsed");
                }
            }
        }
        
        public void SaveGameData(int score, TimeSpan gameTime, int livesUsed)
        {
            try
            {
                // Format: Date,Score,TimePlayed,LivesUsed
                string timePlayed = $"{gameTime.Minutes:00}:{gameTime.Seconds:00}";
                string gameData = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{score},{timePlayed},{livesUsed}";
                
                using (StreamWriter sw = File.AppendText("game_history.csv"))
                {
                    sw.WriteLine(gameData);
                }
            }
            catch (Exception ex)
            {
                Console.SetCursorPosition(0, Console.WindowHeight - 3);
                Console.WriteLine($"Error saving game data: {ex.Message}");
            }
        }
    }
}
