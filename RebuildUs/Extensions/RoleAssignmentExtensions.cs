using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RebuildUs.Roles;
using UnityEngine;

namespace RebuildUs.Extensions;

internal static class RoleAssignmentExtensions
{
    private static int crewValues;
    private static int impValues;

    private static void AssignRoles()
    {
        var data = GetRoleAssignmentData();
        AssignSpecialRoles(data);
        AssignEnsuredRoles(data);
        AssignChanceRoles(data);
    }

    internal static RoleAssignmentData GetRoleAssignmentData()
    {
        // 役職を付与するプレイヤーの取得
        // クルーメイトにはクルーまたは第三
        // インポスターにはインポスター
        var crewmates = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
        var impostors = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

        // 最大最小の取得
        var crewmateMin = CustomOptionHolders.CrewmateRolesCountMin.GetSelection();
        var crewmateMax = CustomOptionHolders.CrewmateRolesCountMax.GetSelection();
        var impostorMin = CustomOptionHolders.ImpostorRolesCountMin.GetSelection();
        var impostorMax = CustomOptionHolders.ImpostorRolesCountMax.GetSelection();
        var neutralMin = CustomOptionHolders.NeutralRolesCountMin.GetSelection();
        var neutralMax = CustomOptionHolders.NeutralRolesCountMax.GetSelection();

        // 最小値のほうが大きいとき、最大値と同値にする
        if (crewmateMin > crewmateMax) crewmateMin = crewmateMax;
        if (impostorMin > impostorMax) impostorMin = impostorMax;
        if (neutralMin > neutralMax) neutralMin = neutralMax;

        // 第三陣営の設定を考慮して、強制的に全員にクルー役職を付与
        if (CustomOptionHolders.CrewmateRolesFill.GetBool())
        {
            crewmateMax = crewmates.Count - neutralMin;
            crewmateMin = crewmates.Count - neutralMax;
        }

        // 各陣営の設定に基づき、出現数を取得
        var crewCountSettings = Plugin.Instance.Random.Next(crewmateMin, crewmateMax + 1);
        var neutralCountSettings = Plugin.Instance.Random.Next(neutralMin, neutralMax + 1);
        var impCountSettings = Plugin.Instance.Random.Next(impostorMin, impostorMax + 1);
        // クルーで埋めるときの調整
        while (crewCountSettings + neutralCountSettings < crewmates.Count && CustomOptionHolders.CrewmateRolesFill.GetBool())
        {
            crewCountSettings++;
        }

        // プレイヤーの人数に合わせる
        var maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
        var maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
        var maxImpostorRoles = Mathf.Min(impostors.Count, impCountSettings);

        // プレイヤーに割り当てる役職を辞書に記入する
        // マフィアやラバーズなどの特殊な役職は含まない
        var impSettings = new Dictionary<RoleId, (int rate, int count)>();
        var crewSettings = new Dictionary<RoleId, (int rate, int count)>();
        var neutralSettings = new Dictionary<RoleId, (int rate, int count)>();

        crewSettings.Add(RoleId.Sheriff, CustomOptionHolders.SheriffSpawnRate.data);

        return new RoleAssignmentData
        {
            Crewmates = crewmates,
            Impostors = impostors,
            impSettings = impSettings,
            crewSettings = crewSettings,
            neutralSettings = neutralSettings,
            MaxCrewmateRoles = maxCrewmateRoles,
            MaxImpostorRoles = maxImpostorRoles,
            MaxNeutralRoles = maxNeutralRoles,
        };
    }

    private static void AssignSpecialRoles(RoleAssignmentData data)
    {
        // 特殊な条件で出現する役職はここに記入する
    }

