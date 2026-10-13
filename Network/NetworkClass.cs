using BrokenLimbsDontHeal.Effects;
using EFT.HealthSystem;

namespace BrokenLimbsDontHeal.Network
{
    public class HealedFracture : NetworkHealthController.Effect, IHealedFracture, IEffectTriggersUIPanel
    {
    }
}