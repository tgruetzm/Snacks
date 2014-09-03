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
        double passedOutDurationMax = 10;
        double passedOutIntervalMax = 30;
        FlightInputCallback flightInputCallback; 
        FlightCtrlState passedOutState;
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
                passedOutTime = currentTime + RandomTime(passedOutIntervalMax);
                passedOutEnd = passedOutTime + RandomTime(passedOutDurationMax);
                flightInputCallback = new FlightInputCallback(PassedOutKerbal); ;
                FlightGlobals.ActiveVessel.OnFlyByWire += flightInputCallback;
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
            currentTime = Planetarium.GetUniversalTime();
            if (currentTime > passedOutTime && currentTime < passedOutEnd)
            {
                if (passedOutState == null)
                {
                    passedOutState = new FlightCtrlState();
                    passedOutState.CopyFrom(state);
                    ScreenMessages.PostScreenMessage("Kerbal passed out at the controls, no snacks!", 2, ScreenMessageStyle.UPPER_LEFT);
                }
                state.CopyFrom(passedOutState);

            }
            if (currentTime > passedOutEnd)
            {
                passedOutTime = currentTime + RandomTime(passedOutIntervalMax);
                passedOutEnd = passedOutTime + RandomTime(passedOutDurationMax);
                passedOutState = null;
            }
        }

        double RandomTime(double max)
        {
            return random.NextDouble() * max;
        }




    }
}