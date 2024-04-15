using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;

namespace AnchorRework
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.app24.sailwindmoddinghelper", "2.0.3")]

    internal class Main : BaseUnityPlugin
    {
        public const string GUID = "com.nandbrew.anchorimprovements";
        public const string NAME = "Anchor Improvements";
        public const string VERSION = "1.1.0";

        internal static Main instance;

        internal static ManualLogSource logSource;

        internal static ConfigEntry<bool> simplePhysics; 

        public void Awake()
        {
            logSource = Logger;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);

            simplePhysics = Config.Bind("options", "Simple Physics", true, new ConfigDescription(""));
        }
    }
}