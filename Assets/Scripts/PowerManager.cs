using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PowerManager : MonoBehaviour
{
    [Header("Powers Settings")]
    [SerializeField] Button[] powerButtons;
    [SerializeField] GameObject powerBarHolder;
    [SerializeField] RawImage powerReloadBar;
    [SerializeField] Color fullBarColor;
    [SerializeField] Color emptyBarColor;
    [SerializeField] Color blinkColor;
    [SerializeField] AudioClip reloadedSound;
    [SerializeField] AudioClip invincibilityCountdown;
    [Header("Invincibility Settings")]
    [SerializeField] GameObject invincibilityBarHolder;
    [SerializeField] RawImage invincibilityReloadBar;
    [SerializeField] public int numberOfBreakableObstacles = 5;
    [SerializeField] Volume globalVolume;
    [SerializeField] float invincibilityTransition = 0.25f;
    [SerializeField] public float targetInvincibilityHue = -70f;
    [SerializeField] public float targetInvincibilityDistortion =0.5f;
    [SerializeField] public float targetInvincibilityScaling = 1.2f;
    
    [HideInInspector] public bool fullBar = true;
    [HideInInspector] public PlayerMovement player;
    TimeControl timeControl;
    Shrink shrink;
    ForceField forceField;
    Power[] powers;
    MovableSpawner movableSpawner;
    MenuManager menuManager;
    [HideInInspector] public float defaultBarSize;
    [HideInInspector] public AudioSource audioSource;
    float invincibilityReloadBarSize;
    [HideInInspector] public float invincibilityDuration;
    ColorAdjustments colorAdjustments;
    LensDistortion lensDistortion;
    AudioSource countdownAS;

    private void Awake()
    {
        defaultBarSize = powerReloadBar.rectTransform.sizeDelta.x;
        invincibilityReloadBarSize = invincibilityReloadBar.rectTransform.sizeDelta.x;
        player = GetComponent<PlayerMovement>();

        timeControl = GetComponent<TimeControl>();
        shrink = GetComponent<Shrink>();
        forceField = GetComponent<ForceField>();
        powers = new Power[] { timeControl, shrink, forceField };

        powerReloadBar.color = fullBarColor;
        audioSource = gameObject.AddComponent<AudioSource>();
        countdownAS = gameObject.AddComponent<AudioSource>();
        MakePowerBarVisible(false);
        MakeInvincibilityBarVisible(false);
        globalVolume.profile.TryGet(out colorAdjustments);
        globalVolume.profile.TryGet(out lensDistortion);

        movableSpawner = FindObjectOfType<MovableSpawner>();
        menuManager = FindObjectOfType<MenuManager>();
    }

    public void Initialize()
    {
        fullBar = true;
        StopAllCoroutines();
        powerReloadBar.rectTransform.sizeDelta = new Vector2(defaultBarSize, powerReloadBar.rectTransform.sizeDelta.y);
        powerReloadBar.color = fullBarColor;
        MakePowerButtonsVisible(true);
        EnablePowerButtons(true);
        MakePowerBarVisible(false);
        MakeInvincibilityBarVisible(false);
        audioSource.Stop();
    }

    public void EnablePowerButtons(bool enabled)
    {
        for (int i=0; i<powers.Length; i++)
        {
            if (powers[i].unlocked)
                powerButtons[i].interactable = enabled;
        }
    }

    public void MakePowerButtonsVisible(bool visible)
    {
        foreach (Button button in powerButtons)
        {
            button.gameObject.SetActive(visible);
        }
    }

    public void MakeBarVisible(GameObject holder, bool visible)
    {
        if (visible)
            holder.transform.localScale = Vector3.one;
        else
            holder.transform.localScale = Vector3.zero;
    }

    public void MakePowerBarVisible(bool visible)
    {
        MakeBarVisible(powerBarHolder, visible);
    }

    public void MakeInvincibilityBarVisible(bool visible)
    {
        MakeBarVisible(invincibilityBarHolder, visible);
    }

    // Progressively empty the power bar
    public void EmptyBar(float duration)
    {
        fullBar = false;
        StartCoroutine(ResizePowerReloadBar(duration, 0f, emptyBarColor));
        EnablePowerButtons(false);
        MakePowerBarVisible(true);
        menuManager.ShowPauseButton(false);
    }

    // Progressively reload the powar bar
    public void ReloadBar(float reloadTime)
    {
        StartCoroutine(ResizePowerReloadBar(reloadTime, defaultBarSize, fullBarColor));
    }

    // Generic coroutine to modify the state of the power bar(empty and refill)
    public IEnumerator ResizePowerReloadBar(float reloadTime, float targetSize, Color targetColor)
    {
        float currentBarSize = powerReloadBar.rectTransform.sizeDelta.x;
        Color currentColor = powerReloadBar.color;
        float elapsed = 0f;
        float t;
        float newSize;
        while (elapsed < reloadTime)
        {
            t = Mathf.Clamp(1f - ((reloadTime - elapsed) / reloadTime), 0, 1);
            newSize = Mathf.Lerp(currentBarSize, targetSize, t);
            powerReloadBar.rectTransform.sizeDelta = new Vector2(newSize, powerReloadBar.rectTransform.sizeDelta.y);
            powerReloadBar.color = Color.Lerp(currentColor, targetColor, t);
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
        powerReloadBar.rectTransform.sizeDelta = new Vector2(targetSize, powerReloadBar.rectTransform.sizeDelta.y);
        powerReloadBar.color = targetColor;

        // Bar is fully reloaded
        if (targetSize == defaultBarSize && !player.hasExploded)
        {
            fullBar = true;
            StartCoroutine(BlinkBar(0.35f, blinkColor));
            audioSource.PlayOneShot(reloadedSound);
            EnablePowerButtons(true);
            menuManager.ShowPauseButton(true);
        }
    }

    IEnumerator BlinkBar(float duration, Color targetColor)
    {
        Color currentColor = powerReloadBar.color;
        float elapsed = 0f;
        float t;
        Color newColor;
        while (elapsed < duration)
        {
            t = Mathf.Clamp(1f - ((duration - elapsed) / duration), 0, 1);
            newColor = Color.Lerp(currentColor, targetColor, t);
            powerReloadBar.color = newColor;
            elapsed += Time.unscaledDeltaTime;

            // Lerp back to initial color
            if (elapsed > duration / 2)
            {
                targetColor = fullBarColor;
            }

            yield return null;
        }
        powerReloadBar.color = targetColor;
        MakePowerBarVisible(false);
    }

    public IEnumerator StartInvincibility()
    {
        if (timeControl.runningCoroutine != null)
        {
            StopCoroutine(timeControl.runningCoroutine);
        }
        player.isInvincible = true;
        invincibilityDuration = numberOfBreakableObstacles * (movableSpawner.distanceBetweenSpawns / movableSpawner.invincibilityScrollSpeed);
        FillInvincibilityBar();
        MakeInvincibilityBarVisible(true);

        float currentBarSize = invincibilityReloadBar.rectTransform.sizeDelta.x;
        float elapsed = 0f;
        float t;
        float t_transition;
        float newSize;
        float currentHue = colorAdjustments.hueShift.value;
        float currentDistortion = lensDistortion.intensity.value;
        float currentScaling = lensDistortion.scale.value;
        bool stopping = false;
        while (elapsed < invincibilityDuration)
        {
            t = Mathf.Clamp(1f - ((invincibilityDuration - elapsed) / invincibilityDuration), 0, 1);
            newSize = Mathf.Lerp(currentBarSize, 0f, t);
            invincibilityReloadBar.rectTransform.sizeDelta = new Vector2(newSize, invincibilityReloadBar.rectTransform.sizeDelta.y);
            elapsed += Time.deltaTime;

            if (elapsed < invincibilityTransition)
            {
                t_transition = Mathf.Clamp(1f - ((invincibilityTransition - elapsed) / invincibilityTransition), 0, 1);
                colorAdjustments.hueShift.value = Mathf.Lerp(currentHue, targetInvincibilityHue, t_transition);
                lensDistortion.intensity.value = Mathf.Lerp(currentDistortion, targetInvincibilityDistortion, t_transition);
                lensDistortion.scale.value = Mathf.Lerp(currentScaling, targetInvincibilityScaling, t_transition);
            }
            else if (invincibilityDuration - elapsed < invincibilityTransition)
            {
                if (!stopping)
                    player.StopInvincibility();
                stopping = true;
                t_transition = Mathf.Clamp(((invincibilityTransition - (invincibilityDuration - elapsed)) / invincibilityTransition), 0, 1);
                colorAdjustments.hueShift.value = Mathf.Lerp(targetInvincibilityHue, 0f, t_transition);
                lensDistortion.intensity.value = Mathf.Lerp(targetInvincibilityDistortion, 0f, t_transition);
                lensDistortion.scale.value = Mathf.Lerp(targetInvincibilityScaling, 1f, t_transition);
            }
            else if (!countdownAS.isPlaying && invincibilityDuration - elapsed < invincibilityCountdown.length)
            {
                print("SOUND");
                countdownAS.PlayOneShot(invincibilityCountdown);
            }

            yield return null;
        }
        colorAdjustments.hueShift.value = 0f;
        lensDistortion.intensity.value = 0f;

        player.isInvincible = false;
        MakeInvincibilityBarVisible(false);
        FillInvincibilityBar();
    }

    public void FillInvincibilityBar()
    {
        invincibilityReloadBar.rectTransform.sizeDelta = new Vector2(invincibilityReloadBarSize, invincibilityReloadBar.rectTransform.sizeDelta.y);
    }
}
