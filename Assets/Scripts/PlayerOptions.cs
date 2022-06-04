using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class PlayerOptions : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] MenuManager menu;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] AudioClip optionChangeSound;

    // Sensitivity
    [Header("Sensitivity")]
    [SerializeField] float defaultSensitivity = 25f;
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] TMP_Text sensitivityValue;
    string SENSITIVITY = "horizontalSensitivity";
    [HideInInspector] public float sensitivity;

    // Powers
    [Header("Powers")]
    [SerializeField] PowerManager powerManager;
    [SerializeField] GameObject powerButtonsHolder;
    [SerializeField] GameObject[] powerButtons;
    [SerializeField] GameObject[] makeTransparent;
    [SerializeField] float targetSideXPosition;
    [SerializeField] GameObject blackPanel;
    [SerializeField] GameObject backupPanel;
    [SerializeField] float targetXOffsetCenter = 0;
    [SerializeField] float targetYOffsetSide = 0;
    [SerializeField] Vector2 sideHeightBounds;
    [SerializeField] Vector2 centerHeightBounds;
    [SerializeField] float visibilityDuration = 0.75f;
    [SerializeField] Slider sideSlider;
    [SerializeField] Slider heightSlider;
    [SerializeField] float defaultHeight = -500f;
    [SerializeField] TMP_Text heightValue;
    string BUTTONS_SIDE = "buttonsSide";
    int buttonsSide;
    string BUTTONS_HEIGHT = "buttonsHeight";
    float buttonsHeight;
    IEnumerator currentPowerButtonsCoroutine;
    string currentHeightText = "";

    // Music
    [Header("Music")]
    [SerializeField] int defautVolume = 100;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] TMP_Text musicVolumeText;
    [SerializeField] MusicPlayer musicPlayer;
    string MUSIC_VOLUME = "musicVolume";
    float musicVolume;

    // Invincibility
    [Header("Invincibility")]
    [SerializeField] GameObject invincibilityHolder;

    // Difficulty
    [Header("Difficulty")]
    [SerializeField] int defaultDifficulty = 0;
    [SerializeField] Slider difficultySlider;
    [HideInInspector] public int difficulty;
    [HideInInspector] public string[] difficulties = { "Easy", "Medium", "Hard" };
    string DIFFICULTY = "Difficulty";

    // Username
    [Header("Username")]
    [SerializeField] TextMeshProUGUI noUsernameText;
    [SerializeField] GameObject setUsernameButton;
    [SerializeField] TextMeshProUGUI usernameText;
    [SerializeField] GameObject setUsernameText;
    [SerializeField] GameObject setAgainText;
    [HideInInspector] public string USERNAME = "USERNAME";
    [HideInInspector] public string username;
    string USERNAME_SET = "USERNAME_SET";
    Leaderboard leaderboard;
    [HideInInspector] public bool usernameSet = false;
    TouchScreenKeyboard keyboard;

    // Welcome menu
    [Header("Welcome Menu")]
    [SerializeField] GameObject welcomeMenu;
    string HAS_BEEN_WELCOMED = "hasBeenWelcomed";

    // VHS Effect
    [Header("VHS Effect")]
    [SerializeField] Toggle VHSToggle;
    [SerializeField] GameObject VHS;
    string ENABLE_VHS = "enableVHS";

    AudioSource optionsAS;
    bool awake = false;

    private void Awake()
    {
        optionsAS = gameObject.AddComponent<AudioSource>();

        // Difficulty
        difficulty = PlayerPrefs.GetInt(DIFFICULTY, defaultDifficulty);
        difficultySlider.value = difficulty;

        // Sensitivity
        sensitivity = PlayerPrefs.GetFloat(SENSITIVITY, defaultSensitivity);
        sensitivitySlider.value = sensitivity;
        sensitivityValue.text = sensitivity.ToString();

        // Powers buttons side (left, center, right) and height
        buttonsSide = PlayerPrefs.GetInt(BUTTONS_SIDE, 0);
        sideSlider.value = buttonsSide;
        buttonsHeight = PlayerPrefs.GetFloat(BUTTONS_HEIGHT, defaultHeight);
        powerButtonsHolder.transform.localPosition = new Vector3(powerButtonsHolder.transform.localPosition.x, buttonsHeight, powerButtonsHolder.transform.localPosition.z);
        heightSlider.value = buttonsHeight;
        // update height bounds
        float newMin, newMax;
        if (buttonsSide != 0)
        {
            newMin = sideHeightBounds[0];
            newMax = sideHeightBounds[1];
        }
        else
        {
            newMin = centerHeightBounds[0];
            newMax = centerHeightBounds[1];
        }
        heightSlider.minValue = newMin;
        heightSlider.maxValue = newMax;
        heightSlider.value = buttonsHeight;
        float t = 100 * Mathf.InverseLerp(heightSlider.minValue, heightSlider.maxValue, heightSlider.value);
        heightValue.text = t.ToString("0") + "%";

        // Music
        musicVolume = PlayerPrefs.GetInt(MUSIC_VOLUME, defautVolume);
        musicVolumeSlider.value = musicVolume;
        musicVolumeText.text = musicVolume.ToString("0") + "%";
        musicPlayer.ASMusic.volume = musicVolume / 100f;

        // Username
        leaderboard = GetComponent<Leaderboard>();
        usernameSet = bool.Parse(PlayerPrefs.GetString(USERNAME_SET, "false"));

        print("REMOVE");
        //usernameSet = true;
        //PlayerPrefs.SetString(USERNAME, "kek");

        if (!usernameSet)
        {
            usernameText.text = "NO USERNAME";
        }
        else
        {
            username = PlayerPrefs.GetString(USERNAME, "NO USERNAME");
            usernameText.text = username;
            setUsernameButton.SetActive(false);
            usernameText.gameObject.SetActive(true);
            noUsernameText.gameObject.SetActive(false);
        }

        // Welcome Menu
        bool hasBeenWelcomed = bool.Parse(PlayerPrefs.GetString(HAS_BEEN_WELCOMED, "false"));
        if (!hasBeenWelcomed)
        {
            welcomeMenu.SetActive(true);
            PlayerPrefs.SetString(HAS_BEEN_WELCOMED, "true");
        }

        // VHS Effect
        bool isVHSOn = bool.Parse(PlayerPrefs.GetString(ENABLE_VHS, "false"));
        if (isVHSOn)
        {
            VHSToggle.isOn = true;
            EnableVHSEffect();
        }
    }

    private void Start()
    {
        awake = true;
    }

    public void ChangeInvincibilitySide()
    {
        Vector3 localPos = invincibilityHolder.transform.localPosition;
        invincibilityHolder.transform.localPosition = new Vector3(-localPos.x, localPos.y, localPos.z);
    }

    public void DifficultySliderUpdated()
    {
        difficulty = (int)difficultySlider.value;
        PlayerPrefs.SetInt(DIFFICULTY, difficulty);
        scoreManager.SelectAppropriateHighscore();
    }

    public void SensitivitySliderUpdated()
    {
        UpdateSliderValue(SENSITIVITY, sensitivitySlider, sensitivityValue, ref sensitivity);
    }

    public void MusicVolumeSliderUpdated()
    {
        musicVolume = musicVolumeSlider.value;
        PlayerPrefs.SetInt(MUSIC_VOLUME, (int)musicVolume);
        musicPlayer.ASMusic.volume = musicVolume / 100f;
        musicVolumeText.text = musicVolume.ToString("0") + "%";
    }

    void UpdateSliderValue(string pref, Slider slider, TMP_Text text, ref float value)
    {
        PlayerPrefs.SetFloat(pref, slider.value);
        value = PlayerPrefs.GetFloat(pref);
        text.text = value.ToString();
    }

    public void ChangePowerButtonsSide()
    {
        ChangeSideSliderBounds();

        float height = PlayerPrefs.GetFloat(BUTTONS_HEIGHT, defaultHeight);
        switch (sideSlider.value)
        {
            case -1:
                powerButtonsHolder.transform.localPosition = new Vector3(targetSideXPosition, height, 0);
                powerButtons[0].transform.localPosition = Vector3.up * targetYOffsetSide;
                powerButtons[1].transform.localPosition = Vector3.zero;
                powerButtons[2].transform.localPosition = Vector3.down * targetYOffsetSide;
                break;

            case 0:
                powerButtonsHolder.transform.localPosition = new Vector3(0, height, 0);
                powerButtons[0].transform.localPosition = Vector3.left * targetXOffsetCenter;
                powerButtons[1].transform.localPosition = Vector3.zero;
                powerButtons[2].transform.localPosition = Vector3.right * targetXOffsetCenter;
                break;

            case 1:
                powerButtonsHolder.transform.localPosition = new Vector3(-targetSideXPosition, height, 0);
                powerButtons[0].transform.localPosition = Vector3.up * targetYOffsetSide;
                powerButtons[1].transform.localPosition = Vector3.zero;
                powerButtons[2].transform.localPosition = Vector3.down * targetYOffsetSide;
                break;
        }

        if (awake)
        {
            PlayerPrefs.SetInt(BUTTONS_SIDE, (int)sideSlider.value);

            // Briefly show the power buttons
            if (currentPowerButtonsCoroutine != null)
                StopCoroutine(currentPowerButtonsCoroutine);
            currentPowerButtonsCoroutine = ShowPowerButtons(visibilityDuration);
            StartCoroutine(currentPowerButtonsCoroutine);
        }
    }

    public void ChangePowerButtonsHeight()
    {
        if (awake)
        {
            powerButtonsHolder.transform.localPosition = new Vector3(powerButtonsHolder.transform.localPosition.x, heightSlider.value, powerButtonsHolder.transform.localPosition.z);
            float t = 100 * Mathf.InverseLerp(heightSlider.minValue, heightSlider.maxValue, heightSlider.value);
            heightValue.text = t.ToString("0") + "%";
            OnTextChanged(currentHeightText, heightValue.text);
            currentHeightText = heightValue.text;
            PlayerPrefs.SetFloat(BUTTONS_HEIGHT, powerButtonsHolder.transform.localPosition.y);

            // Briefly show the power buttons
            if (currentPowerButtonsCoroutine != null)
                StopCoroutine(currentPowerButtonsCoroutine);
            currentPowerButtonsCoroutine = ShowPowerButtons(visibilityDuration);
            StartCoroutine(currentPowerButtonsCoroutine);
        }
    }

    void OnTextChanged(string originalText, string newText)
    {
        if (newText != originalText)
            PlayOptionSound();
    }

    private void ChangeSideSliderBounds()
    {
        // Get current % value from the slider
        int nchars = heightValue.text.Length;
        float currentFraction = float.Parse(heightValue.text.Substring(0, nchars - 1)) / 100f;

        // Determine new bounds for the current side (left, middle, right)
        float newMin = 0f;
        float newMax = 0f;
        if (sideSlider.value != 0)
        {
            newMin = sideHeightBounds[0];
            newMax = sideHeightBounds[1];
        }
        else
        {
            newMin = centerHeightBounds[0];
            newMax = centerHeightBounds[1];
        }

        if (awake)
        {
            heightSlider.minValue = newMin;
            heightSlider.maxValue = newMax;
            float newHeight = Mathf.Lerp(newMin, newMax, currentFraction);
            PlayerPrefs.SetFloat(BUTTONS_HEIGHT, newHeight);
            heightSlider.value = newHeight;
        }
    }

    IEnumerator ShowPowerButtons(float duration)
    {
        SetUnnecessaryUITransparent(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        SetUnnecessaryUITransparent(false);
    }

    private void SetUnnecessaryUITransparent(bool enable)
    {
        foreach (GameObject button in powerButtons)
        {
            button.SetActive(enable);
            button.GetComponent<Button>().interactable = enable;
        }

        foreach (GameObject obj in makeTransparent)
        {
            obj.SetActive(!enable);
        }
        blackPanel.GetComponent<RawImage>().enabled = !enable;
        backupPanel.SetActive(enable);
    }

    public void PlayOptionSound()
    {
        if (awake)
        {
            if (!optionsAS.isPlaying)
                optionsAS.PlayOneShot(optionChangeSound);
        }
    }

    public void SetUsername()
    {
        StartCoroutine(WaitForInputAndSetUsername());
    }

    public void EnableVHSEffect()
    {
        string isOnPref;
        if (VHSToggle.isOn)
        {
            isOnPref = "true";
            StartCoroutine(LoadAndPlayVHS());
            print("PREPLAY");
        }
        else
        {
            isOnPref = "false";
            VHS.GetComponent<MeshRenderer>().enabled = false;
            VHS.GetComponent<VideoPlayer>().enabled = false;
            VHS.SetActive(false);
        }
        PlayerPrefs.SetString(ENABLE_VHS, isOnPref);
    }

    // Play video only when it's ready to avoid white frames at the beginning
    IEnumerator LoadAndPlayVHS()
    {
        VHS.SetActive(true);
        VideoPlayer video = VHS.GetComponent<VideoPlayer>();
        video.enabled = true;
        video.Prepare();
        while (!video.isPrepared)
            yield return new WaitForEndOfFrame();

        video.frame = 0;
        video.gameObject.GetComponent<MeshRenderer>().enabled = true;
        video.Play();
    }

    public IEnumerator WaitForInputAndSetUsername()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NamePhonePad, false, false, false, false, "Username (3 to 10 char. max.)", 10);

        while (keyboard.status != TouchScreenKeyboard.Status.Done)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (keyboard.text.Length >= 3)
        {
            usernameSet = true;
            PlayerPrefs.SetString(USERNAME_SET, "true");

            username = keyboard.text;
            usernameText.text = username;
            PlayerPrefs.SetString(USERNAME, username);
            setUsernameButton.SetActive(false);
            leaderboard.CheckLastPublishedHighscores();
            usernameText.gameObject.SetActive(true);
            noUsernameText.gameObject.SetActive(false);
        }
        else
        {
            setUsernameText.SetActive(false);
            setAgainText.SetActive(true);
        }
    }
}