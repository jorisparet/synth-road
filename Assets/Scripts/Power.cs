using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Power : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] protected float duration;
    [SerializeField] protected float reloadTime;
    [SerializeField] protected Button activateButton;

    [HideInInspector] public bool unlocked;
    [HideInInspector] public bool active = false;
    [HideInInspector] public bool activated = false;

    protected PowerManager powerManager;
    protected AudioSource audioSource;
    protected float lastActivation;
    protected bool isPressed = false;

    protected virtual void Awake()
    {
        powerManager = GetComponent<PowerManager>();
        audioSource = gameObject.AddComponent<AudioSource>();

        lastActivation = -(reloadTime + duration);

        activateButton.onClick.AddListener(ButtonPressed);
    }

    public virtual void Initialize()
    {
        active = true;
        isPressed = false;
        activated = false;
        lastActivation = -(reloadTime + duration);
        audioSource.Stop();
    }

    protected virtual void Update()
    {
        if (active)
            UsePower();
    }

    protected virtual void UsePower()
    {
        if (!activated && isPressed && (Time.unscaledTime - lastActivation > reloadTime + duration) && powerManager.fullBar)
        {
            activated = true;
            lastActivation = Time.unscaledTime;
            powerManager.EmptyBar(duration);
            StartPower();
        }
        else if (activated && (Time.unscaledTime - lastActivation > duration))
        {
            activated = false;
            isPressed = false;
            powerManager.ReloadBar(reloadTime);
            EndPower();
        }
    }

    protected virtual void StartPower() { }

    protected virtual void EndPower() { }

    void ButtonPressed()
    {
        if (powerManager.fullBar)
            isPressed = true;
    }
}
