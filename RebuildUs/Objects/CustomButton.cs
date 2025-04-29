using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RebuildUs.Objects;

public class CustomButton
{
    public static List<CustomButton> AllButtons = [];

    public ActionButton actionButton;
    public Vector3 positionOffset;
    public Vector3 localScale = Vector3.one;
    public float maxTimer = float.MaxValue;
    public float timer = 0f;
    public bool cancellable = false;
    private readonly Action onClick;
    private readonly Action onMeetingEnds;
    private readonly Func<bool> hasButton;
    private readonly Func<bool> couldUse;
    private readonly Action onEffectEnds;
    public bool hasEffect;
    public bool isEffectActive = false;
    public bool showButtonText = true;
    public string buttonText = null;
    public float effectDuration;
    public Sprite sprite;
    private readonly HudManager hudManager;
    private readonly bool isMirror;
    private readonly KeyCode? hotkey;

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
        bool isMirror = false,
        string buttonText = null
        )
    {
        this.hudManager = hudManager;
        this.onClick = onClick;
        this.hasButton = hasButton;
        this.couldUse = couldUse;
        this.positionOffset = positionOffset;
        this.onMeetingEnds = onMeetingEnds;
        this.hasEffect = hasEffect;
        this.effectDuration = effectDuration;
        this.onEffectEnds = onEffectEnds;
        this.sprite = sprite;
        this.isMirror = isMirror;
        this.hotkey = hotkey;
        this.buttonText = buttonText;

        timer = 16.2f;
        AllButtons.Add(this);
        actionButton = UnityEngine.Object.Instantiate(hudManager.KillButton, hudManager.KillButton.transform.parent);
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)OnClickEvent);

        localScale = actionButton.transform.localScale;
        if (textTemplate)
        {
            UnityEngine.Object.Destroy(actionButton.buttonLabelText);
            actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
        }

        SetActive(false);
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
        bool isMirror = false,
        string buttonText = null
        )
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
        isMirror,
        buttonText)
    { }

    private void OnClickEvent()
    {
        if ((timer < 0f && hasButton() && couldUse()) || (hasEffect && isEffectActive && cancellable))
        {
            actionButton.graphic.color = new Color(1f, 1f, 1f, 0.3f);
            onClick();

            if (hasEffect && !isEffectActive)
            {
                timer = effectDuration;
                actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                isEffectActive = true;
            }
        }
    }

    public static void HudUpdate()
    {
        AllButtons.RemoveAll(item => item.actionButton == null);

        for (int i = 0; i < AllButtons.Count; i++)
        {
            try
            {
                AllButtons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from HudUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    public static void MeetingEndedUpdate()
    {
        AllButtons.RemoveAll(item => item.actionButton == null);
        for (int i = 0; i < AllButtons.Count; i++)
        {
            try
            {
                AllButtons[i].onMeetingEnds();
                AllButtons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    public static void ResetAllCooldowns()
    {
        for (int i = 0; i < AllButtons.Count; i++)
        {
            try
            {
                AllButtons[i].timer = AllButtons[i].maxTimer;
                AllButtons[i].Update();
            }
            catch (NullReferenceException)
            {
                System.Console.WriteLine("[WARNING] NullReferenceException from MeetingEndedUpdate().HasButton(), if theres only one warning its fine");
            }
        }
    }

    public void SetActive(bool isActive)
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

    private void Update()
    {
        if (PlayerControl.LocalPlayer.Data == null || MeetingHud.Instance || ExileController.Instance || !hasButton())
        {
            SetActive(false);
            return;
        }
        SetActive(hudManager.UseButton.isActiveAndEnabled);

        actionButton.graphic.sprite = sprite;
        if (showButtonText && buttonText != null)
        {
            actionButton.OverrideText(buttonText);
        }
        actionButton.buttonLabelText.enabled = showButtonText; // Only show the text if it's a kill button

        if (hudManager.UseButton != null)
        {
            Vector3 pos = hudManager.UseButton.transform.localPosition;
            if (isMirror) pos = new Vector3(-pos.x, pos.y, pos.z);
            actionButton.transform.localPosition = pos + positionOffset;
            actionButton.transform.localScale = localScale;
        }

        if (couldUse())
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat("_Desat", 0f);
        }
        else
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
            actionButton.graphic.material.SetFloat("_Desat", 1f);
        }

        if (timer >= 0)
        {
            if (hasEffect && isEffectActive)
            {
                timer -= Time.deltaTime;
            }
            else if (!PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable)
            {
                timer -= Time.deltaTime;
            }
        }

        if (timer <= 0 && hasEffect && isEffectActive)
        {
            isEffectActive = false;
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
            onEffectEnds();
        }

        actionButton.SetCoolDown(timer, (hasEffect && isEffectActive) ? effectDuration : maxTimer);

        // Trigger OnClickEvent if the hotkey is being pressed down
        if (hotkey.HasValue && Input.GetKeyDown(hotkey.Value)) OnClickEvent();
    }
}