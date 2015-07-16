using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace PlayerProgression
{
    public class ProfileState : GrainState
    {
        int Kills { get; set; }
        int Death { get; set; }
        int Experience { get; set; }
    }

    /*
    class PlayerProfile : IPlayerProfile
    {

    }
    */
}
