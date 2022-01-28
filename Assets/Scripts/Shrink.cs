using System.Collections;
using UnityEngine;

public class Shrink : MonoBehaviour
{
    // Main features
    [SerializeField] float duration = 2f;
    [SerializeField] float reloadTime = 4f;
    [SerializeField] float targetScale = 0.35f;
    [SerializeField] float transitionTime = 0.25f;
    [SerializeField] ParticleSystem shrinkEffect;
    // Sound effects
    [SerializeField] AudioClip shrinkSound;
    [SerializeField] AudioClip expandSound;

    bool active = false;
    PowerManager powerManager;
    float lastActivation;
    bool activated = false;
    AudioSource audioSource;

    void Start()
    {
        powerManager = GetComponent<PowerManager>();
        lastActivation = -(reloadTime + duration);
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (active)
            ShrinkPlayer();
    }

    public void Initialize()
    {
        active = true;
        transform.localScale = Vector3.one;
        activated = false;
        lastActivation = -(reloadTime + duration);
    }

    private void ShrinkPlayer()
    {
        if (!activated && Input.GetKeyDown(KeyCode.S) && (Time.unscaledTime - lastActivation > reloadTime + duration) && powerManager.fullBar)
        {
            activated = true;
            lastActivation = Time.unscaledTime;
            shrinkEffect.Play();
            audioSource.PlayOneShot(shrinkSound);
            StartCoroutine(ResizePlayer(transitionTime, targetScale));
            powerManager.EmptyBar(duration);
        }
        else if (activated && (Time.unscaledTime - lastActivation > duration))
        {
            activated = false;
            audioSource.PlayOneShot(expandSound);
            StartCoroutine(ResizePlayer(transitionTime, 1.0f));
            powerManager.ReloadBar(reloadTime);
        }
    }

    IEnumerator ResizePlayer(float duration, float targetScale)
    {
        float currentScale = transform.localScale.x;
        float elapsed = 0f;
        float t;
        float newScale;
        while (elapsed < duration)
        {
            t = 1f - ((duration - elapsed) / duration);
            newScale = Mathf.Lerp(currentScale, targetScale, t);
            transform.localScale = Vector3.one * newScale;
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
    }
}
