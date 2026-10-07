using System;
using BrokenLimbsDontHeal.Effects;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;

namespace BrokenLimbsDontHeal.Patches
{
    public static class PlayerEffectEventPatches
    {
        [HarmonyPatch(typeof(Player), nameof(Player.OnHealthEffectAdded))]
        private static class EffectStarted
        {
            [HarmonyPostfix]
            private static void Postfix(Player __instance, IHealthEffect effect)
            {
                if (!RaidSession.IsPlayer(__instance)) return;
                try
                {
                    if (effect is IFracture)
                    {
                        var controller = __instance.HealthController as ActiveHealthController;
                        controller?.FindExistingEffect<HealedFracture>(effect.BodyPart)?.ForceRemove();
                    }

                    if (effect is HealedFracture)
                    {
                        RaidSession.RefreshPenalties();
                    }
                }
                catch (Exception e)
                {
                    Plugin.LOGSource.LogError($"[BrokenLimbsDontHeal] Effect-start hook failed: {e}");
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnHealthEffectRemoved))]
        private static class EffectResidued
        {
            [HarmonyPostfix]
            private static void Postfix(Player __instance, IHealthEffect effect)
            {
                if (RaidSession.IsPlayer(__instance) && effect is HealedFracture)
                {
                    RaidSession.RefreshPenalties();
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.HealthControllerOnBodyPartDestroyedEvent))]
        private static class BodyPartDestroyed
        {
            [HarmonyPostfix]
            private static void Postfix(Player __instance, EBodyPart arg1)
            {
                if (!RaidSession.IsPlayer(__instance))
                {
                    return;
                }
                
                try
                {
                    var controller = __instance.HealthController as ActiveHealthController;
                    
                    controller?.FindExistingEffect<HealedFracture>(arg1)?.ForceRemove();
                }
                catch (Exception e)
                {
                    Plugin.LOGSource.LogError($"[BrokenLimbsDontHeal] Limb-destruction hook failed: {e}");
                }
            }
        }
    }
}