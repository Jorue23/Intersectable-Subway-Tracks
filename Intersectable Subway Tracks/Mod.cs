using Colossal.Logging;
using Game;
using Game.Modding;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Intersectable_Subway_Tracks
{
    public class Mod : IMod
    {
        public static ILog log = LogManager.GetLogger($"{nameof(Intersectable_Subway_Tracks)}.{nameof(Mod)}").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            var harmony = new Harmony("Intersectable Subway Tracks");
            harmony.PatchAll();

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
                log.Info($"Current mod asset at {asset.path}");
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
        }
    }

    [HarmonyPatch(typeof(NetInitializeSystem), "OnUpdate")]
    public class NetInitializeSystemPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int patchCount = 0;

            for (int i = 0; i < codes.Count; i++)
            {
                // Find Layer.SubwayTrack constant (2048)
                if (codes[i].opcode == OpCodes.Ldc_I4 &&
                    codes[i].operand != null &&
                    codes[i].operand.Equals(2048))
                {
                    // Replace with SubwayTrack | Road | Pathway (2177)
                    codes[i].operand = 2177;
                    patchCount++;
                }
            }
            return codes;
        }
    }
}
