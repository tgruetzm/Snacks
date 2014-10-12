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

        [KSPField(guiActive = true, guiName = "Soil Potential",guiFormat="0.00")]
        public float soilPotential = 0;

        private const double maxQuantity = .5;
        private const int soilBlockSize = 100;
        private const float kerbalMass = 0.09375f;
        private const double evaSoilCap = 1;
        private float soilDensity;
        private System.Random random = new System.Random();

        private int soilResourceId = SnackConfiguration.Instance().SoilResourceId;

        private int latitude = 0;
        private int longitude = 0;
        private int cLatitude;
        private int cLongitude;
        private bool landed = false;

        [KSPEvent(guiActive = true, guiName = "Gather Soil")]
        public void GatherSoil()
        {
            try
            {
                if (FlightGlobals.ActiveVessel.srfSpeed > .1)
                {
                    ScreenMessages.PostScreenMessage("Can't gather soil while moving!", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
                else if (latitude == cLatitude && longitude == cLongitude)
                {
                    ScreenMessages.PostScreenMessage("The soil doesn't have the correct composition, better look somewhere else.", 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
                else
                {
                    latitude = Convert.ToInt32(this.vessel.latitude * soilBlockSize);
                    longitude = Convert.ToInt32(this.vessel.longitude * soilBlockSize);
                    float newQty = Convert.ToSingle(soilPotential);
                    soilPotential = 0;
                    part.RequestResource(soilResourceId, newQty * -1);
                    ScreenMessages.PostScreenMessage("This soil has some potential. Added " + newQty.ToString("0.00") , 5.0f, ScreenMessageStyle.UPPER_LEFT);
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
            double got = part.RequestResource(soilResourceId, evaSoilCap);
        }

        public override void OnFixedUpdate()
        {
            try
            {
                if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready)
                {
                    cLatitude = Convert.ToInt32(this.vessel.latitude * soilBlockSize);
                    cLongitude = Convert.ToInt32(this.vessel.longitude * soilBlockSize);
                    CalculatePotential();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - NA OnFixedUpdate: " + ex.Message + ex.StackTrace);
            }
        }

        /*
         * Called after the scene is loaded.
         */
        public override void OnAwake()
        {
            try
            {
                Debug.Log("Nutritive Analyzer OnAwake()");
                GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
                PartResourceDefinition soilResource = PartResourceLibrary.Instance.GetDefinition("Soil");
                soilDensity = soilResource.density;
                part.force_activate();
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - NA OnAwake: " + ex.Message + ex.StackTrace);
            }
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

        private void CalculatePotential()
        {
            if (latitude != cLatitude && longitude != cLongitude && landed)
                soilPotential = Mathf.PerlinNoise(Convert.ToSingle(cLatitude) / soilBlockSize, Convert.ToSingle(cLongitude) / soilBlockSize) / 2;
            else
                soilPotential = 0;
        }

        public void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> change) 
        {
            //Debug.Log("SitChange to:" + change.to + " from:" + change.from);
            if (change.from == Vessel.Situations.LANDED && change.to == Vessel.Situations.FLYING)
            {
                Events["GatherSoil"].active = false;
                landed = false;
                CalculatePotential();
            }
            else if (change.from == Vessel.Situations.FLYING && change.to == Vessel.Situations.LANDED)
            {
                Events["GatherSoil"].active = true;
                landed = true;
                CalculatePotential();
            }
        }
    }
}
