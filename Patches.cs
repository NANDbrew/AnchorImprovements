using HarmonyLib;
using UnityEngine;
using System.Collections;

namespace AnchorRework
{
    internal class Patches
    {
        [HarmonyPatch(typeof(GoPointer))]
        public static class Thrower
        {
            static float currentThrowPower;
            static float initialHoldDist;
            [HarmonyPatch("PickUpItem")]
            [HarmonyPostfix]
            private static void PickUpItem(PickupableItem item)
            {
                initialHoldDist = item.holdDistance;
            }

            [HarmonyPatch("LateUpdate")]
            [HarmonyPostfix]
            private static void LateUpdate(GoPointer __instance, PickupableItem ___heldItem, float ___timerAfterPickup, GoPointerButton ___pointedAtButton, float ___throwDelay)
            {
                if ((bool)___heldItem && ___timerAfterPickup >= 0.66f && ___pointedAtButton == null && ___heldItem.GetComponent<ShipItem>() == null && ___heldItem.GetComponent<Rigidbody>() is Rigidbody component)
                {
                    if (GameInput.GetKey(InputName.Throw) || (Settings.autoThrow && GameInput.GetKey(InputName.PickUp)))
                    {
                        currentThrowPower += Time.deltaTime;
                        if (currentThrowPower > 1f)
                        {
                            currentThrowPower = 1f;
                        }
                        float num2 = Mathf.Lerp(0, 0.5f, currentThrowPower);
                        ___heldItem.transform.Translate(__instance.transform.forward * (0f - num2), Space.World);
                        ___heldItem.holdDistance = initialHoldDist - num2;
                    }
                    else if (GameInput.GetKeyUp(InputName.Throw) || GameInput.GetKeyUp(InputName.PickUp))
                    {
                        if (GameInput.GetKeyUp(InputName.Throw))
                        {
                            ___heldItem.OnDrop();
                            ___heldItem.holdDistance = initialHoldDist;
                            __instance.DropItem();
                            if (currentThrowPower > ___throwDelay)
                            {
                                __instance.StartCoroutine(ThrowItemAfterDelay(__instance, component, currentThrowPower - ___throwDelay));
                            }

                        }

                        currentThrowPower = 0f;
                    }
                }
            }

            private static IEnumerator ThrowItemAfterDelay(GoPointer parent, Rigidbody heldRigidbody, float force)
            {
                yield return new WaitForFixedUpdate();
                if (force > 1f)
                {
                    force = 1f;
                }

                heldRigidbody.AddForce(parent.transform.forward * parent.throwForce * force * heldRigidbody.mass);
            }
        }


        [HarmonyPatch(typeof(RopeControllerAnchor))]
        private static class RopeControllerAnchorPatches
        {
/*            [HarmonyPrefix]
            [HarmonyPatch("ResetAnchor")]
            public static void ResetPatch(RopeControllerAnchor __instance)
            {
                __instance.joint.transform.position = __instance.transform.position + Vector3.down * 0.5f;

            }*/
            
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPatch(RopeControllerAnchor __instance, ref float ___maxLength)
            {

                ___maxLength *= 2f;


            }
        }
    }
}
