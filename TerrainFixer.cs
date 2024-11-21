using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace AnchorRework
{
    [HarmonyPatch(typeof(IslandHorizon), "Start")]
    internal static class TerrainFixer
    {
        [HarmonyPostfix]
        public static void Postfix(IslandHorizon __instance)
        {
            foreach (Transform child in __instance.GetComponentsInChildren<Transform>())
            {
                if (child.CompareTag("Untagged") && (child.name.ToLower().Contains("terrain")))
                {
                    child.tag = "Terrain";
                }
            }
        }
    }
}
