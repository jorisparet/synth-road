using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadMainScene : MonoBehaviour
{
    [SerializeField] Image logo;
    [SerializeField] Image title;
    [SerializeField] float screenDuration = 6f;    
    [SerializeField] float delayBeforeLogoFadeIn = 1f;
    [SerializeField] float logoFadeInTime = 2.5f;
    [SerializeField] float delayBeforeTitle = 4f;
    [SerializeField] AudioClip loadingSound;
    [SerializeField] AudioClip music;

    bool loading = false;
    bool fading = false;
    bool titleActive = false;
    float start;
    float elapsed;
    AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Time.time - start > delayBeforeLogoFadeIn && !fading)
        {
            fading = true;
            audioSource.PlayOneShot(music, 0.35f);
            StartCoroutine(fadeInLogo());
        }

        if (Time.time - start > delayBeforeTitle && !titleActive)
        {
            titleActive = true;
            //audioSource.PlayOneShot(loadingSound);
            title.gameObject.SetActive(true);
        }

        if (Time.time - start > screenDuration && !loading)
        {
            loading = true;
            SceneManager.LoadScene(1);
        }
    }

    IEnumerator fadeInLogo()
    {
        elapsed = 0f;
        while (elapsed < logoFadeInTime)
        {
            float t = Mathf.Clamp(1f - ((logoFadeInTime - elapsed) / logoFadeInTime), 0, 1);
            logo.color = new Color(255, 255, 255, t);
            elapsed += Time.deltaTime;

            yield return null;
        }
    }
}
