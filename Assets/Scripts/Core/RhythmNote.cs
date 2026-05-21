using System;

namespace SootDDR.Core
{
    /// <summary>
    /// A single note entry in a rhythm chart.
    /// JSON-serializable fields are populated by JsonUtility.
    /// Runtime fields are computed by RhythmGameManager before play begins.
    /// </summary>
    [Serializable]
    public class RhythmNote
    {
        // ── Serialized from JSON ──────────────────────────────────────────────────

        /// <summary>Song-relative time (seconds) when the player should hit this note.</summary>
        public float time;

        /// <summary>
        /// The grid hole this note belongs to.
        /// Must match a NoteDirection enum name exactly (e.g. "Left", "UpRight").
        /// </summary>
        public string hole;

        // ── Computed at runtime — not serialized ─────────────────────────────────

        /// <summary>Parsed enum value of <see cref="hole"/>.</summary>
        [NonSerialized] public NoteDirection noteDirection;

        /// <summary>Absolute DSP timestamp for this note's hit moment, set when the song is scheduled.</summary>
        [NonSerialized] public double hitDspTime;

        /// <summary>Song-relative time the soot sprite should pop up: time − approachDuration.</summary>
        [NonSerialized] public float spawnTime;

        [NonSerialized] public bool hasBeenSpawned;
        [NonSerialized] public bool hasBeenJudged;
    }
}
