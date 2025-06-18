using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ONSPPropagationMaterial;

namespace AnchorRework
{
    [HarmonyPatch(typeof(GPButtonRopeWinch), "Awake")]
    public static class WinchStartPatch
    {
        public static void Postfix(GPButtonRopeWinch __instance, PurchasableBoat ___boat, ref float ___gearRatio, ref float ___rotationSpeed, RopeController ___rope)
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
    }
    internal static class WinchTextPatch
    {
        public static void UpdatePatch(GPButtonRopeWinch __instance, GoPointer ___stickyClickedBy, bool ___isLookedAt, ref string ___description, RopeController ___rope)
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
#if DEBUG
                    if (Main.advancedInfo.Value)
                    {
                        float spring = Mathf.Round(rope.joint.linearLimitSpring.spring);
                        float dist = Mathf.Round(Vector3.Distance(rope.joint.connectedBody.transform.TransformPoint(rope.joint.connectedAnchor), rope.joint.transform.position));
                        float ang = Mathf.Round(Vector3.Angle(rope.joint.connectedBody.transform.TransformPoint(rope.joint.connectedAnchor) - rope.joint.transform.position, Vector3.up));
                        float power = Mathf.Round(rope.joint.gameObject.GetComponent<Anchor>().unsetForce);
                        float tensPercent = Mathf.Round(100 * (rope.joint.currentForce.magnitude / power));
                        string color = tensPercent > 80 ? "#7C0000" : "#113905";
                        text = "length: " + len + "\ndistance: " + dist + "\nangle: " + ang + "\u00B0\nspring: " + spring + "\npower: " + power + "<color="+ color + ">\ntension: " + tensPercent + "%</color>";
                    }
#endif
                    ___description = text;
                }

            }
        }
    }
}
