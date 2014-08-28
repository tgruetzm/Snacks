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
    class SnackConfiguration
    {

        private ConfigNode node;
        private double snacksPerMeal;
        private double lossPerDayPerKerbal;
        private int snackResourceId;
        private int mealsPerDay = 1;
        public double SnacksPerMeal
        {
            get { return snacksPerMeal; }
        }
        public double LossPerDay
        {
            get { return lossPerDayPerKerbal; }
        }
        public int SnackResourceId
        {
            get { return snackResourceId; }
        }
        public int MealsPerDay
        {
            get { return mealsPerDay; }
        }

        private SnackConfiguration()
        {
            PartResourceDefinition snacksResource = PartResourceLibrary.Instance.GetDefinition("Snacks");
            snackResourceId = snacksResource.id;
            string file = IOUtils.GetFilePathFor(this.GetType(), "snacks.cfg");
            Debug.Log("loading file:" + file);
            node = ConfigNode.Load(file).GetNode("SNACKS");
            snacksPerMeal = double.Parse(node.GetValue("snacksPerMeal"));
            lossPerDayPerKerbal = double.Parse(node.GetValue("repLossPercent"));
            mealsPerDay = int.Parse(node.GetValue("mealsPerDay"));
            Debug.Log("snacksPerMeal:" + snacksPerMeal + "mealsPerDay:" + mealsPerDay);
        
        }

        private static SnackConfiguration snackConfig;

        public static SnackConfiguration Instance()
        {
            if(snackConfig == null)
                snackConfig = new SnackConfiguration();
            return snackConfig;
        }
    }
}
