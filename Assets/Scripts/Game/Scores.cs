using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scores : MonoBehaviour
{
    public Text scoreText;

    private int currentScores;

    private void Start()
    {
        currentScores = 0;
        UpdateScoreText();
    }

    private void OnEnable()
    {
        GameEvents.AddScores += AddScores;
    }

    private void OnDisable()
    {
        GameEvents.AddScores -= AddScores;
    }

    private void AddScores(int scores)
    {
        currentScores += scores;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = currentScores.ToString();
    }
}
