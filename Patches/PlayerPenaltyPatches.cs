using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BrokenLimbsDontHeal.Effects;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace BrokenLimbsDontHeal.Patches
{
    public static class PlayerPenaltyPatches
    {
        // A separate cause survives native weight and armor recalculations
        private const Player.ESpeedLimit HealedFractureSpeedLimit = (Player.ESpeedLimit)0x424C4448;

        private static float MovementFactor =>
            ModConfig.LegPenaltyEnabled.Value
                ? Mathf.Max(0.05f,
                    1f - Mathf.Clamp(ModConfig.LegSpeedPenaltyPercent.Value, 0f, 95f) * RaidSession.HealedLegs / 100f)
                : 1f;

        internal static void RefreshMovementLimit(MovementContext context)
        {
            if (!RaidSession.IsMovementContext(context))
            {
                return;
            }

            context.RemoveStateSpeedLimit(HealedFractureSpeedLimit);

            float factor = MovementFactor;
            if (factor < 1f)
            {
                context.AddStateSpeedLimit(context.MaxSpeed * factor, HealedFractureSpeedLimit);
            }

            // Recompute the native minimum and set the cap for the meter
            context.method_4();
        }

        internal static void RemoveMovementLimit(MovementContext context)
        {
            if (context == null)
            {
                return;
            }

            context.RemoveStateSpeedLimit(HealedFractureSpeedLimit);
            context.method_4();
        }

        [HarmonyPatch(typeof(MovementContext), nameof(MovementContext.OnStrengthSkillLevelChanged))]
        private static class StrengthChanged
        {
            [HarmonyPostfix]
            private static void Postfix(MovementContext __instance)
            {
                RefreshMovementLimit(__instance);
            }
        }

        // Native walk limits dont reduce the separate sprint speed limit so less work
        [HarmonyPatch(typeof(MovementContext), nameof(MovementContext.StateSprintSpeedLimit), MethodType.Getter)]
        private static class SprintSpeed
        {
            [HarmonyPostfix]
            private static void Postfix(MovementContext __instance, ref float __result)
            {
                if (RaidSession.IsMovementContext(__instance))
                {
                    __result *= MovementFactor;
                }
            }
        }

        private static float GetSprintMeterSpeed(MovementContext context)
        {
            float inputSpeed = context.CharacterMovementSpeed;
            if (!RaidSession.IsMovementContext(context) || MovementFactor >= 1f)
                return inputSpeed;

            // 
            return Mathf.Min(inputSpeed, context.StateSpeedLimit);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.RaiseChangeSpeedEvent))]
        private static class SprintMeterSpeed
        {
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var getter = AccessTools.PropertyGetter(typeof(MovementContext),
                    nameof(MovementContext.CharacterMovementSpeed));
                var replacement = AccessTools.Method(typeof(PlayerPenaltyPatches), nameof(GetSprintMeterSpeed));
                int matches = 0;
                foreach (var instruction in codes)
                {
                    if ((instruction.opcode == OpCodes.Call || instruction.opcode == OpCodes.Callvirt)
                        && Equals(instruction.operand, getter))
                    {
                        instruction.opcode = OpCodes.Call;
                        instruction.operand = replacement;
                        matches++;
                    }
                }

                if (matches != 1)
                {
                    throw new InvalidOperationException(
                        "Expected one speed meter input in Player.RaiseChangeSpeedEvent");
                }

                return codes;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.ErgonomicsPenalty), MethodType.Getter)]
        private static class ArmErgonomics
        {
            [HarmonyPostfix]
            private static void Postfix(Player __instance, ref float __result)
            {
                if (!RaidSession.IsPlayer(__instance) || !ModConfig.ArmPenaltyEnabled.Value) return;
                __result -= Mathf.Clamp(ModConfig.ArmErgonomicsPenaltyPercent.Value, 0f, 95f)
                    * RaidSession.HealedArms / 100f;
            }
        }
    }
}