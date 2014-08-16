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

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SnackController : MonoBehaviour
    {

        private PartResourceDefinition snacksResource;
        private double snackTime = -1;
        private const int secondsInDay = 6 * 60 * 60;
        private const double snacksPer = .25;
        private const float lossPerDayPerKerbal = 0.0025f;
        private bool loadingNewScene = false;
        private System.Random random = new System.Random();

        void Awake()
        {
            try
            {
                snacksResource = PartResourceLibrary.Instance.GetDefinition("Snacks");
                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - Awake error: " + ex.Message);
            }
            
        }

        void Start()
        {
            try
            {
                snackTime = random.NextDouble() * secondsInDay + Planetarium.GetUniversalTime();
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - Start error: " + ex.Message);
            }
        }

        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            Debug.Log("EVA End");
            double got = GetSnackResource(data.from, 1.0);
            Debug.Log("EVA Got:" + got);
            List<PartResource> resources = new List<PartResource>();
            data.to.GetConnectedResources(snacksResource.id,ResourceFlowMode.ALL_VESSEL, resources);
            resources.First().amount += got;
        }

        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            Debug.Log("EVA start");
            double got = GetSnackResource(data.from,1.0);
            Debug.Log("EVA Got:" + got);
            if (!data.to.Resources.Contains(snacksResource.id))
            {
                ConfigNode node = new ConfigNode("RESOURCE");
                node.AddValue("name", "Snacks");
                data.to.Resources.Add(node);
            }
            List<PartResource> resources = new List<PartResource>();
            data.to.GetConnectedResources(snacksResource.id,ResourceFlowMode.ALL_VESSEL, resources);
            resources.First().amount = got;
            resources.First().maxAmount = 1;

        }

        void FixedUpdate()
        {
            try
            {

                if (Time.timeSinceLevelLoad < 1.0f)
                {
                    return;
                }

                double currentTime = Planetarium.GetUniversalTime();

                if (currentTime > snackTime)
                {
                    System.Random rand = new System.Random();
                    snackTime = rand.NextDouble() * secondsInDay + currentTime;
                    Debug.Log("Next Snack Time!:" + currentTime);
                    EatSnacks();


                }
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - FixedUpdate: " + ex.Message);
            }
        }

        void OnDestroy()
        {
            try
            {
                GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - OnDestroy: " + ex.Message);
            }
        }

        private bool GetRandomChance(double prob)
        {
            if (random.NextDouble() < prob)
                return true;
            return false;
        }

        private double GetSnackResource(Part p, double demand)
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

        private double GetSnackResource(List<ProtoPartSnapshot> protoPartSnapshots, double demand)
        {
            double supplied = 0;
            foreach (ProtoPartSnapshot pps in protoPartSnapshots)
            {
                var res = from r in pps.resources
                          where r.resourceName == "Snacks"
                          select r;
                if (res.Count() > 0)
                {
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
            return supplied;
        }

        private double CalculateSnacksRequired(List<ProtoCrewMember> crew)
        {

            double extra = 0;
            foreach (ProtoCrewMember pc in crew)
            {
                if (GetRandomChance(pc.courage/2.0))
                    extra += snacksPer;
                if (GetRandomChance(pc.stupidity/2.0))
                    extra -= snacksPer;
                if (pc.isBadass && GetRandomChance(.2))
                    extra -= snacksPer;
            }
            //Debug.Log("Extra:" + extra + " total:" + crew.Count * snacksPer);
            return extra + crew.Count * snacksPer;
        }

        private double RemoveSnacks(ProtoVessel pv)
        {
            double demand = CalculateSnacksRequired(pv.GetVesselCrew());
            double fed = GetSnackResource(pv.protoPartSnapshots, demand);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return pv.GetVesselCrew().Count * snacksPer;
            return demand - fed;
        }

        private double RemoveSnacks(Vessel v)
        {

            double demand = CalculateSnacksRequired(v.GetVesselCrew());
            double fed = GetSnackResource(v.rootPart, demand);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return v.GetCrewCount() * snacksPer;
            return demand - fed;
        }

        private void EatSnacks()
        {
            double snacksMissed = 0;
            foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                SetVesselOutOfSnacks(pv);
                if (pv.GetVesselCrew().Count > 0)
                {
                    if (!pv.vesselRef.loaded)
                    {
                        double snacks = RemoveSnacks(pv);
                        snacksMissed += snacks;
                        if (snacks > 0)
                            Debug.Log("No snacks for: " + pv.vesselName);
                    }
                }
            }
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if(v.GetCrewCount()> 0)
                {
                    double snacks = RemoveSnacks(v);
                    snacksMissed += snacks;
                    if(snacks > 0)
                        Debug.Log("No snacks for: " + v.vesselName);
                }
            
            }

            if (snacksMissed > 0)
            {
                int fastingKerbals = Convert.ToInt32(snacksMissed/snacksPer);
                float repLoss;
                if (Reputation.CurrentRep > 0)
                    repLoss = fastingKerbals * lossPerDayPerKerbal * Reputation.Instance.reputation;
                else
                    repLoss = fastingKerbals;
                Reputation.Instance.AddReputation(-1f * repLoss, fastingKerbals + " Kerbals out of snacks!");
                ScreenMessages.PostScreenMessage(fastingKerbals + " Kerbals didn't have any snacks(reputation decreased by " + repLoss + ")",5, ScreenMessageStyle.UPPER_LEFT);
            }
        }

        private void SetVesselOutOfSnacks(ProtoVessel pv)
        {
            Debug.Log(pv.ctrlState);
        }
    }
}
