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

            // look for the specific pattern:
            // ldc.i4 2048 (Layer.SubwayTrack)
            // stloc.s (store to layer2 variable)

            for (int i = 0; i < codes.Count - 1; i++)
            {
                // check for ldc.i4 2048
                if (codes[i].opcode == OpCodes.Ldc_I4 &&
                    codes[i].operand != null &&
                    codes[i].operand.Equals(2048))
                {
                    // check if next instruction is storing to a local variable
                    var nextOp = codes[i + 1].opcode;

                    // only patch if it's followed by stloc
                    // and if we haven't already patched
                    if (patchCount == 0 &&
                        (nextOp == OpCodes.Stloc ||
                         nextOp == OpCodes.Stloc_0 ||
                         nextOp == OpCodes.Stloc_1 ||
                         nextOp == OpCodes.Stloc_2 ||
                         nextOp == OpCodes.Stloc_3 ||
                         nextOp == OpCodes.Stloc_S))
                    {
                        // verify this isn't the first subway assignment by checking context
                        // look back to see if we recently saw another 2048
                        bool isSecondAssignment = false;
                        for (int j = i - 1; j >= 0 && j >= i - 10; j--)
                        {
                            if (codes[j].opcode == OpCodes.Ldc_I4 &&
                                codes[j].operand != null &&
                                codes[j].operand.Equals(2048))
                            {
                                isSecondAssignment = true;
                                break;
                            }
                        }

                        if (isSecondAssignment)
                        {
                            codes[i].operand = 2177;
                            patchCount++;
                        }
                    }
                }
            }
            return codes;
        }
    }
}
