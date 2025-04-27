using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using RebuildUs.Patches;

using UnityEngine;
using Object = UnityEngine.Object;

namespace RebuildUs;

internal static class SubmergedCompatibility
{
    internal static class Classes
    {
        internal const string ElevatorMover = "ElevatorMover";
    }

    internal const string SUBMERGED_GUID = "Submerged";
    internal const ShipStatus.MapType SUBMERGED_MAP_TYPE = (ShipStatus.MapType)6;

    internal static SemanticVersioning.Version Version { get; private set; }
    internal static bool Loaded { get; private set; }
    internal static bool LoadedExternally { get; private set; }
    internal static BasePlugin Plugin { get; private set; }
    internal static Assembly Assembly { get; private set; }
    internal static Type[] Types { get; private set; }
    internal static Dictionary<string, Type> InjectedTypes { get; private set; }

    internal static MonoBehaviour SubmarineStatus { get; private set; }

    internal static bool IsSubmerged { get; private set; }


    internal static void SetupMap(ShipStatus map)
    {
        if (map == null)
        {
            IsSubmerged = false;
            SubmarineStatus = null;
            return;
        }

        IsSubmerged = map.Type == SUBMERGED_MAP_TYPE;
        if (!IsSubmerged) return;

        SubmarineStatus = map.GetComponent(Il2CppType.From(SubmarineStatusType))?.TryCast(SubmarineStatusType) as MonoBehaviour;
    }

    private static Type SubmarineStatusType;
    private static MethodInfo CalculateLightRadiusMethod;

    private static MethodInfo RpcRequestChangeFloorMethod;
    private static Type FloorHandlerType;
    private static MethodInfo GetFloorHandlerMethod;

    private static Type VentPatchDataType;
    private static PropertyInfo InTransitionField;

    private static Type CustomTaskTypesType;
    private static FieldInfo RetrieveOxygenMaskField;
    internal static TaskTypes RetrieveOxygenMask;
    private static Type SubmarineOxygenSystemType;
    private static MethodInfo SubmarineOxygenSystemInstanceField;
    private static MethodInfo RepairDamageMethod;

    internal static bool TryLoadSubmerged()
    {
        try
        {
            Plugin.Instance.Logger.LogMessage("Trying to load Submerged...");
            var thisAsm = Assembly.GetCallingAssembly();
            var resourceName = thisAsm.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith("Submerged.dll"));
            if (resourceName == default) return false;

            using var submergedStream = thisAsm.GetManifestResourceStream(resourceName)!;
            byte[] assemblyBuffer = new byte[submergedStream.Length];
            submergedStream.Read(assemblyBuffer, 0, assemblyBuffer.Length);
            Assembly = Assembly.Load(assemblyBuffer);

            var pluginType = Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(BasePlugin)));
            Plugin = (BasePlugin)Activator.CreateInstance(pluginType!);
            Plugin.Load();

            Version = pluginType.GetCustomAttribute<BepInPlugin>().Version.BaseVersion(); ;

            IL2CPPChainloader.Instance.Plugins[SUBMERGED_GUID] = new();
            return true;
        }
        catch (Exception e)
        {
            Plugin.Instance.Logger.LogError(e);
        }

        return false;
    }

    internal static void Initialize()
    {
        Loaded = IL2CPPChainloader.Instance.Plugins.TryGetValue(SUBMERGED_GUID, out PluginInfo plugin);
        if (!Loaded)
        {
            if (TryLoadSubmerged()) Loaded = true;
            else return;
        }
        else
        {
            LoadedExternally = true;
            Plugin = plugin!.Instance as BasePlugin;
            Version = plugin.Metadata.Version.BaseVersion();
            Assembly = Plugin!.GetType().Assembly;
        }

        Types = AccessTools.GetTypesFromAssembly(Assembly);

        InjectedTypes = (Dictionary<string, Type>)AccessTools.PropertyGetter(Types.FirstOrDefault(t => t.Name == "ComponentExtensions"), "RegisteredTypes").Invoke(null, Array.Empty<object>());

        SubmarineStatusType = Types.First(t => t.Name == "SubmarineStatus");
        CalculateLightRadiusMethod = AccessTools.Method(SubmarineStatusType, "CalculateLightRadius");

        FloorHandlerType = Types.First(t => t.Name == "FloorHandler");
        GetFloorHandlerMethod = AccessTools.Method(FloorHandlerType, "GetFloorHandler", [typeof(PlayerControl)]);
        RpcRequestChangeFloorMethod = AccessTools.Method(FloorHandlerType, "RpcRequestChangeFloor");

        VentPatchDataType = Types.First(t => t.Name == "VentPatchData");

        InTransitionField = AccessTools.Property(VentPatchDataType, "InTransition");

        CustomTaskTypesType = Types.First(t => t.Name == "CustomTaskTypes");
        RetrieveOxygenMaskField = AccessTools.Field(CustomTaskTypesType, "RetrieveOxygenMask");
        var RetrieveOxygenMaskTaskTypeField = AccessTools.Field(CustomTaskTypesType, "taskType");
        var OxygenMaskCustomTaskType = RetrieveOxygenMaskField.GetValue(null);
        RetrieveOxygenMask = (TaskTypes)RetrieveOxygenMaskTaskTypeField.GetValue(OxygenMaskCustomTaskType);

        SubmarineOxygenSystemType = Types.First(t => t.Name == "SubmarineOxygenSystem" && t.Namespace == "Submerged.Systems.Oxygen");
        SubmarineOxygenSystemInstanceField = AccessTools.PropertyGetter(SubmarineOxygenSystemType, "Instance");
        RepairDamageMethod = AccessTools.Method(SubmarineOxygenSystemType, "RepairDamage");
    }

    internal static MonoBehaviour AddSubmergedComponent(this GameObject obj, string typeName)
    {
        if (!Loaded) return obj.AddComponent<MissingSubmergedBehaviour>();
        bool validType = InjectedTypes.TryGetValue(typeName, out Type type);
        return validType ? obj.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>() : obj.AddComponent<MissingSubmergedBehaviour>();
    }

    internal static float GetSubmergedNeutralLightRadius(bool isImpostor)
    {
        if (!Loaded) return 0;
        return (float)CalculateLightRadiusMethod.Invoke(SubmarineStatus, [null, true, isImpostor]);
    }

    internal static void ChangeFloor(bool toUpper)
    {
        if (!Loaded) return;
        MonoBehaviour _floorHandler = ((Component)GetFloorHandlerMethod.Invoke(null, [PlayerControl.LocalPlayer])).TryCast(FloorHandlerType) as MonoBehaviour;
        RpcRequestChangeFloorMethod.Invoke(_floorHandler, [toUpper]);
    }

    internal static bool GetInTransition()
    {
        if (!Loaded) return false;
        return (bool)InTransitionField.GetValue(null);
    }

    internal static void RepairOxygen()
    {
        if (!Loaded) return;
        try
        {
            ShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);
            RepairDamageMethod.Invoke(SubmarineOxygenSystemInstanceField.Invoke(null, Array.Empty<object>()), [PlayerControl.LocalPlayer, 64]);
        }
        catch (NullReferenceException)
        {
            Plugin.Instance.Logger.LogMessage("null reference in engineer oxygen fix");
        }
    }
}

internal class MissingSubmergedBehaviour : MonoBehaviour
{
    static MissingSubmergedBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<MissingSubmergedBehaviour>();
    internal MissingSubmergedBehaviour(IntPtr ptr) : base(ptr) { }
}