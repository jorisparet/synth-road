using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimeControl : MonoBehaviour
{
    // Main features
    [SerializeField] float duration = 4f;
    [SerializeField] float reloadTime = 5f;
    [SerializeField] float targetTimeScale = 0.35f;
    // Lens distorsion
    [SerializeField] Volume globalVolume;
    [SerializeField] float targetLensDistorsion = 0.5f;
    [SerializeField] float targetLensScale = 1.2f;
    [SerializeField] float targetChromaticAberration = 0.5f;
    [SerializeField] float lensTransitionTime = 0.25f;
    // Sound effects
    [SerializeField] AudioClip timeSlowSound;
    [SerializeField] AudioClip timeFastSound;

    bool active = false;
    PowerManager powerManager;
    float lastActivation;
    public bool activated = false;
    float defaultFixedDeltaTime;
    LensDistortion lensDistorsion;
    ChromaticAberration chromaticAberration;
    Vignette vignette;
    AudioSource audioSource;

    public void Initialize()
    {
        active = true;
        Time.timeScale = 1.0f;
        activated = false;
        lastActivation = -(reloadTime + duration);
        lensDistorsion.intensity.value = 0f;
        lensDistorsion.scale.value = 1f;
        chromaticAberration.intensity.value = 0f;
        vignette.intensity.value = 0f;
    }

    private void Awake()
    {
        powerManager = GetComponent<PowerManager>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        Time.timeScale = 1.0f;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        lastActivation = -(reloadTime+duration);
        globalVolume.profile.TryGet(out lensDistorsion);
        globalVolume.profile.TryGet(out chromaticAberration);
        globalVolume.profile.TryGet(out vignette);
    }

    void Update()
    {
        if (active)
            ChangeTime();
    }

    // Enable/disable slow time
    private void ChangeTime()
    {
        if (!activated && Input.GetKeyDown(KeyCode.Space) && (Time.unscaledTime - lastActivation > reloadTime + duration) && powerManager.fullBar)
        {
            activated = true;
            lastActivation = Time.unscaledTime;
            Time.timeScale = targetTimeScale;
            Time.fixedDeltaTime = Time.timeScale * defaultFixedDeltaTime;
            StartCoroutine(DistortLens(lensTransitionTime, targetLensDistorsion, targetLensScale, targetChromaticAberration, 0.7f));
            powerManager.EmptyBar(duration);
            audioSource.PlayOneShot(timeSlowSound);
        }
        else if (activated && (Time.unscaledTime - lastActivation > duration))
        {
            activated = false;
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
            StartCoroutine(DistortLens(lensTransitionTime, 0.0f, 1.0f, 0.0f, 0.0f));
            powerManager.ReloadBar(reloadTime);
            audioSource.PlayOneShot(timeFastSound);
        }
    }

    // Generic coroutine to progressively enable/disable lens effects
    IEnumerator DistortLens(float duration, float targetDistortion, float targetScaling, float targetChromaticAberration, float targetVignette)
    {
        float currentDistorsion = lensDistorsion.intensity.value;
        float currentScaling = lensDistorsion.scale.value;
        float currentChromaticAbberation = chromaticAberration.intensity.value;
        float currentVignette = vignette.intensity.value;
        
        float elapsed = 0f;
        float t;
        while (elapsed < duration)
        {
            t = 1f - (duration - elapsed) / duration;
            lensDistorsion.intensity.value = Mathf.Lerp(currentDistorsion, targetDistortion, t);
            lensDistorsion.scale.value = Mathf.Lerp(currentScaling, targetScaling, t);
            chromaticAberration.intensity.value = Mathf.Lerp(currentChromaticAbberation, targetChromaticAberration, t);
            vignette.intensity.value = Mathf.Lerp(currentVignette, targetVignette, t);
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
    }
}
