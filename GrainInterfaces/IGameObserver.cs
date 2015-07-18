using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{
    public interface IGameObserver : IGrainObserver
    {
        // void UpdateGameScore(Dictionary<long, Progression> status);
    }
}
