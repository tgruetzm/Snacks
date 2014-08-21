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
    class SnackFactory : PartModule
    {

        [KSPEvent(guiActive = true, guiName = "Activate Snack Factory")]
        public void ActivateEvent()
        {
            ScreenMessages.PostScreenMessage("Clicked Activate", 5.0f, ScreenMessageStyle.UPPER_CENTER);

            // This will hide the Activate event, and show the Deactivate event.
            Events["ActivateEvent"].active = false;
            Events["DeactivateEvent"].active = true;
        }

        [KSPEvent(guiActive = true, guiName = "Deactivate Snack Factory", active = false)]
        public void DeactivateEvent()
        {
            ScreenMessages.PostScreenMessage("Clicked Deactivate", 5.0f, ScreenMessageStyle.UPPER_CENTER);

            // This will hide the Deactivate event, and show the Activate event.
            Events["ActivateEvent"].active = true;
            Events["DeactivateEvent"].active = false;
        }

        /*
         * Called after the scene is loaded.
         */
        public override void OnAwake()
        {
            Debug.Log("SnackFactory OnAwake()");
        }

        /*
         * Called when the part is activated/enabled. This usually occurs either when the craft
         * is launched or when the stage containing the part is activated.
         * You can activate your part manually by calling part.force_activate().
         */
        public override void OnActive()
        {
            Debug.Log("Snacks - Start SnackModuleActive");
        }

        /*
         * Called after OnAwake.
         */
        public override void OnStart(PartModule.StartState state)
        {
        }


        /*
         * KSP adds the return value to the information box in the VAB/SPH.
         */
        public override string GetInfo()
        {
            return "Generates snacks";
        }

        /*
         * Called when the part is deactivated. Usually because it was destroyed.
         */
        public override void OnInactive()
        {
            Debug.Log("Snacks - Start FlightController Destroyed");
        }

        /*
         * Called when the game is loading the part information. It comes from: the part's cfg file,
         * the .craft file, the persistence file, or the quicksave file.
         */
        public override void OnLoad(ConfigNode node)
        {

        }

        /*
         * Called when the game is saving the part information.
         */
        public override void OnSave(ConfigNode node)
        {

        }

    
    }
}
