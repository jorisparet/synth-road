using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    //[SerializeField] public float defaultSpeed = 20f;
    //[SerializeField] public float defaultSensitivity = 15f;
    [SerializeField] float horizontalSpeed = 45f;
    [SerializeField] Camera mainCamera;
    [SerializeField] float xBound = 4.5f;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] AudioClip playerExplosionSound;
    [SerializeField] AudioClip playerExplosionSoundSlow;
    [SerializeField] AudioClip scoreBonusSound;
    [SerializeField] AudioClip scoreBonusSoundSlow;
    [SerializeField] AudioClip invincibilityBonusSound;
    [SerializeField] AudioClip invincibilityBonusSoundSlow;
    [SerializeField] AudioClip invincibilityBonusSoundOff;
    [SerializeField] AudioClip goingFastSound;
    [SerializeField] AudioClip goingFastSoundSlow;
    [SerializeField] int playerLayer;
    [SerializeField] int obstacleLayer;
    [SerializeField] int bonusLayer;
    [SerializeField] int invincibilityLayer;
    [SerializeField] int forceFieldLayer;
    [SerializeField] CameraShake cameraShake;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] MenuManager mainMenu;
    [SerializeField] PlayerOptions options;
    [SerializeField] Leaderboard leaderbord;

    [HideInInspector] public bool active = false;
    [HideInInspector] public bool hasExploded = false;
    [HideInInspector] public bool isInvincible = false;
    [HideInInspector] public int bonusCount = 0;
    [HideInInspector] public float invincibilityStartTime;
    float horizontalSensitivity;
    MeshRenderer meshRenderer;
    BoxCollider boxCollider;
    PowerManager powerManager;
    TimeControl timeControl;
    Shrink shrink;
    AudioSource playerExplosionAS;
    AudioClip currentPlayerExplosionSound;
    [HideInInspector] public AudioSource bonusAS;
    [HideInInspector] public AudioSource invincibilityAS;
    [HideInInspector] public AudioSource goingFastAS;
    [HideInInspector] public AudioSource goingFastSlowAS;
    Vector3 initialPosition;
    Quaternion initialRotation;
    Vector3 screenTouchOrigin = Vector3.zero;
    Vector3 wordlTouchOrigin = Vector3.zero;
    Vector3 originalPosition = Vector3.zero;
    IEnumerator currentInvincibilityCoroutine;
    IEnumerator currentScoreMultiplierCoroutine;
    Rigidbody body;

    public void Initialize()
    {
        active = true;
        horizontalSensitivity = options.sensitivity;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        hasExploded = false;
        isInvincible = false;
        explosion.Clear();
        meshRenderer.enabled = true;
        boxCollider.enabled = true;
        timeControl.enabled = true;
        shrink.enabled = true;
        bonusCount = 0;
        // Stop any playing sound
        playerExplosionAS.Stop();
        bonusAS.Stop();
        invincibilityAS.Stop();
    }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        body = GetComponent<Rigidbody>();
        body.sleepThreshold = 0f;

        powerManager = GetComponent<PowerManager>();
        timeControl = GetComponent<TimeControl>();
        shrink = GetComponent<Shrink>();

        playerExplosionAS = gameObject.AddComponent<AudioSource>();
        bonusAS = gameObject.AddComponent<AudioSource>();
        invincibilityAS = gameObject.AddComponent<AudioSource>();
        goingFastAS = gameObject.AddComponent<AudioSource>();
        goingFastSlowAS = gameObject.AddComponent<AudioSource>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (active)
        {
            MoveLeftOrRight();
            PlayInvincibilitySound();
        }
    }

    private void PlayInvincibilitySound()
    {
        if (isInvincible)
        {
            if (timeControl.activated && !goingFastSlowAS.isPlaying)
            {
                goingFastAS.Stop();
                goingFastSlowAS.PlayOneShot(goingFastSoundSlow);
            }
            else if (!timeControl.activated && !goingFastAS.isPlaying)
            {
                goingFastSlowAS.Stop();
                goingFastAS.PlayOneShot(goingFastSound);
            }
        }
        else
        {
            goingFastAS.Stop();
            goingFastSlowAS.Stop();
        }
    }

    void MoveLeftOrRight()
    {
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                ResetTouch(ref touch);
            }

            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 newScreenTouch = new Vector3(touch.position.x, touch.position.y, mainCamera.transform.position.y + horizontalSensitivity);
                Vector3 newWordlTouch = mainCamera.ScreenToWorldPoint(newScreenTouch);
                Vector3 targetPosition = originalPosition + new Vector3(newWordlTouch.x - wordlTouchOrigin.x, transform.position.y, transform.position.z);
                Vector3 clampedTargetPosition = new Vector3(Mathf.Clamp(targetPosition.x, -xBound, xBound), targetPosition.y, targetPosition.z);
                transform.position = Vector3.MoveTowards(transform.position, clampedTargetPosition, horizontalSpeed * Time.unscaledDeltaTime);

                if (Mathf.Abs(Mathf.Abs(transform.position.x) - xBound) < 1e-5)
                {
                    ResetTouch(ref touch);
                }
            }
        }
    }

    void ResetTouch(ref Touch touch)
    {
        screenTouchOrigin = new Vector3(touch.position.x, touch.position.y, mainCamera.transform.position.y + horizontalSensitivity);
        wordlTouchOrigin = mainCamera.ScreenToWorldPoint(screenTouchOrigin);
        originalPosition = new Vector3(transform.position.x, 0f, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Collision with an obstacle
        if (collision.gameObject.layer == obstacleLayer)
        {
            // Destroy the obstacle when invincible
            if (isInvincible && collision.GetContact(0).thisCollider.gameObject.layer == playerLayer)
            {
                CollideWithObstacle(collision, true);
            }
            // Destroy the obstacle with the force field
            else if (collision.GetContact(0).thisCollider.gameObject.layer == forceFieldLayer)
            {
                CollideWithObstacle(collision, false);
            }
            // Get destroyed by the obstacle
            else if (!hasExploded)
            {
                explosion.Play();
                currentPlayerExplosionSound = timeControl.activated ? playerExplosionSoundSlow : playerExplosionSound;
                playerExplosionAS.PlayOneShot(currentPlayerExplosionSound);
                StartCoroutine(cameraShake.Shake(.2f, 0.5f));
                hasExploded = true;
                meshRenderer.enabled = false;
                boxCollider.enabled = false;
                scoreManager.StopUpdatingScore();
                scoreManager.UpdateCurrentHighscore();
                leaderbord.CheckLastPublishedHighscores();
                mainMenu.DisplayGameOverMenu();
                DisablePowers();
                powerManager.MakePowerButtonsVisible(false);
                powerManager.MakePowerBarVisible(false);
            }
        }
    }

    private void CollideWithObstacle(Collision collision, bool enableSound)
    {
        collision.contacts[0].otherCollider.gameObject.GetComponent<MeshRenderer>().enabled = false;
        collision.contacts[0].otherCollider.gameObject.GetComponent<BoxCollider>().enabled = false;
        collision.contacts[0].otherCollider.gameObject.GetComponent<ObstacleExplosion>().Explose(timeControl.activated, enableSound);
    }

    // Collect bonuses
    private void OnTriggerEnter(Collider other)
    {
        
        // Score bonus
        if (other.gameObject.layer == bonusLayer)
        {
            bonusCount++;
            CollectBonus(other, bonusAS, scoreBonusSound, scoreBonusSoundSlow);
            if (currentScoreMultiplierCoroutine != null)
                StopCoroutine(currentScoreMultiplierCoroutine);
            currentScoreMultiplierCoroutine = scoreManager.MultiplierZoom(0.35f, 1.5f);
            StartCoroutine(currentScoreMultiplierCoroutine);
        }

        // Invincibility bonus
        else if (other.gameObject.layer == invincibilityLayer)
        {
            CollectBonus(other, invincibilityAS, invincibilityBonusSound, invincibilityBonusSoundSlow);
            invincibilityStartTime = Time.time;

            // Allows to reset invincibility if two bonuses are collected successively
            if (currentInvincibilityCoroutine != null)
                StopCoroutine(currentInvincibilityCoroutine);
            currentInvincibilityCoroutine = powerManager.StartInvincibility();
            StartCoroutine(currentInvincibilityCoroutine);
        }
    }

    public void StopInvincibility()
    {
        invincibilityAS.PlayOneShot(invincibilityBonusSoundOff);
        goingFastAS.Stop();
    }

    private void CollectBonus(Collider other, AudioSource audioSource, AudioClip sound, AudioClip soundSlow)
    {
        other.gameObject.GetComponentInChildren<ParticleSystem>().Play();
        if (timeControl.activated)
        {
            audioSource.PlayOneShot(soundSlow);
        }
        else
        {
            audioSource.PlayOneShot(sound);
        }
        other.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void DisablePowers()
    {
        DisableTimeControl();
        DisableShrink();
    }

    private void DisableTimeControl()
    {
        timeControl.enabled = false;
    }

    private void DisableShrink()
    {
        shrink.enabled = false;
    }
}
