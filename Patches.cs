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
                if (!__instance.GetComponent<PickupableBoatAnchor>())
                {
                    __instance.gameObject.AddComponent<PickupableBoatAnchor>();
                }
            }
            
            [HarmonyPrefix]
            [HarmonyPatch("ReleaseAnchor")]
            public static bool Prefix(Anchor __instance, ConfigurableJoint ___joint, float ___unsetForce, AudioSource ___audio, float ___lastLength)
            {
                Vector3 bottomAttach = ___joint.transform.position;
                Vector3 topAttach;
                float angle;
                if (___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne"))
                {
                    topAttach = ___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position;
                    Vector3 vec3 = topAttach - bottomAttach;
                    angle = Vector3.Angle(vec3, ___joint.transform.root.up);
                    if (___joint.linearLimit.limit + 0.01f < ___lastLength && angle < 45)
                    {
                        return true;
                    }
                }
                if (___joint.currentForce.magnitude >= ___unsetForce && !___audio.isPlaying)
                {
                    return true;
                }

                if (__instance.GetComponent<PickupableBoatAnchor>().held)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
