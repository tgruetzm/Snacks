using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SnackController : MonoBehaviour
    {

        private PartResourceDefinition snacksResource;
        private double dayStartTime, snackTime;
        private int secondsInDay = 6 * 60 * 60;
        private bool loadingNewScene = false;

        void Awake()
        {
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
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
            throw new NotImplementedException();
        }

        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            Debug.Log("EVA start");
            double got = data.from.RequestResource(snacksResource.id, 1f);
            Debug.Log("EVA Got:" + got);
            //ConfigNode node = new ConfigNode("RESOURCE");
            //node.AddValue("name", "Snacks");
            //node.AddValue("amount ", got);
            //node.AddValue("maxAmount ", "1");
            //node.AddValue("flowState ", "True");
            //node.AddValue("isTweakable ", "False");
            //node.AddValue("hideFlow ", "False");
           // node.AddValue("flowMode  ", "both");
            data.to.RequestResource(snacksResource.id, -1 * got);
        }

        /*
         * Called every frame
         */
        void Update()
        {
            //Debug.Log("Snacks update:" + Time.time);
        }

        /*
         * Called at a fixed time interval determined by the physics time step.
         */
        void FixedUpdate()
        {
            if (Time.timeSinceLevelLoad < 1.0f || !FlightGlobals.ready || loadingNewScene)
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

        int TryRemoveSnacks(ProtoVessel pv, double request)
        {
            double supplied = 0;
            foreach (ProtoPartSnapshot pps in pv.protoPartSnapshots)
            {
                var res = from r in pps.resources
                          where r.resourceName == "Snacks"
                          select r;
                if (res.Count() > 0)
                {
                    //Debug.Log(res.First().resourceValues);
                    ConfigNode node = res.First().resourceValues;
                    double amount = Double.Parse(node.GetValue("amount"));
                    if (amount >= request)
                    {
                        node.SetValue("amount", (amount -= request).ToString());
                        Debug.Log("removed snacks from: " + pv.vesselName);
                        return 0;//met request
                    }
                    else
                    {
                        node.SetValue("amount", "0");
                        supplied += amount;
                        request -= amount;
                    }
                }

            }

            return pv.GetVesselCrew().Count;
        }


        int TryRemoveSnacks(Vessel v, double request)
        {
            List<PartResource> resources = new List<PartResource>();
            v.rootPart.GetConnectedResources(snacksResource.id, resources);

            double demand = 1 * v.GetCrewCount();
            double supplied = 0;
            foreach (PartResource res in resources)
            {
                Debug.Log("eating snacks for v:" + v.vesselName);
                supplied = res.amount >= demand ? demand : res.amount;
                res.amount -= supplied;
                demand = demand - supplied;
                if (supplied >= demand)
                    return 0;//we supplied enough snacks
            }

            return v.GetCrewCount();
        }

        void EatSnacks()
        {
            int fastingKerbals = 0;
            foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                if (pv.GetVesselCrew().Count > 0)
                {
                    Debug.Log(pv.vesselRef);
                    if (!pv.vesselRef.loaded)
                        fastingKerbals += TryRemoveSnacks(pv, pv.GetVesselCrew().Count);
                    else
                        fastingKerbals += TryRemoveSnacks(pv.vesselRef, pv.vesselRef.GetCrewCount());
                }

            }
            if (fastingKerbals > 0)
            {
                Debug.Log("fasting kerbals:" + fastingKerbals);
                Reputation.Instance.AddReputation(-1f * fastingKerbals, fastingKerbals + " Kerbals out of snacks!");
                ScreenMessages.PostScreenMessage(fastingKerbals + " Kerbals out of snacks(reputation decreased by " + fastingKerbals + ")");
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
            Debug.Log("Snacks destroy:" + Time.time);
        }
    }
}
