using Il2CppSystem.Runtime.ExceptionServices;
using Rewired;
using System;
using System.Collections.Generic;
using RebuildUs.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RebuildUs.RebuildUs;

namespace RebuildUs.Objects;

public class CustomButton
{
    public static List<CustomButton> buttons = [];
    public ActionButton actionButton;
    public Vector3 PositionOffset;
    public Vector3 LocalScale = Vector3.one;
    public float MaxTimer = float.MaxValue;
    public float Timer = 0f;
    public bool effectCancellable = false;
    public float DeputyTimer = 0f;
    private Action OnClick;
    private Action InitialOnClick;
    private Action OnMeetingEnds;
    public Func<bool> HasButton;
    public Func<bool> CouldUse;
    private Action OnEffectEnds;
    public bool HasEffect;
    public bool isEffectActive = false;
    public bool showButtonText = false;
    public string buttonText = null;
    public float EffectDuration;
    public Sprite Sprite;
    public HudManager hudManager;
    public bool mirror;
    public KeyCode? hotkey;
    public bool isHandcuffed = false;
    private static readonly int Desat = Shader.PropertyToID("_Desat");

    public static class ButtonPositions
    {
        public static readonly Vector3 lowerRowRight = new(-2f, -0.06f, 0);  // Not usable for imps beacuse of new button positions!
        public static readonly Vector3 lowerRowCenter = new(-3f, -0.06f, 0);
        public static readonly Vector3 lowerRowLeft = new(-4f, -0.06f, 0);
        public static readonly Vector3 upperRowRight = new(0f, 1f, 0f);  // Not usable for imps beacuse of new button positions!
        public static readonly Vector3 upperRowCenter = new(-1f, 1f, 0f);  // Not usable for imps beacuse of new button positions!
        public static readonly Vector3 upperRowLeft = new(-2f, 1f, 0f);
        public static readonly Vector3 upperRowFarLeft = new(-3f, 1f, 0f);
        public static readonly Vector3 highRowRight = new(0f, 2.06f, 0f);
    }

