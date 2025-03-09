using System;
using System.Collections.Generic;
using System.Reflection;

namespace RebuildUs.Modules;

internal class CustomOptions
{
    internal static List<CustomOptionBase> AllOptions { get; } = [];

    internal static void RegisterOptions()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var option = field.GetCustomAttribute<CustomOptionBase>();
                if (option == null) continue;

                AllOptions.Add(option);
                RebuildUsPlugin.Instance.Logger.LogMessage($"Registering option: {nameof(field)}");
            }
        }
    }

    internal static void Initialize()
    {

    }
}

[AttributeUsage(AttributeTargets.Field)]
internal class CustomOptionBase : Attribute
{
    internal uint Id { get; }
    internal uint ParentId { get; }
    internal string NameKey { get; }
    internal string Format { get; } = "";
}

[AttributeUsage(AttributeTargets.Field)]
internal class CustomNumericOptionAttribute : CustomOptionBase
{
    internal float MinValue { get; }
    internal float MaxValue { get; }
    internal float Interval { get; }
    internal float DefaultValue { get; }
}

[AttributeUsage(AttributeTargets.Field)]
internal class CustomBooleanOptionAttribute : CustomOptionBase
{
    internal bool DefaultValue { get; }
}

[AttributeUsage(AttributeTargets.Field)]
internal class CustomStringOptionAttribute : CustomOptionBase
{
    internal int DefaultValue { get; }
    internal string[] Values { get; }
}