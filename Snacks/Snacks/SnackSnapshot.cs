using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    class SnackSnapshot
    {

        private SnackSnapshot()
        {}

        private static Dictionary<string, List<ShipSupply>> vessels;

        public static Dictionary<string, List<ShipSupply>> Vessels()
        {
            try
            {
                if (vessels == null)
                {
                    Debug.Log("rebuilding snapshot");
                    int snackResourceId = SnackConfiguration.Instance().SnackResourceId;
                    vessels = new Dictionary<string, List<ShipSupply>>();

                    foreach (var pv in HighLogic.CurrentGame.flightState.protoVessels)
                    {
                        if (!pv.vesselRef.loaded)
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
                            supply.SnackAmount = Convert.ToInt32(snackAmount);
                            supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                            supply.CrewCount = pv.GetVesselCrew().Count;
                            //Debug.Log(pv.vesselName + supply.CrewCount);
                            supply.DayEstimate = Convert.ToInt32(snackAmount / supply.CrewCount / (SnackConfiguration.Instance().MealsPerDay * SnackConfiguration.Instance().SnacksPerMeal));
                            //Debug.Log(pv.vesselName + supply.DayEstimate);
                            //Debug.Log("sa:" + snackAmount + " sm:" + snackMax);
                            supply.Percent = snackMax == 0 ? 0 : Convert.ToInt32(snackAmount / snackMax * 100);
                            //Debug.Log(pv.vesselName + supply.Percent);
                            AddShipSupply(supply, pv.vesselRef.mainBody.name);

                        }
                        else
                        {
                            Vessel v = pv.vesselRef;
                            if (v.GetVesselCrew().Count < 1)
                                continue;
                            List<PartResource> resources = new List<PartResource>();
                            v.rootPart.GetConnectedResources(snackResourceId, ResourceFlowMode.ALL_VESSEL, resources);
                            double snackAmount = 0;
                            double snackMax = 0;
                            foreach (PartResource r in resources)
                            {
                                snackAmount += r.amount;
                                snackMax += r.maxAmount;
                            }
                            //Debug.Log(pv.vesselName + "3");
                            ShipSupply supply = new ShipSupply();
                            supply.VesselName = v.vesselName;
                            supply.SnackAmount = Convert.ToInt32(snackAmount);
                            supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                            supply.CrewCount = v.GetVesselCrew().Count;
                            //Debug.Log(v.vesselName + "4");
                            supply.DayEstimate = Convert.ToInt32(snackAmount / supply.CrewCount / (SnackConfiguration.Instance().MealsPerDay * SnackConfiguration.Instance().SnacksPerMeal));
                            supply.Percent = snackMax == 0 ? 0 : Convert.ToInt32(snackAmount / snackMax * 100);
                            AddShipSupply(supply, v.mainBody.name);
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

        private static void AddShipSupply(ShipSupply supply, string planet)
        {
            if (!vessels.ContainsKey(planet))
                vessels.Add(planet, new List<ShipSupply>());

            List<ShipSupply> ships;
            bool suc = vessels.TryGetValue(planet, out ships);
            ships.Add(supply);
        }

        public static void SetRebuildSnapshot()
        {
            Debug.Log("reset snapshot");
            vessels = null;
        }

    }
}
