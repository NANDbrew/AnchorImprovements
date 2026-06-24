using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AnchorRework.Fixes
{

    //[HarmonyPatch(typeof(SaveableObject), "UnpurchasedBoatOrMooringRope")]
    public static class AnchorPatch
    {
        private static void Postfix(SaveableObject __instance, bool extraSetting, ref bool __result)
        {
            if (!Main.fixes.Value) return;
            if ((bool)__instance.anchor)
            {
                var boatSaveable = (SaveableObject)AccessTools.Field(typeof(Anchor), "boatSaveable").GetValue(__instance.anchor);
                if (!boatSaveable.extraSetting) __result = true;
            }

        }
    }
    //[HarmonyPatch(typeof(Anchor))]
    public static class AnchorPatch2
    {
        //[HarmonyPatch("Awake")]
        //[HarmonyPrefix]
        public static void Awake(Anchor __instance)
        {
            if (!Main.fixes.Value) return;
            if (__instance.GetComponent<ConfigurableJoint>() is ConfigurableJoint joint)
            {
                __instance.transform.parent = joint.connectedBody.transform.GetChild(0);
            }
        }
    }

    //[HarmonyPatch(typeof(Anchor))]
    public static class AnchorPatch3
    {

        //[HarmonyPatch("ExtraFixedUpdate")]
        //[HarmonyPostfix]
        public static void ExtraFixedUpdate(Anchor __instance, GoPointer ___held, RopeControllerAnchor ___rope, ConfigurableJoint ___joint)
        {
            if (!Main.fixes.Value) return;
            if ((bool)___held)
            {
                float value = Vector3.Distance(___joint.connectedBody.transform.TransformPoint(___joint.connectedAnchor), __instance.transform.position);
                float num = Mathf.InverseLerp(0f, ___rope.maxLength, value);
                if (num > ___rope.currentLength - 0.01f)
                {
                    ___rope.canPull = false;
                    if (num > ___rope.currentLength)
                    {
                        ___rope.currentLength = num;
                    }
                }

            }
        }
    }


}
