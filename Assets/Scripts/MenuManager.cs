using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI highScoreText;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] ObstacleManager obstacleManager;
    [SerializeField] ScoreManager scoreManager;

    KeyCode reloadKey = KeyCode.R;

    private void Start()
    {
        highScoreText.text = GetHighScore();
    }

    private void Update()
    {
        highScoreText.text = GetHighScore();

        if (Input.GetKeyDown(reloadKey))
        {
            LoadNewGame();
        }
    }

    public void Run()
    {
        playerMovement.enabled = true;
        obstacleManager.enabled = true;
        scoreManager.enabled = true;
    }
    string GetHighScore()
    {
        return PlayerPrefs.GetFloat("highScore", 0f).ToString("0");
    }

    public void LoadNewGame()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
