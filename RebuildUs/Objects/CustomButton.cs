using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RebuildUs.Objects;

public class CustomButton
{
    public static List<CustomButton> buttons = [];
    public ActionButton actionButton;
    public Vector3 positionOffset;
    public Vector3 LocalScale = Vector3.one;
    public float MaxTimer = float.MaxValue;
    public float Timer = 0f;
    public bool effectCancellable = false;
    public float DeputyTimer = 0f;
    private Action onClick;
    private Action initialOnClick;
    private Action onMeetingEnds;
    public Func<bool> hasButton;
    public Func<bool> couldUse;
    private Action onEffectEnds;
    public bool hasEffect;
    public bool isEffectActive = false;
    public bool showButtonText = false;
    public string buttonText = null;
    public float effectDuration;
    public Sprite sprite;
    public HudManager hudManager;
    public bool mirror;
    public KeyCode? hotkey;
    public bool isHandcuffed = false;
    private static readonly int Desat = Shader.PropertyToID("_Desat");

    public static Vector3 GetGameButtonPosition(ButtonOffset offset)
    {
        return offset switch
        {
            ButtonOffset.LowerRight => new(-2f, -0.06f, 0),
            ButtonOffset.LowerCenter => new(-3f, -0.06f, 0),
            ButtonOffset.LowerLeft => new(-4f, -0.06f, 0),
            ButtonOffset.UpperRight => new(0f, 1f, 0f),
            ButtonOffset.UpperCenter => new(-1f, 1f, 0f),
            ButtonOffset.UpperLeft => new(-2f, 1f, 0f),
            ButtonOffset.UpperFarLeft => new(-3f, 1f, 0f),
            ButtonOffset.HighRight => new(0f, 2.06f, 0f),
            _ => Vector3.zero
        };
    }

    public CustomButton(
        Action onClick,
        Func<bool> hasButton,
        Func<bool> couldUse,
        Action onMeetingEnds,
        Sprite sprite,
        Vector3 positionOffset,
        HudManager hudManager,
        ActionButton textTemplate,
        KeyCode? hotkey,
        bool hasEffect,
        float effectDuration,
        Action onEffectEnds,
        bool mirror = false,
        string buttonText = "")
    {
        this.hudManager = hudManager;
        this.onClick = onClick;
        initialOnClick = onClick;
        this.hasButton = hasButton;
        this.couldUse = couldUse;
        this.positionOffset = positionOffset;
        this.onMeetingEnds = onMeetingEnds;
        this.hasEffect = hasEffect;
        this.effectDuration = effectDuration;
        this.onEffectEnds = onEffectEnds;
        this.sprite = sprite;
        this.mirror = mirror;
        this.hotkey = hotkey;
        this.buttonText = buttonText;
        Timer = 16.2f;
        buttons.Add(this);
        actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)OnClickEvent);
        LocalScale = actionButton.transform.localScale;
        if (textTemplate)
        {
            UnityEngine.Object.Destroy(actionButton.buttonLabelText);
            actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
        }
        setActive(false);
    }

    public CustomButton(
        Action onClick,
        Func<bool> hasButton,
        Func<bool> couldUse,
        Action onMeetingEnds,
        Sprite sprite,
        ButtonOffset offset,
        HudManager hudManager,
        ActionButton textTemplate,
        KeyCode? hotkey,
        bool hasEffect,
        float effectDuration,
        Action onEffectEnds,
        bool mirror = false,
        string buttonText = "")
    : this(
        onClick,
        hasButton,
        couldUse,
        onMeetingEnds,
        sprite,
        GetGameButtonPosition(offset),
        hudManager,
        textTemplate,
        hotkey,
        false,
        0f,
        () => { },
        mirror,
        buttonText)
    { }

    public CustomButton(
        Action onClick,
        Func<bool> hasButton,
        Func<bool> couldUse,
        Action onMeetingEnds,
        Sprite sprite,
        Vector3 positionOffset,
        HudManager hudManager,
        ActionButton textTemplate,
        KeyCode? hotkey,
        bool mirror = false,
        string buttonText = "")
    : this(
        onClick,
        hasButton,
        couldUse,
        onMeetingEnds,
        sprite,
        positionOffset,
        hudManager,
        textTemplate,
        hotkey,
        false,
        0f,
        () => { },
        mirror,
        buttonText)
    { }

    public CustomButton(
        Action onClick,
        Func<bool> hasButton,
        Func<bool> couldUse,
        Action onMeetingEnds,
        Sprite sprite,
        ButtonOffset offset,
        HudManager hudManager,
        ActionButton textTemplate,
        KeyCode? hotkey,
        bool mirror = false,
        string buttonText = "")
    : this(
        onClick,
        hasButton,
        couldUse,
        onMeetingEnds,
        sprite,
        GetGameButtonPosition(offset),
        hudManager,
        textTemplate,
        hotkey,
        false,
        0f,
        () => { },
        mirror,
        buttonText)
    { }

    public void OnClickEvent()
    {
        if ((Timer < 0f && hasButton() && couldUse()) || (hasEffect && isEffectActive && effectCancellable))
        {
            actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
            onClick();

            if (hasEffect && !isEffectActive)
            {
                DeputyTimer = effectDuration;
                Timer = effectDuration;
                actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                isEffectActive = true;
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
                buttons[i].onMeetingEnds();
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
        if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !hasButton())
        {
            setActive(false);
            return;
        }
        setActive(hudManager.UseButton.isActiveAndEnabled || hudManager.PetButton.isActiveAndEnabled);

        if (DeputyTimer >= 0)
        { // This had to be reordered, so that the handcuffs do not stop the underlying timers from running
            if (hasEffect && isEffectActive)
                DeputyTimer -= Time.deltaTime;
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                DeputyTimer -= Time.deltaTime;
        }

        if (DeputyTimer <= 0 && hasEffect && isEffectActive)
        {
            isEffectActive = false;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            onEffectEnds();
        }

        if (isHandcuffed)
        {
            setActive(false);
            return;
        }

        actionButton.graphic.sprite = sprite;
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
            actionButton.transform.localPosition = pos + positionOffset;
            actionButton.transform.localScale = LocalScale;
        }
        if (couldUse())
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
            if (hasEffect && isEffectActive)
                Timer -= Time.deltaTime;
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
                Timer -= Time.deltaTime;
        }

        if (Timer <= 0 && hasEffect && isEffectActive)
        {
            isEffectActive = false;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            onEffectEnds();
        }

        actionButton.SetCoolDown(Timer, (hasEffect && isEffectActive) ? effectDuration : MaxTimer);
        // Trigger OnClickEvent if the hotkey is being pressed down
        if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) OnClickEvent();

        onClick = initialOnClick;
    }
}