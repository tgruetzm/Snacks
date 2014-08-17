using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    class SnackModule : PartModule
    {

        double passedOutTime = -1;
        double currentTime = -1;
        double passedOutEnd = -1;

        /*
         * Called after the scene is loaded.
         */
        public override void OnAwake()
        {
            Debug.Log("TAC Examples-SimplePartModule [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.0000") + "]: OnAwake: " + this.name);
        }

        /*
         * Called when the part is activated/enabled. This usually occurs either when the craft
         * is launched or when the stage containing the part is activated.
         * You can activate your part manually by calling part.force_activate().
         */
        public override void OnActive()
        {
            Debug.Log("Snacks - Start SnackModuleActive");
            currentTime = Planetarium.GetUniversalTime();
            passedOutTime = currentTime + 30;
            passedOutTime = passedOutTime + 5;
            FlightGlobals.ActiveVessel.OnFlyByWire += new FlightInputCallback(PassedOutKerbal);
        }

        /*
         * Called after OnAwake.
         */
        public override void OnStart(PartModule.StartState state)
        {
            Debug.Log("TAC Examples-SimplePartModule [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.0000") + "]: OnStart: " + state);
        }

        /*
         * Called every frame
         */
        public override void OnUpdate()
        {
          
        }

        /*
         * Called at a fixed time interval determined by the physics time step.
         */
        public override void OnFixedUpdate()
        {
           
        }

        /*
         * KSP adds the return value to the information box in the VAB/SPH.
         */
        public override string GetInfo()
        {
            Debug.Log("TAC Examples-SimplePartModule [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.0000") + "]: GetInfo");
            return "\nContains the TAC Example - Simple Part Module\n";
        }

        /*
         * Called when the part is deactivated. Usually because it was destroyed.
         */
        public override void OnInactive()
        {
            Debug.Log("Snacks - Start FlightController Destroyed");
            FlightGlobals.ActiveVessel.OnFlyByWire -= new FlightInputCallback(PassedOutKerbal);
        }

        /*
         * Called when the game is loading the part information. It comes from: the part's cfg file,
         * the .craft file, the persistence file, or the quicksave file.
         */
        public override void OnLoad(ConfigNode node)
        {
            Debug.Log("TAC Examples-SimplePartModule [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.0000") + "]: OnLoad: " + node);
        }

        /*
         * Called when the game is saving the part information.
         */
        public override void OnSave(ConfigNode node)
        {
            Debug.Log("TAC Examples-SimplePartModule [" + this.GetInstanceID().ToString("X")
                + "][" + Time.time.ToString("0.0000") + "]: OnSave: " + node);
        }


        void PassedOutKerbal(FlightCtrlState state)
        {
            currentTime = Planetarium.GetUniversalTime();
            if (currentTime > passedOutTime && currentTime < passedOutEnd)
            {
                if (state.pitch == 0)
                {
                    Debug.Log("passout starting");
                    state.pitch = -.1f;
                }
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
