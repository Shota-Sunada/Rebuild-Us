using System.Linq;
using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Hacker : RoleBase<Hacker>
{
    public static Color Color = new Color32(117, 250, 76, byte.MaxValue);

    public static float cooldown => CustomOptionHolder.hackerCooldown.getFloat();
    public static float duration => CustomOptionHolder.hackerHackingDuration.getFloat();
    public static bool onlyColorType => CustomOptionHolder.hackerOnlyColorType.getBool();
    public static float toolsNumber => CustomOptionHolder.hackerToolsNumber.getFloat();
    public static int rechargeTasksNumber => CustomOptionHolder.hackerRechargeTasksNumber.getInt();
    public static int rechargedTasks => CustomOptionHolder.hackerRechargeTasksNumber.getInt();
    public static int chargesVitals => Mathf.RoundToInt(CustomOptionHolder.hackerToolsNumber.getFloat() / 2);
    public static int chargesAdminTable => Mathf.RoundToInt(CustomOptionHolder.hackerToolsNumber.getFloat() / 2);
    public static bool cantMove => CustomOptionHolder.hackerNoMove.getBool();

    private static CustomButton hackerButton;
    public static CustomButton hackerVitalsButton;
    public static CustomButton hackerAdminTableButton;
    public static TMPro.TMP_Text hackerAdminTableChargesText;
    public static TMPro.TMP_Text hackerVitalsChargesText;

    public float hackerTimer = 0f;
    public int adminTableCharge = 0;
    public int vitalCharge = 0;
    public int tasksRecharged = 0;

    public Minigame vitals = null;
    public Minigame doorLog = null;

    public Hacker()
    {
        baseRoleId = roleId = RoleId.Hacker;
        hackerTimer = 0f;
        adminTableCharge = chargesAdminTable;
        vitalCharge = chargesVitals;
        tasksRecharged = rechargedTasks;
        vitals = null;
        doorLog = null;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && !PlayerControl.LocalPlayer.isDead())
        {
            var (playerCompleted, _) = TasksHandler.taskInfo(PlayerControl.LocalPlayer.Data);
            if (playerCompleted == tasksRecharged)
            {
                tasksRecharged += rechargeTasksNumber;
                if (toolsNumber > chargesVitals) vitalCharge++;
                if (toolsNumber > chargesAdminTable) adminTableCharge++;
            }
        }
    }
    public override void HudUpdate()
    {
        var dt = Time.deltaTime;
        hackerTimer -= dt;
    }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        hackerButton = new CustomButton(
            () =>
            {
                hackerTimer = duration;
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && !PlayerControl.LocalPlayer.isDead(); },
            () => { return true; },
            () =>
            {
                hackerButton.Timer = hackerButton.MaxTimer;
                hackerButton.isEffectActive = false;
                hackerButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            getButtonSprite(),
            ButtonOffset.UpperRight,
            hm,
            hm.UseButton,
            KeyCode.F,
            true,
            0f,
            () => { hackerButton.Timer = hackerButton.MaxTimer; }
        );

        hackerAdminTableButton = new CustomButton(
            () =>
            {
                if (!MapBehaviour.Instance || !MapBehaviour.Instance.isActiveAndEnabled)
                {
                    HudManager __instance = FastDestroyableSingleton<HudManager>.Instance;
                    __instance.InitMap();
                    MapBehaviour.Instance.ShowCountOverlay(allowedToMove: true, showLivePlayerPosition: true, includeDeadBodies: true);
                }
                if (cantMove) PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement
                adminTableCharge--;
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && !PlayerControl.LocalPlayer.isDead(); },
            () =>
            {
                if (hackerAdminTableChargesText != null) hackerAdminTableChargesText.text = $"{chargesAdminTable} / {toolsNumber}";
                return chargesAdminTable > 0;
            },
            () =>
            {
                hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                hackerAdminTableButton.isEffectActive = false;
                hackerAdminTableButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            getAdminSprite(),
            ButtonOffset.LowerRight,
            hm,
            hm.UseButton,
            KeyCode.G,
            true,
            0f,
            () =>
            {
                hackerAdminTableButton.Timer = hackerAdminTableButton.MaxTimer;
                if (!hackerVitalsButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                if (MapBehaviour.Instance && MapBehaviour.Instance.isActiveAndEnabled) MapBehaviour.Instance.Close();
            },
            GameOptionsManager.Instance.currentNormalGameOptions.MapId == 3,
            "ADMIN"
        );

        // Hacker Admin Table Charges
        hackerAdminTableChargesText = Object.Instantiate(hackerAdminTableButton.actionButton.cooldownTimerText, hackerAdminTableButton.actionButton.cooldownTimerText.transform.parent);
        hackerAdminTableChargesText.text = "";
        hackerAdminTableChargesText.enableWordWrapping = false;
        hackerAdminTableChargesText.transform.localScale = Vector3.one * 0.5f;
        hackerAdminTableChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        hackerVitalsButton = new CustomButton(
            () =>
            {
                if (GameOptionsManager.Instance.currentNormalGameOptions.MapId != 1)
                {
                    if (vitals == null)
                    {
                        var e = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals") || x.gameObject.name.Contains("Vitals"));
                        if (e == null || Camera.main == null) return;
                        vitals = Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                    }
                    vitals.transform.SetParent(Camera.main.transform, false);
                    vitals.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                    vitals.Begin(null);
                }
                else
                {
                    if (doorLog == null)
                    {
                        var e = Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("SurvLogConsole"));
                        if (e == null || Camera.main == null) return;
                        doorLog = Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                    }
                    doorLog.transform.SetParent(Camera.main.transform, false);
                    doorLog.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                    doorLog.Begin(null);
                }

                if (cantMove) PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.NetTransform.Halt(); // Stop current movement

                vitalCharge--;
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Hacker) && !PlayerControl.LocalPlayer.isDead() && GameOptionsManager.Instance.currentGameOptions.MapId != 0 && GameOptionsManager.Instance.currentNormalGameOptions.MapId != 3; },
            () =>
            {
                if (hackerVitalsChargesText != null) hackerVitalsChargesText.text = $"{chargesVitals} / {toolsNumber}";
                hackerVitalsButton.actionButton.graphic.sprite = Helpers.isMira() ? getLogSprite() : getVitalsSprite();
                hackerVitalsButton.actionButton.OverrideText(Helpers.isMira() ? "DOORLOG" : "VITALS");
                return chargesVitals > 0;
            },
            () =>
            {
                hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                hackerVitalsButton.isEffectActive = false;
                hackerVitalsButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            getVitalsSprite(),
            ButtonOffset.LowerCenter,
            hm,
            hm.UseButton,
            KeyCode.H,
            true,
            0f,
            () =>
            {
                hackerVitalsButton.Timer = hackerVitalsButton.MaxTimer;
                if (!hackerAdminTableButton.isEffectActive) PlayerControl.LocalPlayer.moveable = true;
                if (Minigame.Instance)
                {
                    if (Helpers.isMira()) doorLog.ForceClose();
                    else vitals.ForceClose();
                }
            },
            false,
            Helpers.isMira() ? "DOORLOG" : "VITALS"
        );

        // Hacker Vitals Charges
        hackerVitalsChargesText = Object.Instantiate(hackerVitalsButton.actionButton.cooldownTimerText, hackerVitalsButton.actionButton.cooldownTimerText.transform.parent);
        hackerVitalsChargesText.text = "";
        hackerVitalsChargesText.enableWordWrapping = false;
        hackerVitalsChargesText.transform.localScale = Vector3.one * 0.5f;
        hackerVitalsChargesText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
    }
    public override void SetButtonCooldowns()
    {
        hackerButton.MaxTimer = cooldown;
        hackerVitalsButton.MaxTimer = cooldown;
        hackerAdminTableButton.MaxTimer = cooldown;
        hackerButton.effectDuration = duration;
        hackerVitalsButton.effectDuration = duration;
        hackerAdminTableButton.effectDuration = duration;
    }

    public override void Clear()
    {
        players = [];
    }

    private static Sprite buttonSprite;
    private static Sprite vitalsSprite;
    private static Sprite logSprite;
    private static Sprite adminSprite;

    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.HackerButton.png", 115f);
        return buttonSprite;
    }

    public static Sprite getVitalsSprite()
    {
        if (vitalsSprite) return vitalsSprite;
        vitalsSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.VitalsButton].Image;
        return vitalsSprite;
    }

    public static Sprite getLogSprite()
    {
        if (logSprite) return logSprite;
        logSprite = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.DoorLogsButton].Image;
        return logSprite;
    }

    public static Sprite getAdminSprite()
    {
        byte mapId = GameOptionsManager.Instance.currentNormalGameOptions.MapId;
        UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
        if (Helpers.isSkeld() || mapId == 3) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
        else if (Helpers.isMira()) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
        else if (Helpers.isAirship()) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
        else if (Helpers.isFungle()) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton];  // Hacker can Access the Admin panel on Fungle
        adminSprite = button.Image;
        return adminSprite;
    }
}