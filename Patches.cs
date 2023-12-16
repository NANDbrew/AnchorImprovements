using BepInEx.Logging;
using HarmonyLib;
using SailwindModdingHelper;
using UnityEngine;

namespace AnchorRework
{
    internal class Patches
    {
        [HarmonyPatch(typeof(Anchor))]
        private static class AnchorPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            public static void StartPatch(Anchor __instance)
            {
                __instance.gameObject.layer = 0;

                __instance.gameObject.AddComponent<PickupableBoatAnchor>();
                //Main.logSource.LogError("Anchor Start");
            }
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPatch2(Anchor __instance, ref float ___initialMass)
            {
                if (___initialMass == 1f) ___initialMass = 75f;
                __instance.unsetForce = ___initialMass * 20f;
            }
            [HarmonyPrefix]
            [HarmonyPatch("ReleaseAnchor")]
            public static bool Prefix(Anchor __instance, ConfigurableJoint ___joint, float ___unsetForce, AudioSource ___audio, float ___lastLength)
            {
                //Main.logSource.LogInfo("Anchor Released");

                Vector3 bottomAttach = ___joint.transform.position;
                Vector3 topAttach;
                float angle;
                if (___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne"))
                {
                    topAttach = ___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position;
                    Vector3 vec3 = topAttach - bottomAttach;
                    angle = Vector3.Angle(vec3, ___joint.transform.root.up);
                    if (___joint.linearLimit.limit + 0.01f < ___lastLength && angle < 45f)
                    {   
                        //ModLogger.Log(Main.mod, angle.ToString());

                        return true;
                    }
                }
                if (___joint.currentForce.magnitude >= ___unsetForce && !___audio.isPlaying)
                {
                        //ModLogger.Log(Main.mod, "exceeded force");

                        return true;
                }

                if (__instance.GetComponent<PickupableBoatAnchor>().held)
                {
                        //ModLogger.Log(Main.mod, "picked up");

                        return true;
                }
                return false;
            }
/*            [HarmonyPostfix]
            [HarmonyPatch("OnCollisionEnter")]
            public static void CollisionenterPatch(Collision collision, Anchor __instance, ConfigurableJoint ___joint, ref bool ___grounded)
            {
                if (collision.collider.CompareTag("Boat") && ___joint.linearLimit.limit > 1f)
                {
                    __instance.transform.SetParent(collision.transform.parent);
                    ___grounded = true;
                }
            }*/
        }
    }
}
