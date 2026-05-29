using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using SootDDR.Input;
using SootDDR.Scoring;
using SootDDR.UI;
using SootDDR.Core;
using UnityEngine.SceneManagement;

public class BeatmapProcessor : MonoBehaviour
{
    [Header("Beatmap")]
    public TextAsset beatmapJson;
    public float audioDelay = 0f;

    [Header("Music")]
    public AudioSource songSource;

    [Header("References")]
    public EnemyDatabase enemyDatabase;
    public HoleSetup[] holes;

    [Header("Managers")]
    public ScoreManager scoreManager;
    public UIManager uiManager;
    public RhythmInputManager inputManager;

    [Header("Timing Windows (seconds)")]
    public float perfectWindow = 0.12f;
    public float goodWindow = 0.25f;
    public float badWindow = 0.40f;

    [Header("Input Buffer")]
    public float inputBufferWindow = 0.10f;

    [Header("Debug")]
    public TextMeshProUGUI debugTMP;

    [Header("GLOBAL OFFSET (TUNE THIS)")]
    public float globalOffset = 0f;

    [Header("Game Over")]
    public float gameOverTime = 93f;
    public string gameOverScene = "GameOver";

    private bool gameOverTriggered = false;

    private Dictionary<int, HoleSetup> holeLookup;
    private BeatmapData beatmap;
    private int currentNoteIndex = 0;

    private double songStartDSP;
    private bool songStarted;

    private class ActiveNote
    {
        public int holeID;
        public int enemyID;
        public float hitTime;
        public float spawnTime;
        public bool consumed;
    }

    private List<ActiveNote> activeNotes = new List<ActiveNote>();

    void Start()
    {
        LoadBeatmap();

        holeLookup = holes.ToDictionary(h => h.holeID, h => h);

        if (scoreManager != null)
        {
            scoreManager.OnHitRegistered += HandleScoreEvent;
            scoreManager.ResetScore();
        }

        PlaySong();
    }

    void Update()
    {
        if (beatmap == null || !songStarted)
            return;

        float songTime = GetSongTime();

        uiManager?.UpdateSongTime(songTime);

        HandleSpawning(songTime);
        HandleInput(songTime);
        HandleMisses(songTime);
        HandleDebug(songTime);

        if (!gameOverTriggered && songTime >= gameOverTime)
        {
            gameOverTriggered = true;

            if (scoreManager != null)
            {
                scoreManager.SaveFinalScore();
            }

            SceneManager.LoadScene(gameOverScene);
        }
    }

    float GetSongTime()
    {
        return (float)(AudioSettings.dspTime - songStartDSP - globalOffset);
    }

    void PlaySong()
    {
        songStartDSP = AudioSettings.dspTime + audioDelay;
        songStarted = true;

        songSource.PlayScheduled(songStartDSP);
    }

    // SPAWNING

    void HandleSpawning(float songTime)
    {
        float spawnLead = badWindow + inputBufferWindow;

        while (currentNoteIndex < beatmap.notes.Length)
        {
            NoteData note = beatmap.notes[currentNoteIndex];

            float hitTime = BeatToSeconds(note.beat);
            float spawnTime = hitTime - spawnLead;

            if (songTime >= spawnTime)
            {
                Spawn(note, hitTime, spawnTime);
                currentNoteIndex++;
            }
            else break;
        }
    }

    void Spawn(NoteData note, float hitTime, float spawnTime)
    {
        if (!holeLookup.TryGetValue(note.holeID, out HoleSetup hole))
            return;

        GameObject enemyPrefab = enemyDatabase.GetEnemy(note.enemyID);
        if (enemyPrefab == null) return;

        hole.SpawnEnemy(enemyPrefab);

        activeNotes.Add(new ActiveNote
        {
            holeID = note.holeID,
            enemyID = note.enemyID,
            hitTime = hitTime,
            spawnTime = spawnTime,
            consumed = false
        });
    }

    // INPUT

