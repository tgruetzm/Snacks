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

            //if (currentTime > dayStartTime + secondsInDay)
            // oncePerDayUpdate();

            if (currentTime > snackTime)
            {
                System.Random rand = new System.Random();
                snackTime = rand.NextDouble() * secondsInDay + Planetarium.GetUniversalTime();
                Debug.Log("Next Snack Time!:" + snackTime);
                EatSnacks();


            }



        }

        bool TryRemoveSnacks(ProtoVessel pv, double request)
        {
            Debug.Log("tryremove 1");
            double supplied = 0;
            foreach (ProtoPartSnapshot pps in pv.protoPartSnapshots)
            {
                Debug.Log("tryremove 2");
                var res = from r in pps.resources
                          where r.resourceName == "Snacks"
                          select r;
                if (res.Count() > 0)
                {
                    Debug.Log("tryremove 3");
                    //Debug.Log(res.First().resourceValues);
                    ConfigNode node = res.First().resourceValues;
                    double amount = Double.Parse(node.GetValue("amount"));
                    Debug.Log("tryremove 4");
                    if (amount >= request)
                    {
                        node.SetValue("amount", (amount -= request).ToString());
                        Debug.Log("removed snacks from: " + pv.vesselName);
                        return true;//met request
                    }
                    else
                    {
                        node.SetValue("amount", "0");
                        supplied += amount;
                        request -= amount;
                    }
                }

            }

            return false;
        }


        bool TryRemoveSnacks(Vessel pv, double request)
        {
            double supplied = 0;



            return false;
        }

        void oncePerDayUpdate()
        {
            Debug.Log("Once per day update:" + Time.time);
            dayStartTime = Planetarium.GetUniversalTime();
            int fastingCrew = 0;
            foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                if (pv.GetVesselCrew().Count > 0)
                {
                    // Debug.Log("snacks name" + pv.vesselName);
                    // Debug.Log("snacks vessel ref" + pv.vesselRef);




                }


            }
            if (fastingCrew > 0)
            {
                Reputation.Instance.AddReputation(-1f * fastingCrew, fastingCrew + " Kerbals out of snacks!");
                ScreenMessages.PostScreenMessage(fastingCrew + " Kerbals out of snacks(reputation decreased by " + fastingCrew + ")");
            }


        }

        void EatSnacks()
        {
            foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                Debug.Log("es1:" + pv.vesselName);
                Debug.Log("es2:" + pv.GetVesselCrew().Count);
                if (pv.GetVesselCrew().Count > 0)
                {
                    Debug.Log("es3");
                    Debug.Log(pv.vesselRef);
                    if (!pv.vesselRef.loaded)
                    {
                        Debug.Log("es4");
                        TryRemoveSnacks(pv, pv.GetVesselCrew().Count);
                    }
                    else
                    {

                        List<PartResource> resources = new List<PartResource>();
                        pv.vesselRef.rootPart.GetConnectedResources(snacksResource.id, resources);

                        double demand = 1 * pv.vesselRef.GetCrewCount();
                        double supplied = 0;
                        foreach (PartResource res in resources)
                        {
                            Debug.Log("eating snacks for v:" + pv.vesselName);
                            supplied = res.amount >= demand ? demand : res.amount;
                            res.amount -= supplied;
                            demand = demand - supplied;
                            if (supplied >= demand)
                                break;//we supplied enough snacks
                        }
                    }
                }

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
