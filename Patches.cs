using BepInEx.Logging;
using HarmonyLib;
using SailwindModdingHelper;
using UnityEngine;
using static ONSPPropagationMaterial;

namespace AnchorRework
{
    internal class Patches
    {
        private static float curve = 37f;
        private static float slope = 4.3f;
        private static float newCurve = 0.172574626866f;

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
                //__instance.unsetForce = ___initialMass * 150f;
            }

/*            [HarmonyPostfix]
            [HarmonyPatch("FixedUpdate")]
            public static void FixedUpdatePatch(Rigidbody ___body, ConfigurableJoint ___joint, float ___anchorDrag, float ___anchorDragUp)
            {
                if (___joint.linearLimit.limit < 2f)
                {
                    ___body.drag = ___anchorDragUp;
                }
                else
                {
                    ___body.drag = ___anchorDrag;
                }
            }*/
 /*           [HarmonyPrefix]
            [HarmonyPatch("FixedUpdate")]
            public static bool FixedUpdatePatch(Anchor __instance, ConfigurableJoint ___joint, float ___unsetForce, AudioSource ___audio, ref float ___lastLength, ref bool ___grounded, Rigidbody ___body, float ___anchorDrag, float ___initialMass, ref float ___outCurrentForce)
            {
                if (___grounded)
                {
                    if (___body.drag < ___anchorDrag)
                    {
                        ___body.drag += Time.deltaTime * ___anchorDrag * 1.5f;
                    }

                    if (___body.drag > ___anchorDrag)
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
                Vector3 bottomAttach = ___joint.transform.position;
                Vector3 topAttach;
                float angle;
                topAttach = ___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position;
                angle = Vector3.Angle(topAttach - bottomAttach, ___joint.transform.root.up);

                if (___joint.currentForce.magnitude > ___unsetForce * (newCurve * Mathf.Exp(angle / curve) - newCurve) && !___audio.isPlaying)
                {
                    __instance.InvokePrivateMethod("ReleaseAnchor");
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
            }*/

            [HarmonyPrefix]
            [HarmonyPatch("ReleaseAnchor")]
            public static bool Prefix(Anchor __instance, ConfigurableJoint ___joint, float ___unsetForce, AudioSource ___audio, float ___lastLength)
            {
                //Main.logSource.LogInfo("Anchor Released");

                Vector3 bottomAttach = ___joint.transform.position;
                Vector3 topAttach;
                float angle;
                topAttach = ___joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position;
                angle = Vector3.Angle(topAttach - bottomAttach, ___joint.transform.root.up);
                if (angle < 45f && ___joint.linearLimit.limit < Vector3.Distance(topAttach, bottomAttach))
                {
                    Debug.Log("anchor line was < 45");

                    return true;
                }
                if (angle <= 75f && ___joint.currentForce.magnitude >= ___unsetForce)
                {
                    Debug.Log("anchor broke free <= 75");

                    return true;
                }
                if (angle > 75f && ___joint.currentForce.magnitude >= ___unsetForce * 1.5)
                {
                    Debug.Log("anchor broke free > 75");

                    return true;
                }
                /*if (___joint.currentForce.magnitude > ___unsetForce * (newCurve * Mathf.Exp(angle / curve) - newCurve))
                {
                    return true;
                }*/
                if (__instance.GetComponent<PickupableBoatAnchor>().held)
                {
                    Debug.Log("anchor is held");

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

        [HarmonyPatch(typeof(GPButtonRopeWinch))]
        private static class WinchPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void Postfix(bool ___stickyClickedBy, bool ___isLookedAt, ref string ___description, RopeController ___rope, ref string ___lookText)
            {
                if (___isLookedAt || ___stickyClickedBy)
                {
                    if (___rope is RopeControllerAnchor rope)
                    {
                        ___description = Mathf.RoundToInt(rope.joint.linearLimit.limit).ToString() + " yds";
                        ___lookText = Mathf.RoundToInt(Vector3.Distance(rope.joint.connectedBody.gameObject.GetComponent<BoatMooringRopes>().GetAnchorController().GetComponent<RopeEffect>().GetPrivateField<Transform>("attachmentOne").position, rope.joint.transform.position)).ToString();

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
