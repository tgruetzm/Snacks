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
    class SnackLab : PartModule
    {

        private int soilResourceId = SnackConfiguration.Instance().SoilResourceId;
        private int snacksResourceId = SnackConfiguration.Instance().SnackResourceId;
        private int elecResourceId;

        [KSPField(isPersistant = true)]
        public bool processingStatus = false;
        [KSPField(isPersistant = true)]
        public double nextProcessingTime = 0;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Snack Lab Status")]
        public string snackLabStatus = "Idle";

        private double processingElecRate = 10;
        private double snackCompleteInterval = 60;
        private double snacksPerInterval = 5;
        private double soilPerInterval = .1;
        private const double compareThreshold = 0.0001;

        [KSPEvent(guiActive = true, guiName = "Process Snacks")]
        public void ProcessEvent()
        {
            processingStatus = true;
            snackLabStatus = "Processing";
            Events["ProcessEvent"].active = false;
        }

        /*
         * Called after the scene is loaded.
         */
        public override void OnAwake()
        {
            try
            {
                PartResourceDefinition ec = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");
                elecResourceId = ec.id;
                part.force_activate();
            }
            catch (Exception ex)
            {
                Debug.Log("SnackLab OnAwake(): " + ex.Message + ex.StackTrace);
            }
        }

        public override void OnFixedUpdate()
        {
            try
            {
                if (processingStatus == true)
                {
                    if (this.part.protoModuleCrew.Count < 2)
                    {
                        StopProcessing();
                        Debug.Log("Not enough crew");
                        return;
                    }
                        

                    double elecReq = processingElecRate * TimeWarp.fixedDeltaTime;
                    double got = this.part.vessel.rootPart.RequestResource(elecResourceId, elecReq);
                    if (elecReq - got > compareThreshold)
                    {
                        Debug.Log("not enough elec" + got);
                        StopProcessing();
                        return;
                    }
                    
                    double currentTime = Planetarium.GetUniversalTime();

                    if(nextProcessingTime == 0){
                        nextProcessingTime = currentTime + snackCompleteInterval;
                        return;
                    }
                    if (currentTime > nextProcessingTime)
                    {
                        nextProcessingTime = currentTime + snackCompleteInterval;
                        double soilGot = this.vessel.rootPart.RequestResource(soilResourceId, soilPerInterval);

                        if (soilPerInterval - soilGot > compareThreshold)
                        {
                            Debug.Log("soilgot < interval" + soilGot);
                            this.vessel.rootPart.RequestResource(soilResourceId, soilGot);
                            StopProcessing();
                            return;
                        }
                        else
                        {
                            this.vessel.rootPart.RequestResource(snacksResourceId, -1 * snacksPerInterval);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.Log("SnackLab OnFixedUpdate(): " + ex.Message + ex.StackTrace);
            }
        }

        private void StopProcessing()
        {
            processingStatus = false;
            snackLabStatus = "Idle";
            Events["ProcessEvent"].active = true;
        }


        /*
         * KSP adds the return value to the information box in the VAB/SPH.
         */
        public override string GetInfo()
        {
            return "Converts soil into artificially flavored somewhat edible snacks.";
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
