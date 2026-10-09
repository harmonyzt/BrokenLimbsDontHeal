using System;
using BrokenLimbsDontHeal.Effects;
using EFTEffectManager;
using EFT;
using EFT.Ballistics;
using EFT.HealthSystem;
using HarmonyLib;
using UnityEngine;

namespace BrokenLimbsDontHeal.Patches
{
    [HarmonyPatch(typeof(ActiveHealthController), nameof(ActiveHealthController.ApplyDamage),
        new Type[] { typeof(EBodyPart), typeof(float), typeof(DamageInfo) })]
    public static class HealedFractureReBreakPatch
    {
        [ThreadStatic] private static ActiveHealthController _suppressedController;
        [ThreadStatic] private static EBodyPart _suppressedPart;

        private struct DamageState
        {
            public ActiveHealthController PreviousController;
            public EBodyPart PreviousPart;
            public HealedFracture Healed;
            public bool ScopeCaptured;
        }

        [HarmonyPrefix]
        private static void Prefix(ActiveHealthController __instance, EBodyPart bodyPart,
            float damage, DamageInfo damageInfo, out DamageState __state)
        {
            __state = new DamageState
            {
                PreviousController = _suppressedController,
                PreviousPart = _suppressedPart,
                ScopeCaptured = true
            };

            _suppressedController = null;
            if (!EffectsManager.IsLocalController(__instance) || !ModConfig.ReBreakEnabled.Value
                                                      || damage <= 0f || !HealedFractureRules.IsLimb(bodyPart)
                                                      || (damageInfo.DamageType != EDamageType.Bullet &&
                                                          damageInfo.DamageType != EDamageType.Sniper))
            {
                return;
            }

            __state.Healed = __instance.FindActiveEffect<HealedFracture>(bodyPart);
            if (__state.Healed == null)
            {
                return;
            }

            // Replace the native fracture roll for this healed shot
            _suppressedController = __instance;
            _suppressedPart = bodyPart;
        }

        [HarmonyPostfix]
        private static void Postfix(ActiveHealthController __instance, EBodyPart bodyPart,
            float __result, DamageState __state)
        {
            RestoreScope(__state);
            if (__state.Healed == null || __result <= 0f || !EffectsManager.IsLocalController(__instance))
            {
                return;
            }

            try
            {
                if (!__instance.IsAlive || __instance.IsBodyPartDestroyed(bodyPart)
                                        || !__state.Healed.Active ||
                                        __instance.FindActiveEffect<IFracture>(bodyPart) != null)
                {
                    return;
                }

                float chance = Mathf.Clamp(ModConfig.ReBreakChancePercent.Value, 0f, 100f);
                if (chance <= 0f || (chance < 100f && UnityEngine.Random.value >= chance / 100f))
                {
                    return;
                }

                __instance.DoFracture(bodyPart);
                // if (__instance.FindActiveEffect<IFracture>(bodyPart) != null)
                // {
                //     Plugin.LOGSource.LogInfo($"[BrokenLimbsDontHeal] Shot re-broke {bodyPart}");
                // }
            }
            catch (Exception e)
            {
                Plugin.LOGSource.LogError($"[BrokenLimbsDontHeal] Re-break hook failed: {e}");
            }
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception, DamageState __state)
        {
            RestoreScope(__state);

            return __exception;
        }

        private static void RestoreScope(DamageState state)
        {
            if (!state.ScopeCaptured)
            {
                return;
            }

            _suppressedController = state.PreviousController;
            _suppressedPart = state.PreviousPart;
        }

        [HarmonyPatch(typeof(ActiveHealthController), nameof(ActiveHealthController.DoFracture))]
        private static class NativeFractureDuringShot
        {
            [HarmonyPrefix]
            private static bool Prefix(ActiveHealthController __instance, EBodyPart bodyPart) =>
                !ReferenceEquals(_suppressedController, __instance) || _suppressedPart != bodyPart ||
                !EffectsManager.IsLocalController(__instance);
        }
    }
}