    private static void AssignEnsuredRoles(RoleAssignmentData data)
    {
        // 出現確率が100%の役職を選択
        var ensuredCrewmateRoles = data.crewSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
        var ensuredImpostorRoles = data.impSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
        var ensuredNeutralRoles = data.neutralSettings.Where(x => x.Value.rate == 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

        // 割当先のプレイヤーが居なくなるか、割り当てる役職が無くなるまで続ける
        while (
            (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) ||
            (data.Crewmates.Count > 0 && (
                (data.MaxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) ||
                (data.MaxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
            )))
        {
            var rolesToAssign = new Dictionary<RoleType, List<RoleId>>();
            if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0) rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
            if (data.Crewmates.Count > 0 && data.MaxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0) rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
            if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

            // 割当処理
            var roleType = rolesToAssign.Keys.ElementAt(Plugin.Instance.Random.Next(0, rolesToAssign.Keys.Count));
            var players = roleType is RoleType.Crewmate or RoleType.Neutral ? data.Crewmates : data.Impostors;
            var index = Plugin.Instance.Random.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            SetRoleToRandomPlayer(rolesToAssign[roleType][index], players);
            rolesToAssign[roleType].RemoveAt(index);

            if (CustomOptionHolders.BlockedRolePairings.ContainsKey(roleId))
            {
                foreach (var blockedRoleId in CustomOptionHolders.BlockedRolePairings[roleId])
                {
                    // ブロックされた役職の確率を100%未満の場合は0に指定
                    if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = (0, 0);
                    if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = (0, 0);
                    if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = (0,0);
                    // 100%でも削除
                    foreach (var ensuredRolesList in rolesToAssign.Values)
                    {
                        ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                    }
                }
            }

            // 役職の制限を調整
            switch (roleType)
            {
                case RoleType.Crewmate:
                    data.MaxCrewmateRoles--;
                    crewValues -= 10;
                    break;
                case RoleType.Neutral:
                    data.MaxNeutralRoles--;
                    break;
                case RoleType.Impostor:
                    data.MaxImpostorRoles--;
                    impValues -= 10;
                    break;
            }
        }
    }

    private static void AssignChanceRoles(RoleAssignmentData data)
    {
        // 出現確率が0%以上100%未満のすべての役職を取得し、その重みに基づいたチケットプールを作成
        var crewmateTickets = data.crewSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
        var neutralTickets = data.neutralSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();
        var impostorTickets = data.impSettings.Where(x => x.Value.rate is > 0 and < 10).Select(x => Enumerable.Repeat(x.Key, x.Value.count)).SelectMany(x => x).ToList();

        // 割当先のプレイヤーが居なくなるか、割り当てる役職が無くなるまで続ける
        while (
                (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && impostorTickets.Count > 0) ||
                (data.Crewmates.Count > 0 && (
                    (data.MaxCrewmateRoles > 0 && crewmateTickets.Count > 0) ||
                    (data.MaxNeutralRoles > 0 && neutralTickets.Count > 0)
                )))
        {
            var rolesToAssign = new Dictionary<RoleType, List<RoleId>>();
            if (data.Crewmates.Count > 0 && data.MaxCrewmateRoles > 0 && crewmateTickets.Count > 0) rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
            if (data.Crewmates.Count > 0 && data.MaxNeutralRoles > 0 && neutralTickets.Count > 0) rolesToAssign.Add(RoleType.Neutral, neutralTickets);
            if (data.Impostors.Count > 0 && data.MaxImpostorRoles > 0 && impostorTickets.Count > 0) rolesToAssign.Add(RoleType.Impostor, impostorTickets);

            // 割当処理
            var roleType = rolesToAssign.Keys.ElementAt(Plugin.Instance.Random.Next(0, rolesToAssign.Keys.Count));
            var players = roleType is RoleType.Crewmate or RoleType.Neutral ? data.Crewmates : data.Impostors;
            var index = Plugin.Instance.Random.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            SetRoleToRandomPlayer(roleId, players);
            rolesToAssign[roleType].RemoveAll(x => x == roleId);

            if (CustomOptionHolders.BlockedRolePairings.ContainsKey(roleId))
            {
                foreach (var blockedRoleId in CustomOptionHolders.BlockedRolePairings[roleId])
                {
                    // ブロックされた役職を除外
                    crewmateTickets.RemoveAll(x => x == blockedRoleId);
                    neutralTickets.RemoveAll(x => x == blockedRoleId);
                    impostorTickets.RemoveAll(x => x == blockedRoleId);
                }
            }

            // 役職の制限を調整
            switch (roleType)
            {
                case RoleType.Crewmate: data.MaxCrewmateRoles--; break;
                case RoleType.Neutral: data.MaxNeutralRoles--; break;
                case RoleType.Impostor: data.MaxImpostorRoles--; break;
            }
        }
    }

    private static byte SetRoleToRandomPlayer(RoleId roleId, List<PlayerControl> playerList, bool removePlayer = true)
    {
        var index = Plugin.Instance.Random.Next(0, playerList.Count);
        var playerId = playerList[index].PlayerId;

        if (removePlayer) playerList.RemoveAt(index);

        var rpc = RPCProcedure.SendRPC(CustomRPC.SetRole);
        rpc.Write((byte)roleId);
        rpc.Write(playerId);
        RPCProcedure.SetRole((byte)roleId, playerId);
        return playerId;
    }
}

internal class RoleAssignmentData
{
    internal List<PlayerControl> Crewmates { get; set; }
    internal List<PlayerControl> Impostors { get; set; }
    internal Dictionary<RoleId, (int rate, int count)> impSettings = [];
    internal Dictionary<RoleId, (int rate, int count)> neutralSettings = [];
    internal Dictionary<RoleId, (int rate, int count)> crewSettings = [];
    internal int MaxCrewmateRoles { get; set; }
    internal int MaxNeutralRoles { get; set; }
    internal int MaxImpostorRoles { get; set; }
}