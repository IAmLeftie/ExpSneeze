using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Logging;
using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using ExpSneeze;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace ExpSneeze
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "lefty.expsneeze";
        public const string ModName = "ExpSneeze";
        public const string ModVersion = "1.4.0";

        internal static new ManualLogSource Logger;
        private readonly Harmony _harmony = new(ModGUID);
        public static Plugin Instance { get; private set; } = null!;

        public static Sprite makeshiftMaskSprite;

        void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            makeshiftMaskSprite = AssetLoader.LoadSpriteFromPluginFolder(this, "Images/makeshiftmask.png");

            _harmony.PatchAll();
            RegisterConsoleCommands();
            ExpSneeze.Items.RegisterItems();
            ExpSneeze.Items.RegisterRecipes();

            ModOptionsRegistry.Register(ModOptionDefinition.Float("expsneeze.sneezetimemax",
                LocaleRegistry.Get("other", "gamesetexpsneeze.sneezetimemax", "Time between sneezes"),
                LocaleRegistry.Get("other", "gamesetexpsneeze.sneezetimemaxdsc", "Time in seconds between sneezes, assuming the player does absolutely nothing between sneezes and has no dust mask equipped."),
                Setting.SettingCategory.Game,
                PlayerPrefs.GetFloat("ExpSneeze_SneezeTimeMax", 450f),
                180f,
                720f,
                value =>
                {
                    PlayerPrefs.SetFloat("ExpSneeze_SneezeTimeMax", value);
                    PlayerPrefs.Save();
                },
                value => Mathf.RoundToInt(value) + " sec."
            ));

            ModOptionsRegistry.Register(ModOptionDefinition.Float("expsneeze.sneezeforce",
                LocaleRegistry.Get("other", "gamesetexpsneeze.sneezeforce", "Sneeze force"),
                LocaleRegistry.Get("other", "gamesetexpsneeze.sneezeforcedsc", "Knockback force on sneezes. Setting to 0 effectively disables knockback. Values above 25 are increasingly silly and shouldn't be used seriously."),
                Setting.SettingCategory.Game,
                PlayerPrefs.GetFloat("ExpSneeze_SneezeForce", 5f),
                0f,
                750f,
                value =>
                {
                    PlayerPrefs.SetFloat("ExpSneeze_SneezeForce", value);
                    PlayerPrefs.Save();
                },
                value => Mathf.Round(value).ToString()
            ));

            ModOptionsRegistry.Register(ModOptionDefinition.Int("expsneeze.dropchance",
                LocaleRegistry.Get("other", "gamesetexpsneeze.dropchance", "Chance to drop item in mouth when sneezing"),
                LocaleRegistry.Get("other", "gamesetexpsneeze.dropchancedsc", "Chance out of 10 to drop the currently held item in the house when sneezing. Setting to 0 effectively disables this."),
                Setting.SettingCategory.Game,
                PlayerPrefs.GetInt("ExpSneeze_DropChance", 1),
                0,
                10,
                value =>
                {
                    PlayerPrefs.SetInt("ExpSneeze_DropChance", value);
                    PlayerPrefs.Save();
                }
            ));

            ModOptionsRegistry.Register(ModOptionDefinition.Bool("expsneeze.ragdoll",
                LocaleRegistry.Get("other", "gamesetexpsneeze.ragdoll", "Ragdoll on sneeze"),
                LocaleRegistry.Get("other", "gamesetexpsneeze.ragdolldsc", "Combine with high sneeze force for shenanigans.\n<i>Joke setting. Please don't actually turn this on seriously.</i>"),
                Setting.SettingCategory.Game,
                PlayerPrefs.GetInt("ExpSneeze_Ragdoll", 0) == 1,
                value =>
                {
                    PlayerPrefs.SetInt("ExpSneeze_Ragdoll", value ? 1 : 0);
                    PlayerPrefs.Save();
                }
            ));

            Logger.LogInfo($"Plugin {ModName} is loaded!");
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            Instance = null!;
        }

        private void RegisterConsoleCommands()
        {
            ConsoleCommandRegistry.Register(
                "sneeze",
                "Force a sneeze",
                args =>
                {
                    CUCoreUtils.ConsoleLog(ConsoleScript.instance, "Achoo!");
                    Body body = PlayerCamera.main.body;
                    SneezeStatus status = body.GetStatus<SneezeStatus>();
                    status.SneezeTime = status.SneezeTimeMax - 5f;
                },
                null
            );
        }
    }
}
