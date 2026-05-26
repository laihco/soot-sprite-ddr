using System;
using UnityEngine;

namespace SootDDR.Scoring
{
    public enum HitResult
    {
        Perfect,
        Good,
        Bad,
        Miss
    }

    /// <summary>
    /// Tracks score and combo state. Exposes an event so UI/audio/VFX can react.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Values")]
        public int perfectScore = 100;
        public int goodScore = 50;
        public int badScore = 20;

        public int TotalScore { get; private set; }
        public int Combo { get; private set; }
        public int MaxCombo { get; private set; }

        /// <summary>
        /// (result, points awarded, current combo)
        /// </summary>
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

            int basePoints =
                result == HitResult.Perfect ? perfectScore :
                result == HitResult.Good ? goodScore :
                result == HitResult.Bad ? badScore :
                0;

            // Combo multiplier: +5% per hit, capped at +50%
            float multiplier = 1f + Mathf.Min(Combo - 1, 10) * 0.05f;
            int points = Mathf.RoundToInt(basePoints * multiplier);

            TotalScore += points;
            OnHitRegistered?.Invoke(result, points, Combo);
        }

        public void ResetScore()
        {
            TotalScore = 0;
            Combo = 0;
            MaxCombo = 0;
        }
    }
}