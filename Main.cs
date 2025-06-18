using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;

namespace AnchorRework
{
    [BepInPlugin(GUID, NAME, VERSION)]
    //[BepInDependency("com.app24.sailwindmoddinghelper", "2.0.3")]

    internal class Main : BaseUnityPlugin
    {
        public const string GUID = "com.nandbrew.anchorimprovements";
        public const string NAME = "Anchor Improvements";
        public const string VERSION = "1.1.8";

        internal static ManualLogSource logSource;

        internal static ConfigEntry<PhysicsType> simplePhysics;
        internal static ConfigEntry<bool> saveAnchorPosition;
        internal static ConfigEntry<bool> winchInfo;
        internal static ConfigEntry<bool> advancedInfo;

        internal static List<PickupableBoatAnchor> boatAnchors = new List<PickupableBoatAnchor>();

        public void Awake()
        {
            bool sailInfo = Chainloader.PluginInfos.ContainsKey("pr0skynesis.sailinfo");
            string infoDesc = sailInfo ? "Disabled while SailInfo is installed. Use SailInfo's anchorOutText instead" : "Show how many yards of rope are out when looking at anchor winch";
            
            logSource = Logger;
            Harmony harmony = new Harmony(GUID);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
            
            simplePhysics = Config.Bind("Options", "Anchor mechanics", PhysicsType.Simple, new ConfigDescription("Simple: Normal with minor improvements \nRealistic: Holding power based on scope (angle)"));
            saveAnchorPosition = Config.Bind("Options", "Save anchor position", true, new ConfigDescription(""));
            winchInfo = Config.Bind("Options", "Winch info", true, new ConfigDescription(infoDesc));

#if DEBUG
            advancedInfo = Config.Bind("Options", "Advanced info", true, new ConfigDescription("Show extra info when looking at windlass"));
#endif
            MethodInfo original15 = AccessTools.Method(typeof(GPButtonRopeWinch), "Update");
            MethodInfo patch15 = AccessTools.Method(typeof(WinchTextPatch), "UpdatePatch");
            if (!sailInfo)
            {
                harmony.Patch(original15, new HarmonyMethod(patch15));
            }
        }

    }

    internal enum PhysicsType
    {
        Simple,
        Realistic
    }
}