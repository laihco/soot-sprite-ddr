using System;
using System.Collections;
using UnityEngine;
using SootDDR.Scoring;

namespace SootDDR.Gameplay
{
    /// <summary>
    /// Attached to the soot sprite prefab.  Handles:
    ///   • Pop-up scale animation when spawned
    ///   • Osu-style shrinking timing circle (LineRenderer child)
    ///   • TryHit() judgement (Perfect / Good / Miss)
    ///   • Auto-miss when the good window has passed
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SootSpriteNote : MonoBehaviour
    {
        // ── Inspector / Prefab wiring ─────────────────────────────────────────────
        [Header("Timing circle")]
        [Tooltip("LineRenderer on the 'TimingCircle' child GameObject.")]
        public LineRenderer timingCircle;

        [Header("Timing circle radii")]
        public float circleStartRadius = 0.9f;
        public float circleEndRadius   = 0.35f;

        [Header("Popup animation")]
        public float popupDuration = 0.12f;

        // ── Runtime state ─────────────────────────────────────────────────────────
        private double       _hitDspTime;
        private float        _approachDuration;
        private float        _perfectWindow;
        private float        _goodWindow;
        private Func<float>  _getSongTime;
        private ScoreManager _scoreManager;
        private Action<SootSpriteNote> _onDone;

        private float  _spawnSongTime;   // song time at which this note was spawned
        private bool   _judged;
        private static readonly int CircleSegments = 64;

        // ── Initialisation ────────────────────────────────────────────────────────

        /// <summary>
        /// Called by HoleController immediately after Instantiate.
        /// </summary>
        public void Initialize(double hitDspTime, float approachDuration,
                               float perfectWindow, float goodWindow,
                               Func<float> getSongTime, ScoreManager scoreManager,
                               Action<SootSpriteNote> onDone)
        {
            _hitDspTime       = hitDspTime;
            _approachDuration = approachDuration;
            _perfectWindow    = perfectWindow;
            _goodWindow       = goodWindow;
            _getSongTime      = getSongTime;
            _scoreManager     = scoreManager;
            _onDone           = onDone;

            _spawnSongTime = getSongTime();

            SetupTimingCircle();
            StartCoroutine(PopupAnimation());
        }

        // ── Unity loop ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (_judged) return;

            float songTime = _getSongTime();
            float elapsed  = songTime - _spawnSongTime;
            float t        = Mathf.Clamp01(elapsed / _approachDuration);

            UpdateTimingCircle(t);

            // Auto-miss once we're past the good window
            double dspNow = AudioSettings.dspTime;
            double delta  = dspNow - _hitDspTime;
            if (delta > _goodWindow)
            {
                Judge(HitResult.Miss);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by HoleController when the player presses the matching key.
        /// Returns the hit result; returns null if already judged.
        /// </summary>
        public HitResult? TryHit(double inputDsp)
        {
            if (_judged) return null;

            double delta = Math.Abs(inputDsp - _hitDspTime);

            HitResult result;
            if (delta <= _perfectWindow)
                result = HitResult.Perfect;
            else if (delta <= _goodWindow)
                result = HitResult.Good;
            else
                return null;  // pressed too early (before good window opens)

            Judge(result);
            return result;
        }

        // ── Internals ─────────────────────────────────────────────────────────────

        private void Judge(HitResult result)
        {
            if (_judged) return;
            _judged = true;

            _scoreManager.RegisterHit(result);

            if (result == HitResult.Miss)
                StartCoroutine(MissAnimation());
            else
                StartCoroutine(HitAnimation());
        }

        private void SetupTimingCircle()
        {
            if (timingCircle == null) return;

            timingCircle.positionCount = CircleSegments + 1;
            timingCircle.loop          = false;
            timingCircle.useWorldSpace = true;
            timingCircle.startWidth    = 0.04f;
            timingCircle.endWidth      = 0.04f;
            DrawCircle(timingCircle, transform.position, circleStartRadius);
        }

        private void UpdateTimingCircle(float t)
        {
            if (timingCircle == null) return;

            float r = Mathf.Lerp(circleStartRadius, circleEndRadius, t);

            // Fade white → yellow as t → 1
            Color col = Color.Lerp(Color.white, Color.yellow, t);
            timingCircle.startColor = col;
            timingCircle.endColor   = col;

            DrawCircle(timingCircle, transform.position, r);
        }

        private static void DrawCircle(LineRenderer lr, Vector3 centre, float radius)
        {
            int count = lr.positionCount;
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)(count - 1)) * 2f * Mathf.PI;
                lr.SetPosition(i, centre + new Vector3(Mathf.Cos(angle) * radius,
                                                       Mathf.Sin(angle) * radius, 0f));
            }
        }

        // ── Animations ────────────────────────────────────────────────────────────

        private IEnumerator PopupAnimation()
        {
            transform.localScale = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < popupDuration)
            {
                elapsed += Time.deltaTime;
                float s  = Mathf.SmoothStep(0f, 1f, elapsed / popupDuration);
                transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        private IEnumerator HitAnimation()
        {
            if (timingCircle != null)
                timingCircle.enabled = false;

            // Quick squish-and-fade
            float dur     = 0.15f;
            float elapsed = 0f;
            var   sr      = GetComponent<SpriteRenderer>();
            Color baseCol = sr != null ? sr.color : Color.white;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t  = elapsed / dur;
                float s  = Mathf.Lerp(1f, 1.4f, t);
                transform.localScale = new Vector3(s, s, 1f);
                if (sr != null)
                    sr.color = new Color(baseCol.r, baseCol.g, baseCol.b, 1f - t);
                yield return null;
            }

            _onDone?.Invoke(this);
            Destroy(gameObject);
        }

        private IEnumerator MissAnimation()
        {
            if (timingCircle != null)
                timingCircle.enabled = false;

            // Shrink and fade
            float dur     = 0.2f;
            float elapsed = 0f;
            var   sr      = GetComponent<SpriteRenderer>();
            Color baseCol = sr != null ? sr.color : Color.white;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t  = elapsed / dur;
                float s  = Mathf.Lerp(1f, 0f, t);
                transform.localScale = new Vector3(s, s, 1f);
                if (sr != null)
                    sr.color = new Color(baseCol.r, baseCol.g, baseCol.b, 1f - t);
                yield return null;
            }

            _onDone?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
