using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Attributes;

namespace BCLDLL
{
    [BepInPlugin(PluginGUID, "BetterCrewLink API DLL", VersionString)]
    [BepInProcess("Among Us.exe")]
    [ReactorModFlags(Reactor.Networking.ModFlags.None)]
    public class BetterCrewLink : BasePlugin
    {
        private const string VersionString = "0.0.1";
        private const string PluginGUID = "com.essence.BCLDLL";
        private static readonly Harmony harmony = new Harmony(PluginGUID);

        public static ManualLogSource Logger;

        public override void Load()
        {
            Logger = Log;
            API api = new API();
            Logger.LogDebug("API is loaded:" + api.loaded.ToString());
            harmony.PatchAll();
        }
    }
}