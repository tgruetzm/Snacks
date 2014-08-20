using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class SupplyWindow : KSPPluginFramework.MonoBehaviourWindow
    {

        private Texture2D texture;
        private static ApplicationLauncherButton button;
        private Vector2 scrollPos = new Vector2();
 
        internal override void Awake()
        {
            WindowCaption = "Snack Supply";
            WindowRect = new Rect(0, 0, 250, 250);
            Visible = false;
            string textureName = "Snacks/Textures/snacks";
            texture = GameDatabase.Instance.GetTexture(textureName,false);
            GameEvents.onGUIApplicationLauncherReady.Add(SetupGUI);
        }

        private void SetupGUI()
        {
            if(button == null)
                button = ApplicationLauncher.Instance.AddModApplication(ShowGUI, HideGUI, null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER,texture);
        }


        private void ShowGUI()
        {
            Visible = true;
        }

        private void HideGUI()
        {
            Visible = false;
        }

        internal override void DrawWindow(int id)
        {
            DragEnabled = true;
            Vector2 pos = new Vector2();
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Height(250), GUILayout.Width(250));
            //GUILayout.BeginVertical();
            //GUILayout.BeginArea(new Rect(10,10,200,200));
            //GUILayout.ExpandHeight(true);

            var ships = from ProtoVessel v in HighLogic.CurrentGame.flightState.protoVessels
                        where v.GetVesselCrew().Count() > 0
                        group v by v.orbitSnapShot.ReferenceBodyIndex into g
                        select new { Body = g.Key ,Vessels = g };
                   
            foreach (var s in ships)
            {
                if (s.Body == 1)
                    GUILayout.Label("Kerbin:");
                else if (s.Body == 2)
                    GUILayout.Label("Mun:");
                foreach (var v in s.Vessels)
                {
                    double snackAmount = 0;
                    double snackMax = 0;
                    foreach (ProtoPartSnapshot pps in v.protoPartSnapshots)
                    {
                        var res = from r in pps.resources
                                  where r.resourceName == "Snacks"
                                  select r;
                        if (res.Count() > 0)
                        {
                            ConfigNode node = res.First().resourceValues;
                            snackAmount += Double.Parse(node.GetValue("amount"));
                            snackMax += Double.Parse(node.GetValue("maxAmount"));

                        }
                    }
                    GUILayout.Label(v.vesselName + ": " + Convert.ToInt32(snackAmount) + "/" + Convert.ToInt32(snackMax));
                }
            }

            //GUILayout.EndArea();
            GUILayout.EndScrollView();

  
            /*GUILayout.Label(new GUIContent("Window Contents", "Here is a reallly long tooltip to demonstrate the war and peace model of writing too much text in a tooltip\r\n\r\nIt even includes a couple of carriage returns to make stuff fun"));
            GUILayout.Label(String.Format("Drag Enabled:{0}", DragEnabled.ToString()));
            GUILayout.Label(String.Format("ClampToScreen:{0}", ClampToScreen.ToString()));
            GUILayout.Label(String.Format("Tooltips:{0}", TooltipsEnabled.ToString()));

            if (GUILayout.Button("Toggle Drag"))
                DragEnabled = !DragEnabled;
            if (GUILayout.Button("Toggle Screen Clamping"))
                ClampToScreen = !ClampToScreen;

            if (GUILayout.Button(new GUIContent("Toggle Tooltips", "Can you see my Tooltip?")))
                TooltipsEnabled = !TooltipsEnabled;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Tooltip Width");
            TooltipMaxWidth = Convert.ToInt32(GUILayout.TextField(TooltipMaxWidth.ToString()));
            GUILayout.EndHorizontal();
            GUILayout.Label("Width of 0 means no limit");

            GUILayout.Label("Alt+F11 - shows/hides window");*/

        }
        /*private void onDestroy()
        {
            Debug.Log("SupplyWindow destroyed");
            if (button != null)
                ApplicationLauncher.Instance.RemoveModApplication(button);
        }*/
    }



}
