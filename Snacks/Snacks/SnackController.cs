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
using KSP.IO;

namespace Snacks
{

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SnackController : MonoBehaviour
    {
        private double snackTime = -1;
        private System.Random random = new System.Random();

        private SnackConsumer consumer;
        private double snacksPerMeal;
        private double lossPerDayPerKerbal;
        private int snackResourceId;
        private int snackFrequency;

        void Awake()
        {
            try
            {
                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
                GameEvents.onUndock.Add(OnUndock);
                GameEvents.onPartCouple.Add(OnDock);
                SnackConfiguration snackConfig = SnackConfiguration.Instance();
                snackResourceId = snackConfig.SnackResourceId;
                snackFrequency = 6 * 60 * 60 * 2 / snackConfig.MealsPerDay;
                snacksPerMeal = snackConfig.SnacksPerMeal;
                lossPerDayPerKerbal = snackConfig.LossPerDay;
                consumer = new SnackConsumer(snackConfig.SnacksPerMeal, snackConfig.LossPerDay);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - Awake error: " + ex.Message + ex.StackTrace);
            }
            
        }
        void Start()
        {
            try
            {
                snackTime = random.NextDouble() * snackFrequency + Planetarium.GetUniversalTime();
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - Start error: " + ex.Message + ex.StackTrace);
            }
        }

        private void OnUndock(EventReport data)
        {
            //SnackSnapshot.SetRebuildSnapshot();
        }

        private void OnDock(GameEvents.FromToAction<Part, Part> data)
        {
            //SnackSnapshot.SetRebuildSnapshot();
        }


        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Debug.Log("EVA End");
                double got = consumer.GetSnackResource(data.from, 1.0);
                Debug.Log("EVA Got:" + got);
                List<PartResource> resources = new List<PartResource>();
                data.to.GetConnectedResources(snackResourceId, ResourceFlowMode.ALL_VESSEL, resources);
                resources.First().amount += got;
                //SnackSnapshot.SetRebuildSnapshot();
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - OnCrewBoardVessel: " + ex.Message + ex.StackTrace);
            }
        }

        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Debug.Log("EVA start");
                double got = consumer.GetSnackResource(data.from, 1.0);
                Debug.Log("EVA Got:" + got);
                if (!data.to.Resources.Contains(snackResourceId))
                {
                    ConfigNode node = new ConfigNode("RESOURCE");
                    node.AddValue("name", "Snacks");
                    data.to.Resources.Add(node);
                }
                List<PartResource> resources = new List<PartResource>();
                data.to.GetConnectedResources(snackResourceId, ResourceFlowMode.ALL_VESSEL, resources);
                resources.First().amount = got;
                resources.First().maxAmount = 1;
                //SnackSnapshot.SetRebuildSnapshot();
                //data.to.AddModule("EVANutritiveAnalyzer");
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - OnCrewOnEva " + ex.Message + ex.StackTrace);
            }

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
                    snackTime = rand.NextDouble() * snackFrequency + currentTime;
                    Debug.Log("Next Snack Time!:" + currentTime);
                    EatSnacks();
                    SnackSnapshot.SetRebuildSnapshot();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - FixedUpdate: " + ex.Message + ex.StackTrace);
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
                Debug.Log("Snacks - OnDestroy: " + ex.Message + ex.StackTrace);
            }
        }

        private void EatSnacks()
        {
            double snacksMissed = 0;
            foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
            {  
                if (pv.GetVesselCrew().Count > 0)
                {
                    if (!pv.vesselRef.loaded)
                    {
                        double snacks = consumer.RemoveSnacks(pv);
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
                    double snacks = consumer.RemoveSnacks(v);
                    snacksMissed += snacks;
                    if(snacks > 0)
                        Debug.Log("No snacks for: " + v.vesselName);
                }
            
            }

            if (snacksMissed > 0)
            {
                int fastingKerbals = Convert.ToInt32(snacksMissed/snacksPerMeal);
                double repLoss;
                if (Reputation.CurrentRep > 0)
                    repLoss = fastingKerbals * lossPerDayPerKerbal * Reputation.Instance.reputation;
                else
                    repLoss = fastingKerbals;
                Reputation.Instance.AddReputation(Convert.ToSingle(-1 * repLoss), fastingKerbals + " Kerbals out of snacks!");
                ScreenMessages.PostScreenMessage(fastingKerbals + " Kerbals didn't have any snacks(reputation decreased by " + Convert.ToInt32(repLoss) + ")",5, ScreenMessageStyle.UPPER_LEFT);
            }
        }

        private void SetVesselOutOfSnacks(Vessel v)
        {
            v.GetVesselCrew()[0].courage = 0;

            //Debug.Log(pv.ctrlState);
        }
    }
}
