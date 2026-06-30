using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace ModNamespace
{
    internal class Patches
    {
        [HarmonyPatch(typeof(ConsoleScript))]
        internal static class ConsolePatch
        {
            [HarmonyPatch(nameof(ConsoleScript.Start))]
            [HarmonyPostfix]
            private static void StartPatch()
            {
                ConsoleScript.instance.LogToConsole("Hello World!");
            }
        }
    }
}
