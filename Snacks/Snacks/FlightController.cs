using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightController : MonoBehaviour
    {
        System.Random random = new System.Random();
        double passedOutTime = -1;
        double currentTime = -1;
        double passedOutEnd = -1;
        double passedOutDurationMax = 3;
        double passedOutDurationMin = 1;
        double passedOutIntervalMax = 30;

        double delayMin = 1;
        double delayMax = 2;

        Queue<FlightState> flightState;
        FlightInputCallback flightInputCallback; 
        FlightCtrlState lastState;
        SnackSnapshot snackSnapshot;



        void Awake()
        {
                Debug.Log("Snacks - Awake FlightController");
        }

        void Start()  //Called when vessel is placed on the launchpad
        {
            Debug.Log("Snacks - Start FlightController");

            GameEvents.onVesselChange.Add(OnVesselChange);
            snackSnapshot = SnackSnapshot.Instance();
            snackSnapshot.SnackSnapShotChanged += snackSnapshot_SnackSnapShotChanged;
            CheckCurrentVesselState();
        }

        private void OnVesselChange(Vessel data)
        {
            Debug.Log("OnVesselChange");
            CheckCurrentVesselState();
        }

        private void snackSnapshot_SnackSnapShotChanged(object sender, EventArgs e)
        {
            Debug.Log("snack snapshotChanged");
            CheckCurrentVesselState();
        }

        private void CheckCurrentVesselState()
        {
            if (!FlightGlobals.ready)
                return;
            Debug.Log("Checking Current Vessel State");
            bool outOfSnacks = snackSnapshot.IsShipOutOfSnacks(FlightGlobals.ActiveVessel.id);
            if (outOfSnacks)
            {
                Debug.Log("current ship out");
                currentTime = Planetarium.GetUniversalTime();
                passedOutTime = currentTime + RandomTime(0,passedOutIntervalMax);
                passedOutEnd = passedOutTime + RandomTime(passedOutDurationMin,passedOutDurationMax);
                flightInputCallback = new FlightInputCallback(PassedOutKerbal); ;
                FlightGlobals.ActiveVessel.OnFlyByWire += flightInputCallback;
                flightState = new Queue<FlightState>();
            }
            else
            {
                Debug.Log("current ship has snacks");
                if (flightInputCallback != null)
                    FlightGlobals.ActiveVessel.OnFlyByWire -= flightInputCallback;
            }
        }

        //remove the fly-by-wire function when we get destroyed:
        void OnDestroy()
        {
            Debug.Log("Snacks - Start FlightController Destroyed");
        }

        void PassedOutKerbal(FlightCtrlState state)
        {
            try
            {
                currentTime = Planetarium.GetUniversalTime();
                double waitTime = currentTime + 1;// RandomTime(delayMin, delayMax);
                Debug.Log("ct:" + currentTime + " wt:" + waitTime + "stateP:" + state.pitch);
                FlightCtrlState storeState = new FlightCtrlState();
                storeState.CopyFrom(state);
                flightState.Enqueue(new FlightState(storeState, waitTime));

                
                //Debug.Log("ct:" + currentTime + " tsTime:" + topState.Time + "tsPitch:" + topState.State.pitch);
                while(flightState.Count > 0 && currentTime > flightState.Peek().Time)
                {
                    FlightState topState = flightState.Dequeue();
                    state.CopyFrom(topState.State);
                    Debug.Log("apply state pitch:" + state.pitch);
                    lastState = topState.State;
                    return;
                }
                if (lastState != null)
                    state.CopyFrom(lastState);
                else
                    state.Neutralize();
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - PassedOutKerbal: " + ex.Message + ex.StackTrace);
            }
        }

        double RandomTime(double min, double max)
        {
            return random.NextDouble() * (max+min) - min;
        }




    }
}