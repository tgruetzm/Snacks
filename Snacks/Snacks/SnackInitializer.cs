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
using KSP.IO;

namespace Snacks
{
//[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    //not used currently
    class SnackInitializer : MonoBehaviour
    {

        private ConfigNode node;

        void Awake()
        {
            string file = IOUtils.GetFilePathFor(this.GetType(),"snacks.cfg");
            Debug.Log("loading file:" + file);
            node = ConfigNode.Load(file);
            if (node.GetValue("initialized") != "1")
            {
                foreach (ProtoVessel pv in HighLogic.CurrentGame.flightState.protoVessels)
                {
                    Debug.Log("loading:" + pv.vesselName);
                    pv.vesselRef.Load();
                    pv.vesselRef.Unload();
                    Debug.Log("Snacks initilized: " + pv.vesselName);
                }

                node.SetValue("initialized", "1");
            }

            Debug.Log("Snacks Initializer Complete:");
        }

    }
}
