using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    class SoilContainer : PartModule
    {

        private int soilResourceId = SnackConfiguration.Instance().SoilResourceId;
        private double evaSoilCap = 1;


        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 5f, guiName = "Store Soil")]
        public void StoreSoil()
        {
            try
            {
                Vessel eva = FlightGlobals.ActiveVessel;
                double got = eva.rootPart.RequestResource(soilResourceId, evaSoilCap);
                double stored = this.part.RequestResource(soilResourceId, got * -1);
                eva.rootPart.RequestResource(soilResourceId, got + stored);
                ScreenMessages.PostScreenMessage("Stored Soil in: " + this.part.vessel.vesselName, 5.0f, ScreenMessageStyle.UPPER_LEFT);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - StoreSoil: " + ex.Message + ex.StackTrace);
            }
        }

        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 5f, guiName = "Take Soil")]
        public void TakeSoil()
        {
            try
            {
                double got = this.part.RequestResource(soilResourceId, evaSoilCap);
                double stored = FlightGlobals.ActiveVessel.rootPart.RequestResource(soilResourceId, got * -1);
                this.part.RequestResource(soilResourceId, got + stored);
                ScreenMessages.PostScreenMessage("Took Soil from: " + this.part.vessel.vesselName, 5.0f, ScreenMessageStyle.UPPER_LEFT);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - TakeSoil: " + ex.Message + ex.StackTrace);
            }
        } 

    }
}
