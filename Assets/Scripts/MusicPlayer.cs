using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip music;
    [SerializeField] AudioClip UIClick;

    [HideInInspector] public AudioSource ASMusic;
    AudioSource ASUI;

    void Awake()
    {
        // Background music
        ASMusic = gameObject.AddComponent<AudioSource>();
        ASMusic.clip = music;
        ASMusic.loop = true;
        PlayMusic();

        // UI click
        ASUI = gameObject.AddComponent<AudioSource>();
        ASUI.ignoreListenerPause = true;
    }

    public void StopMusic()
    {
        ASMusic.Stop();
    }

    public void PlayMusic()
    {
        ASMusic.Play();
    }

    public void PauseMusic()
    {
        ASMusic.Pause();
    }

    public void PauseUnpauseMusic()
    {
        if (!ASMusic.isPlaying)
            ASMusic.Play();
        else
            ASMusic.Pause();
    }

    public void PlayClick()
    {
        if (ASUI.isPlaying)
            ASUI.Stop();
        ASUI.PlayOneShot(UIClick);
    }
}
