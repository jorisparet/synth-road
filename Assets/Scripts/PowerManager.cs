using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour
{
    [SerializeField] RawImage reloadBar;
    [SerializeField] Color blinkColor;
    [SerializeField] AudioClip reloadedSound;

    [HideInInspector] public bool fullBar = true;
    public float defaultBarSize;
    PlayerMovement player;
    Color defaultBarColor;
    AudioSource audioSource;

    public void Initialize()
    {
        fullBar = true;
        StopAllCoroutines();
        reloadBar.rectTransform.sizeDelta = new Vector2(defaultBarSize, reloadBar.rectTransform.sizeDelta.y);
    }

    private void Awake()
    {
        defaultBarSize = reloadBar.rectTransform.sizeDelta.x;
        player = GetComponent<PlayerMovement>();
        defaultBarColor = reloadBar.color;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Progressively empty the power bar
    public void EmptyBar(float duration)
    {
        fullBar = false;
        StartCoroutine(ResizeReloadBar(duration, 0f));
    }

    // Progressively reload the powar bar
    public void ReloadBar(float reloadTime)
    {
        StartCoroutine(ResizeReloadBar(reloadTime, defaultBarSize));
    }

    // Generic coroutine to modify the state of the power bar (empty and refill)
    public IEnumerator ResizeReloadBar(float reloadTime, float targetSize)
    {
        float currentBarSize = reloadBar.rectTransform.sizeDelta.x;
        float elapsed = 0f;
        float t;
        float newSize;
        while (elapsed < reloadTime)
        {
            t = 1f - ((reloadTime - elapsed) / reloadTime);
            newSize = Mathf.Lerp(currentBarSize, targetSize, t);
            reloadBar.rectTransform.sizeDelta = new Vector2(newSize, reloadBar.rectTransform.sizeDelta.y);
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
        // Bar is fully reloaded
        if (targetSize == defaultBarSize && !player.hasExploded)
        {
            fullBar = true;
            StartCoroutine(BlinkBar(0.75f, blinkColor));
            audioSource.PlayOneShot(reloadedSound);
        }

        IEnumerator BlinkBar(float duration, Color targetColor)
        {
            Color currentColor = reloadBar.color;
            float elapsed = 0f;
            float t;
            Color newColor;
            while (elapsed < duration)
            {
                t = 1f - ((duration - elapsed) / duration);
                newColor = Color.Lerp(currentColor, targetColor, t);
                reloadBar.color = newColor;
                elapsed += Time.unscaledDeltaTime;

                // Lerp back to initial color
                if (elapsed > duration / 2)
                {
                    targetColor = defaultBarColor;
                }

                yield return null;
            }
        }
    }
}
