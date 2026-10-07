using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BrokenLimbsDontHeal.Effects
{
    public static class IconHelper
    {
        public const int IconSize = 64;

        private static Sprite _cachedSprite;

        // Should never be null, since we ship it
        // If it is, it's user error
        private static Sprite GetIcon()
        {
            if (_cachedSprite == null)
            {
                _cachedSprite = CreateIcon();
            }

            return _cachedSprite;
        }
        
        // Add our effect to the vanilla icon dictionary
        public static void EnsureIcon()
        {
            if (!RaidSession.IsActive) return;
            try
            {
                Sprite sprite = GetIcon();
                if (sprite == null)
                {
                    return;
                }

                Dictionary<Type, Sprite> icons = GetIconDictionary();
                if (icons != null && !icons.ContainsKey(typeof(IHealedFracture)))
                {
                    icons.Add(typeof(IHealedFracture), sprite);
                    
                    //Plugin.LOGSource.LogInfo("[BrokenLimbsDontHeal] Effect icon registered");
                }
            }
            catch (Exception e)
            {
                Plugin.LOGSource.LogWarning($"[BrokenLimbsDontHeal] Failed to register effect icon: {e}");
            }
        }

        /// EFTHardSettings.Instance.StaticIcons.EffectIcons.EffectIcons
        private static Dictionary<Type, Sprite> GetIconDictionary()
        {
            EFTHardSettings hardSettings = EFTHardSettings.Instance;

            if (hardSettings == null || hardSettings.StaticIcons == null || hardSettings.StaticIcons.EffectIcons == null)
            {
                return null;
            }

            return hardSettings.StaticIcons.EffectIcons.EffectIcons;
        }

        private static Sprite CreateIcon()
        {
            Texture2D texture = LoadTextureFromDisk();

            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect);
        }

        private static Texture2D LoadTextureFromDisk()
        {
            try
            {
                string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(directory))
                {
                    return null;
                }

                string path = Path.Combine(directory, "HealedFracture.png");
                if (!File.Exists(path))
                {
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (texture.LoadImage(bytes))
                {
                    //Plugin.LOGSource.LogInfo($"[BrokenLimbsDontHeal] Loaded effect icon");
                    
                    return texture;
                }

                Plugin.LOGSource.LogWarning($"[BrokenLimbsDontHeal] Could not decode '{path}'");
                UnityEngine.Object.Destroy(texture);
            }
            catch (Exception e)
            {
                Plugin.LOGSource.LogWarning($"[BrokenLimbsDontHeal] Error loading icon from disk: {e}");
            }

            return null;
        }
    }
}