using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI newHighscoreText;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] MovableSpawner movableSpawner;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject scoreHolder;
    [SerializeField] Button playButton;
    [SerializeField] Button pauseButton;
    [Header("Game Over")]
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] TextMeshProUGUI difficultyText;
    [SerializeField] TextMeshProUGUI currentHighscoreText;
    [SerializeField] TextMeshProUGUI avoidedObstaclesText;

    PlayerOptions options;

    private void Awake()
    {
        options = GetComponent<PlayerOptions>();
    }

    public void DisplayMainMenu()
    {
        newHighscoreText.gameObject.SetActive(false);
        mainMenu.SetActive(true);
        ShowPauseButton(false);
        DisplayScoreHolder(false);
    }

    public void DisplayGameOverMenu()
    {
        gameOverMenu.SetActive(true);
        ShowPauseButton(false);
        difficultyText.text = options.difficulties[options.difficulty];
        currentHighscoreText.text = scoreManager.currentHighscore.ToString("0");
        avoidedObstaclesText.text = (movableSpawner.numberOfSpawnedObstacles - 4).ToString("0");
    }

    public void DisplayScoreHolder(bool display)
    {
        scoreHolder.SetActive(display);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowPauseButton(bool show)
    {
        pauseButton.gameObject.SetActive(show);
    }
}
