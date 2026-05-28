using System;
using UnityEngine;

namespace SootDDR.Scoring
{
    public enum HitResult { Perfect, Good, Miss }

    /// <summary>
    /// Tracks score and combo state. Exposes an event so any system (UI, audio,
    /// particle effects) can react to judgements without tight coupling.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Values")]
        public int perfectScore = 100;
        public int goodScore    = 50;

        public int TotalScore { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }
        public int HighScore { get; private set; }

        /// <summary>Fired after every judgement: (result, points awarded this hit, current combo).</summary>
        public event Action<HitResult, int, int> OnHitRegistered;

        public void RegisterHit(HitResult result)
        {
            if (result == HitResult.Miss)
            {
                Combo = 0;
                OnHitRegistered?.Invoke(result, 0, 0);
                return;
            }

            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);

            int basePoints = result == HitResult.Perfect ? perfectScore : goodScore;

            // Combo multiplier: +5 % per consecutive hit, capped at +50 %
            float multiplier = 1f + Mathf.Min(Combo - 1, 10) * 0.05f;
            int   points     = Mathf.RoundToInt(basePoints * multiplier);

            TotalScore += points;
            OnHitRegistered?.Invoke(result, points, Combo);
        }

        public void ResetScore()
        {
            TotalScore = 0;
            Combo      = 0;
            MaxCombo   = 0;
        }

        public void CheckHighScore()
        {
            HighScore = PlayerPrefs.GetInt("HighScore", 0);
            if (HighScore < TotalScore)
            {
                HighScore = TotalScore;
                PlayerPrefs.SetInt("HighScore", TotalScore);
                PlayerPrefs.Save();
            }
        }
    }
}
