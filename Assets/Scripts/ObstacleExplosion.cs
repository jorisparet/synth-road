using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleExplosion : MonoBehaviour
{

    [SerializeField] AudioClip[] explosionSounds;
    [SerializeField] AudioClip[] explosionSoundsSlow;
    [SerializeField] ParticleSystem explosionEffect;

    AudioSource audioSource;
    AudioClip explosionSound;
    AudioClip explosionSoundSlow;
    AudioClip playedSound;
    int playedIndex;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        playedIndex = Random.Range(0, explosionSounds.Length);
        explosionSound = explosionSounds[playedIndex];
        explosionSoundSlow = explosionSoundsSlow[playedIndex];
    }

    public void Explose(bool slow, bool enableSound)
    {
        explosionEffect.Play();
        if (enableSound)
        {
            playedSound = slow ? explosionSoundSlow : explosionSound;
            audioSource.PlayOneShot(playedSound);
        }
    }
}
