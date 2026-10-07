using System;
using BrokenLimbsDontHeal.Effects;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;

namespace BrokenLimbsDontHeal.Patches
{
    [HarmonyPatch(typeof(ActiveHealthController.MedEffect), nameof(ActiveHealthController.MedEffect.Residue))]
    public static class MedEffectResidueHealPatch
    {
        private struct HealState
        {
            public ActiveHealthController Controller;
            public EBodyPart BodyPart;
            public bool HadFracture;
        }

        [HarmonyPrefix]
        private static void Prefix(ActiveHealthController.MedEffect __instance, out HealState __state)
        {
            __state = default;

            var controller = __instance.HealthController;
            if (!RaidSession.IsController(controller) || !EffectRegistry.IsLimb(__instance.BodyPart))
            {
                return;
            }

            __state.Controller = controller;
            __state.BodyPart = __instance.BodyPart;
            __state.HadFracture = controller.FindActiveEffect<IFracture>(__instance.BodyPart) != null;
        }

        [HarmonyPostfix]
        private static void Postfix(HealState __state)
        {
            var controller = __state.Controller;

            if (!__state.HadFracture || !RaidSession.IsController(controller))
            {
                return;
            }
            
            // Onheal
            try
            {
                if (!controller.IsAlive
                    || controller.IsBodyPartDestroyed(__state.BodyPart) 
                    || controller.FindActiveEffect<IFracture>(__state.BodyPart) != null
                    || controller.FindExistingEffect<HealedFracture>(__state.BodyPart) != null)
                {
                    return;
                }

                IconHelper.EnsureIcon();
                float duration = HealedFracture.DurationSeconds;
                controller.AddEffect<HealedFracture>(__state.BodyPart, 0f, duration, 0f, 1f);
                
                //Plugin.LOGSource.LogInfo($"[BrokenLimbsDontHeal] Healed fracture on {__state.BodyPart}: {duration:0.#} seconds.");
            }
            catch (Exception e)
            {
                Plugin.LOGSource.LogError($"[BrokenLimbsDontHeal] Fracture healing hook failed: {e}");
            }
        }
    }
}