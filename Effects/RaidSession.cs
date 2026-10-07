using System;
using System.Collections.Generic;
using EFT;
using EFT.HealthSystem;
using BrokenLimbsDontHeal.Patches;

namespace BrokenLimbsDontHeal.Effects
{
    internal static class RaidSession
    {
        private static GameWorld _world;
        internal static Player LocalPlayer { get; private set; }
        internal static int HealedLegs { get; private set; }
        internal static int HealedArms { get; private set; }
        internal static bool IsActive => _world != null && LocalPlayer != null;

        internal static bool IsPlayer(Player player) =>
            IsActive && ReferenceEquals(LocalPlayer, player);

        internal static bool IsController(ActiveHealthController controller) =>
            IsActive && controller != null && ReferenceEquals(LocalPlayer.HealthController, controller);

        internal static bool IsMovementContext(MovementContext context) =>
            IsActive && context != null && ReferenceEquals(LocalPlayer.MovementContext, context);

        internal static void Begin(GameWorld world)
        {
            var player = world?.MainPlayer;
            if (player == null || !player.IsYourPlayer || player is HideoutPlayer
                || string.Equals(player.Location, "hideout", StringComparison.OrdinalIgnoreCase)
                || !(player.HealthController is ActiveHealthController))
            {
                return;
            }

            if (ReferenceEquals(_world, world))
            {
                return;
            }

            End();
            _world = world;
            LocalPlayer = player;
            IconHelper.EnsureIcon();
            RefreshPenalties();

            //Plugin.LOGSource.LogInfo("[BrokenLimbsDontHeal] Raid started");
        }

        internal static void RefreshPenalties()
        {
            if (!IsActive)
            {
                return;
            }

            try
            {
                HealedLegs = 0;
                HealedArms = 0;
                var controller = LocalPlayer.HealthController as ActiveHealthController;
                if (controller == null)
                {
                    return;
                }

                foreach (var effect in controller.Effects)
                {
                    if (!(effect is HealedFracture) || !effect.Active) continue;
                    if (EffectRegistry.IsLeg(effect.BodyPart)) HealedLegs++;
                    if (EffectRegistry.IsArm(effect.BodyPart)) HealedArms++;
                }

                PlayerPenaltyPatches.RefreshMovementLimit(LocalPlayer.MovementContext);
                (LocalPlayer.HandsController as Player.FirearmController)?.RecalculateErgonomic();
                LocalPlayer.ProceduralWeaponAnimation?.UpdateWeaponVariables();
            }
            catch (Exception e)
            {
                Plugin.LOGSource.LogError($"[BrokenLimbsDontHeal] Penalty refresh failed: {e}");
            }
        }

        internal static void End(GameWorld world = null)
        {
            if (world != null && !ReferenceEquals(_world, world))
            {
                return;
            }

            var player = LocalPlayer;
            _world = null;
            LocalPlayer = null;
            HealedLegs = HealedArms = 0;

            if (player == null)
            {
                return;
            }

            try
            {
                if (player.HealthController is ActiveHealthController controller)
                {
                    var effects = new List<HealedFracture>();
                    foreach (var effect in controller.Effects)
                    {
                        if (effect is HealedFracture healed)
                        {
                            effects.Add(healed);
                        }
                    }

                    foreach (var effect in effects)
                    {
                        effect.ForceRemove();
                    }
                }

                PlayerPenaltyPatches.RemoveMovementLimit(player.MovementContext);
                (player.HandsController as Player.FirearmController)?.RecalculateErgonomic();

                // Plugin.LOGSource?.LogInfo("[BrokenLimbsDontHeal] Raid ended");
            }
            catch (Exception e)
            {
                Plugin.LOGSource?.LogError($"[BrokenLimbsDontHeal] Raid cleanup failed: {e}");
            }
        }
    }
}