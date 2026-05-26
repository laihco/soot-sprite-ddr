using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SootDDR.Gameplay;
using SootDDR.Input;
using SootDDR.Scoring;
using SootDDR.UI;

namespace SootDDR.Core
{
    public enum Difficulty { Easy, Medium, Hard }
    public enum GameState  { Idle, Playing, Finished }

    /// <summary>
    /// Master orchestrator for the rhythm whack-a-mole game.
    /// </summary>
    public class RhythmGameManager : MonoBehaviour
    {
        [Header("References")]
        public AudioSource        audioSource;
        public RhythmInputManager inputManager;
        public ScoreManager       scoreManager;
        public UIManager          uiManager;

        [Header("Debug")]
        public GameObject beatCounterGO;

        private TextMeshProUGUI _beatCounterText;
        private float _secondsPerBeat;

        [Tooltip("All 8 HoleController instances (one per NoteDirection).")]
        public HoleController[] holeControllers;

        [Header("Difficulty")]
        public Difficulty selectedDifficulty = Difficulty.Easy;

        [Header("Timing (seconds)")]
        public float approachDuration = 1.0f;
        public float perfectWindow    = 0.08f;
        public float goodWindow       = 0.15f;

        [Header("Grid")]
        public float gridSpacing      = 1.8f;

        // ── Runtime state ─────────────────────────────────────────────────────────

        private double _songStartDsp;
        private GameState _state = GameState.Idle;
        private RhythmMap _map;
        private int _nextNoteIndex;
        private int _resolvedCount;

        private readonly Dictionary<NoteDirection, HoleController> _holes = new();

        public float SongTime =>
            _songStartDsp > 0 ? (float)(AudioSettings.dspTime - _songStartDsp) : 0f;

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        void Start()
        {
            BuildHoleDictionary();
            SetupHoles();
            LoadMap();

            if (beatCounterGO != null)
                _beatCounterText = beatCounterGO.GetComponent<TextMeshProUGUI>();

            if (scoreManager != null)
                scoreManager.OnHitRegistered += HandleHitRegistered;

            StartGame();
        }

        void OnDestroy()
        {
            if (scoreManager != null)
                scoreManager.OnHitRegistered -= HandleHitRegistered;
        }

        void Update()
        {
            if (_state != GameState.Playing) return;

            float songTime = SongTime;

            uiManager?.UpdateSongTime(songTime);

            UpdateBeatCounter(songTime);

            SpawnDueNotes(songTime);
            CheckPlayerInput();
        }

        // ── Beat Counter ──────────────────────────────────────────────────────────

        void UpdateBeatCounter(float songTime)
        {
            if (_beatCounterText == null || _secondsPerBeat <= 0f)
                return;

            float currentBeat = songTime / _secondsPerBeat;

            _beatCounterText.text =
                $"Beat: {currentBeat:F2}\n" +
                $"Measure: {Mathf.FloorToInt(currentBeat / 4f) + 1}";
        }

        // ── Setup ─────────────────────────────────────────────────────────────────

        void BuildHoleDictionary()
        {
            _holes.Clear();
            if (holeControllers == null) return;

            NoteDirection[] allDirs = (NoteDirection[])Enum.GetValues(typeof(NoteDirection));

            foreach (var hc in holeControllers)
            {
                if (hc == null) continue;

                foreach (var dir in allDirs)
                {
                    if (hc.CompareTag(dir.ToString()))
                    {
                        _holes[dir] = hc;
                        break;
                    }
                }
            }

            Debug.Log($"[SootWhack] Registered {_holes.Count} holes.");
        }

        void SetupHoles()
        {
            foreach (var kv in _holes)
                kv.Value.Setup(scoreManager, approachDuration, perfectWindow, goodWindow, () => SongTime);
        }

        void LoadMap()
        {
            string path = selectedDifficulty switch
            {
                Difficulty.Easy   => "RhythmMaps/sample_easy",
                Difficulty.Medium => "RhythmMaps/sample_medium",
                Difficulty.Hard   => "RhythmMaps/sample_hard",
                _                 => "RhythmMaps/sample_easy",
            };

            _map = RhythmMap.Load(path);
            if (_map == null) return;

            _secondsPerBeat = 60f / _map.bpm;

            foreach (var note in _map.notes)
            {
                NoteDirectionHelper.TryParse(note.hole, out note.noteDirection);

                note.time = note.beat * _secondsPerBeat;
                note.spawnTime = note.time - approachDuration;

                note.hasBeenSpawned = false;
                note.hasBeenJudged  = false;
            }

            Array.Sort(_map.notes, (a, b) => a.time.CompareTo(b.time));
        }

        void StartGame()
        {
            if (_map == null)
            {
                Debug.LogError("[SootWhack] No rhythm map loaded — cannot start game.");
                return;
            }

            scoreManager?.ResetScore();
            uiManager?.UpdateScore(0);
            uiManager?.UpdateCombo(0);

            _nextNoteIndex = 0;
            _resolvedCount = 0;

            double startDsp = AudioSettings.dspTime + 0.1;
            if (audioSource != null && audioSource.clip != null)
                audioSource.PlayScheduled(startDsp);

            _songStartDsp = startDsp;
            _state = GameState.Playing;

            Debug.Log($"[SootWhack] Game started — map: {_map.songName}, notes: {_map.notes.Length}");
        }

        // ── Gameplay ─────────────────────────────────────────────────────────────

        void SpawnDueNotes(float songTime)
        {
            while (_nextNoteIndex < _map.notes.Length)
            {
                RhythmNote note = _map.notes[_nextNoteIndex];
                if (note.spawnTime > songTime) break;

                ActivateNote(note);
                _nextNoteIndex++;
            }
        }

        void ActivateNote(RhythmNote note)
        {
            if (!_holes.TryGetValue(note.noteDirection, out var hole))
            {
                Debug.LogWarning($"[SootWhack] No hole registered for direction {note.noteDirection}. Skipping note.");
                _resolvedCount++;
                CheckEndGame();
                return;
            }

            double hitDspTime = _songStartDsp + note.time;
            hole.ActivateNote(hitDspTime);
            note.hasBeenSpawned = true;
        }

        void CheckPlayerInput()
        {
            if (!inputManager.AnyArrowPressedThisFrame()) return;

            NoteDirection? dir = inputManager.GetActiveDirection();
            if (dir == null) return;

            if (_holes.TryGetValue(dir.Value, out var hole))
                hole.TryHit(AudioSettings.dspTime);
        }

        void HandleHitRegistered(HitResult result, int points, int combo)
        {
            uiManager?.ShowHitFeedback(result);
            uiManager?.UpdateScore(scoreManager.TotalScore);
            uiManager?.UpdateCombo(combo);

            _resolvedCount++;
            CheckEndGame();
        }

        void CheckEndGame()
        {
            if (_resolvedCount >= _map.notes.Length && _nextNoteIndex >= _map.notes.Length)
                EndGame();
        }

        void EndGame()
        {
            if (_state == GameState.Finished) return;
            _state = GameState.Finished;

            Debug.Log($"[SootWhack] Game Over — Score: {scoreManager?.TotalScore} | MaxCombo: {scoreManager?.MaxCombo}");
        }
    }
}