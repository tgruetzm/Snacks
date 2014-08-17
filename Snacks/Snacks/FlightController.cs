using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    //[KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightController : MonoBehaviour
    {
        bool passedOut = false;
        double passedOutTime = -1;
        double currentTime = -1;
        double passedOutEnd = -1;


        void Awake()
        {
                Debug.Log("Snacks - Awake FlightController");
        }

        void Start()  //Called when vessel is placed on the launchpad
        {
            Debug.Log("Snacks - Start FlightController");
            currentTime = Planetarium.GetUniversalTime();
            passedOutTime = currentTime + 30;
            passedOutTime = passedOutTime + 5;
            FlightGlobals.ActiveVessel.OnFlyByWire += new FlightInputCallback(PassedOutKerbal);


        }

        //remove the fly-by-wire function when we get destroyed:
        void OnDestroy()
        {
            Debug.Log("Snacks - Start FlightController Destroyed");
            FlightGlobals.ActiveVessel.OnFlyByWire -= new FlightInputCallback(PassedOutKerbal);

        }


        void PassedOutKerbal(FlightCtrlState state)
        {
            currentTime = Planetarium.GetUniversalTime();
            if (currentTime > passedOutTime && currentTime < passedOutEnd)
            {
                Debug.Log("passout starting");
                state.Y = -1f;
            }
            if (currentTime > passedOutEnd)
            {
                Debug.Log("passout ended, resetting");
                passedOutTime = currentTime + 30;
                passedOutEnd = passedOutTime + 5;
            }

            
        }




    }
}