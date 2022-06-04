using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Shrink : Power
{
    [Header("Power-specific Settings")]

    // Main features
    [SerializeField] float targetScale = 0.35f;
    [SerializeField] float transitionTime = 0.25f;
    [SerializeField] ParticleSystem shrinkEffect;
    // Sound effects
    [SerializeField] AudioClip shrinkSound;
    [SerializeField] AudioClip expandSound;

    protected override void Awake()
    {
        base.Awake();
        this.unlocked = true;
    }

    //void Update()
    //{
    //    if (active)
    //        ShrinkPlayer();
    //}

    public override void Initialize()
    {
        base.Initialize();

        transform.localScale = Vector3.one;
    }

    protected override void StartPower()
    {
        shrinkEffect.Play();
        audioSource.PlayOneShot(shrinkSound);
        StartCoroutine(ResizePlayer(transitionTime, targetScale));
    }

    protected override void EndPower()
    {
        audioSource.PlayOneShot(expandSound);
        StartCoroutine(ResizePlayer(transitionTime, 1.0f));
    }

    //private void ShrinkPlayer()
    //{
    //    if (!activated && isPressed && (Time.unscaledTime - lastActivation > reloadTime + duration) && powerManager.fullBar)
    //    {
    //        activated = true;
    //        lastActivation = Time.unscaledTime;
    //        shrinkEffect.Play();
    //        audioSource.PlayOneShot(shrinkSound);
    //        StartCoroutine(ResizePlayer(transitionTime, targetScale));
    //        powerManager.EmptyBar(duration);
    //    }
    //    else if (activated && (Time.unscaledTime - lastActivation > duration))
    //    {
    //        activated = false;
    //        isPressed = false;
    //        audioSource.PlayOneShot(expandSound);
    //        StartCoroutine(ResizePlayer(transitionTime, 1.0f));
    //        powerManager.ReloadBar(reloadTime);
    //    }
    //}

    IEnumerator ResizePlayer(float duration, float targetScale)
    {
        float currentScale = transform.localScale.x;
        float elapsed = 0f;
        float t;
        float newScale;
        while (elapsed < duration)
        {
            t = Mathf.Clamp(1f - ((duration - elapsed) / duration), 0, 1);
            newScale = Mathf.Lerp(currentScale, targetScale, t);
            transform.localScale = Vector3.one * newScale;
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
        transform.localScale = Vector3.one * targetScale;
    }
}
