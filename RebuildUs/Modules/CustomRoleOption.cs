using UnityEngine;

namespace RebuildUs.Modules;

public class CustomRoleOption : CustomOption
{
    public CustomOption countOption = null;
    public bool roleEnabled = true;

    public override bool Enabled => Helpers.RolesEnabled && getBool() && selection > 0;

    public int Rate => Enabled ? selection : 0;

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
                return Mathf.RoundToInt(countOption.getFloat());
            }

            return 1;
        }
    }

    public (int rate, int count) Data => (Rate, Count);

    public CustomRoleOption(int id, int id2, CustomOptionType type, (string key, Color? color) title, int max = 15, bool roleEnabled = true) :
        base(id, type, title, CustomOptionHolder.Percents, "", null, true, null, UnitType.UnitPercent)
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