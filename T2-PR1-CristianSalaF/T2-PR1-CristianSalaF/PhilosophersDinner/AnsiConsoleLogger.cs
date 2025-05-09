using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Philosophers_CristianSalaF.PhilosophersDinner
{
    public class AnsiConsoleLogger : ILogger
    {
        private readonly long startTimeMs;

        private static string[] TextColors = new string[]
        {
            "\u001b[38;2;255;255;255m", // Bright White (for contrast)
            "\u001b[38;2;255;255;0m",   // Bright Yellow
            "\u001b[38;2;255;0;255m",   // Bright Magenta
            "\u001b[38;2;0;255;255m",   // Bright Cyan
            "\u001b[38;2;255;128;0m"    // Bright Orange
        };

        private static string[] BackgroundColors = new string[]
        {
            "\u001b[48;2;0;0;128m",     // Navy Blue (Thinking)
            "\u001b[48;2;128;0;0m",     // Maroon (Waiting for left chopstick)
            "\u001b[48;2;0;128;0m",     // Dark Green (Waiting for right chopstick)
            "\u001b[48;2;128;0;128m",   // Purple (Eating)
            "\u001b[48;2;128;128;0m"    // Olive (Releasing chopsticks)
        };

        private const string Reset = "\u001b[0m";

        public AnsiConsoleLogger()
        {
            startTimeMs = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Log(int philosopherId, int stateIndex, string message)
        {
            string textColor = TextColors[philosopherId % TextColors.Length];
            string bgColor = BackgroundColors[stateIndex % BackgroundColors.Length];
            
            long elapsedMs = DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTimeMs;
            
            Console.WriteLine($"{bgColor}{textColor}[{elapsedMs/1000.0:F3}s] Philosopher {philosopherId} {message}{Reset}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"\u001b[41m\u001b[37m{message}{Reset}");
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"\u001b[0m{message}");
        }
    }
}
