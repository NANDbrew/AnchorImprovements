using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AnchorRework
{
    internal class LookTextPatch
    {
        [HarmonyPatch(typeof(LookUI), "ShowLookText")]
        private static class ControlHintPatch
        {
            public static void Postfix(LookUI __instance, GoPointer ___pointer, GoPointerButton button, TextMesh ___controlsText, TextMesh ___textLicon, TextMesh ___textRIcon, bool ___altIconsOn, bool ___showingIcon)
            {
                //___textLicon.text = "";
                if (button is PickupableBoatAnchor)
                {
                    ___controlsText.text = "pick up\n";
                    AccessTools.Method(__instance.GetType(), "ShowLicon").Invoke(__instance, new object[0]);
                }

            }
        }
    }
}
