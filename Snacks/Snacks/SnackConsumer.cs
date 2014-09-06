/**
The MIT License (MIT)
Copyright (c) 2014 Troy Gruetzmacher

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 * 
 * 
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    class SnackConsumer
    {
        private System.Random random = new System.Random();
        private PartResourceDefinition snacksResource = PartResourceLibrary.Instance.GetDefinition("Snacks");
        private double snacksPer;
        private double lossPerDayPerKerbal;

        public SnackConsumer(double snacksPer, double loss)
        {
            this.snacksPer = snacksPer;
            lossPerDayPerKerbal = loss;
        }

        private bool GetRandomChance(double prob)
        {
            if (random.NextDouble() < prob)
                return true;
            return false;
        }

        public double GetSnackResource(Part p, double demand)
        {
            List<PartResource> resources = new List<PartResource>();
            p.GetConnectedResources(snacksResource.id, ResourceFlowMode.ALL_VESSEL, resources);

            double supplied = 0;
            foreach (PartResource res in resources)
            {
                if (res.amount >= demand)
                {
                    res.amount -= demand;
                    supplied += demand;
                    return supplied;
                }
                else
                {
                    supplied += res.amount;
                    demand -= res.amount;
                    res.amount = 0;
                }

            }
            return supplied;

        }

        public double GetSnackResource(List<ProtoPartSnapshot> protoPartSnapshots, double demand)
        {
            double supplied = 0;
            bool resFound = false;
            foreach (ProtoPartSnapshot pps in protoPartSnapshots)
            {
                var res = from r in pps.resources
                          where r.resourceName == "Snacks"
                          select r;
                if (res.Count() > 0)
                {
                    resFound = true;
                    ConfigNode node = res.First().resourceValues;
                    double amount = Double.Parse(node.GetValue("amount"));
                    if (amount >= demand)
                    {
                        node.SetValue("amount", (amount -= demand).ToString());
                        supplied += demand;
                        return supplied;
                    }
                    else
                    {
                        node.SetValue("amount", "0");
                        supplied += amount;
                        demand -= amount;
                    }
                }
            }
            if (!resFound)
                return demand;//if no snack resources were found, this vessel has not been loaded.  Feed them from the magic bucket.
            return supplied;
        }

        private double CalculateExtraSnacksRequired(List<ProtoCrewMember> crew)
        {

            double extra = 0;
            foreach (ProtoCrewMember pc in crew)
            {
                if (GetRandomChance(pc.courage / 2.0))
                    extra += snacksPer;
                if (GetRandomChance(pc.stupidity / 2.0))
                    extra -= snacksPer;
                if (pc.isBadass && GetRandomChance(.2))
                    extra -= snacksPer;
            }
            return extra;
        }

        /**
         * Removes the calculated number of snacks from the vessel.
         * returns the number of snacks that were required, but missing.
         * */
        public double RemoveSnacks(ProtoVessel pv)
        {
            double demand = pv.GetVesselCrew().Count * snacksPer;
            double extra = CalculateExtraSnacksRequired(pv.GetVesselCrew());
            //Debug.Log("SnackDemand(" + pv.vesselName +"): e: " + extra + " r:" + demand);
            if ((demand + extra) <= 0)
                return 0;
            double fed = GetSnackResource(pv.protoPartSnapshots, demand + extra);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return pv.GetVesselCrew().Count * snacksPer;
            return demand + extra - fed;
        }

        /**
        * Removes the calculated number of snacks from the vessel.
        * returns the number of snacks that were required, but missing.
        * */
        public double RemoveSnacks(Vessel v)
        {

            double demand = v.GetVesselCrew().Count * snacksPer;
            double extra = CalculateExtraSnacksRequired(v.GetVesselCrew());
            //Debug.Log("SnackDemand(" + v.vesselName + "): e: " + extra + " r:" + demand);
            if ((demand + extra) <= 0)
                return 0;
            double fed = GetSnackResource(v.rootPart, demand + extra);
            //Debug.Log("fed" + fed + v.vesselName);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return v.GetCrewCount() * snacksPer;
            return demand + extra - fed;
        }
    }
}
