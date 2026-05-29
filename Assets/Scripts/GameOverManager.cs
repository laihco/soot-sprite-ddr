using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Leaderboard UI")]
    public TMP_InputField nameInput;
    public TextMeshProUGUI leaderboardText;

    private const string HighScoreKey = "HighScore";
    private const string LeaderboardKey = "Leaderboard";

    private int currentScore;

    [Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new();
    }

    private void Start()
    {
        currentScore = PlayerPrefs.GetInt("LastScore", 0);

        int highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        scoreText.text = $"Score: {currentScore}";
        highScoreText.text = $"High Score: {highScore}";

        RefreshLeaderboard();
    }

    public void SubmitScore()
    {
        string playerName = nameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";

        LeaderboardData data = LoadLeaderboard();

        data.entries.Add(new LeaderboardEntry
        {
            playerName = playerName,
            score = currentScore
        });

        data.entries.Sort((a, b) => b.score.CompareTo(a.score));

        if (data.entries.Count > 10)
        {
            data.entries.RemoveRange(
                10,
                data.entries.Count - 10
            );
        }

        SaveLeaderboard(data);
        RefreshLeaderboard();

        nameInput.interactable = false;
    }

    private void RefreshLeaderboard()
    {
        LeaderboardData data = LoadLeaderboard();

        leaderboardText.text = "<b>Leaderboard</b>\n\n";

        if (data.entries.Count == 0)
        {
            leaderboardText.text += "No scores yet!";
            return;
        }

        for (int i = 0; i < data.entries.Count; i++)
        {
            leaderboardText.text +=
                $"{i + 1}. {data.entries[i].playerName} - {data.entries[i].score}\n";
        }
    }

    private LeaderboardData LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LeaderboardKey, "");

        if (string.IsNullOrEmpty(json))
            return new LeaderboardData();

        return JsonUtility.FromJson<LeaderboardData>(json);
    }

    private void SaveLeaderboard(LeaderboardData data)
    {
        string json = JsonUtility.ToJson(data);

        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }
}