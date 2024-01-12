using BepInEx.Logging;
using HarmonyLib;
using SailwindModdingHelper;
using UnityEngine;
using static ONSPPropagationMaterial;

namespace AnchorRework
{
    internal class Patches
    {
 /*       private static float curve = 37f;
        private static float newCurve = 0.172574626866f;

        [HarmonyPatch(typeof(Anchor))]
        private static class AnchorPatch
        {
*//*            [HarmonyPostfix]
            [HarmonyPatch("OnCollisionEnter")]
            public static void OnCollisionEnterPatch(Collision collision, ConfigurableJoint ___joint, AudioSource ___audio)
            {
                if (collision.collider.CompareTag("Terrain") && ___joint.linearLimit.limit > 1f)
                {
                    if ((bool)___audio)
                    {
                        ___audio.enabled = true;
                        ___audio.pitch = 0.5f;
                        ___audio.Play();
                    }
                }
            }*//*

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
                //__instance.unsetForce = ___initialMass * 150f;
            }

            [HarmonyPrefix]
            [HarmonyPatch("FixedUpdate")]
            public static bool FixedUpdatePatch(Anchor __instance, ConfigurableJoint ___joint, float ___unsetForce, AudioSource ___audio, ref float ___lastLength, ref bool ___grounded, Rigidbody ___body, float ___anchorDrag, float ___initialMass, ref float ___outCurrentForce)
            {
                //Vector3 bottomAttach = ___joint.transform.position;
                
                //Vector3 topAttach = ___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position;
                //float angle = Vector3.Angle(topAttach - bottomAttach, ___joint.transform.root.up);

                float angle = Vector3.Angle(___joint.currentForce - ___joint.transform.position, ___joint.transform.root.up);


                if (___joint.currentForce.magnitude > ___unsetForce * (newCurve * Mathf.Exp(angle / curve) - newCurve) && !___audio.isPlaying)
                {
                    __instance.InvokePrivateMethod("ReleaseAnchor");
                }

                if (___grounded)
                {
                    if (___body.drag < ___anchorDrag)
                    {
                        ___body.drag += Time.deltaTime * ___anchorDrag * (angle / 30f);
                    }
                    if (___body.drag >= ___anchorDrag && ___body.velocity.sqrMagnitude < 5)
                    {
                        ___body.drag = ___anchorDrag;
                        if (!___body.isKinematic && !___audio.isPlaying)
                        {
                            __instance.InvokePrivateMethod("SetAnchor");
                        }
                    }
                }
                else if (__instance.transform.position.y < 0f)
                {
                    if (___body.drag > 3f)
                    {
                        ___body.drag -= Time.deltaTime * ___anchorDrag * 0.25f;
                    }
                    else
                    {
                        ___body.drag = 3f;
                    }
                }

                if (___joint.linearLimit.limit < 1f)
                {
                    ___body.mass = 0.2f;
                }
                else
                {
                    ___body.mass = ___initialMass;
                }

                if ((bool)GameState.currentBoat && ___joint.connectedBody.transform == GameState.currentBoat.parent)
                {
                    ___body.interpolation = RigidbodyInterpolation.Interpolate;
                }
                else
                {
                    ___body.interpolation = RigidbodyInterpolation.None;
                }

                if ((bool)___audio && ___audio.enabled && !___audio.isPlaying)
                {
                    ___audio.enabled = false;
                }

                ___lastLength = ___joint.linearLimit.limit;
                ___outCurrentForce = ___joint.currentForce.magnitude;

                return false;
            }

        
        }*/

        [HarmonyPatch(typeof(GPButtonRopeWinch))]
        private static class WinchPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void Postfix(bool ___stickyClickedBy, bool ___isLookedAt, ref string ___description, RopeController ___rope, ref string ___lookText)
            {

                    if (___rope is RopeControllerAnchor rope)
                    {
                        ___description = Mathf.RoundToInt(rope.joint.linearLimit.limit).ToString() + " yds";
                        ___lookText = Mathf.RoundToInt(Vector3.Distance(rope.joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position, rope.joint.transform.position)).ToString();

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
                ___maxLength *= 2;

            }*/

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void Postfix(ConfigurableJoint ___joint, ref float ___currentResistance)
            {
                if (___joint.GetComponent<Anchor>().IsSet())
                {
                    ___currentResistance = Mathf.Max(___joint.currentForce.magnitude, 20f);
                }
                else
                {
                    ___currentResistance = Mathf.Min(___joint.currentForce.magnitude / 10f, 20f);

                }
            }
        }
    }
}
