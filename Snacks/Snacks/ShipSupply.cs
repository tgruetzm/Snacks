using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    class ShipSupply : IComparable<ShipSupply>
    {
        public string BodyName { get; set; }
        public string VesselName { get; set; }
        public int SnackAmount { get; set; }
        public int SnackMaxAmount { get; set; }
        public int DayEstimate { get; set; }
        public int CrewCount { get; set; }
        public int Percent { get; set; }


        public int CompareTo(ShipSupply obj)
        {
            if (obj == null)
                return 1;
            else
                return Percent.CompareTo(obj.Percent);
        }
    }
}
