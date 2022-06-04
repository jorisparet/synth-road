using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI scoreMultiplierText;
    [SerializeField] TextMeshProUGUI newHighscoreText;
    [SerializeField] TextMeshProUGUI currentHighscoreText;
    [SerializeField] TextMeshProUGUI currentDifficultyText;
    [SerializeField] TextMeshProUGUI easyHighscoreText;
    [SerializeField] TextMeshProUGUI mediumHighscoreText;
    [SerializeField] TextMeshProUGUI hardHighscoreText;

    [SerializeField] float baseMultiplier = 5f;
    [SerializeField] PlayerMovement player;
    [SerializeField] MenuManager menu;
    [SerializeField] PlayerOptions options;

    bool active = false;
    [HideInInspector] public string CURRENT_HIGHSCORE;
    [HideInInspector] public string EASY_HIGHSCORE = "EasyHighscore";
    [HideInInspector] public string MEDIUM_HIGHSCORE = "MediumHighscore";
    [HideInInspector] public string HARD_HIGHSCORE = "HardHighscore";
    [HideInInspector] public int currentHighscore;
    int score = 0;
    int bonusMultiplier = 1;
    float runningScore = 0f;
    bool stop = false;
    bool displayHighscoreMessage = false;

    public void Initialize()
    {
        active = true;
        score = 0;
        runningScore = 0f;
        bonusMultiplier = 1;
        stop = false;
        displayHighscoreMessage = false;
        newHighscoreText.gameObject.SetActive(false);
    }

    void Awake()
    {
        scoreText.text = "0";
        scoreMultiplierText.text = "x1";

        // Set all highscores to zero when first running the game
        foreach (string HIGHSCORE in new string[] { EASY_HIGHSCORE, MEDIUM_HIGHSCORE, HARD_HIGHSCORE })
        {
            if (!PlayerPrefs.HasKey(EASY_HIGHSCORE))
            {
                PlayerPrefs.SetFloat(HIGHSCORE, 0f);
            }
        }
    }

    private void Start()
    {
        UpdateCurrentHighscore();
        UpdateCurrentDifficulty();
    }

    public void SelectAppropriateHighscore()
    {
        switch (options.difficulty)
        {
            case 0:
                CURRENT_HIGHSCORE = EASY_HIGHSCORE;
                break;

            case 1:
                CURRENT_HIGHSCORE = MEDIUM_HIGHSCORE;
                break;

            case 2:
                CURRENT_HIGHSCORE = HARD_HIGHSCORE;
                break;
        }
    }

    public void UpdateCurrentHighscore()
    {
        SelectAppropriateHighscore();
        currentHighscore = PlayerPrefs.GetInt(CURRENT_HIGHSCORE, 0);
        currentHighscoreText.text = currentHighscore.ToString();
    }

    public void UpdateCurrentDifficulty()
    {
        currentDifficultyText.text = options.difficulties[options.difficulty];
    }

    public void UpdateDisplayedHighscores()
    {
        easyHighscoreText.text = PlayerPrefs.GetInt(EASY_HIGHSCORE, 0).ToString("N0");
        mediumHighscoreText.text = PlayerPrefs.GetInt(MEDIUM_HIGHSCORE, 0).ToString("N0");
        hardHighscoreText.text = PlayerPrefs.GetInt(HARD_HIGHSCORE, 0).ToString("N0");
    }

    void Update()
    {
        if (active && !stop)
            ComputeScore();
    }

    private void ComputeScore()
    {
        bonusMultiplier = 1 + player.bonusCount;
        if (!stop)
        {
            runningScore += Time.deltaTime * baseMultiplier * bonusMultiplier;
            score = (int)runningScore;
            print("CHECK IF DELTATIME IS OK");
        }
        scoreText.text = "<mspace=0.45em>" + score.ToString() + "</mspace>";
        scoreMultiplierText.text = "x" + bonusMultiplier.ToString();
        ManageHighscore();
    }

    public void StopUpdatingScore()
    {
        stop = true;
    }

    public void ResumeUpdatingScore()
    {
        stop = false;
    }

    private void ManageHighscore()
    {
        if (score > currentHighscore)
        {
            PlayerPrefs.SetInt(CURRENT_HIGHSCORE, score);
            if (!displayHighscoreMessage)
            {
                DisplayHighscoreMessage();
            }
        }
    }

    void DisplayHighscoreMessage()
    {
        displayHighscoreMessage = true;
        newHighscoreText.gameObject.SetActive(true);
    }

    // Zoom effect on score multiplier
    public IEnumerator MultiplierZoom(float duration, float targetScaleMultiplier)
    {
        float initialScale = scoreMultiplierText.transform.localScale.x;
        targetScaleMultiplier *= initialScale;
        float elapsed = 0f;
        float t;
        float newScale;
        float currentScale = initialScale;
        while (elapsed < duration)
        {
            t = Mathf.Clamp(1f - (duration - elapsed) / duration, 0, 1);
            newScale = Mathf.Lerp(currentScale, targetScaleMultiplier, t);
            scoreMultiplierText.transform.localScale = Vector3.one * newScale;
            elapsed += Time.unscaledDeltaTime;

            // Lerp back to initial scale at half time
            if (elapsed > duration/2)
            {
                targetScaleMultiplier = initialScale;
            }

            yield return null;
        }
        scoreMultiplierText.transform.localScale = Vector3.one;
    }

    public void ResetAllHighscores()
    {
        foreach (string HIGHSCORE in new string[] { EASY_HIGHSCORE, MEDIUM_HIGHSCORE, HARD_HIGHSCORE })
        {
            PlayerPrefs.SetInt(HIGHSCORE, 0);
            UpdateDisplayedHighscores();
        }
    }
}
