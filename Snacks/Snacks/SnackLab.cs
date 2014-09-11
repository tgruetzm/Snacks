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

        private double processingElecRate = 10;
        private double snackCompleteInterval = 10;
        private double snacksPerInterval = 1;
        private double soilPerInterval = .01;

        [KSPEvent(guiActive = true, guiName = "Process Snacks")]
        public void ProcessEvent()
        {
            processingStatus = true;
            Events["ProcessEvent"].active = false;
        }
        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 5f, guiName = "Transfer Soil to Lab")]
        public void TransferSoil()
        {
            try
            {
                Vessel eva = FlightGlobals.ActiveVessel;
                double got = eva.rootPart.RequestResource(soilResourceId, 1);
                this.part.RequestResource(soilResourceId, got * -1);
                Debug.Log("transfersoil got:" + got);
                //AddSoil(this.part, got);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - TransferSnacks: " + ex.Message + ex.StackTrace);
            }
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

        private void AddSoil(Part p, double value)
        {
            List<PartResource> resources = new List<PartResource>();
            p.GetConnectedResources(soilResourceId, ResourceFlowMode.ALL_VESSEL, resources);
            PartResource pr = resources.First();
            pr.amount += value;
            if (pr.amount > pr.maxAmount)
                pr.amount = pr.maxAmount;
        }

        /*
         * Called when the part is activated/enabled. This usually occurs either when the craft
         * is launched or when the stage containing the part is activated.
         * You can activate your part manually by calling part.force_activate().
         */
        public override void OnActive()
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Debug.Log("SnackLab OnActive(): " + ex.Message + ex.StackTrace);
            }
        }


        public override void OnFixedUpdate()
        {
            try
            {
                if (processingStatus == true)
                {
                    double elecReq = processingElecRate * TimeWarp.fixedDeltaTime;
                    this.part.vessel.rootPart.RequestResource(elecResourceId, elecReq);
                    
                    double currentTime = Planetarium.GetUniversalTime();

                    if(nextProcessingTime == 0){
                        nextProcessingTime = currentTime + snackCompleteInterval;
                        return;
                    }
                    if (currentTime > nextProcessingTime)
                    {
                        nextProcessingTime = currentTime + snackCompleteInterval;
                        double soilGot = this.vessel.rootPart.RequestResource(soilResourceId, soilPerInterval);
                        if (soilGot < soilPerInterval)
                        {
                            processingStatus = false;
                            Events["ProcessEvent"].active = true;
                        }
                        else
                        {
                            this.vessel.rootPart.RequestResource(snacksResourceId, -1);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.Log("SnackLab OnFixedUpdate(): " + ex.Message + ex.StackTrace);
            }
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
