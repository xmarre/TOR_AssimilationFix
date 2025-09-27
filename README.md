# The Old Realms — Assimilation Crash Fix

Harmony hot-patch that prevents `IndexOutOfRangeException` in `TroopRoster.AddToCountsAtIndex` caused by TOR assimilation when entering custom locations or during recruit/bind/summon flows. No save edits. Safe mid-playthrough.

## Status
- **Built on:** Bannerlord 1.2.12.6623
- **Tested on:** The Old Realms 1.2.11
- **Scope:** Singleplayer

## Why it crashes
`AssimilationCampaignBehavior.SwapTroopsIfNeeded` used:
```csharp
TroopRoster.RemoveTroop(CharacterObject troop, int count, UniqueTroopDescriptor seed, int xp)
```
This call can resolve a stale roster element. The engine then feeds an invalid index into `AddToCountsAtIndex` and throws.

## What this mod changes
- Replaces the seeded remover with a seedless, clamped path.
- Clamps removal to `GetTroopCount(troop)`.
- Uses `RemoveTroop(CharacterObject, int)`.
- Fallback removes one by one on exception.
- Assimilation outcomes are unchanged.

## Known triggers (vanilla TOR)
- Interacting with custom locations like **Hunger Woods** and similar TOR custom locations.
- Being a **vampire** in an **army** with parties led by **companions of other cultures**.
- **Solo companions** in their **own party** visiting those locations and trying to **recruit / bind / summon wraiths**.

## Installation (players)
- Vortex or MO2: install and enable. Load after `TOR_Core`.
- Manual: copy `TOR_AssimilationFix` to `Mount & Blade II Bannerlord/Modules/`, enable in the launcher, load after `TOR_Core`.

**Load order:** `Bannerlord.Harmony` → base modules → `TOR_Core` → `TOR_AssimilationFix`

Safe to add or remove mid-save.

## Building (developers)
- Open `Source/TOR_AssimilationFix.csproj` in Visual Studio or Rider.
- Adjust game DLL reference paths if needed.
- Build Release. The DLL is copied to `Modules/TOR_AssimilationFix/bin/Win64_Shipping_Client/`.

## How it works
A Harmony transpiler targets:
```
TOR_Core.CampaignMechanics.Assimilation.AssimilationCampaignBehavior
  .SwapTroopsIfNeeded(Hero, TroopRoster, CharacterObject, int)
```
and replaces calls to the 4-arg `RemoveTroop(...)` with a safe remover that does not depend on a cached index or seed.

## Compatibility
- Conflicts only with mods that also patch the same method.
- If there is a conflict, place the preferred patch later in load order.
- This mod does **not** add its own logging or telemetry.

## Changelog
- 1.0.0 — Initial release.

## License
Licensed under the Apache License, Version 2.0. See `LICENSE` for details.

## Links
- Nexus Mods: https://www.nexusmods.com/mountandblade2bannerlord/mods/8872
- Source: https://github.com/xmarre/TOR_AssimilationFix
