using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using AnchorRework.Fixes;

namespace AnchorRework
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("pr0skynesis.sailinfo", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.nandbrew.nandfixes", BepInDependency.DependencyFlags.SoftDependency)]

    internal class Main : BaseUnityPlugin
    {
        public const string GUID = "com.nandbrew.anchorimprovements";
        public const string NAME = "Anchor Improvements";
        public const string VERSION = "2.0.0";

        internal static ManualLogSource logSource;

        internal static ConfigEntry<bool> winchInfo;
        internal static ConfigEntry<bool> brigResize;
        internal static ConfigEntry<bool> fixes;

        public void Awake()
        {
            bool sailInfo = Chainloader.PluginInfos.ContainsKey("pr0skynesis.sailinfo");
            bool nandFixes = Chainloader.PluginInfos.ContainsKey("com.nandbrew.nandfixes");
            string infoDesc = sailInfo ? "Disabled while SailInfo is installed. Use SailInfo's anchorOutText instead" : "Show how many yards of rope are out when looking at anchor winch";
            string fixesDesc = nandFixes ? "Disabled while NANDfixes is installed" : "Fix held anchor pulling boat. Fix unpurchased anchors saving position. Fix Jong anchor starting position";

            logSource = Logger;
            Harmony harmony = new Harmony(GUID);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);

            winchInfo = Config.Bind("Options", "Winch info", true, new ConfigDescription(infoDesc));
            fixes = Config.Bind("Options", "Anchor fixes", true, new ConfigDescription(fixesDesc));
            brigResize = Config.Bind("Options", "Resize brig anchor", true, new ConfigDescription("Doubles the size of the brig's anchor and windlass\nREQUIRES A RELOAD"));

#if DEBUG
            advancedInfo = Config.Bind("Options", "Advanced info", true, new ConfigDescription("Show extra info when looking at windlass"));
#endif
            MethodInfo original1 = AccessTools.Method(typeof(SaveableObject), "UnpurchasedBoatOrMooringRope");
            MethodInfo patch1 = AccessTools.Method(typeof(AnchorPatch), "Postfix");
            MethodInfo original2 = AccessTools.Method(typeof(Anchor), "Awake");
            MethodInfo patch2 = AccessTools.Method(typeof(AnchorPatch2), "Awake");
            MethodInfo original3 = AccessTools.Method(typeof(Anchor), "ExtraFixedUpdate");
            MethodInfo patch3 = AccessTools.Method(typeof(AnchorPatch3), "ExtraFixedUpdate");
            if (!nandFixes)
            {
                harmony.Patch(original1, new HarmonyMethod(patch1));
                harmony.Patch(original2, new HarmonyMethod(patch2));
                harmony.Patch(original3, new HarmonyMethod(patch3));
            }



            MethodInfo original15 = AccessTools.Method(typeof(GPButtonRopeWinch), "Update");
            MethodInfo patch15 = AccessTools.Method(typeof(WinchTextPatch), "UpdatePatch");
            if (!sailInfo)
            {
                harmony.Patch(original15, new HarmonyMethod(patch15));
            }
        }

    }

}