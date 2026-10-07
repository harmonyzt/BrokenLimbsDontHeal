using System;
using BrokenLimbsDontHeal.Effects;
using EFT;
using HarmonyLib;

namespace BrokenLimbsDontHeal.Patches
{
    public static class LifecyclePatches
    {
        [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.OnGameStarted))]
        private static class RaidStarted
        {
            [HarmonyPostfix]
            private static void Postfix(GameWorld __instance)
            {
                try { RaidSession.Begin(__instance); }
                catch (Exception e)
                {
                    RaidSession.End(__instance);
                    
                    Plugin.LOGSource.LogError($"[BrokenLimbsDontHeal] Raid initialization failed: {e}");
                }
            }
        }

        [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Dispose))]
        private static class WorldDisposing
        {
            [HarmonyPrefix]
            private static void Prefix(GameWorld __instance) => RaidSession.End(__instance);
        }
    }
}