    void HandleInput(float songTime)
    {
        if (inputManager == null || scoreManager == null)
            return;

        if (!inputManager.AnyArrowPressedThisFrame())
            return;

        var dir = inputManager.GetActiveDirection();
        if (!dir.HasValue)
            return;

        int holeID = DirectionToHoleID(dir.Value);

        ActiveNote note = activeNotes
            .Where(n =>
                n.holeID == holeID &&
                !n.consumed &&
                Mathf.Abs(songTime - n.hitTime) <= badWindow)
            .OrderBy(n => Mathf.Abs(songTime - n.hitTime))
            .FirstOrDefault();

        if (note == null)
            return;

        float diff = songTime - note.hitTime;
        float absDiff = Mathf.Abs(diff);

        HitResult result =
            absDiff <= perfectWindow ? HitResult.Perfect :
            absDiff <= goodWindow ? HitResult.Good :
            absDiff <= badWindow ? HitResult.Bad : 
            HitResult.Miss;

        Register(result, note.enemyID);

        note.consumed = true;
        activeNotes.Remove(note);
    }

    // MISS

    void HandleMisses(float songTime)
    {
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var n = activeNotes[i];

            if (n.consumed)
                continue;

            if (songTime < n.spawnTime)
                continue;

            if (songTime > n.hitTime + badWindow)
            {
                Register(HitResult.Miss, n.enemyID);
                n.consumed = true;
                activeNotes.RemoveAt(i);
            }
        }
    }

    void Register(HitResult result, int enemyID)
    {
        scoreManager.RegisterHit(result, enemyID);
    }

    void HandleScoreEvent(HitResult result, int points, int combo)
    {
        uiManager?.UpdateScore(scoreManager.TotalScore);
        uiManager?.UpdateCombo(combo);
        uiManager?.ShowHitFeedback(result, scoreManager.Combo);
    }

    // DEBUGGER
    void HandleDebug(float songTime)
    {
        if (debugTMP == null) return;

        var dir = inputManager != null ? inputManager.GetActiveDirection() : null;
        bool pressed = inputManager != null && inputManager.AnyArrowPressedThisFrame();

        int holeID = dir.HasValue ? DirectionToHoleID(dir.Value) : -1;

        ActiveNote closest = activeNotes
            .Where(n => !n.consumed)
            .OrderBy(n => Mathf.Abs(songTime - n.hitTime))
            .FirstOrDefault();

        float diff = closest != null ? songTime - closest.hitTime : 0f;

        string predicted = "NONE";

        if (closest != null)
        {
            float abs = Mathf.Abs(diff);

            predicted =
                abs <= perfectWindow ? "PERFECT" :
                abs <= goodWindow ? "GOOD" :
                abs <= badWindow ? "BAD" :
                "MISS";
        }

        float currentBeat = (beatmap != null && currentNoteIndex < beatmap.notes.Length)
            ? beatmap.notes[currentNoteIndex].beat
            : -1f;

        debugTMP.text =
            $"TIME: {songTime:F3}\n" +
            $"BEAT INDEX: {currentNoteIndex}/{(beatmap != null ? beatmap.notes.Length : 0)}\n" +
            $"NEXT BEAT: {(currentBeat >= 0 ? currentBeat.ToString("F2") : "END")}\n" +
            $"PRESSED: {pressed}\n" +
            $"DIR: {(dir.HasValue ? dir.ToString() : "None")}\n" +
            $"HOLE ID: {holeID}\n" +
            $"ACTIVE: {activeNotes.Count}\n" +
            $"ms: {diff * 1000f:F1}\n" +
            $"PREDICTED: {predicted}\n" +
            $"OFFSET: {globalOffset:F3}\n" +
            $"SCORE: {scoreManager?.TotalScore}";
    }

    int DirectionToHoleID(NoteDirection dir)
    {
        return dir switch
        {
            NoteDirection.UpLeft => 0,
            NoteDirection.Up => 1,
            NoteDirection.UpRight => 2,
            NoteDirection.Left => 3,
            NoteDirection.Right => 4,
            NoteDirection.DownLeft => 5,
            NoteDirection.Down => 6,
            NoteDirection.DownRight => 7,
            _ => -1
        };
    }

    float BeatToSeconds(float beat)
    {
        return beat * (60f / beatmap.bpm);
    }

    void LoadBeatmap()
    {
        beatmap = JsonUtility.FromJson<BeatmapData>(beatmapJson.text);
    }

    void OnDestroy()
    {
        if (scoreManager != null)
            scoreManager.OnHitRegistered -= HandleScoreEvent;
    }
}