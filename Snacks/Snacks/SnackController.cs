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
        private double dayStartTime, snackTime;
        private const int secondsInDay = 6 * 60 * 60;
        private const double snacksPer = .25;
        private bool loadingNewScene = false;

        void Awake()
        {
            //GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            snacksResource = PartResourceLibrary.Instance.GetDefinition("Snacks");
            dayStartTime = Planetarium.GetUniversalTime();
            System.Random r = new System.Random();
            snackTime = r.NextDouble() * secondsInDay + dayStartTime;
            Debug.Log("Snacks Awake:" + dayStartTime);
        }

        /*
         * Called next.
         */
        void Start()
        {
            Debug.Log("Snacks start:" + Time.time);
            GameEvents.onCrewOnEva.Add(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
        }

        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            Debug.Log("EVA End");
            double got = GetSnackResource(data.from, 1.0);
            Debug.Log("EVA Got:" + got);
            List<PartResource> resources = new List<PartResource>();
            data.to.GetConnectedResources(snacksResource.id, resources);
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
            data.to.GetConnectedResources(snacksResource.id, resources);
            resources.First().amount = got;
            resources.First().maxAmount = 1;

        }

        /*
         * Called every frame
         */
        void Update()
        {
          
        }

        /*
         * Called at a fixed time interval determined by the physics time step.
         */
        void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f)
            {
                return;
            }

            double currentTime = Planetarium.GetUniversalTime();


            if (currentTime > snackTime)
            {
                System.Random rand = new System.Random();
                snackTime = rand.NextDouble() * secondsInDay + Planetarium.GetUniversalTime();
                Debug.Log("Next Snack Time!:" + snackTime);
                EatSnacks();


            }
        }

        /**Get a random chance of probability
         * 
         **/
        private bool GetRandomChance(int prob)
        {
            System.Random r = new System.Random();
            int i = r.Next() % 100;
            if (i < prob)
                return true;
            return false;
        }

        private double GetSnackResource(Part p, double demand)
        {
            List<PartResource> resources = new List<PartResource>();
            p.GetConnectedResources(snacksResource.id, resources);
     
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

        double CalculateSnacksRequired(List<ProtoCrewMember> crew)
        {

            double extra = 0;
            foreach (ProtoCrewMember pc in crew)
            {
                if (pc.isBadass && GetRandomChance(10))
                    extra += snacksPer;
                if (pc.stupidity > .6 && GetRandomChance(25))
                    extra -= snacksPer;
            }
            //Debug.Log("Extra:" + extra + " total:" + crew.Count * snacksPer);
            return extra + crew.Count * snacksPer;
        }

        double RemoveSnacks(ProtoVessel pv)
        {
            double demand = CalculateSnacksRequired(pv.GetVesselCrew());
            double fed = GetSnackResource(pv.protoPartSnapshots, demand);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return pv.GetVesselCrew().Count * snacksPer;
            return demand - fed;
        }


        double RemoveSnacks(Vessel v)
        {

            double demand = CalculateSnacksRequired(v.GetVesselCrew());
            double fed = GetSnackResource(v.rootPart, demand);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return v.GetCrewCount() * snacksPer;
            return demand - fed;
        }

        void EatSnacks()
        {
            double snacksMissed = 0;
            foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                if (pv.GetVesselCrew().Count > 0)
                {
                    if (!pv.vesselRef.loaded)
                    {
                        snacksMissed += RemoveSnacks(pv);
                       // Debug.Log("Ate snacks for: " + pv.vesselName);
                    }
                }
            }
            foreach (Vessel v in FlightGlobals.Vessels)
            {
                if(v.GetCrewCount()> 0)
                {
                    snacksMissed += RemoveSnacks(v);
                     //Debug.Log("Ate snacks for: " + v.vesselName);
                }
            
            }

            if (snacksMissed > 0)
            {
                int fastingKerbals = Convert.ToInt32(snacksMissed/snacksPer);
                Reputation.Instance.AddReputation(-1f * fastingKerbals, fastingKerbals + " Kerbals out of snacks!");
                ScreenMessages.PostScreenMessage(fastingKerbals + " Kerbals didn't have any snacks(reputation decreased by " + fastingKerbals + ")",5,ScreenMessageStyle.UPPER_LEFT);
            }
        }

        private void OnGameSceneLoadRequested(GameScenes gameScene)
        {
            Debug.Log("Game scene load requested: " + gameScene);

            // Disable this instance becuase a new instance will be created after the new scene is loaded
            loadingNewScene = true;
        }

        /*
         * Called when the game is leaving the scene (or exiting). Perform any clean up work here.
         */
        void OnDestroy()
        {
            //Debug.Log("Snacks destroy:" + Time.time);
            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
        }
    }
}
