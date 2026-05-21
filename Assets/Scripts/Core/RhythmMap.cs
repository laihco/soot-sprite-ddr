using System;
using UnityEngine;

namespace SootDDR.Core
{
    /// <summary>
    /// Full rhythm chart for a song. Loaded from a JSON TextAsset in Resources/.
    /// All notes carry a hitTime (when they should reach the ring); spawnTime is
    /// calculated at runtime from  spawnTime = hitTime − travelDuration.
    /// </summary>
    [Serializable]
    public class RhythmMap
    {
        public string      songName;
        public float       bpm;
        public RhythmNote[] notes;

        /// <summary>
        /// Loads and deserializes a rhythm map from a TextAsset in Resources/.
        /// <paramref name="resourcePath"/> is relative to Resources/ without extension,
        /// e.g. "RhythmMaps/sample_easy".
        /// </summary>
        public static RhythmMap Load(string resourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogError($"[RhythmMap] No TextAsset found at Resources/{resourcePath}");
                return null;
            }

            var map = JsonUtility.FromJson<RhythmMap>(asset.text);
            if (map?.notes == null)
            {
                Debug.LogError($"[RhythmMap] Failed to parse JSON at Resources/{resourcePath}");
                return null;
            }

            return map;
        }
    }
}
