// Editor-only script.  No runtime code here.
#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using SootDDR.Core;
using SootDDR.Gameplay;
using SootDDR.Input;
using SootDDR.Scoring;
using SootDDR.UI;

namespace SootDDR.Editor
{
    public static class SootWhackSceneBuilder
    {
        private const string ScenePath  = "Assets/Scenes/SootWhackPrototype.unity";
        private const string PrefabPath = "Assets/Prefabs/SootSpriteNote.prefab";

        [MenuItem("Tools/SootDDR/Build Whack Scene")]
        public static void Build()
        {
            EnsureDir("Assets/Scenes");
            EnsureDir("Assets/Prefabs");

            // ─────────────────────────────────────────────
            // BUILT-IN SPRITE (NO PROCEDURAL GENERATION)
            // ─────────────────────────────────────────────
            Sprite uiSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite");

            // ─────────────────────────────────────────────
            // NEW SCENE
            // ─────────────────────────────────────────────
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ─────────────────────────────────────────────
            // CAMERA
            // ─────────────────────────────────────────────
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";

            var cam = cameraGo.AddComponent<Camera>();
            cameraGo.transform.position = new Vector3(0, 0, -10);

            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);

            cameraGo.AddComponent<AudioListener>();

            // ─────────────────────────────────────────────
            // GRID (HOLES)
            // ─────────────────────────────────────────────
            float spacing = 1.8f;

            var gridGo = new GameObject("Grid");
            var holeControllers = new HoleController[8];

            NoteDirection[] dirs = (NoteDirection[])Enum.GetValues(typeof(NoteDirection));

            for (int i = 0; i < dirs.Length; i++)
            {
                var dir = dirs[i];
                Vector2 pos = NoteDirectionHelper.ToGridPosition(dir, spacing);

                var holeGo = new GameObject($"Hole_{dir}");
                holeGo.transform.SetParent(gridGo.transform);
                holeGo.transform.position = new Vector3(pos.x, pos.y, 0);

                var sr = holeGo.AddComponent<SpriteRenderer>();
                sr.sprite = uiSprite;
                sr.color = new Color(0.2f, 0.2f, 0.25f, 1f);
                sr.sortingOrder = 0;

                var hc = holeGo.AddComponent<HoleController>();
                holeControllers[i] = hc;
            }

            // ─────────────────────────────────────────────
            // MANAGERS
            // ─────────────────────────────────────────────
            var managersGo = new GameObject("Managers");

            var audioSource = new GameObject("AudioSource")
                .AddComponent<AudioSource>();
            audioSource.transform.SetParent(managersGo.transform);
            audioSource.playOnAwake = false;

            var inputManager = new GameObject("InputManager")
                .AddComponent<RhythmInputManager>();
            inputManager.transform.SetParent(managersGo.transform);

            var scoreManager = new GameObject("ScoreManager")
                .AddComponent<ScoreManager>();
            scoreManager.transform.SetParent(managersGo.transform);

            var uiManager = new GameObject("UIManager")
                .AddComponent<UIManager>();
            uiManager.transform.SetParent(managersGo.transform);

            var rgm = new GameObject("RhythmGameManager")
                .AddComponent<RhythmGameManager>();
            rgm.transform.SetParent(managersGo.transform);

            rgm.audioSource = audioSource;
            rgm.inputManager = inputManager;
            rgm.scoreManager = scoreManager;
            rgm.uiManager = uiManager;
            rgm.holeControllers = holeControllers;

            rgm.approachDuration = 1.0f;
            rgm.perfectWindow = 0.08f;
            rgm.goodWindow = 0.15f;
            rgm.gridSpacing = spacing;

            // ─────────────────────────────────────────────
            // CANVAS (TEXT + TMP UI)
            // ─────────────────────────────────────────────
            BuildCanvas(uiManager);

            // ─────────────────────────────────────────────
            // SAVE
            // ─────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[SootWhack] Scene built: {ScenePath}");
        }

        // ─────────────────────────────────────────────
        // CANVAS (TMP VERSION)
        // ─────────────────────────────────────────────
        private static void BuildCanvas(UIManager uiManager)
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            var score = CreateTMP(canvasGo, "ScoreText", "Score: 0",
                new Vector2(10, -10), TextAlignmentOptions.TopLeft);

            var combo = CreateTMP(canvasGo, "ComboText", "",
                new Vector2(10, -45), TextAlignmentOptions.TopLeft);

            var time = CreateTMP(canvasGo, "TimeText", "Time: 0.00",
                new Vector2(-10, -10), TextAlignmentOptions.TopRight, true);

            var feedback = CreateTMP(canvasGo, "HitFeedback", "",
                new Vector2(0, 60), TextAlignmentOptions.Center);
            feedback.gameObject.SetActive(false);

            uiManager.scoreText = score;
            uiManager.comboText = combo;
            uiManager.songTimeText = time;
            uiManager.hitFeedbackText = feedback;
        }

        private static TMP_Text CreateTMP(
            GameObject parent,
            string name,
            string text,
            Vector2 pos,
            TextAlignmentOptions alignment,
            bool rightAnchor = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);

            var rt = go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();

            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = alignment;

            if (rightAnchor)
            {
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
            }
            else
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
            }

            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(300, 50);

            return tmp;
        }

        // ─────────────────────────────────────────────
        // UTIL
        // ─────────────────────────────────────────────
        private static void EnsureDir(string path)
        {
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
        }
    }
}
#endif