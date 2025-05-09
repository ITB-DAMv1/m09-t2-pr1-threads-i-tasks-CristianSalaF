using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T2_PR1_Philosophers_CristianSalaF.PhilosophersDinner
{
    public interface ILogger
    {
        void Log(int philosopherId, int stateIndex, string message);
        void LogError(string message);
        void LogInfo(string message);
    }
}
