using System;

namespace SootDDR.Core
{
    /// <summary>
    /// A single note entry in a rhythm chart.
    /// Notes are stored as BEATS and converted to seconds at runtime using BPM.
    /// </summary>
    [Serializable]
    public class RhythmNote
    {
        // ── Serialized from JSON ──────────────────────────────────────────────────

        /// <summary>
        /// Beat position in the song.
        /// Example:
        /// 0   = first beat
        /// 1   = second beat
        /// 0.5 = eighth note between beat 0 and 1
        /// </summary>
        public float beat;

        /// <summary>
        /// The grid hole this note belongs to.
        /// Must match a NoteDirection enum name exactly.
        /// </summary>
        public string hole;

        // ── Computed at runtime — not serialized ─────────────────────────────────

        /// <summary>Converted beat → seconds using BPM.</summary>
        [NonSerialized] public float time;

        /// <summary>Parsed enum value of hole.</summary>
        [NonSerialized] public NoteDirection noteDirection;

        /// <summary>Absolute DSP timestamp for this note's hit moment.</summary>
        [NonSerialized] public double hitDspTime;

        /// <summary>Song-relative time the note should spawn.</summary>
        [NonSerialized] public float spawnTime;

        [NonSerialized] public bool hasBeenSpawned;
        [NonSerialized] public bool hasBeenJudged;
    }
}