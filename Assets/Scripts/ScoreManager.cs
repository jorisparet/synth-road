using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI scoreMultiplierText;
    [SerializeField] TextMeshProUGUI newHighscoreText;
    [SerializeField] float baseMultiplier = 5f;
    [SerializeField] PlayerMovement player;

    bool active = false;
    float score = 0f;
    float highScore;
    int bonusMultiplier = 1;
    bool stop = false;
    bool displayHighscoreMessage = false;

    void Awake()
    {
        scoreText.text = "0";
        scoreMultiplierText.text = "x1";
        highScore = PlayerPrefs.GetFloat("highScore", 0f);
    }

    void Update()
    {
        if (active)
            ComputeScore();
    }

    private void ComputeScore()
    {
        bonusMultiplier = 1 + player.bonusCount;
        if (!stop)
        {
            score += Time.deltaTime * baseMultiplier * bonusMultiplier;
        }
        scoreText.text = score.ToString("0");
        scoreMultiplierText.text = "x" + bonusMultiplier.ToString();
        Highscore();
    }

    public void Initialize()
    {
        active = true;
        score = 0f;
        bonusMultiplier = 1;
        stop = false;
        displayHighscoreMessage = false;
    }

    public void Stop()
    {
        stop = true;
    }

    private void Highscore()
    {
        if (score > highScore)
        {
            PlayerPrefs.SetFloat("highScore", score);
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
            t = 1f - (duration - elapsed) / duration;
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
    }
}
