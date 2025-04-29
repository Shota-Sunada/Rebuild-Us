using UnityEngine;

namespace RebuildUs.Modules;

public class CustomRoleOption : CustomOption
{
    public CustomOption countOption = null;
    public bool roleEnabled = true;

    public override bool Enabled { get { return Helpers.IsRoleEnabled() && GetBool() && SelectedIndex > 0; } }

    public int Rate { get { return Enabled ? SelectedIndex : 0; } }

    public int Count
    {
        get
        {
            if (!Enabled)
            {
                return 0;
            }

            if (countOption != null)
            {
                return Mathf.RoundToInt(countOption.GetFloat());
            }

            return 1;
        }
    }

    public (int rate, int count) Data { get { return (Rate, Count); } }

    public CustomRoleOption(int id, int id2, CustomOptionType type, (string key, Color? color) title, int max = 15, bool roleEnabled = true) :
        base(id, type, title, CustomOptionHolders.Percents, "", null, true, null, UnitType.UnitPercent)
    {
        this.roleEnabled = roleEnabled;

        if (max <= 0 || !roleEnabled)
        {
            this.roleEnabled = false;
        }

        if (max > 1)
        {
            countOption = Create(id2, CustomOptionType.Crewmate, "RoleNumAssigned", 1f, 1f, max, 1f, this);
        }
    }
}