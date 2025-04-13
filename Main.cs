using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;

namespace AnchorRework
{
    [BepInPlugin(GUID, NAME, VERSION)]
    //[BepInDependency("com.app24.sailwindmoddinghelper", "2.0.3")]

    internal class Main : BaseUnityPlugin
    {
        public const string GUID = "com.nandbrew.anchorimprovements";
        public const string NAME = "Anchor Improvements";
        public const string VERSION = "1.1.6";

        internal static ManualLogSource logSource;

        internal static ConfigEntry<PhysicsType> simplePhysics;
        internal static ConfigEntry<bool> saveAnchorPosition;
        internal static ConfigEntry<bool> winchInfo;

        internal static List<PickupableBoatAnchor> boatAnchors = new List<PickupableBoatAnchor>();

        public void Awake()
        {
            logSource = Logger;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);

            simplePhysics = Config.Bind("Options", "Anchor mechanics", PhysicsType.Simple, new ConfigDescription("Simple: Normal with minor improvements \nRealistic: Holding power based on scope (angle)"));
            saveAnchorPosition = Config.Bind("Options", "Save anchor position", true, new ConfigDescription(""));
            winchInfo = Config.Bind("Options", "Winch info", true, new ConfigDescription("Show how many yards of rope are out when looking at windlass"));
            //advancedInfo = Config.Bind("Options", "Advanced info", true, new ConfigDescription("Show extra info when looking at windlass"));
        }

    }

    internal enum PhysicsType
    {
        Simple,
        Realistic
    }
}