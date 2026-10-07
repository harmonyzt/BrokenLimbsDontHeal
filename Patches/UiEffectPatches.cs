using System;
using BrokenLimbsDontHeal.Effects;
using EFT;
using HarmonyLib;

namespace BrokenLimbsDontHeal.Patches
{
    public static class UiEffectPatches
    {
        [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.LocalizedValue),
            new Type[] { typeof(string), typeof(string) })]
        private static class LocalizedValuePatch
        {
            [HarmonyPrefix]
            private static bool Prefix(string id, ref string __result)
            {
                if (id != EffectRegistry.EffectName)
                {
                    return true;
                }

                __result = "Healed fracture";

                return false;
            }
        }
    }
}