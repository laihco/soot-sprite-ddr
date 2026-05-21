using System;
using UnityEngine;
using SootDDR.Core;
using SootDDR.Scoring;

namespace SootDDR.Gameplay
{
    /// <summary>
    /// Controls a single soot sprite (note) in the game world.
    ///
    /// Responsibilities:
    ///   • Interpolates position from spawn point to the hit-ring edge on song time.
    ///   • Exposes TryHit() for RhythmGameManager to call when matching input arrives.
    ///   • Auto-registers a Miss once the good timing window expires.
    ///   • After a miss, drifts the sprite toward center and fades it before destroy.
    ///
    /// All public fields except HasBeenJudged are set by RhythmGameManager at spawn time.
    /// </summary>
    public class SootSpriteController : MonoBehaviour
    {
        // ── Configured by RhythmGameManager at instantiation ─────────────────────

        [HideInInspector] public NoteDirection requiredDirection;

        /// <summary>Absolute DSP time when this sprite should arrive at the ring edge.</summary>
        [HideInInspector] public double  hitDspTime;

        /// <summary>Song-relative time this sprite was spawned (hitTime − travelDuration).</summary>
        [HideInInspector] public float   spawnTime;

        [HideInInspector] public float   travelDuration;
        [HideInInspector] public float   perfectWindow;
        [HideInInspector] public float   goodWindow;

        /// <summary>World-space position where this sprite spawns.</summary>
        [HideInInspector] public Vector3 spawnPosition;

        /// <summary>World-space target position (ring-edge point, NOT world center).</summary>
        [HideInInspector] public Vector3 targetPosition;

        public bool HasBeenJudged { get; private set; }

        // ── Private ──────────────────────────────────────────────────────────────

        private Func<float>  _getSongTime;   // lambda supplied by RhythmGameManager
        private ScoreManager _scoreManager;
        private SpriteRenderer _sr;

        private bool  _driftingToCenter;
        private float _driftElapsed;

        // ── Initialization ────────────────────────────────────────────────────────

        /// <summary>Called by RhythmGameManager immediately after instantiation.</summary>
        public void Initialize(ScoreManager scoreManager, Func<float> getSongTime)
        {
            _scoreManager = scoreManager;
            _getSongTime  = getSongTime;
            _sr           = GetComponentInChildren<SpriteRenderer>();
        }

        // ── Unity loop ────────────────────────────────────────────────────────────

        void Update()
        {
            if (_getSongTime == null) return;
            float songTime = _getSongTime();

            if (!_driftingToCenter)
            {
                // Lerp from spawn to ring edge based on song time progress
                float t = Mathf.Clamp01((songTime - spawnTime) / travelDuration);
                transform.position = Vector3.Lerp(spawnPosition, targetPosition, t);

                // Auto-miss when the full good window has elapsed past the hit time
                if (!HasBeenJudged && songTime > spawnTime + travelDuration + goodWindow)
                    RegisterAutoMiss();
            }
            else
            {
                // Post-miss drift: slide toward world center and fade out
                _driftElapsed += Time.deltaTime;
                float driftT = Mathf.Clamp01(_driftElapsed / 0.4f);
                transform.position = Vector3.Lerp(targetPosition, Vector3.zero, driftT);

                if (_sr != null)
                {
                    Color c = _sr.color;
                    _sr.color = new Color(c.r, c.g, c.b, 1f - driftT);
                }

                if (driftT >= 1f)
                    Destroy(gameObject);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Attempt to judge this note given the DSP timestamp of the player's input.
        /// Returns the HitResult; also calls ScoreManager.RegisterHit internally.
        /// The sprite is destroyed immediately on a hit.
        /// </summary>
        public HitResult TryHit(double inputDspTime)
        {
            if (HasBeenJudged) return HitResult.Miss;

            double accuracy = Math.Abs(inputDspTime - hitDspTime);
            HitResult result = accuracy <= perfectWindow ? HitResult.Perfect : HitResult.Good;

            HasBeenJudged = true;
            _scoreManager?.RegisterHit(result);
            Destroy(gameObject);
            return result;
        }

        // ── Private ──────────────────────────────────────────────────────────────

        private void RegisterAutoMiss()
        {
            HasBeenJudged    = true;
            _driftingToCenter = true;
            _scoreManager?.RegisterHit(HitResult.Miss);
        }
    }
}
