using System;
using EFT.HealthSystem;
using UnityEngine;

namespace BrokenLimbsDontHeal.Effects
{
    // Helpers and the two registered penalty providers
    // EffectsManager.MovementSpeedPenalties / ErgonomicsPenalties
    public static class HealedFractureRules
    {
        public static bool IsLimb(EBodyPart part) => IsLeg(part) || IsArm(part);

        public static bool IsLeg(EBodyPart part) =>
            part == EBodyPart.LeftLeg || part == EBodyPart.RightLeg;

        public static bool IsArm(EBodyPart part) =>
            part == EBodyPart.LeftArm || part == EBodyPart.RightArm;

        // Speed fraction (1 = untouched) while healed leg fractures exist
        public static float? SpeedFraction(IHealthController controller)
        {
            if (!ModConfig.LegPenaltyEnabled.Value)
            {
                return 1f;
            }

            int healedLegs = CountHealed(controller, IsLeg);
            if (healedLegs == 0)
            {
                return 1f;
            }

            float penalty = Mathf.Clamp(ModConfig.LegSpeedPenaltyPercent.Value, 0f, 95f) * healedLegs / 100f;
            return Mathf.Max(0.05f, 1f - penalty);
        }

        // Ergonomics fraction if we have healed arm fractures
        public static float ErgonomicsPenalty(IHealthController controller)
        {
            if (!ModConfig.ArmPenaltyEnabled.Value)
            {
                return 0f;
            }

            int healedArms = CountHealed(controller, IsArm);
            if (healedArms == 0)
            {
                return 0f;
            }

            float penalty = Mathf.Clamp(ModConfig.ArmErgonomicsPenaltyPercent.Value, 0f, 95f) * healedArms / 100f;
            return -penalty;
        }

        private static int CountHealed(IHealthController controller, Func<EBodyPart, bool> partFilter)
        {
            if (!(controller is ActiveHealthController active))
            {
                return 0;
            }

            int count = 0;
            foreach (ActiveHealthController.Effect effect in active.Effects)
            {
                if (effect is HealedFracture && effect.Active && partFilter(effect.BodyPart))
                {
                    count++;
                }
            }

            return count;
        }
    }
}