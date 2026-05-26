using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SootDDR.Scoring;

namespace SootDDR.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Debug Panel (top-left)")]
        public Text scoreText;
        public Text comboText;
        public Text songTimeText;

        [Header("Hit Feedback (center screen)")]
        public Text hitFeedbackText;

        [Tooltip("How long the hit result text stays visible (seconds).")]
        public float feedbackDuration = 0.6f;

        private Coroutine _feedbackCoroutine;

        // ─────────────────────────────────────────────
        // SCORE UI
        // ─────────────────────────────────────────────
        public void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }

        public void UpdateCombo(int combo)
        {
            if (comboText != null)
                comboText.text = combo > 1 ? $"Combo: {combo}x" : "Combo: 0";
        }

        public void UpdateSongTime(float songTime)
        {
            if (songTimeText != null)
                songTimeText.text = $"Time: {songTime:F2}s";
        }

        // ─────────────────────────────────────────────
        // HIT FEEDBACK (WITH COMBO DISPLAY)
        // ─────────────────────────────────────────────
        public void ShowHitFeedback(HitResult result, int combo = -1)
        {
            if (hitFeedbackText == null)
                return;

            if (_feedbackCoroutine != null)
                StopCoroutine(_feedbackCoroutine);

            _feedbackCoroutine = StartCoroutine(FlashFeedback(result, combo));
        }

        // ─────────────────────────────────────────────
        // INTERNAL DISPLAY
        // ─────────────────────────────────────────────
        private IEnumerator FlashFeedback(HitResult result, int combo)
        {
            hitFeedbackText.gameObject.SetActive(true);

            string comboTextInline =
                combo >= 0 ? $" x{combo}" : "";

            switch (result)
            {
                case HitResult.Perfect:
                    hitFeedbackText.text = "PERFECT" + comboTextInline;
                    hitFeedbackText.color = Color.yellow;
                    break;

                case HitResult.Good:
                    hitFeedbackText.text = "GOOD" + comboTextInline;
                    hitFeedbackText.color = Color.green;
                    break;

                case HitResult.Bad:
                    hitFeedbackText.text = "BAD" + comboTextInline;
                    hitFeedbackText.color = new Color(1f, 0.6f, 0f);
                    break;

                case HitResult.Miss:
                    hitFeedbackText.text = "MISS" + comboTextInline;
                    hitFeedbackText.color = Color.red;
                    break;

                default:
                    hitFeedbackText.text = "";
                    break;
            }

            yield return new WaitForSeconds(feedbackDuration);

            hitFeedbackText.text = "";
            hitFeedbackText.gameObject.SetActive(false);
        }
    }
}