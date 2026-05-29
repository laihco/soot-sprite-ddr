using System;
using UnityEngine;

namespace SootDDR.Scoring
{
    public enum HitResult { Perfect, Good, Bad, Miss }

    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Score Values")]
        public int perfectScore = 100;
        public int goodScore = 50;
        public int badScore = 10;

        [Header("Enemy Multipliers")]
        [Tooltip("Index corresponds to enemyID. e.g., Element 0 is for enemyID 0.")]
        public float[] enemyMultipliers = { 1.0f, 1.2f, 1.5f, 2.0f, 2.5f };

        public int TotalScore { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public int HighScore { get; private set; }

        /// <summary>
        /// Fired after every judgement:
        /// (result, points awarded this hit, current combo)
        /// </summary>
        public event Action<HitResult, int, int> OnHitRegistered;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterHit(HitResult result, int enemyID = 0)
        {
            if (enemyID != 4 && result == HitResult.Miss)
            {
                Combo = 0;
                OnHitRegistered?.Invoke(result, 0, 0);
                return;
            }

            if (enemyID != 4 && (result == HitResult.Bad || result == HitResult.Good || result == HitResult.Perfect))
            {
                Combo = 0;
                OnHitRegistered?.Invoke(result, 0, 0);
                return;
            }

            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);

            // 1. Base Score
            int basePoints = 0;
            switch(result)
            {
                case HitResult.Perfect: basePoints = perfectScore; break;
                case HitResult.Good: basePoints = goodScore; break;
                case HitResult.Bad: basePoints = badScore; break;
            }

            // 2. Combo Multiplier (+5% per hit, capped at +50%)
            float comboMultiplier = 1f + Mathf.Min(Combo - 1, 10) * 0.05f;

            // 3. Enemy Multiplier
            float enemyMultiplier = 1.0f;
            if (enemyMultipliers != null && enemyID >= 0 && enemyID < enemyMultipliers.Length)
            {
                enemyMultiplier = enemyMultipliers[enemyID];
            }

            // Final Calculation
            int points = Mathf.RoundToInt(basePoints * comboMultiplier * enemyMultiplier);

            TotalScore += points;

            OnHitRegistered?.Invoke(result, points, Combo);
        }

        public void ResetScore()
        {
            TotalScore = 0;
            Combo = 0;
            MaxCombo = 0;
        }

        public void CheckHighScore()
        {
            HighScore = PlayerPrefs.GetInt("HighScore", 0);

            if (TotalScore > HighScore)
            {
                HighScore = TotalScore;

                PlayerPrefs.SetInt("HighScore", HighScore);
                PlayerPrefs.Save();
            }
        }

        public void SaveFinalScore()
        {
            PlayerPrefs.SetInt("LastScore", TotalScore);
            CheckHighScore();
            PlayerPrefs.Save();
        }
    }
}