using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AnchorRework
{
    [HarmonyPatch(typeof(SaveLoadManager))]
    internal class SaveLoadPatches
    {

/*        [HarmonyPostfix]
        [HarmonyPatch("LoadModData")]
        public static void LoadPatch()
        {
            foreach(var anchor in Main.boatAnchors) 
            {
                anchor.LoadAnchorData();
            }
        }*/

        [HarmonyPrefix]
        [HarmonyPatch("SaveModData")]
        public static void SavePatch()
        {
            foreach(var anchor in Main.boatAnchors)
            {
                anchor.SaveAnchorData();
            }
        }
        
    }
}
