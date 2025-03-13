using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Microsoft.Win32;
//using SailwindModdingHelper;
using System;
using UnityEngine;

namespace AnchorRework
{
    internal class Patches
    {
        public static object InvokePrivate(object obj, string name)
        {
            return AccessTools.Method(obj.GetType(), name).Invoke(obj, null);
        }

        [HarmonyPatch(typeof(Anchor))]
        private static class AnchorPatches
        {
            private static readonly GameObject anchorStock = new GameObject { name = "anchor_stock" };
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPatch2(Anchor __instance, ref float ___initialMass, AudioSource ___audio, ref float ___anchorDrag, ConfigurableJoint ___joint)
            {
                __instance.gameObject.layer = 0;

                __instance.gameObject.AddComponent<PickupableBoatAnchor>();

                GameObject gameObject = UnityEngine.GameObject.Instantiate(anchorStock, __instance.transform);
                gameObject.transform.localPosition = new Vector3(0f, 0f, -0.28f);
                CapsuleCollider stockCol = gameObject.AddComponent<CapsuleCollider>();
                stockCol.radius = 0.05f;
                stockCol.height = 0.6f;

                if (___joint.connectedBody.name.Contains("medi medium"))
                {
                    __instance.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                }

                ___anchorDrag += 5;
                ___audio.maxDistance *= 3f;
                if (___initialMass == 1f || ___initialMass == 75f) ___initialMass = 100f;
            }
        }

        #region complex_physics
        [HarmonyPatch(typeof(Anchor))]
        private static class AnchorPatchesComplex
        {
            static float powerDivisor = 64f;
            private static float Power(float angle)
            {
                return Mathf.Pow(angle, 2) / powerDivisor; // 64 is 100% power at 80 degrees, increase for less peak power
            }

