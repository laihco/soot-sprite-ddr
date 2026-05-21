using System;
using UnityEngine;
using SootDDR.Core;
using SootDDR.Scoring;

namespace SootDDR.Gameplay
{
    /// <summary>
    /// Attached to each of the 8 hole GameObjects in the grid.
    /// Owns the currently-active SootSpriteNote and forwards TryHit calls to it.
    /// </summary>
    public class HoleController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────────
        [Tooltip("Prefab with SootSpriteNote component at root.")]
        public SootSpriteNote sootSpriteNotePrefab;

        // ── Runtime ───────────────────────────────────────────────────────────────
        private ScoreManager  _scoreManager;
        private float         _approachDuration;
        private float         _perfectWindow;
        private float         _goodWindow;
        private Func<float>   _getSongTime;

        private SootSpriteNote _activeNote;

        /// <summary>True when a note is visible and waiting to be judged.</summary>
        public bool HasActiveNote => _activeNote != null;

        // ── Setup ─────────────────────────────────────────────────────────────────

        /// <summary>Called once at scene setup by RhythmGameManager.</summary>
        public void Setup(ScoreManager sm, float approachDuration, float perfectWindow,
                          float goodWindow, Func<float> getSongTime)
        {
            _scoreManager    = sm;
            _approachDuration = approachDuration;
            _perfectWindow   = perfectWindow;
            _goodWindow      = goodWindow;
            _getSongTime     = getSongTime;
        }

        // ── Note lifecycle ────────────────────────────────────────────────────────

        /// <summary>
        /// Spawns a SootSpriteNote at this hole's position.
        /// Called by RhythmGameManager when spawnTime arrives.
        /// </summary>
        public void ActivateNote(double hitDspTime)
        {
            if (_activeNote != null)
            {
                // Safety: destroy any orphaned note before spawning a new one.
                Destroy(_activeNote.gameObject);
                _activeNote = null;
            }

            _activeNote = Instantiate(sootSpriteNotePrefab, transform.position, Quaternion.identity);
            _activeNote.Initialize(hitDspTime, _approachDuration, _perfectWindow, _goodWindow,
                                   _getSongTime, _scoreManager, OnNoteDone);
        }

        /// <summary>
        /// Called by RhythmGameManager when the matching key is pressed.
        /// Returns the hit result, or null if no active note.
        /// </summary>
        public HitResult? TryHit(double inputDsp)
        {
            if (_activeNote == null) return null;
            return _activeNote.TryHit(inputDsp);
        }

        // ── Callbacks ─────────────────────────────────────────────────────────────

        private void OnNoteDone(SootSpriteNote note)
        {
            if (_activeNote == note)
                _activeNote = null;
        }
    }
}
