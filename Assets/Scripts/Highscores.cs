using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Highscores : MonoBehaviour
{
    string[] privateCode = {
        "9I4z7dm1HUuAvQzAhSPtmAsiMOwECmEEONy9qCNZNXTA",
        "IDDL8eyfkUyX6obmhjb9ggH7sm4VDkM0iS4iLsZxq2XA",
        "pwWEvxseg0ORvkksfTepigBzMBKj0b5UWW0PXHZI-hwA"};

    string[] publicCode = {
        "62346f248f40bc123c2b39f7",
        "6234a07f8f40bc123c2bb560",
        "6234a0848f40bc123c2bb569" };

    string webURL = "http://dreamlo.com/lb/";

    [HideInInspector] public Highscore[] highscoresList;
    Leaderboard leaderboard;
    PlayerOptions options;
    ScoreManager scoreManager;
    [HideInInspector] public bool isDownloading = false;
    [HideInInspector] public bool isUploading = false;
    [HideInInspector] public int[] ranks = { -1, -1, -1 };

    private void Awake()
    {
        print("DON'T FORGET TO REMOVE BEFORE GITHUB");

        leaderboard = GetComponent<Leaderboard>();
        options = GetComponent<PlayerOptions>();
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    public void AddNewHighscore(string username, int score, int difficulty)
    {
        StartCoroutine(UploadNewHighscore(username, score, difficulty));
    }

    IEnumerator UploadNewHighscore(string username, int score, int difficulty)
    {
        isUploading = true;
        UnityWebRequest www = new UnityWebRequest(webURL + privateCode[difficulty] + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            print("Error uploading: " + www.error);
            isUploading = false;
        }
        else
        {
            print("Upload successful");
            DownloadHighscores(difficulty);
            isUploading = false;
        }
    }

    public void DownloadHighscores(int difficulty)
    {
        StartCoroutine(DownloadHighscoresFromDB(difficulty));
    }

    IEnumerator DownloadHighscoresFromDB(int difficulty)
    {
        isDownloading = true;

        UnityWebRequest www = UnityWebRequest.Get(webURL + publicCode[difficulty] + "/pipe");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            print("Error downloading: " + www.error);
            isDownloading = false;
        }
        else
        {
            FormatAndRankHighscores(www.downloadHandler.text, difficulty);
            leaderboard.OnHighscoresDownloaded(highscoresList);
            isDownloading = false;
        }
    }

    void FormatAndRankHighscores(string textStream, int difficulty)
    {
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        highscoresList = new Highscore[entries.Length];
        string[] userHighscoresPrefs = { scoreManager.EASY_HIGHSCORE, scoreManager.MEDIUM_HIGHSCORE, scoreManager.HARD_HIGHSCORE };
        int playerScore = 0;
        string playerUsername = "";
        if (options.usernameSet)
        {
            playerUsername = PlayerPrefs.GetString(options.USERNAME);
            playerScore = PlayerPrefs.GetInt(userHighscoresPrefs[difficulty], 0);
        }

        // Iterate over all the database entries
        for (int i=0; i < entries.Length; i++)
        {
            // Split piped line into username and score
            string[] entry = entries[i].Split(new char[] { '|' });
            string username = entry[0];
            int score = int.Parse(entry[1]);

            highscoresList[i] = new Highscore(username, score);

            // Handle ranks for current player
            if (options.usernameSet)
            {
                if (username == playerUsername && score == playerScore)
                {
                    ranks[difficulty] = i + 1;
                }
            }
        }
    }
}

public struct Highscore
{
    public string username;
    public int score;

    public Highscore(string _username, int _score)
    {
        username = _username;
        score = _score;
    }
}