using BepInEx.Logging;
using HarmonyLib;
using SailwindModdingHelper;
using UnityEngine;
using static ONSPPropagationMaterial;

namespace AnchorRework
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Anchor))]
        private static class AnchorPatch
        {

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPatch2(Anchor __instance, ref float ___initialMass, ConfigurableJoint ___joint)
            {
                __instance.gameObject.layer = 0;

                __instance.gameObject.AddComponent<PickupableBoatAnchor>();
                if (___joint.connectedBody.name.Contains("medi medium"))
                {
                    __instance.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                }
                if (___initialMass == 1f) ___initialMass = 75f;
                //__instance.unsetForce = ___initialMass * 150f;
                SoftJointLimitSpring spring = ___joint.linearLimitSpring;
                spring.damper = 2000f;
                spring.spring = 2000f;
                ___joint.linearLimitSpring = spring;
            }


            [HarmonyPrefix]
            [HarmonyPatch("ReleaseAnchor")]
            public static bool Prefix(Anchor __instance, ConfigurableJoint ___joint, float ___unsetForce, AudioSource ___audio, float ___lastLength)
            {
                //Main.logSource.LogInfo("Anchor Released");

                Vector3 bottomAttach = ___joint.transform.position;
                Vector3 topAttach;
                topAttach = ___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position;
                float angle = Vector3.Angle(topAttach - bottomAttach, ___joint.transform.root.up);

                //float spring = Mathf.Max(5000 - (angle + ___joint.linearLimit.limit) * 30, 1000);

                if (angle < 45f && ___joint.linearLimit.limit + 0.01f < ___lastLength)
                {
                    Debug.Log("anchor line was < 45");

                    return true;
                }
                if (___joint.currentForce.magnitude >= ___unsetForce)
                {
                    Debug.Log("anchor broke free");

                    return true;
                }

                if (__instance.GetComponent<PickupableBoatAnchor>().held)
                {
                    Debug.Log("anchor is held");

                    return true;
                }
                return false;

            }
        }

        [HarmonyPatch(typeof(GPButtonRopeWinch))]
        private static class WinchPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("FindBoat")]
            public static void FindBoatPatch(GPButtonRopeWinch __instance, PurchasableBoat ___boat, ref float ___gearRatio, ref float ___rotationSpeed)
            {
                if (__instance.name.Contains("anchor"))
                {
                    if (___boat.transform.name.Contains("medi medium"))
                    {
                        __instance.transform.localScale = new Vector3(2.0f, 2.0f, 1.5f);
                    }
                    ___gearRatio = 25f;
                    ___rotationSpeed = 8f;
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void Postfix(GoPointer ___stickyClickedBy, bool ___isLookedAt, ref string ___description, RopeController ___rope, ref string ___lookText)
            {
                if (___isLookedAt || ___stickyClickedBy)
                {
                    if (___rope is RopeControllerAnchor rope)
                    {
                        ___description = Mathf.RoundToInt(rope.joint.linearLimit.limit).ToString() + " yd";
                        //___lookText = Mathf.RoundToInt(Vector3.Distance(rope.joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position, rope.joint.transform.position)).ToString();

                    }
                }
            }
        }
        [HarmonyPatch(typeof(RopeControllerAnchor))]
        private static class RopeControllerAnchorPatches
        {
/*            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPatch(ref float ___maxLength)
            {
                ___maxLength = 75f;

            }*/

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void Postfix(ConfigurableJoint ___joint, ref float ___currentResistance)
            {
                if (___joint.GetComponent<Anchor>().IsSet())
                {
                    ___currentResistance = Mathf.Max(___joint.currentForce.magnitude, 10f);
                }
                else
                {
                    ___currentResistance = Mathf.Min(___joint.currentForce.magnitude / 10f, 10f);

                }
            }
        }
    }
}
