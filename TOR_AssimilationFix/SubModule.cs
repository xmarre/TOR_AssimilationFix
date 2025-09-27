using HarmonyLib;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;

namespace TOR_AssimilationFix
{
    public sealed class SubModule : MBSubModuleBase
    {
        private Harmony _harmony;

        protected override void OnSubModuleLoad()
        {
            _harmony = new Harmony("tor.assimilation.fix");
            try
            {
                _harmony.PatchAll();
                Debug.Print("[TOR Assimilation Fix] Harmony patches applied.");
            }
            catch (System.Exception ex)
            {
                Debug.Print($"[TOR Assimilation Fix] Patch error: {ex}");
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            try { _harmony?.UnpatchAll("tor.assimilation.fix"); }
            catch { /* ignore */ }
        }
    }
}
