using HarmonyLib;
using System.Collections.Generic;
using Il2CppSystem;

namespace RebuildUs.Utilities;

internal static class MapUtilities
{
    internal static ShipStatus CachedShipStatus = ShipStatus.Instance;

    internal static void MapDestroyed()
    {
        CachedShipStatus = ShipStatus.Instance;
        _systems.Clear();
    }

    private static readonly Dictionary<SystemTypes, Object> _systems = [];
    internal static Dictionary<SystemTypes, Object> Systems
    {
        get
        {
            if (_systems.Count == 0) GetSystems();
            return _systems;
        }
    }

    private static void GetSystems()
    {
        if (!CachedShipStatus) return;

        var systems = CachedShipStatus.Systems;
        if (systems.Count <= 0) return;

        foreach (var systemTypes in SystemTypeHelpers.AllTypes)
        {
            if (!systems.ContainsKey(systemTypes)) continue;
            _systems[systemTypes] = systems[systemTypes].TryCast<Object>();
        }
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
internal static class ShipStatus_Awake_Patch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    internal static void Postfix(ShipStatus __instance)
    {
        MapUtilities.CachedShipStatus = __instance;
        // SubmergedCompatibility.SetupMap(__instance);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.OnDestroy))]
internal static class ShipStatus_OnDestroy_Patch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    internal static void Postfix()
    {
        MapUtilities.CachedShipStatus = null;
        MapUtilities.MapDestroyed();
        // SubmergedCompatibility.SetupMap(null);
    }
}