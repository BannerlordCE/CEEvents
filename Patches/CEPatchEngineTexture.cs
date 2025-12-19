using CaptivityEvents.Custom;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.TwoDimension;

namespace CaptivityEvents.Patches
{

    [HarmonyPatch(typeof(EngineTexture))]
    internal class CEPatchEngineTexture
    {
        /// <summary>
        /// Prevents null reference exception when Release() is called on an already released texture.
        /// This can happen when launching a city battle for the second time after winning the first.
        /// </summary>
        [HarmonyPatch("TaleWorlds.TwoDimension.ITexture.Release")]
        [HarmonyPrefix]
        private static bool ITextureReleasePrefix(EngineTexture __instance)
        {
            // Use reflection to check the private Texture field
            FieldInfo textureField = typeof(EngineTexture).GetField("Texture", BindingFlags.NonPublic | BindingFlags.Instance);
            if (textureField != null)
            {
                object textureValue = textureField.GetValue(__instance);
                if (textureValue == null)
                {
                    // Texture is already null, skip the release to prevent null reference exception
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SpriteCategory))]
    internal class CEPatchSpriteCategory
    {
        /// <summary>
        /// Prevents null reference exception when Unload() iterates through SpriteSheets and calls Release().
        /// CE adds custom textures to SpriteSheets which may have already been released or have null PlatformTexture.
        /// This replaces the original Unload() with a safe version that checks for null before releasing.
        /// </summary>
        [HarmonyPatch("Unload")]
        [HarmonyPrefix]
        private static bool UnloadPrefix(SpriteCategory __instance)
        {
            try
            {
                // Get the IsLoaded property value
                if (!__instance.IsLoaded)
                {
                    return false; // Skip original method
                }

                // Safely release each texture, checking for null
                foreach (Texture texture in __instance.SpriteSheets)
                {
                    try
                    {
                        if (texture?.PlatformTexture != null)
                        {
                            if (texture.PlatformTexture is EngineTexture engineTexture && engineTexture.Texture != null)
                            {
                                texture.PlatformTexture.Release();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Texture may have already been released, ignore
                    }
                }

                __instance.SpriteSheets.Clear();

                // Set IsLoaded = false using reflection since it may have a private setter
                PropertyInfo isLoadedProperty = typeof(SpriteCategory).GetProperty("IsLoaded");
                isLoadedProperty?.SetValue(__instance, false);

                // Set IsPartiallyLoaded = false
                PropertyInfo isPartiallyLoadedProperty = typeof(SpriteCategory).GetProperty("IsPartiallyLoaded");
                isPartiallyLoadedProperty?.SetValue(__instance, false);
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("CEPatchSpriteCategory.UnloadPrefix: " + e);
            }

            return false; // Skip original method since we handled it
        }
    }

    [HarmonyPatch(typeof(MissionGauntletCategoryLoadManager))]
    internal class CEPatchMissionGauntletCategoryLoadManager
    {
        /// <summary>
        /// Restores CE custom textures after MissionGauntletCategoryLoadManager.LoadUnloadAllCategories reloads categories.
        /// This is the PRIMARY cause of textures becoming "material_error" after battles.
        /// When categories are unloaded/reloaded during battle transitions, runtime-added CE textures are lost.
        /// </summary>
        [HarmonyPatch("LoadUnloadAllCategories", typeof(bool))]
        [HarmonyPostfix]
        private static void LoadUnloadAllCategoriesPostfix(bool load)
        {
            try
            {
                // Only restore when loading (not when unloading)
                if (!load)
                {
                    return;
                }

                // Check if we're in a valid game state
                if (Mission.Current == null)
                {
                    return;
                }

                // Get the CESubModule instance to call restore method
                CESubModule ceSubModule = CESubModule.Instance;
                if (ceSubModule != null)
                {
                    CECustomHandler.LogToFile("MissionGauntletCategoryLoadManager reloaded categories - restoring CE textures");
                    ceSubModule.ReloadImagesAgain();
                }
            }
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile($"CEPatchMissionGauntletCategoryLoadManager.LoadUnloadAllCategoriesPostfix failed: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}
