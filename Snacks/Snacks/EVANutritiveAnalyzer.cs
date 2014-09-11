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
    class EVANutritiveAnalyzer : PartModule
    {

        //[KSPField(isPersistant = true, guiActive = true, guiName = "Soil Quantity:", guiUnits="%")]
        //public int soilQuantity = 0;

        private const double maxQuantity = .5;
        private const double soilChangeDistance = 0.005;
        private const double kerbalMass = 0.09375;

        private int soilResourceId = SnackConfiguration.Instance().SoilResourceId;

        private double latitude = 0;
        private double longitude = 0;

        [KSPEvent(guiActive = true, guiName = "Gather Soil")]
        public void GatherSoil()
        {
            try
            {
                if (latitude == 0 && longitude == 0)
                {
                    latitude = this.vessel.latitude;
                    longitude = this.vessel.longitude;
                    System.Random rand = new System.Random();
                    double newQty = rand.NextDouble() * maxQuantity;
                    AddRemoveSoil(this.vessel.rootPart, newQty);
                    ScreenMessages.PostScreenMessage("This soil has some potential.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
                //Debug.Log("lat:" + this.vessel.latitude + " long:" + this.vessel.longitude + " slat:" + latitude + "slong:" + longitude);
                double diff1 = Math.Abs(this.vessel.latitude - latitude);
                double diff2 = Math.Abs(this.vessel.longitude - longitude);
                bool isSoilFull = false;
                //Debug.Log("diff1:" + diff1 + " diff2:" + diff2);
                if (diff1 < soilChangeDistance && diff2 < soilChangeDistance)
                {
                    ScreenMessages.PostScreenMessage("The soil hasn't changed much, better look somewhere else.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                }
                else
                {
                    latitude = this.vessel.latitude;
                    longitude = this.vessel.longitude;
                    System.Random rand = new System.Random();
                    double newQty = rand.NextDouble() * maxQuantity;
                    AddRemoveSoil(this.vessel.rootPart, newQty);
                    ScreenMessages.PostScreenMessage("This soil has some potential.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                }
                if (isSoilFull)
                {
                    Events["GatherSoil"].active = false;
                    ScreenMessages.PostScreenMessage("You can't carry anymore.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - GatherSoil: " + ex.Message + ex.StackTrace);
            }
        }

        [KSPEvent(guiActive = true, guiName = "Dump Soil")]
        public void DumpSoil()
        {
            this.AddRemoveSoil(this.vessel.rootPart, Double.MinValue);
            Events["GatherSoil"].active = true;
        }

        private bool AddRemoveSoil(Part p, double value)
        {
            Debug.Log("adding soil:" + value);
            List<PartResource> resources = new List<PartResource>();
            p.GetConnectedResources(soilResourceId, ResourceFlowMode.ALL_VESSEL, resources);
            PartResource pr = resources.First();
            pr.amount += value;
            if (pr.amount < 0)
                pr.amount = 0;
            if (pr.amount >= pr.maxAmount)
            {
                pr.amount = pr.maxAmount;
                return true;
            }
            p.mass = Convert.ToSingle(kerbalMass + pr.amount * kerbalMass);
            return false;
        }

        /*
         * Called after the scene is loaded.
         */
        public override void OnAwake()
        {
            Debug.Log("Nutritive Analyzer OnAwake()");
            GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
        }

        /*
         * Called after OnAwake.
         */
        public override void OnStart(PartModule.StartState state)
        {
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

        public void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> change) 
        {
            //Debug.Log("SitChange to:" + change.to + " from:" + change.from);
            if (change.from == Vessel.Situations.LANDED && change.to == Vessel.Situations.FLYING)
            {
                Events["GatherSoil"].active = false;
            }
            else if (change.from == Vessel.Situations.FLYING && change.to == Vessel.Situations.LANDED)
            {
                Events["GatherSoil"].active = true;
            }
        }
    }
}
