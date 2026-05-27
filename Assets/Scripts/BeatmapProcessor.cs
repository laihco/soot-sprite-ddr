using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BeatmapProcessor : MonoBehaviour
{
    [Header("Beatmap")]
    public TextAsset beatmapJson;
    
    [Tooltip("Use this to delay the song slightly if the audio starts before the first beat")]
    public float audioDelay = 0f;

    [Header("Music")]
    public AudioSource songSource;

    [Header("References")]
    public EnemyDatabase enemyDatabase;
    public HoleSetup[] holes;

    private Dictionary<int, HoleSetup> holeLookup;
    private BeatmapData beatmap;
    private int currentNoteIndex = 0;

    private void Start()
    {
        LoadBeatmap();

        // Build the lookup dictionary so we can quickly find holes by ID
        holeLookup = holes.ToDictionary(
            hole => hole.holeID,
            hole => hole
        );

        if (audioDelay > 0)
        {
            songSource.PlayDelayed(audioDelay);
        }
        else
        {
            songSource.Play();
        }
    }

    private void Update()
    {
        if (beatmap == null || !songSource.isPlaying)
            return;

        float songTime = songSource.time;

        while (currentNoteIndex < beatmap.notes.Length)
        {
            NoteData note = beatmap.notes[currentNoteIndex];
            float noteTime = BeatToSeconds(note.beat);

            // If the song has reached the time for this note, spawn it
            if (songTime >= noteTime)
            {
                SpawnNote(note);
                currentNoteIndex++;
            }
            else
            {
                // If it's not time for the next note yet, break the loop and wait for the next frame
                break;
            }
        }
    }

    void LoadBeatmap()
    {
        beatmap = JsonUtility.FromJson<BeatmapData>(beatmapJson.text);
        Debug.Log($"Loaded {beatmap.notes.Length} notes from {beatmap.songName}");
    }

    float BeatToSeconds(float beat)
    {
        return beat * (60f / beatmap.bpm);
    }

    void SpawnNote(NoteData note)
    {
        // 1. Find the correct hole
        if (!holeLookup.TryGetValue(note.holeID, out HoleSetup hole))
        {
            Debug.LogWarning($"Hole ID {note.holeID} not found! Check your HoleSetup scripts.");
            return;
        }

        // 2. Find the correct enemy type for this specific note
        GameObject enemyPrefab = enemyDatabase.GetEnemy(note.enemyID);
        if (enemyPrefab == null)
        {
            return;
        }

        // 3. Tell the hole to spawn that exact enemy
        hole.SpawnEnemy(enemyPrefab);

        Debug.Log($"Beat {note.beat}: Spawned Enemy {note.enemyID} in Hole {note.holeID}");
    }
}