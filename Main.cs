using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace AnchorRework
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.app24.sailwindmoddinghelper", "2.0.0")]

    internal class Main : BaseUnityPlugin
    {
        public const string GUID = "com.nandbrew.anchorimprovements";
        public const string NAME = "Anchor Improvements";
        public const string VERSION = "1.0.3";

        internal static Main instance;

        internal static ManualLogSource logSource;

        private void Awake()
        {
            logSource = Logger;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
        }
    }
}