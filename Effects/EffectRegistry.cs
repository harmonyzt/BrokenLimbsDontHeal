using System;
using EFT;
using EFT.HealthSystem;

namespace BrokenLimbsDontHeal.Effects
{
    public static class EffectRegistry
    {
        public const string EffectName = "HealedFracture";

        public static void Register()
        {
            byte code;
            
            if (!HealthHelper.EffectTypeCode._typeToByte.TryGetValue(EffectName, out code))
            {
                int available = -1;
                
                for (int candidate = byte.MaxValue; candidate >= 0; candidate--)
                {
                    if (!HealthHelper.EffectTypeCode._byteToType.ContainsKey((byte)candidate))
                    {
                        available = candidate;
                        
                        break;
                    }
                }

                if (available < 0)
                {
                    throw new InvalidOperationException("No free health-effect type code - report it to the dev");
                }

                code = (byte)available;
                
                HealthHelper.EffectTypeCode._typeToByte.Add(EffectName, code);
                HealthHelper.EffectTypeCode._byteToType.Add(code, EffectName);
            }
            else if (!HealthHelper.EffectTypeCode._byteToType.TryGetValue(code, out var name) || name != EffectName)
            {
                throw new InvalidOperationException("Health-effect type code is inconsistent.");
            }

            // specific effect classes are incompatible with one another
            HealthHelper.EffectActivator<ActiveHealthController>._constructors[EffectName] =
                () => new HealedFracture();
            HealthHelper._effectNames[typeof(IHealedFracture)] = EffectName;
            // Plugin.LOGSource.LogInfo($"[BrokenLimbsDontHeal] Registered {EffectName} (type code {code}).");
        }

        public static bool IsLimb(EBodyPart part) => IsLeg(part) || IsArm(part);
        public static bool IsLeg(EBodyPart part) =>
            part == EBodyPart.LeftLeg || part == EBodyPart.RightLeg;
        public static bool IsArm(EBodyPart part) =>
            part == EBodyPart.LeftArm || part == EBodyPart.RightArm;
    }
}