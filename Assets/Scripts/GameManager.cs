using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] MovableSpawner movableSpawner;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GroundScroller groundScroller;
    [SerializeField] MenuManager menuManager;
    [Header("Powers")]
    [SerializeField] PowerManager powerManager;
    [SerializeField] TimeControl timeControl;
    [SerializeField] Shrink shrink;
    [SerializeField] ForceField forceField;
    [Header("Settings")]
    [SerializeField] MusicPlayer musicPlayer;
    [SerializeField] Button pauseButton;
    [SerializeField] AudioClip countdownSound;
    [SerializeField] TextMeshProUGUI countdownText;
    [Header("Rewarded Ad")]
    [SerializeField] Button addButton;
    [SerializeField] RawImage buttonOutline;
    [SerializeField] Image lightningImage;
    [SerializeField] TextMeshProUGUI buttonText;
    [SerializeField] GameObject unlockedText;
    [SerializeField] GameObject playButton;
    [SerializeField] GameObject lockImage;

    [HideInInspector] public bool gamePaused;
    float timeScaleAtPause;
    AudioSource audioSource;

    private void Awake()
    {
        // Target frame rate
        Application.targetFrameRate = 60;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.ignoreListenerPause = true;
    }

    // Initialize all the main game objects to their starting values and parameters
    public void Run()
    {
        mainMenu.SetActive(false);
        menuManager.DisplayScoreHolder(true);
        playerMovement.Initialize();
        movableSpawner.Initialize();
        scoreManager.Initialize();
        groundScroller.Initialize();
        powerManager.Initialize();
        timeControl.Initialize();
        shrink.Initialize();
        forceField.Initialize();
        pauseButton.gameObject.SetActive(true);
    }

    public void PauseGame()
    {
        gamePaused = true;

        playerMovement.active = false;
        powerManager.EnablePowerButtons(false);
        movableSpawner.active = false;

        AudioListener.pause = true;
        timeScaleAtPause = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        StartCoroutine(CountdownBeforeResume());
    }

    IEnumerator CountdownBeforeResume()
    {
        // Prevent from pausing while game is still in pause
        pauseButton.interactable = false;

        // Countdown on screen
        float elapsed = 0f;
        int elapsedCompare = 0;
        int countdown = 3;
        countdownText.gameObject.SetActive(true);
        countdownText.text = countdown.ToString();
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(countdownSound);
        while (elapsed < 3f)
        {
            if ((int) elapsed != elapsedCompare)
            {
                elapsedCompare = (int)elapsed;
                audioSource.PlayOneShot(countdownSound);
                countdownText.text = (countdown - elapsedCompare).ToString();
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        countdownText.gameObject.SetActive(false);

        // Resume game
        gamePaused = false;
        playerMovement.active = true;
        if (powerManager.fullBar)
            powerManager.EnablePowerButtons(true);
        movableSpawner.active = true;
        AudioListener.pause = false;
        Time.timeScale = timeScaleAtPause;

        // Allow to pause again
        pauseButton.interactable = true;
    }

    // Workaround to avoid crash in RewardedAdsButton.cs
    public IEnumerator EnableAdReward()
    {
        // Let some time pass after ad is finished
        float elapsed = 0f;
        while (elapsed < 0.1)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Enable rewards
        movableSpawner.invincibilityBonusProbability = 0.15f;
        forceField.unlocked = true;
        addButton.interactable = false;
        addButton.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        buttonOutline.gameObject.SetActive(false);
        lightningImage.enabled = false;
        buttonText.text = "UNLOCKED";
        unlockedText.SetActive(true);
        playButton.SetActive(true);
        lockImage.SetActive(false);
    }
}
