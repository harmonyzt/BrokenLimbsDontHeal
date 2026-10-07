using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BrokenLimbsDontHeal.Effects;
using HarmonyLib;

namespace BrokenLimbsDontHeal
{
    [BepInPlugin("com.harmonyzt.BrokenLimbsDontHeal", "Broken Limbs Dont Heal", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LOGSource;
        private Harmony _harmony;
        private volatile bool _penaltiesDirty;

        private void Awake()
        {
            LOGSource = Logger;
            try
            {
                ModConfig.Init(Config);
                EffectRegistry.Register();
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
            RaidSession.RefreshPenalties();
        }

        private void OnDestroy()
        {
            RaidSession.End();
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