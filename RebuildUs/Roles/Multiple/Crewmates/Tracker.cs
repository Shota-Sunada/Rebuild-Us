using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RebuildUs.Objects;
using UnityEngine;
using static RebuildUs.Patches.PlayerControlFixedUpdatePatch;

namespace RebuildUs.Roles;

[HarmonyPatch]
public class Tracker : RoleBase<Tracker>
{
    public static Color Color = new Color32(100, 58, 220, byte.MaxValue);

    public static float updateInterval => CustomOptionHolder.trackerUpdateInterval.getFloat();
    public static bool resetTargetAfterMeeting => CustomOptionHolder.trackerResetTargetAfterMeeting.getBool();
    public static bool canTrackCorpses => CustomOptionHolder.trackerCanTrackCorpses.getBool();
    public static float corpsesTrackingCooldown => CustomOptionHolder.trackerCorpsesTrackingCooldown.getFloat();
    public static float corpsesTrackingDuration => CustomOptionHolder.trackerCorpsesTrackingDuration.getFloat();
    public static int trackingMode => CustomOptionHolder.trackerTrackingMethod.getSelection();

    public List<Arrow> localArrows = [];

    public PlayerControl currentTarget;
    public PlayerControl tracked;
    public float corpsesTrackingTimer = 0f;
    public bool usedTracker = false;
    public float timeUntilUpdate = 0f;
    public Arrow arrow = new(Color.blue);

    public GameObject DangerMeterParent;
    public DangerMeter Meter;

    private static CustomButton trackerTrackPlayerButton;
    private static CustomButton trackerTrackCorpsesButton;

    public static List<Vector3> deadBodyPositions = [];

    public Tracker()
    {
        baseRoleId = roleId = RoleId.Tracker;
    }

    public override void OnMeetingStart() { }
    public override void OnMeetingEnd() { }
    public override void FixedUpdate()
    {
        currentTarget = setTarget();
        if (!usedTracker) setPlayerOutline(currentTarget, Color);

        // Handle player tracking
        if (arrow?.arrow != null)
        {
            if (!exists || !PlayerControl.LocalPlayer.isRole(RoleId.Tracker))
            {
                arrow.arrow.SetActive(false);
                if (DangerMeterParent) DangerMeterParent.SetActive(false);
                return;
            }

            if (tracked != null && !player.isDead())
            {
                timeUntilUpdate -= Time.fixedDeltaTime;

                if (timeUntilUpdate <= 0f)
                {
                    bool trackedOnMap = !tracked.isDead();
                    Vector3 position = tracked.transform.position;
                    if (!trackedOnMap)
                    { // Check for dead body
                        var body = Object.FindObjectsOfType<DeadBody>().FirstOrDefault(b => b.ParentId == tracked.PlayerId);
                        if (body != null)
                        {
                            trackedOnMap = true;
                            position = body.transform.position;
                        }
                    }

                    if (trackingMode is 1 or 2) UpdateProximity(position);
                    if (trackingMode is 0 or 2)
                    {
                        arrow.Update(position);
                        arrow.arrow.SetActive(trackedOnMap);
                    }
                    timeUntilUpdate = updateInterval;
                }
                else
                {
                    if (trackingMode is 0 or 2) arrow.Update();
                }
            }
            else if (player.isDead())
            {
                DangerMeterParent?.SetActive(false);
                Meter?.gameObject.SetActive(false);
            }
        }

        // Handle corpses tracking
        if (exists && player.isRole(RoleId.Tracker) && corpsesTrackingTimer >= 0f && !player.isDead())
        {
            bool arrowsCountChanged = localArrows.Count != deadBodyPositions.Count();
            int index = 0;

            if (arrowsCountChanged)
            {
                foreach (var arrow in localArrows)
                {
                    Object.Destroy(arrow.arrow);
                }
                localArrows = [];
            }
            foreach (var position in deadBodyPositions)
            {
                if (arrowsCountChanged)
                {
                    localArrows.Add(new(Color));
                    localArrows[index].arrow.SetActive(true);
                }
                localArrows[index]?.Update(position);
                index++;
            }
        }
        else if (localArrows.Count > 0)
        {
            foreach (var arrow in localArrows)
            {
                Object.Destroy(arrow.arrow);
            }
            localArrows = [];
        }
    }
    public override void HudUpdate()
    {
        var dt = Time.deltaTime;
        corpsesTrackingTimer -= dt;
    }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void MakeButtons(HudManager hm)
    {
        trackerTrackPlayerButton = new CustomButton(
            () =>
            {
                using var writer = RPCProcedure.SendRPC(CustomRPC.TrackerUsedTracker);
                writer.Write(player.PlayerId);
                writer.Write(currentTarget.PlayerId);
                RPCProcedure.trackerUsedTracker(player.PlayerId, currentTarget.PlayerId);
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Tracker) && !PlayerControl.LocalPlayer.isDead(); },
            () => { return PlayerControl.LocalPlayer.CanMove && currentTarget != null && !usedTracker; },
            () =>
            {
                if (resetTargetAfterMeeting) resetTracked();
                else if (currentTarget != null && currentTarget.isDead()) currentTarget = null;
            },
            getButtonSprite(),
            ButtonOffset.LowerRight,
            hm,
            hm.UseButton,
            KeyCode.F
        );

