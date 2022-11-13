﻿using BattleTech;
using Harmony;
using System;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(BaseComponentRef), "RefreshComponentDef")]
    public static class BaseComponentRef_RefreshComponentDef
    {
        [HarmonyPostfix]
        public static void SetFixed(BaseComponentRef __instance)
        {
            try
            {
                if (__instance.Def == null)
                    return;
                if (__instance.Def.Flags<CCFlags>().NoRemove)
                {
                    __instance.IsFixed = true;
                }
            }
            catch (Exception e)
            {
                Logging.Error?.Log($"Error in {__instance?.ComponentDefID} refreshing, check item", e);
            }
        }
    }
}