    public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, ActionButton textTemplate, KeyCode? hotkey, bool HasEffect, float EffectDuration, Action OnEffectEnds, bool mirror = false, string buttonText = "")
    {
        this.hudManager = hudManager;
        this.OnClick = OnClick;
        this.InitialOnClick = OnClick;
        this.HasButton = HasButton;
        this.CouldUse = CouldUse;
        this.PositionOffset = PositionOffset;
        this.OnMeetingEnds = OnMeetingEnds;
        this.HasEffect = HasEffect;
        this.EffectDuration = EffectDuration;
        this.OnEffectEnds = OnEffectEnds;
        this.Sprite = Sprite;
        this.mirror = mirror;
        this.hotkey = hotkey;
        this.buttonText = buttonText;
        Timer = 16.2f;
        buttons.Add(this);
        actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)onClickEvent);
        LocalScale = actionButton.transform.localScale;
        if (textTemplate)
        {
            UnityEngine.Object.Destroy(actionButton.buttonLabelText);
            actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
        }
        setActive(false);
    }

    public CustomButton(Action OnClick, Func<bool> HasButton, Func<bool> CouldUse, Action OnMeetingEnds, Sprite Sprite, Vector3 PositionOffset, HudManager hudManager, ActionButton? textTemplate, KeyCode? hotkey, bool mirror = false, string buttonText = "")
    : this(OnClick, HasButton, CouldUse, OnMeetingEnds, Sprite, PositionOffset, hudManager, textTemplate, hotkey, false, 0f, () => {}, mirror, buttonText) { }
    public void onClickEvent()
    {
        if ((this.Timer < 0f && HasButton() && CouldUse()) || (this.HasEffect && this.isEffectActive && this.effectCancellable))
        {
            actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
            this.OnClick();

            // Deputy skip onClickEvent if handcuffed
            if (Deputy.handcuffedKnows.ContainsKey(PlayerControl.LocalPlayer.PlayerId) && Deputy.handcuffedKnows[PlayerControl.LocalPlayer.PlayerId] > 0f) return;

            if (this.HasEffect && !this.isEffectActive)
            {
                this.DeputyTimer = this.EffectDuration;
                this.Timer = this.EffectDuration;
                actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                this.isEffectActive = true;
            }
        }
    }
    public static void HudUpdate()
    {
        buttons.RemoveAll(item => item.actionButton == null);

        for (int i = 0; i < buttons.Count; i++)
        {
            try
            {
                buttons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }
    public static void MeetingEndedUpdate()
    {
        buttons.RemoveAll(item => item.actionButton == null);
        for (int i = 0; i < buttons.Count; i++)
        {
            try
            {
                buttons[i].OnMeetingEnds();
                buttons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }
    public static void ResetAllCooldowns()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            try
            {
                buttons[i].Timer = buttons[i].MaxTimer;
                buttons[i].DeputyTimer = buttons[i].MaxTimer;
                buttons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }
    public void setActive(bool isActive)
    {
        if (isActive)
        {
            actionButton.gameObject.SetActive(true);
            actionButton.graphic.enabled = true;
        }
        else
        {
            actionButton.gameObject.SetActive(false);
            actionButton.graphic.enabled = false;
        }
    }
    public void Update()
    {
        if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !HasButton())
        {
            setActive(false);
            return;
        }
        setActive(hudManager.UseButton.isActiveAndEnabled || hudManager.PetButton.isActiveAndEnabled);

        if (DeputyTimer >= 0)
        { // This had to be reordered, so that the handcuffs do not stop the underlying timers from running
            if (HasEffect && isEffectActive)
                DeputyTimer -= Time.deltaTime;
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                DeputyTimer -= Time.deltaTime;
        }

        if (DeputyTimer <= 0 && HasEffect && isEffectActive)
        {
            isEffectActive = false;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            OnEffectEnds();
        }

        if (isHandcuffed)
        {
            setActive(false);
            return;
        }

        actionButton.graphic.sprite = Sprite;
        if (showButtonText && buttonText != "")
        {
            actionButton.OverrideText(buttonText);
        }
        actionButton.buttonLabelText.enabled = showButtonText; // Only show the text if it's a kill button
        if (hudManager.UseButton != null)
        {
            Vector3 pos = hudManager.UseButton.transform.localPosition;
            if (mirror)
            {
                float aspect = Camera.main.aspect;
                float safeOrthographicSize = CameraSafeArea.GetSafeOrthographicSize(Camera.main);
                float xpos = 0.05f - safeOrthographicSize * aspect * 1.70f;
                pos = new Vector3(xpos, pos.y, pos.z);
            }
            actionButton.transform.localPosition = pos + PositionOffset;
            actionButton.transform.localScale = LocalScale;
        }
        if (CouldUse())
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat(Desat, 0f);
        }
        else
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
            actionButton.graphic.material.SetFloat(Desat, 1f);
        }
        if (Timer >= 0)
        {
            if (HasEffect && isEffectActive)
                Timer -= Time.deltaTime;
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                Timer -= Time.deltaTime;
        }

        if (Timer <= 0 && HasEffect && isEffectActive)
        {
            isEffectActive = false;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            OnEffectEnds();
        }

        actionButton.SetCoolDown(Timer, (HasEffect && isEffectActive) ? EffectDuration : MaxTimer);
        // Trigger OnClickEvent if the hotkey is being pressed down
        if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) onClickEvent();

        // Deputy disable the button and display Handcuffs instead...
        if (Deputy.handcuffedPlayers.Contains(PlayerControl.LocalPlayer.PlayerId))
        {
            OnClick = () =>
            {
                Deputy.setHandcuffedKnows();
            };
        }
        else // Reset.
        {
            OnClick = InitialOnClick;
        }
    }
}