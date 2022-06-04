using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] highscoresText;
    [SerializeField] Color topColor;
    [SerializeField] Color bottomColor;
    [SerializeField] PlayerOptions options;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] TextMeshProUGUI[] rankTexts;
    [SerializeField] RawImage[] localityOutline;
    [SerializeField] Color localityOutlineColor;
    [SerializeField] Button[] difficultyButtons;
    [SerializeField] RawImage[] modesOutline;
    [SerializeField] Color modeOutlineColor;
    
    Highscores highscoreManager;
    IEnumerator currentRefreshCoroutine = null;
    int currentOnlineHighscoreDisplayed;
    //string HAS_PUBLISHED_ONCE = "hasPublishedOnce";
    //bool hasPublished = false;
    int[] lastPublishedHighscores = { -1, -1, -1 };
    bool isUpdatingRanks = false;

    void Start()
    {
        DisplayLoading();

        highscoreManager = GetComponent<Highscores>();
        ManageDifficultyButtons(options.difficulty);
        //currentRefreshCoroutine = RefreshHighscores(options.difficulty);
        //StartCoroutine(currentRefreshCoroutine);

        ColorGradientHighscores();

        //hasPublished = bool.Parse(PlayerPrefs.GetString(HAS_PUBLISHED_ONCE, "false"));
    }

    private void ColorGradientHighscores()
    {
        float t;
        for (int i = 0; i < highscoresText.Length; i++)
        {
            t = ((float)i / (highscoresText.Length - 1));
            highscoresText[i].color = Color.Lerp(topColor, bottomColor, t);
        }
    }

    public void ManageLocalityButtons(int locality)
    {
        localityOutline[locality].color = localityOutlineColor;
        localityOutline[1-locality].color = Color.white;
    }

    public void ManageDifficultyButtons(int difficulty)
    {
        for (int i=0; i<difficultyButtons.Length; i++)
        {
            if (i == difficulty)
            {
                difficultyButtons[i].interactable = false;
                currentOnlineHighscoreDisplayed = i;
                modesOutline[i].color = modeOutlineColor;
            }
            else
            {
                difficultyButtons[i].interactable = true;
                modesOutline[i].color = Color.white;
            }
        }
    }

    private void DisplayLoading()
    {
        for (int i = 0; i < highscoresText.Length; i++)
        {
            highscoresText[i].text = i + 1 + ". Loading...";
        }
    }

    public void OnHighscoresDownloaded(Highscore[] highscoreList)
    {
        for (int i = 0; i < highscoresText.Length; i++)
        {
            highscoresText[i].text = "<mspace=0.75em>" + (i + 1) + "</mspace>" +  ". " ;
            if (i < highscoreList.Length)
            {
                highscoresText[i].text += highscoreList[i].username + ": " + highscoreList[i].score.ToString("N0");
            }
        }
    }

    public void CheckLastPublishedHighscores()
    {
        int difficulty = 0;
        bool upToDate = true;
        foreach (string SCORE in new string[] { scoreManager.EASY_HIGHSCORE, scoreManager.MEDIUM_HIGHSCORE, scoreManager.HARD_HIGHSCORE })
        {
            int score = PlayerPrefs.GetInt(SCORE, 0);
            upToDate &= score <= lastPublishedHighscores[difficulty];
            difficulty++;
        }

        // Update publish button state accordingly
        if (!upToDate)
        {
            //hasPublished = false;
            PublishHighscores();
            print("PUBLISH");
        }
    }

    public void PublishHighscores()
    {
        if (options.usernameSet)
            StartCoroutine(PublishUserHighscores());
    }

    IEnumerator PublishUserHighscores()
    {
        // Display "loading" in place of the scores
        for (int i=0; i<rankTexts.Length; i++)
            rankTexts[i].text = "Loading...";

        // Publish player's highscores
        string username = PlayerPrefs.GetString(options.USERNAME);
        int difficulty = 0;
        bool doneUploading = false;
        while (!doneUploading)
        {
            foreach (string SCORE in new string[] { scoreManager.EASY_HIGHSCORE, scoreManager.MEDIUM_HIGHSCORE, scoreManager.HARD_HIGHSCORE })
            {
                int score = PlayerPrefs.GetInt(SCORE, 0);
                highscoreManager.AddNewHighscore(username, score, difficulty);
                while (highscoreManager.isUploading)
                {
                    yield return null;
                }
                lastPublishedHighscores[difficulty] = score;
                difficulty++;
            }
            doneUploading = true;
        }
        //PlayerPrefs.SetString(HAS_PUBLISHED_ONCE, "true");
        //hasPublished = true;

        CheckLastPublishedHighscores();

        // Update ranks based on latest highscores
        StartCoroutine(UpdateRanks());
        while(isUpdatingRanks)
        {
            yield return null;
        }
    }

    public void OnUpdateRanks()
    {
        StartCoroutine(UpdateRanks());
    }

    IEnumerator UpdateRanks()
    {
        isUpdatingRanks = true;

        // Download latest highscores to take into account the player's highscores that were just published
        //DownloadAllHighscores();
        bool doneDownloading = false;
        while (!doneDownloading)
        {
            for (int i = 0; i < 3; i++)
            {
                highscoreManager.DownloadHighscores(i);
                while (highscoreManager.isDownloading)
                {
                    yield return null;
                }
            }
            doneDownloading = true;
        }

        // Update ranks
        for (int i=0; i<rankTexts.Length; i++)
        {
            //bool hasPublishedOnce = bool.Parse(PlayerPrefs.GetString(HAS_PUBLISHED_ONCE, "false"));
            if (options.usernameSet)// && hasPublishedOnce)
            {
                if (highscoreManager.ranks[i] == -1)
                    rankTexts[i].text = ">100";
                else
                    rankTexts[i].text = highscoreManager.ranks[i].ToString();
            }
            else
            {
                rankTexts[i].text = "???";
            }
        }

        isUpdatingRanks = false;
        yield return null;
    }

    public void RefreshCurrentOnlineHighscoreDisplayed()
    {
        RefreshHighscoreFromButton(currentOnlineHighscoreDisplayed);
    }

    public void RefreshHighscoreFromButton(int difficulty)
    {
        if (currentRefreshCoroutine != null)
            StopCoroutine(currentRefreshCoroutine);
        currentRefreshCoroutine = RefreshHighscores(difficulty);
        StartCoroutine(currentRefreshCoroutine);
    }

    public IEnumerator RefreshHighscores(int difficulty)
    {
        print("DOWNLOAD");
        bool doneDownloading = false;
        while (!doneDownloading)
        {
            DisplayLoading();
            highscoreManager.DownloadHighscores(difficulty);
            while (highscoreManager.isDownloading)
            {
                yield return null;
            }
            doneDownloading = true;
        }
    }
}
