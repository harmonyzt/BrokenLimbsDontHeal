using BepInEx.Configuration;

namespace BrokenLimbsDontHeal.Effects
{
    public static class ModConfig
    {
        public static ConfigEntry<bool> LegPenaltyEnabled;
        public static ConfigEntry<float> LegSpeedPenaltyPercent;

        public static ConfigEntry<bool> ArmPenaltyEnabled;
        public static ConfigEntry<float> ArmErgonomicsPenaltyPercent;

        public static ConfigEntry<float> HealDurationMinutes;
        public static ConfigEntry<bool> ReBreakEnabled;
        public static ConfigEntry<float> ReBreakChancePercent;

        public static void Init(ConfigFile config)
        {
            LegPenaltyEnabled = config.Bind(
                "Legs",
                "Leg Fracture Penalty",
                true,
                "Apply the movement speed penalty while a healed leg fracture exists.");

            LegSpeedPenaltyPercent = config.Bind(
                "Legs",
                "Leg Fracture Speed Penalty",
                18f,
                new ConfigDescription(
                    "Movement speed reduction (percent) applied per healed leg fracture. " +
                    "Two healed legs stack (Example Penalty: 18% -> 36%).",
                    new AcceptableValueRange<float>(5f, 45f))
            );

            ArmPenaltyEnabled = config.Bind(
                "Arms",
                "Arm Fracture Penalty",
                true,
                "Apply the ergonomics penalty while a healed arm fracture exists.");

            ArmErgonomicsPenaltyPercent = config.Bind(
                "Arms",
                "Arm Fracture Ergonomics Penalty",
                10f,
                new ConfigDescription(
                    "Movement speed reduction (percent) applied per healed arm fracture. " +
                    "Two healed arms stack (Example Penalty: 10% -> 20%).",
                    new AcceptableValueRange<float>(5f, 45f))
            );

            HealDurationMinutes = config.Bind(
                "General",
                "Fraction Duration",
                6f,
                new ConfigDescription(
                    "How long (in minutes) a healed fracture keeps applying its penalties before the bone fully recovers.",
                    new AcceptableValueRange<float>(3f, 25f))
            );

            ReBreakEnabled = config.Bind(
                "Misc",
                "Enabled",
                true,
                "If enabled, taking a bullet hit in a limb with a healed fracture has a chance to break the bone again.");

            ReBreakChancePercent = config.Bind(
                "Misc",
                "Chance to break the limb again",
                25f,
                new ConfigDescription(
                    "Chance (percent) that a shot to a limb with a healed fracture breaks it again.",
                    new AcceptableValueRange<float>(1f, 100f))
            );
        }
    }
}