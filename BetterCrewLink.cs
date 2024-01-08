using BCLDLL.Interfaces;
using BCLDLL.Mods;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Reactor.Networking.Attributes;
using System;
using System.Collections.Generic;

namespace BCLDLL
{
    [BepInPlugin(PluginGUID, "BetterCrewLink API DLL", VersionString)]
    [BepInProcess("Among Us.exe")]
    [ReactorModFlags(Reactor.Networking.ModFlags.None)]
    [BepInDependency(SubmergedComptability.SUBMERGED_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class BetterCrewLink : BasePlugin
    {
        private const string VersionString = "0.0.1";
        private const string PluginGUID = "com.essence.BCLDLL";
        private static readonly Harmony harmony = new Harmony(PluginGUID);

        public static ManualLogSource Logger;

        private readonly Dictionary<string, Type> mods = new();

        public override void Load()
        {
            var modCompat = new SubmergedCompatability2();
            if (IL2CPPChainloader.Instance.Plugins.ContainsKey(modCompat.MOD_GUID))
            {
                RegisterModCompat("Submerged", modCompat);
            }

            Logger = Log;
            RestAPI api = new RestAPI("1234");
            Logger.LogInfo("Loaded REST API");

            //SubmergedComptability.Initialize();
            harmony.PatchAll();
        }

        public void RegisterModCompat<T>(string GUID, T mod) where T : IModCompatability
        {
            mods.Add(GUID, typeof(T));
        }
    }
}