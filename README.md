# Template BepInEx Plugin

A small BepInEx plugin template for Unity modding Casualties Unknown (or any other unity game).

## Overview

- Project: `ScavTemplate`
- Target framework: `net48`
- Purpose: Simplification
- Includes Harmony, Newtonsoft.Json, and Unity assembly references

## Build

1. Open the project in Visual Studio or JetBrains Rider.
2. Build `ScavTemplate/Template.csproj`.

## Usage

1. Place your game assemblies path in `vars.targets` if needed.
2. Update `Plugin.cs` with your plugin GUID, name, version.
3. Update namespace in `Plugin.cs` and `Patches.cs`
3. Add your mod logic and Harmony patches.
4. Run the game with BepInEx installed.
