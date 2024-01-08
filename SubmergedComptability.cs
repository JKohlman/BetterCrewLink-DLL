using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BCLDLL
{
    public enum SubmergedCameraLocation
    {
        NONE
    }

    public static class SubmergedComptability
    {
        public const string SUBMERGED_GUID = "Submerged";

        public static bool isLoaded;
        private static Type[] Types;

        private static Type CamMinigameClass;
        private static FieldInfo SelectedCamField;

        public static void Initialize()
        {
            isLoaded = IL2CPPChainloader.Instance.Plugins.ContainsKey(SUBMERGED_GUID);
            if (isLoaded)
            {
                BetterCrewLink.Logger.LogInfo("Submerged Detected");
                PluginInfo submergedPlugin = IL2CPPChainloader.Instance.Plugins["Submerged"];

                Types = AccessTools.GetTypesFromAssembly(submergedPlugin.Instance.GetType().Assembly);

                CamMinigameClass = Types.First(type => type.Name == "SubmarineSurvillanceMinigame");
                SelectedCamField = AccessTools.Field(CamMinigameClass, "SelectedCam");
            }
            else
            {
                BetterCrewLink.Logger.LogInfo("Submerged Not Detected");
            }
        }

        // Thank you to probablyadnf
        public static T Cast<T>(object obj) where T : MonoBehaviour => obj as T;

        public static SubmergedCameraLocation getCamera()
        {
            if (!isLoaded)
            {
                BetterCrewLink.Logger.LogError("Cannot get Submerged cam because Submerged is not loaded");
                return SubmergedCameraLocation.NONE;
            }
            try
            {
                var minigame = Minigame.Instance;
                if (minigame == null) return SubmergedCameraLocation.NONE;

                var castMethod = typeof(SubmergedComptability).GetMethod(nameof(Cast)).MakeGenericMethod(CamMinigameClass);
                // The try cast here will get the managed version rather than just an interop wrapper
                var camMinigame = castMethod.Invoke(null, new object[] { minigame });
                if (camMinigame == null) return SubmergedCameraLocation.NONE;

                return (SubmergedCameraLocation)(int)SelectedCamField.GetValue(camMinigame);
            }
            catch (Exception e)
            {
                BetterCrewLink.Logger.LogError("Get Camera Error:\n" + e.ToString());
                return SubmergedCameraLocation.NONE;
            }
        }
    }
}
