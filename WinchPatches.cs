using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AnchorRework
{
    internal static class WinchPatches
    {
        [HarmonyPatch(typeof(GPButtonRopeWinch))]
        [HarmonyPostfix]
        [HarmonyPatch("FindBoat")]
        public static void FindBoatPatch(GPButtonRopeWinch __instance, PurchasableBoat ___boat, ref float ___gearRatio, ref float ___rotationSpeed, RopeController ___rope)
        {
            if (___rope is RopeControllerAnchor)
            {
                if (___boat.transform.name.Contains("medi medium"))
                {
                    __instance.transform.localScale = new Vector3(2.0f, 2.0f, 1.5f);
                }
                ___gearRatio = 25f;
                ___rotationSpeed = 8f;
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("Update")]
        public static void Postfix(GPButtonRopeWinch __instance, GoPointer ___stickyClickedBy, bool ___isLookedAt, ref string ___description, RopeController ___rope, ref string ___lookText)
        {
            if (___isLookedAt || (bool)___stickyClickedBy)
            {
                if (___rope is RopeControllerAnchor rope)
                {
                    string text = "";
                    float len = Mathf.Round(rope.joint.linearLimit.limit);
                    if (Main.winchInfo.Value)
                    {
                        text = len + " yd";

                    }
                    //___lookText = System.Math.Round(angleReadout, 2) + " degrees";
                    //if (rope.joint.currentForce.magnitude > rope.joint.gameObject.GetComponent<Anchor>().unsetForce * 0.5) __instance.enableRedOutline = true;
                    /*if (Main.advancedInfo.Value)
                    {
                        float spring = Mathf.Round(rope.joint.linearLimitSpring.spring);
                        float dist = Mathf.Round(Vector3.Distance(rope.joint.GetComponent<PickupableBoatAnchor>().GetTopAttach().position, rope.joint.transform.position));
                        float ang = Mathf.Round(Vector3.Angle(rope.joint.GetComponent<PickupableBoatAnchor>().GetTopAttach().position - rope.joint.transform.position, rope.joint.transform.root.up));
                        float power = Mathf.Round(rope.joint.gameObject.GetComponent<Anchor>().unsetForce);
                        float tensPercent = Mathf.Round(100 * (rope.joint.currentForce.magnitude / power));

                        text = "length: " + len + "\ndistance: " + dist + "\nangle: " + ang + "\u00B0\nspring: " + spring + "\npower: " + power + "\ntension: " + tensPercent + "%";
                    }*/
                    ___description = text;
                }

            }
        }
    }
}
