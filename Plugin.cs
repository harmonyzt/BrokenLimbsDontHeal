using System;
using System.Reflection;
using BepInEx;
using BrokenLimbsDontHeal.Effects;
using BrokenLimbsDontHeal.Patches;
using EFT.HealthSystem;
using EFTEffectManager;
using HarmonyLib;

namespace BrokenLimbsDontHeal
{
    [BepInPlugin("com.harmonyzt.BrokenLimbsDontHeal", "Broken Limbs Dont Heal", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource LOGSource;
        private Harmony _harmony;
        private volatile bool _penaltiesDirty;

        private void Awake()
        {
            LOGSource = Logger;
            try
            {
                ModConfig.Init(Config);

                // Register our effect
                EffectsManager.RegisterEffect<HealedFracture>(typeof(Plugin).Assembly, new EffectOptions
                {
                    DisplayName = "Healed fracture",
                    IconFilePath = "HealedFracture.png",
                    DelayTime = 0f,
                    PersistAcrossRaids = false,
                    RemoveOnBodyPartDestroyed = false,
                    MedHealTriggers = { typeof(IFracture) },
                    InvalidatedBy = { typeof(IFracture) },
                });

                EffectsManager.MovementSpeedPenalties.Add("BrokenLimbsDontHeal.LegFracture",
                    HealedFractureRules.SpeedFraction);

                EffectsManager.ErgonomicsPenalties.Add("BrokenLimbsDontHeal.ArmFracture",
                    HealedFractureRules.ErgonomicsPenalty);

                // Re-break roll on bullet hits
                _harmony = new Harmony("com.harmonyzt.BrokenLimbsDontHeal");
                _harmony.PatchAll(Assembly.GetExecutingAssembly());

                ModConfig.LegPenaltyEnabled.SettingChanged += OnPenaltySettingChanged;
                ModConfig.LegSpeedPenaltyPercent.SettingChanged += OnPenaltySettingChanged;
                ModConfig.ArmPenaltyEnabled.SettingChanged += OnPenaltySettingChanged;
                ModConfig.ArmErgonomicsPenaltyPercent.SettingChanged += OnPenaltySettingChanged;

                Logger.LogInfo("[BrokenLimbsDontHeal] Loaded!");
            }
            catch (Exception e)
            {
                _harmony?.UnpatchSelf();
                Logger.LogError($"[BrokenLimbsDontHeal] Init failed: {e}");
                enabled = false;
            }
        }

        private void OnPenaltySettingChanged(object sender, EventArgs args)
        {
            _penaltiesDirty = true;
        }

        private void Update()
        {
            if (!_penaltiesDirty)
            {
                return;
            }

            _penaltiesDirty = false;
            LifecyclePatches.RefreshPenalties();
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();

            if (ModConfig.LegPenaltyEnabled == null)
            {
                return;
            }

            ModConfig.LegPenaltyEnabled.SettingChanged -= OnPenaltySettingChanged;
            ModConfig.LegSpeedPenaltyPercent.SettingChanged -= OnPenaltySettingChanged;
            ModConfig.ArmPenaltyEnabled.SettingChanged -= OnPenaltySettingChanged;
            ModConfig.ArmErgonomicsPenaltyPercent.SettingChanged -= OnPenaltySettingChanged;
        }
    }
}