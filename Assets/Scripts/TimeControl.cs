using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TimeControl : Power
{
    [Header("Power-specific Settings")]

    // Main features
    [SerializeField] float targetTimeScale = 0.35f;

    // Lens distorsion
    [SerializeField] Volume globalVolume;
    [SerializeField] float targetLensDistorsion = 0.5f;
    [SerializeField] float targetLensScale = 1.5f;
    [SerializeField] float initialChromaticAberration = 0.3f;
    [SerializeField] float targetChromaticAberration = 0.5f;
    [SerializeField] float distortTransitionTime = 0.25f;
    [SerializeField] float targetHueShift = 37f;

    // Sound effects
    [SerializeField] AudioClip timeSlowSound;
    [SerializeField] AudioClip timeFastSound;

    float defaultFixedDeltaTime;
    LensDistortion lensDistortion;
    ChromaticAberration chromaticAberration;
    Vignette vignette;
    ColorAdjustments colorAdjustments;
    [HideInInspector] public IEnumerator runningCoroutine;

    public override void Initialize()
    {
        base.Initialize();

        Time.timeScale = 1.0f;
        lensDistortion.intensity.value = 0f;
        lensDistortion.scale.value = 1f;
        chromaticAberration.intensity.value = initialChromaticAberration;
        vignette.intensity.value = 0f;
        colorAdjustments.hueShift.value = 0f;
    }

    protected override void Awake()
    {
        base.Awake();
        this.unlocked = true;
    }

    void Start()
    {
        Time.timeScale = 1.0f;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        lastActivation = -(reloadTime+duration);
        globalVolume.profile.TryGet(out lensDistortion);
        globalVolume.profile.TryGet(out chromaticAberration);
        globalVolume.profile.TryGet(out vignette);
        globalVolume.profile.TryGet(out colorAdjustments);
    }

    //void Update()
    //{
    //    if (active)
    //        ChangeTime();
    //}


    // Enable/disable slow time
    protected override void StartPower()
    {
        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = Time.timeScale * defaultFixedDeltaTime;
        StartCoroutine(DistortView(distortTransitionTime, targetLensDistorsion, targetLensScale, targetChromaticAberration, 0.7f, targetHueShift));
        audioSource.PlayOneShot(timeSlowSound);
    }

    protected override void EndPower()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        StartCoroutine(DistortView(distortTransitionTime, 0f, 1f, 0f, 0f, 0f));
        audioSource.PlayOneShot(timeFastSound);
    }

    //// Enable/disable slow time
    //private void ChangeTime()
    //{
    //    if (!activated && isPressed && (Time.unscaledTime - lastActivation > reloadTime + duration) && powerManager.fullBar)
    //    {
    //        activated = true;
    //        lastActivation = Time.unscaledTime;
    //        Time.timeScale = targetTimeScale;
    //        Time.fixedDeltaTime = Time.timeScale * defaultFixedDeltaTime;
    //        StartCoroutine(DistortView(distortTransitionTime, targetLensDistorsion, targetLensScale, targetChromaticAberration, 0.7f, targetHueShift));
    //        powerManager.EmptyBar(duration);
    //        audioSource.PlayOneShot(timeSlowSound);
    //    }
    //    else if (activated && (Time.unscaledTime - lastActivation > duration))
    //    {
    //        activated = false;
    //        isPressed = false;
    //        Time.timeScale = 1.0f;
    //        Time.fixedDeltaTime = defaultFixedDeltaTime;
    //        StartCoroutine(DistortView(distortTransitionTime, 0f, 1f, 0f, 0f, 0f));
    //        powerManager.ReloadBar(reloadTime);
    //        audioSource.PlayOneShot(timeFastSound);
    //    }
    //}

    // Generic coroutine to progressively enable/disable lens effects
    IEnumerator DistortView(float duration, float targetDistortion, float targetScaling, float targetChromaticAberration, float targetVignette, float targetHue)
    {
        float currentDistorsion = lensDistortion.intensity.value;
        float currentScaling = lensDistortion.scale.value;
        float currentChromaticAbberation = chromaticAberration.intensity.value;
        float currentVignette = vignette.intensity.value;
        float currentHue = colorAdjustments.hueShift.value;

        float elapsed = 0f;
        float t;
        while (elapsed < duration)
        {
            t = Mathf.Clamp(1f - (duration - elapsed) / duration, 0, 1);
            if (!powerManager.player.isInvincible) // only modify if player is not invincible to avoid overlap
            {
                lensDistortion.intensity.value = Mathf.Lerp(currentDistorsion, targetDistortion, t);
                lensDistortion.scale.value = Mathf.Lerp(currentScaling, targetScaling, t);
                colorAdjustments.hueShift.value = Mathf.Lerp(currentHue, targetHue, t);
            }
            chromaticAberration.intensity.value = Mathf.Lerp(currentChromaticAbberation, targetChromaticAberration, t);
            vignette.intensity.value = Mathf.Lerp(currentVignette, targetVignette, t);
            
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        if (!powerManager.player.isInvincible) // make sure we reach the correct values if player is not invincible
        {
            lensDistortion.intensity.value = targetDistortion;
            lensDistortion.scale.value = targetScaling;
            chromaticAberration.intensity.value = targetChromaticAberration;
            vignette.intensity.value = targetVignette;
            colorAdjustments.hueShift.value = targetHue;
        }
    }
}
