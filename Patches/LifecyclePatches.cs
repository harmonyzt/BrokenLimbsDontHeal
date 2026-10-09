using System;
using EFT;
using EFTEffectManager;
using HarmonyLib;

namespace BrokenLimbsDontHeal.Patches
{
    /// <summary>
    /// The only raid lifecycle we need: remember the local player so we can refresh the penalties on the go
    /// Effect creation / invalidation / cleanup and the penalty application itself all live in EFTEffectManager
    /// </summary>
    public static class LifecyclePatches
    {
        internal static Player LocalPlayer { get; private set; }

        internal static void RefreshPenalties()
        {
            Player player = LocalPlayer;
            if (player == null)
            {
                return;
            }

            try
            {
                EffectsManager.RefreshPenalties(player);
            }
            catch (Exception e)
            {
                Plugin.LOGSource?.LogError($"[BrokenLimbsDontHeal] Penalty refresh failed: {e}");
            }
        }

        [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.OnGameStarted))]
        private static class RaidStarted
        {
            [HarmonyPostfix]
            private static void Postfix(GameWorld __instance)
            {
                Player player = __instance?.MainPlayer;
                LocalPlayer = player != null && player.IsYourPlayer ? player : null;
            }
        }

        [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Dispose))]
        private static class WorldDisposing
        {
            [HarmonyPrefix]
            private static void Prefix()
            {
                LocalPlayer = null;
            }
        }
    }
}