            [HarmonyPrefix]
            [HarmonyPatch("FixedUpdate")]
            public static bool FixedUpdatePatch(Anchor __instance, ConfigurableJoint ___joint, ref float ___unsetForce, AudioSource ___audio, ref float ___lastLength, ref bool ___grounded, Rigidbody ___body, float ___anchorDrag, float ___initialMass, ref float ___outCurrentForce)
            {
                if (Main.simplePhysics.Value != PhysicsType.Realistic) return true;
                if (!GameState.playing || !___joint.connectedBody.GetComponentInChildren<BoatHorizon>().closeToPlayer) return true;

                if (___joint.linearLimit.limit > 1f)
                {
                    float power2;

                    ___body.mass = ___initialMass;

                    Vector3 bottomAttach = ___joint.transform.position;
                    Vector3 topAttach = ___joint.connectedBody.transform.TransformPoint(___joint.connectedAnchor);
                    float angle1 = Vector3.Angle(topAttach - bottomAttach, ___joint.transform.root.up);
                    if (angle1 < 90) power2 = Power(angle1);
                    else power2 = Power(-angle1 % 90);

                    ___unsetForce = ___initialMass * power2;

                    SoftJointLimitSpring spring = ___joint.linearLimitSpring;
                    spring.spring = Mathf.Max(5000 - (power2 + ___joint.linearLimit.limit) * 20, ___initialMass * 10);
                    spring.damper = spring.spring;
                    ___joint.linearLimitSpring = spring;

                    SoftJointLimit limit = ___joint.linearLimit;
                    limit.contactDistance = (power2 + limit.limit) / 30;
                    ___joint.linearLimit = limit;


                    if (___joint.currentForce.magnitude > ___unsetForce && !___audio.isPlaying)
                    {
                        InvokePrivate(__instance, "ReleaseAnchor");
                        //Debug.Log("anchor unset. angle was: " + angle);
                    }
                    if (___joint.linearLimit.limit + 0.01 < ___lastLength && angle1 < 20)
                    {
                        InvokePrivate(__instance, "ReleaseAnchor");
                    }


                    if (___grounded)
                    {
                        if (___body.drag < ___anchorDrag)
                        {
                            ___body.drag += Time.deltaTime * (angle1 / 45 + 0.1f);
                        }
                        else if (___body.velocity.sqrMagnitude < 5)
                        {
                            ___body.drag = ___anchorDrag;
                            if (!___body.isKinematic && !___audio.isPlaying)
                            {
                                InvokePrivate(__instance, "SetAnchor");
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
                }
                else
                {
                    ___body.mass = 1f;
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
        }
        #endregion

        #region simple_physics
        [HarmonyPatch(typeof(Anchor))]
        private static class AnchorPatchesSimple
        { 
            [HarmonyPatch("FixedUpdate")]
            [HarmonyPostfix]
            public static void FixedUpdatePatchSimple(Anchor __instance, Rigidbody ___body, bool ___grounded, float ___anchorDrag, AudioSource ___audio, ref ConfigurableJoint ___joint)
            {
                if (Main.simplePhysics.Value != PhysicsType.Simple) return;
                if (!GameState.playing || !___joint.connectedBody.GetComponentInChildren<BoatHorizon>().closeToPlayer) return;

                SoftJointLimitSpring spring = ___joint.linearLimitSpring;
                spring.damper = 2000f;
                spring.spring = 2000f;
                ___joint.linearLimitSpring = spring;

                if (___grounded && ___body.drag >= ___anchorDrag && ___body.velocity.sqrMagnitude < 1f)
                {
                    bool isColliding = __instance.GetComponent<PickupableBoatAnchor>().isColliding;
                    ___body.drag = ___anchorDrag;
                    if (!___body.isKinematic && !___audio.isPlaying && isColliding)
                    {
                        InvokePrivate(__instance, "SetAnchor");
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("ReleaseAnchor")]
            public static bool Prefix(Anchor __instance, ConfigurableJoint ___joint, float ___unsetForce, AudioSource ___audio, float ___lastLength)
            {
                //Main.logSource.LogInfo("Anchor Released");
                if (Main.simplePhysics.Value != PhysicsType.Simple) return true;

                Vector3 bottomAttach = ___joint.transform.position;
                Vector3 topAttach = ___joint.connectedBody.transform.TransformPoint(___joint.connectedAnchor);
                float angle = Vector3.Angle(topAttach - bottomAttach, ___joint.transform.root.up);

                if (angle < 45f && ___joint.linearLimit.limit + 0.01f < ___lastLength)
                {
#if DEBUG 
                    Debug.Log("anchor line was < 45"); 
#endif

                    return true;
                }
                if (___joint.currentForce.magnitude >= ___unsetForce)
                {
#if DEBUG
                    Debug.Log("anchor broke free");
#endif
                    return true;
                }

                if (__instance.GetComponent<PickupableBoatAnchor>().held)
                {
#if DEBUG
                    Debug.Log("anchor is held");
#endif
                    return true;
                }
                return false;
            }
        }
        #endregion

        [HarmonyPatch(typeof(RopeControllerAnchor))]
        private static class RopeControllerAnchorPatches
        {
/*            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPatch(ref float ___maxLength)
            {
                ___maxLength *= 3;

            }*/

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void UpdatePatch(RopeControllerAnchor __instance, ConfigurableJoint ___joint, ref float ___currentResistance, ref float ___maxLength)
            {
                if (Main.simplePhysics.Value == PhysicsType.Simple)
                {
                    ___maxLength = 50f;
                }
                if (Main.simplePhysics.Value == PhysicsType.Realistic)
                {
                    ___maxLength = 150f;
                }

                //if (__instance.transform.parent != GameState.currentBoat) return;

                if (___joint.GetComponent<PickupableBoatAnchor>().isColliding || ___joint.GetComponent<Anchor>().IsSet())
                {
                    if (Vector3.Distance(___joint.connectedBody.transform.TransformPoint(___joint.connectedAnchor), ___joint.transform.position) >= ___joint.linearLimit.limit)
                    {
                        ___currentResistance = Mathf.Max(___joint.currentForce.magnitude * 0.8f, 5f);
                    }
                    else ___currentResistance = 5f;
                    
                }
                else
                {
                    ___currentResistance = Mathf.Min(___joint.currentForce.magnitude / 10f, 10f);

                }
            }
        }
    }
}
