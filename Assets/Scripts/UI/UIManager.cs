using System.Collections;
using UnityEngine;
using TMPro;
using SootDDR.Scoring;

namespace SootDDR.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Debug Panel (top-left)")]
        public TMP_Text scoreText;
        public TMP_Text comboText;
        public TMP_Text songTimeText;

        [Header("Hit Feedback (center screen)")]
        public TMP_Text hitFeedbackText;

        [Tooltip("How long the hit result text stays visible (seconds).")]
        public float feedbackDuration = 0.6f;

        private Coroutine _feedbackCoroutine;

        // ─────────────────────────────────────────────
        // COLORS (soft / desaturated)
        // ─────────────────────────────────────────────
        private static readonly Color PerfectColor = new Color32(255, 255, 0, 255);
        private static readonly Color GoodColor    = new Color32(0, 255, 0, 255);
        private static readonly Color MissColor    = new Color32(255, 0, 0, 255);

        // ─────────────────────────────────────────────
        // SCORE UI
        // ─────────────────────────────────────────────
        public void UpdateScore(int score)
        {
            if (scoreText == null) return;
            scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (comboText == null) return;
            comboText.text = combo > 1 ? $"Combo: {combo}x" : "Combo: 0";
        }

        public void UpdateSongTime(float songTime)
        {
            if (songTimeText == null) return;
            songTimeText.text = $"Time: {songTime:F2}";
        }

        // ─────────────────────────────────────────────
        // HIT FEEDBACK ENTRY
        // ─────────────────────────────────────────────
        public void ShowHitFeedback(HitResult result, int combo = -1)
        {
            if (hitFeedbackText == null) return;

            if (_feedbackCoroutine != null)
                StopCoroutine(_feedbackCoroutine);

            _feedbackCoroutine = StartCoroutine(FeedbackRoutine(result, combo));
        }

        // ─────────────────────────────────────────────
        // CORE FEEDBACK LOGIC
        // ─────────────────────────────────────────────
        private IEnumerator FeedbackRoutine(HitResult result, int combo)
        {
            hitFeedbackText.gameObject.SetActive(true);

            ApplyFeedback(result, combo);

            yield return new WaitForSeconds(feedbackDuration);

            ClearFeedback();
            _feedbackCoroutine = null;
        }

        // ─────────────────────────────────────────────
        // APPLY FEEDBACK
        // ─────────────────────────────────────────────
        private void ApplyFeedback(HitResult result, int combo)
        {
            string comboTextInline = combo >= 0 ? $" x{combo}" : "";

            switch (result)
            {
                case HitResult.Perfect:
                    hitFeedbackText.text = $"Perfect{comboTextInline}";
                    hitFeedbackText.color = PerfectColor;
                    break;

                case HitResult.Good:
                    hitFeedbackText.text = $"Good{comboTextInline}";
                    hitFeedbackText.color = GoodColor;
                    break;

                case HitResult.Miss:
                    hitFeedbackText.text = $"Miss{comboTextInline}";
                    hitFeedbackText.color = MissColor;
                    break;

                default:
                    hitFeedbackText.text = "";
                    break;
            }
        }

        // ─────────────────────────────────────────────
        // CLEAR FEEDBACK
        // ─────────────────────────────────────────────
        private void ClearFeedback()
        {
            if (hitFeedbackText == null) return;

            hitFeedbackText.text = "";
            hitFeedbackText.gameObject.SetActive(false);
        }
    }
}