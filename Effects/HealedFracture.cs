using EFT.HealthSystem;
using UnityEngine;

namespace BrokenLimbsDontHeal.Effects
{
    public interface IHealedFracture : IHealthEffect
    {
    }

    // IRestorable / recovery belongs to this raid.
    public class HealedFracture : ActiveHealthController.Effect, IHealedFracture, IEffectTriggersUIPanel
    {
        public static float DurationSeconds => Mathf.Clamp(ModConfig.HealDurationMinutes.Value, 0.1f, 60f) * 60f;

        public override float DefaultDelayTime => 1f;
        public override float DefaultWorkTime => DurationSeconds;
        public override float DefaultResidueTime => 0f;
    }
}