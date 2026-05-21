using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SootDDR.Scoring;

namespace SootDDR.UI
{
    /// <summary>
    /// Manages the on-screen debug overlay: score, combo, song time, and the
    /// center-screen hit-feedback flash (PERFECT / GOOD / MISS).
    /// References are wired in the scene (or by SootDDRSceneBuilder at build time).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Debug Panel (top-left)")]
        public Text scoreText;
        public Text comboText;
        public Text songTimeText;

        [Header("Hit Feedback (center screen)")]
        public Text  hitFeedbackText;
        [Tooltip("How long the hit result text stays visible (seconds).")]
        public float feedbackDuration = 0.6f;

        private Coroutine _feedbackCoroutine;

        // ── Public update methods (called by RhythmGameManager / ScoreManager) ───

        public void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (comboText != null)
                comboText.text = combo > 1 ? $"Combo: {combo}x" : string.Empty;
        }

        public void UpdateSongTime(float songTime)
        {
            if (songTimeText != null) songTimeText.text = $"Time: {songTime:F2}s";
        }

        /// <summary>Interrupts any existing feedback flash and shows the new result.</summary>
        public void ShowHitFeedback(HitResult result)
        {
            if (hitFeedbackText == null) return;
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(FlashFeedback(result));
        }

        // ── Private ──────────────────────────────────────────────────────────────

        private IEnumerator FlashFeedback(HitResult result)
        {
            hitFeedbackText.text = result switch
            {
                HitResult.Perfect => "PERFECT",
                HitResult.Good    => "GOOD",
                HitResult.Miss    => "MISS",
                _                 => string.Empty,
            };

            hitFeedbackText.color = result switch
            {
                HitResult.Perfect => Color.yellow,
                HitResult.Good    => Color.green,
                HitResult.Miss    => Color.red,
                _                 => Color.white,
            };

            hitFeedbackText.gameObject.SetActive(true);
            yield return new WaitForSeconds(feedbackDuration);
            hitFeedbackText.gameObject.SetActive(false);
        }
    }
}
