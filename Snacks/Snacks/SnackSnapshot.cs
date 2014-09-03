using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    class SnackSnapshot
    {

        public event EventHandler SnackSnapShotChanged;

        protected virtual void OnSnackSnapShotChanged(EventArgs e)
        {
            EventHandler handler = SnackSnapShotChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        private SnackSnapshot()
        {}

        private static SnackSnapshot snapshot;

        public static SnackSnapshot Instance()
        {
            if (snapshot == null)
            {
                snapshot = new SnackSnapshot();
                snapshot.Vessels();
            }
            return snapshot;
        }

        private Dictionary<int, List<ShipSupply>> vessels;
        private Dictionary<Guid, bool> outOfSnacks;

        public Dictionary<int, List<ShipSupply>> Vessels()
        {
            try
            {
                if (vessels == null)
                {
                    Debug.Log("rebuilding snapshot");
                    int snackResourceId = SnackConfiguration.Instance().SnackResourceId;
                    vessels = new Dictionary<int, List<ShipSupply>>();
                    outOfSnacks = new Dictionary<Guid, bool>();

                    List<Guid> activeVessels = new List<Guid>();

                    foreach (Vessel v in FlightGlobals.Vessels)
                    {
                        //Debug.Log("processing v:" + v.vesselName);
                        if (v.GetVesselCrew().Count > 0 && v.loaded)
                        {
                            activeVessels.Add(v.id);
                            List<PartResource> resources = new List<PartResource>();
                            v.rootPart.GetConnectedResources(snackResourceId, ResourceFlowMode.ALL_VESSEL, resources);
                            double snackAmount = 0;
                            double snackMax = 0;
                            foreach (PartResource r in resources)
                            {
                                snackAmount += r.amount;
                                snackMax += r.maxAmount;
                            }
 
                            ShipSupply supply = new ShipSupply();
                            supply.VesselName = v.vesselName;
                            supply.BodyName = v.mainBody.name;
                            supply.SnackAmount = Convert.ToInt32(snackAmount);
                            supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                            supply.CrewCount = v.GetVesselCrew().Count;
                            supply.DayEstimate = Convert.ToInt32(snackAmount / supply.CrewCount / (SnackConfiguration.Instance().MealsPerDay * SnackConfiguration.Instance().SnacksPerMeal));
                            supply.Percent = snackMax == 0 ? 0 : Convert.ToInt32(snackAmount / snackMax * 100);
                            AddShipSupply(supply, v.protoVessel.orbitSnapShot.ReferenceBodyIndex);
                            outOfSnacks.Add(v.id, snackAmount != 0.0 ? false : true);
                        }
                    }

                    foreach (var pv in HighLogic.CurrentGame.flightState.protoVessels)
                    {
                        //Debug.Log("processing pv:" + pv.vesselName);
                        if (!pv.vesselRef.loaded && !activeVessels.Contains(pv.vesselID))
                        {
                            if (pv.GetVesselCrew().Count < 1)
                                continue;
                            double snackAmount = 0;
                            double snackMax = 0;
                            foreach (ProtoPartSnapshot pps in pv.protoPartSnapshots)
                            {
                                var res = from r in pps.resources
                                          where r.resourceName == "Snacks"
                                          select r;
                                if (res.Count() > 0)
                                {
                                    ConfigNode node = res.First().resourceValues;
                                    snackAmount += Double.Parse(node.GetValue("amount"));
                                    snackMax += Double.Parse(node.GetValue("maxAmount"));

                                }
                            }
                            //Debug.Log(pv.vesselName + "1");
                            ShipSupply supply = new ShipSupply();
                            supply.VesselName = pv.vesselName;
                            supply.BodyName = pv.vesselRef.mainBody.name;
                            supply.SnackAmount = Convert.ToInt32(snackAmount);
                            supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                            supply.CrewCount = pv.GetVesselCrew().Count;
                            //Debug.Log(pv.vesselName + supply.CrewCount);
                            supply.DayEstimate = Convert.ToInt32(snackAmount / supply.CrewCount / (SnackConfiguration.Instance().MealsPerDay * SnackConfiguration.Instance().SnacksPerMeal));
                            //Debug.Log(pv.vesselName + supply.DayEstimate);
                            //Debug.Log("sa:" + snackAmount + " sm:" + snackMax);
                            supply.Percent = snackMax == 0 ? 0 : Convert.ToInt32(snackAmount / snackMax * 100);
                            //Debug.Log(pv.vesselName + supply.Percent);
                            AddShipSupply(supply, pv.orbitSnapShot.ReferenceBodyIndex);
                            outOfSnacks.Add(pv.vesselID, snackAmount != 0.0 ? false : true);

                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("building snapshot failed: " + ex.Message + ex.StackTrace);
            }
            return vessels;
        }

        private void AddShipSupply(ShipSupply supply, int planet)
        {
            if (!vessels.ContainsKey(planet))
                vessels.Add(planet, new List<ShipSupply>());

            List<ShipSupply> ships;
            bool suc = vessels.TryGetValue(planet, out ships);
            ships.Add(supply);
        }

        public void SetRebuildSnapshot()
        {
            //Debug.Log("reset snapshot");
            vessels = null;
            outOfSnacks = null;
            OnSnackSnapShotChanged(EventArgs.Empty);
        }

        public bool IsShipOutOfSnacks(Guid id)
        {
            if (outOfSnacks == null)
                Vessels();
            bool value = false;
            outOfSnacks.TryGetValue(id, out value);
            return value;
        }

    }
}
