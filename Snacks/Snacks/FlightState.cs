using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    class FlightState
    {

        public FlightState(FlightCtrlState state, double time)
        {
            State = state;
            Time = time;
        }

        public FlightCtrlState State { get; set; }
        public double Time { get; set; }
    }
}