        trackerTrackCorpsesButton = new CustomButton(
            () =>
            {
                corpsesTrackingTimer = corpsesTrackingDuration;
            },
            () => { return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.isRole(RoleId.Tracker) && !PlayerControl.LocalPlayer.isDead() && canTrackCorpses; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
                trackerTrackCorpsesButton.isEffectActive = false;
                trackerTrackCorpsesButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
            },
            getTrackCorpsesButtonSprite(),
            ButtonOffset.LowerCenter,
            hm,
            hm.UseButton,
            KeyCode.G,
            true,
            corpsesTrackingDuration,
            () =>
            {
                trackerTrackCorpsesButton.Timer = trackerTrackCorpsesButton.MaxTimer;
            }
        );
    }
    public override void SetButtonCooldowns()
    {
        trackerTrackPlayerButton.MaxTimer = 0f;
        trackerTrackCorpsesButton.MaxTimer = corpsesTrackingCooldown;
        trackerTrackCorpsesButton.effectDuration = corpsesTrackingDuration;
    }

    public override void Clear()
    {
        players = [];
        resetTracked();
        timeUntilUpdate = 0f;
        if (localArrows != null)
        {
            foreach (var arrow in localArrows)
            {
                if (arrow?.arrow != null)
                {
                    Object.Destroy(arrow.arrow);
                }
            }
        }
        corpsesTrackingTimer = 0f;
        if (DangerMeterParent)
        {
            Meter.gameObject.Destroy();
            DangerMeterParent.Destroy();
        }
        deadBodyPositions = [];
    }

    public void resetTracked()
    {
        currentTarget = tracked = null;
        usedTracker = false;
        if (arrow?.arrow != null) Object.Destroy(arrow.arrow);
        arrow = new Arrow(Color.blue);
        arrow.arrow?.SetActive(false);
    }

    public void UpdateProximity(Vector3 position)
    {
        if (!GameManager.Instance.GameHasStarted) return;

        if (DangerMeterParent == null)
        {
            DangerMeterParent = Object.Instantiate(GameObject.Find("ImpostorDetector"), HudManager.Instance.transform);
            Meter = DangerMeterParent.transform.GetChild(0).GetComponent<DangerMeter>();
            DangerMeterParent.transform.localPosition = new(3.7f, -1.6f, 0);
            var backgroundRend = DangerMeterParent.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
            backgroundRend.color = backgroundRend.color.SetAlpha(0.5f);
        }
        DangerMeterParent.SetActive(MeetingHud.Instance == null && LobbyBehaviour.Instance == null && !player.isDead() && tracked != null);
        Meter.gameObject.SetActive(MeetingHud.Instance == null && LobbyBehaviour.Instance == null && !player.isDead() && tracked != null);
        if (player.isDead()) return;
        if (tracked == null)
        {
            Meter.SetDangerValue(0, 0);
            return;
        }
        if (DangerMeterParent.transform.localPosition.x != 3.7f) DangerMeterParent.transform.localPosition = new(3.7f, -1.6f, 0);
        float num = float.MaxValue;
        float dangerLevel1;
        float dangerLevel2;

        float sqrMagnitude = (position - player.transform.position).sqrMagnitude;
        if (sqrMagnitude < (55 * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod) && num > sqrMagnitude)
        {
            num = sqrMagnitude;
        }

        dangerLevel1 = Mathf.Clamp01((55 - num) / (55 - 15 * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod));
        dangerLevel2 = Mathf.Clamp01((15 - num) / (15 * GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod));

        Meter.SetDangerValue(dangerLevel1, dangerLevel2);
    }

    private static Sprite trackCorpsesButtonSprite;
    public static Sprite getTrackCorpsesButtonSprite()
    {
        if (trackCorpsesButtonSprite) return trackCorpsesButtonSprite;
        trackCorpsesButtonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.PathfindButton.png", 115f);
        return trackCorpsesButtonSprite;
    }

    private static Sprite buttonSprite;
    public static Sprite getButtonSprite()
    {
        if (buttonSprite) return buttonSprite;
        buttonSprite = Helpers.loadSpriteFromResources("RebuildUs.Resources.TrackerButton.png", 115f);
        return buttonSprite;
    }
}