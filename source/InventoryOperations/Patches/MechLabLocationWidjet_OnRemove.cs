﻿using System.Collections.Generic;
using System.Reflection.Emit;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget), "OnRemoveItem")]
public static class MechLabLocationWidjet_OnRemove
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var i = 0;

        foreach (var codeInstruction in instructions)
        {
               
            if (i >=3 && i <=8)
                yield return new(OpCodes.Nop);
            else
                yield return codeInstruction;

            i++;
        }

    }
}