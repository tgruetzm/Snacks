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
    //[KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightController : MonoBehaviour
    {
        System.Random random = new System.Random();

        double currentTime;

        double delayTime = .15;
 

        Queue<FlightState> flightState;
        FlightCtrlState lastState;
        Vessel currentVessel;
        bool currentVesselOut = false;

        void Awake()
        {
                Debug.Log("Snacks - Awake FlightController");
        }

        void Start()  //Called when vessel is placed on the launchpad
        {
            delayTime = SnackConfiguration.Instance().DelayedReaction;

            if (delayTime > 0)
            {
                Debug.Log("Snacks - Start FlightController");

                GameEvents.onVesselChange.Add(OnVesselChangeMod);
                GameEvents.onFlightReady.Add(OnFlightReady);
                GameEvents.onVesselWasModified.Add(OnVesselChangeMod);
                SnackController.SnackTime += SnackController_SnackTime;
            }
        }

        void SnackController_SnackTime(object sender, EventArgs e)
        {
            CheckCurrentVesselState();
        }

        private void OnVesselChangeMod(Vessel data)
        {
            Debug.Log("OnVesselChangeMod");
            CheckCurrentVesselState();
        }

        private void OnFlightReady()
        {
            Debug.Log("OnFlightReady");
            CheckCurrentVesselState();
        }

        private void CheckCurrentVesselState()
        {
            try
            {
                if (FlightGlobals.ActiveVessel == null)
                    return;
                bool outOfSnacks = IsOutOfSnacks(FlightGlobals.ActiveVessel);
                if (FlightGlobals.ActiveVessel != currentVessel || outOfSnacks != currentVesselOut)
                {
                    currentVesselOut = outOfSnacks;
                    if(currentVessel != null)
                        currentVessel.OnFlyByWire -= new FlightInputCallback(GrumpyKerbal);
                    
                    if (outOfSnacks)
                    {
                        Debug.Log("current ship out of snacks");
                        ScreenMessages.PostScreenMessage("You are out of snacks.  Kerbals may exhibit delayed reactions.  Resupply ASAP!", 5, ScreenMessageStyle.UPPER_LEFT);
                        FlightGlobals.ActiveVessel.OnFlyByWire += new FlightInputCallback(GrumpyKerbal);
                        flightState = new Queue<FlightState>();
                    }
                    else
                    {
                        Debug.Log("current ship has snacks");
                        FlightGlobals.ActiveVessel.OnFlyByWire -= new FlightInputCallback(GrumpyKerbal);
                    } 
                }
                currentVessel = FlightGlobals.ActiveVessel;  
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - CheckVesselState: " + ex.Message + ex.StackTrace);
            }
        }

        //remove the fly-by-wire function when we get destroyed:
        void OnDestroy()
        {
            Debug.Log("Snacks - Start FlightController Destroyed");
            FlightGlobals.ActiveVessel.OnFlyByWire -= new FlightInputCallback(GrumpyKerbal);
        }

        void GrumpyKerbal(FlightCtrlState state)
        {
            try
            {
                currentTime = Planetarium.GetUniversalTime();
                double waitTime = currentTime + delayTime;
                FlightCtrlState storeState = new FlightCtrlState();
                storeState.CopyFrom(state);
                flightState.Enqueue(new FlightState(storeState, waitTime));

                
               
                while(currentTime > flightState.Peek().Time)
                {
                    FlightState topState = flightState.Dequeue();
                    state.CopyFrom(topState.State);
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
                Debug.Log("Snacks - GrumpyKerbal: " + ex.Message + ex.StackTrace);
            }
        }

        private double RandomTime(double min, double max)
        {
            return random.NextDouble() * (max+min) - min;
        }

        private bool IsOutOfSnacks(Vessel v)
        {
            if (v.GetVesselCrew().Count > 0)
            {
                List<PartResource> resources = new List<PartResource>();
                v.rootPart.GetConnectedResources(SnackConfiguration.Instance().SnackResourceId, ResourceFlowMode.ALL_VESSEL, resources);
                foreach (PartResource r in resources)
                {
                    if (r.amount > 0)
                        return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

    }
}