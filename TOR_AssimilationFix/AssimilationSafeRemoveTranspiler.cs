using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TOR_AssimilationFix
{
    /// <summary>
    /// Transpiler: in TOR_Core.CampaignMechanics.Assimilation.AssimilationCampaignBehavior.SwapTroopsIfNeeded
    /// replace TroopRoster.RemoveTroop(troop, count, seed, xp) with SafeRemoveTroop(roster, troop, count).
    /// This avoids IndexOutOfRange in AddToCountsAtIndex from stale indices/seeds.
    /// </summary>
    [HarmonyPatch]
    internal static class AssimilationSwapTranspiler
    {
        static MethodBase TargetMethod()
        {
            // Resolve target via name to avoid hard ref versioning issues.
            var type = AccessTools.TypeByName("TOR_Core.CampaignMechanics.Assimilation.AssimilationCampaignBehavior");
            return AccessTools.Method(type, "SwapTroopsIfNeeded",
                new[] { typeof(Hero), typeof(TroopRoster), typeof(CharacterObject), typeof(int) });
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var list = new List<CodeInstruction>(instructions);

            var miRemove4 = AccessTools.Method(typeof(TroopRoster), "RemoveTroop",
                new[] { typeof(CharacterObject), typeof(int), typeof(UniqueTroopDescriptor), typeof(int) });

            var miSafe = AccessTools.Method(typeof(AssimilationSwapTranspiler), nameof(SafeRemoveTroop));

            int replacements = 0;

            for (int i = 0; i < list.Count; i++)
            {
                var ins = list[i];

                if (ins.opcode == OpCodes.Callvirt && ins.operand is MethodInfo m && m == miRemove4)
                {
                    // Stack just before callvirt currently:
                    // this(roster), troop, number, seed, xp
                    // We need: roster, troop, number
                    // Pop xp and seed
                    var pops = new[]
                    {
                        new CodeInstruction(OpCodes.Pop), // pop xp
                        new CodeInstruction(OpCodes.Pop)  // pop seed
                    };

                    foreach (var p in pops)
                        yield return p;

                    // Replace with call SafeRemoveTroop(roster, troop, number)
                    yield return new CodeInstruction(OpCodes.Call, miSafe);
                    replacements++;
                    continue;
                }

                yield return ins;
            }

            Debug.Print($"[TOR Assimilation Fix] Transpiler replacements: {replacements}");
        }

        /// <summary>
        /// Safe remover: clamps, avoids seed/index, falls back to unit steps if needed.
        /// </summary>
        private static void SafeRemoveTroop(TroopRoster roster, CharacterObject troop, int numberToRemove)
        {
            if (roster == null || troop == null || numberToRemove <= 0) return;

            int have = roster.GetTroopCount(troop);
            if (have <= 0) return;

            int toRemove = System.Math.Min(numberToRemove, have);

            try
            {
                // Safe overload: no UniqueTroopDescriptor, no xp, engine resolves entry freshly.
                roster.RemoveTroop(troop, toRemove);
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[TOR Assimilation Fix] SafeRemoveTroop fallback: {ex.GetType().Name}: {ex.Message}");
                int left = System.Math.Min(toRemove, roster.GetTroopCount(troop));
                while (left-- > 0)
                {
                    roster.RemoveTroop(troop, 1);
                }
            }
        }
    }
}
