// Editor-only script.  No runtime code here.
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SootDDR.Core;
using SootDDR.Gameplay;
using SootDDR.Input;
using SootDDR.Scoring;
using SootDDR.UI;

namespace SootDDR.Editor
{
    /// <summary>
    /// Menu item: Tools → SootDDR → Build Whack Scene
    ///
    /// Creates Assets/Scenes/SootWhackPrototype.unity with:
    ///   • Main Camera, Global Light 2D
    ///   • 3×3 grid (8 holes) each with HoleController + hole-bg sprite
    ///   • SootSpriteNote prefab (body sprite + TimingCircle LineRenderer)
    ///   • Managers hierarchy (RhythmGameManager, InputManager, ScoreManager, UIManager)
    ///   • Canvas with score / combo / time / hit-feedback labels
    /// All inter-component references are wired automatically.
    /// </summary>
    public static class SootWhackSceneBuilder
    {
        private const string ScenePath    = "Assets/Scenes/SootWhackPrototype.unity";
        private const string PrefabPath   = "Assets/Prefabs/SootSpriteNote.prefab";
        private const string SpritesDir   = "Assets/Sprites";
        private const int    TexSize      = 128;

        // ── Entry point ───────────────────────────────────────────────────────────

        [MenuItem("Tools/SootDDR/Build Whack Scene")]
        public static void Build()
        {
            // ── 1. Ensure directories ────────────────────────────────────────────
            EnsureDir("Assets/Scenes");
            EnsureDir("Assets/Prefabs");
            EnsureDir(SpritesDir);

            // ── 2. Generate sprite assets ────────────────────────────────────────
            Sprite holeBgSprite = GetOrCreateCircleSprite("SootDDR_HoleBg",
                new Color(0.18f, 0.18f, 0.22f, 1f), TexSize);
            Sprite bodySprite   = GetOrCreateCircleSprite("SootDDR_Body",
                new Color(0.08f, 0.08f, 0.10f, 1f), TexSize);
            Sprite eyeSprite    = GetOrCreateCircleSprite("SootDDR_Eye",
                new Color(0.95f, 0.95f, 1.00f, 1f), 32);

            // ── 3. Create / refresh SootSpriteNote prefab ────────────────────────
            SootSpriteNote notePrefab = BuildNotePrefab(bodySprite, eyeSprite);

            // ── 4. Open a new scene ──────────────────────────────────────────────
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── 5. Camera ────────────────────────────────────────────────────────
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic     = true;
            cam.orthographicSize = 5f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.05f, 0.05f, 0.08f, 1f);
            cam.nearClipPlane    = 0.3f;
            cam.farClipPlane     = 1000f;
            cameraGo.AddComponent<AudioListener>();

            // ── 6. Global Light 2D (URP) ─────────────────────────────────────────
            //    Conditionally compiled so the build still succeeds when the 2D Renderer package is absent.
#if USING_2D_RENDERER || UNITY_2022_2_OR_NEWER
            TryAddGlobalLight2D();
#endif

            // ── 7. Grid (8 holes) ────────────────────────────────────────────────
            float     spacing        = 1.8f;
            var       gridGo         = new GameObject("Grid");
            var       holeControllers = new HoleController[8];

            NoteDirection[] dirs = (NoteDirection[])Enum.GetValues(typeof(NoteDirection));
            for (int i = 0; i < dirs.Length; i++)
            {
                NoteDirection dir = dirs[i];
                Vector2       pos = NoteDirectionHelper.ToGridPosition(dir, spacing);

                var holeGo = new GameObject($"Hole_{dir}");
                holeGo.transform.SetParent(gridGo.transform, false);
                holeGo.transform.position = new Vector3(pos.x, pos.y, 0f);

                // Dark circle background
                var holeSr = holeGo.AddComponent<SpriteRenderer>();
                holeSr.sprite       = holeBgSprite;
                holeSr.sortingOrder  = 0;
                holeSr.color         = new Color(1f, 1f, 1f, 0.85f);

                // HoleController
                var hc = holeGo.AddComponent<HoleController>();
                hc.sootSpriteNotePrefab = notePrefab;
                holeControllers[i] = hc;
            }

            // ── 8. Managers ──────────────────────────────────────────────────────
            var managersGo = new GameObject("Managers");

            // AudioSource (no clip — swap via Inspector)
            var audioSourceGo = new GameObject("AudioSource");
            audioSourceGo.transform.SetParent(managersGo.transform, false);
            var audioSource = audioSourceGo.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            var inputManagerGo = new GameObject("InputManager");
            inputManagerGo.transform.SetParent(managersGo.transform, false);
            var inputManager = inputManagerGo.AddComponent<RhythmInputManager>();

            var scoreManagerGo = new GameObject("ScoreManager");
            scoreManagerGo.transform.SetParent(managersGo.transform, false);
            var scoreManager = scoreManagerGo.AddComponent<ScoreManager>();

            var uiManagerGo = new GameObject("UIManager");
            uiManagerGo.transform.SetParent(managersGo.transform, false);
            var uiManager = uiManagerGo.AddComponent<UIManager>();

            var rgmGo = new GameObject("RhythmGameManager");
            rgmGo.transform.SetParent(managersGo.transform, false);
            var rgm = rgmGo.AddComponent<RhythmGameManager>();

            // Wire RhythmGameManager references
            rgm.audioSource    = audioSource;
            rgm.inputManager   = inputManager;
            rgm.scoreManager   = scoreManager;
            rgm.uiManager      = uiManager;
            rgm.holeControllers = holeControllers;
            rgm.approachDuration = 1.0f;
            rgm.perfectWindow    = 0.08f;
            rgm.goodWindow       = 0.15f;
            rgm.gridSpacing      = spacing;

            // ── 9. Canvas ────────────────────────────────────────────────────────
            BuildCanvas(uiManager);

            // ── 10. Save scene ───────────────────────────────────────────────────
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"[SootWhack] Scene built and saved to {ScenePath}");
            EditorUtility.DisplayDialog("SootWhack",
                $"Scene built!\nOpen: {ScenePath}", "OK");
        }

        // ── Sprite helpers ────────────────────────────────────────────────────────

        /// <summary>Returns (or creates) a solid-colour circle sprite PNG.</summary>
        private static Sprite GetOrCreateCircleSprite(string name, Color colour, int size)
        {
            string assetPath = $"{SpritesDir}/{name}.png";
            string absPath   = Path.Combine(Application.dataPath,
                                            assetPath.Replace("Assets/", ""));

            if (!File.Exists(absPath))
            {
                var tex     = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels  = new Color[size * size];
                float cx    = (size - 1) * 0.5f;
                float cy    = (size - 1) * 0.5f;
                float rOuter = cx * 0.95f;
                float rInner = rOuter * 0.70f;   // soft anti-alias band

                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dist  = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    float alpha = Mathf.Clamp01((rOuter - dist) / (rOuter - rInner));
                    pixels[y * size + x] = new Color(colour.r, colour.g, colour.b,
                                                     colour.a * alpha);
                }
                tex.SetPixels(pixels);
                tex.Apply();

                Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);
                File.WriteAllBytes(absPath, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(assetPath);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType     = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        // ── Prefab builder ────────────────────────────────────────────────────────

        private static SootSpriteNote BuildNotePrefab(Sprite bodySprite, Sprite eyeSprite)
        {
            // Build in-scene, then save as prefab
            var root = new GameObject("SootSpriteNote");

            // Body sprite renderer
            var bodySr = root.AddComponent<SpriteRenderer>();
            bodySr.sprite       = bodySprite;
            bodySr.sortingOrder = 2;

            // Eye child
            var eyeGo = new GameObject("Eye");
            eyeGo.transform.SetParent(root.transform, false);
            eyeGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            eyeGo.transform.localScale    = new Vector3(0.3f, 0.3f, 1f);
            var eyeSr = eyeGo.AddComponent<SpriteRenderer>();
            eyeSr.sprite       = eyeSprite;
            eyeSr.sortingOrder = 3;

            // Timing circle child
            var circleGo = new GameObject("TimingCircle");
            circleGo.transform.SetParent(root.transform, false);
            var lr = circleGo.AddComponent<LineRenderer>();
            ConfigureTimingCircleLR(lr);

            // SootSpriteNote component
            var note = root.AddComponent<SootSpriteNote>();
            note.timingCircle      = lr;
            note.circleStartRadius = 0.9f;
            note.circleEndRadius   = 0.35f;
            note.popupDuration     = 0.12f;

            // Save as prefab
            EnsureDir("Assets/Prefabs");
            var prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            return prefabAsset.GetComponent<SootSpriteNote>();
        }

        private static void ConfigureTimingCircleLR(LineRenderer lr)
        {
            lr.positionCount = 65;   // 64 segments + close loop
            lr.loop          = false;
            lr.useWorldSpace = true;
            lr.startWidth    = 0.04f;
            lr.endWidth      = 0.04f;
            lr.startColor    = Color.white;
            lr.endColor      = Color.white;
            lr.sortingOrder  = 10;

            Shader shader = Shader.Find("Sprites/Default")
                         ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                var mat   = new Material(shader);
                mat.color = Color.white;
                var matPath = "Assets/Sprites/TimingCircleMat.mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(matPath) == null)
                    AssetDatabase.CreateAsset(mat, matPath);
                else
                    mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                lr.material = mat;
            }
        }

        // ── Canvas builder ────────────────────────────────────────────────────────

        private static void BuildCanvas(UIManager uiManager)
        {
            var canvasGo = new GameObject("Canvas");
            var canvas   = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Score (top-left)
            var scoreText    = CreateLabel(canvasGo, "ScoreText",    "Score: 0",
                                           new Vector2(10, -10), new Vector2(250, 30),
                                           TextAnchor.UpperLeft);
            // Combo (top-left, below score)
            var comboText    = CreateLabel(canvasGo, "ComboText",    "",
                                           new Vector2(10, -45), new Vector2(250, 30),
                                           TextAnchor.UpperLeft);
            // Song time (top-right)
            var songTimeText = CreateLabel(canvasGo, "SongTimeText", "Time: 0.00s",
                                           new Vector2(-10, -10), new Vector2(200, 30),
                                           TextAnchor.UpperRight, anchorRight: true);
            // Hit feedback (centre)
            var feedbackText = CreateLabel(canvasGo, "HitFeedback",  "",
                                           new Vector2(0, 60), new Vector2(400, 60),
                                           TextAnchor.MiddleCenter, centred: true,
                                           fontSize: 36);
            feedbackText.gameObject.SetActive(false);

            // Wire UIManager
            uiManager.scoreText       = scoreText;
            uiManager.comboText       = comboText;
            uiManager.songTimeText    = songTimeText;
            uiManager.hitFeedbackText = feedbackText;
        }

        private static Text CreateLabel(GameObject canvas, string name, string defaultText,
                                        Vector2 offset, Vector2 size, TextAnchor alignment,
                                        bool anchorRight = false, bool centred = false,
                                        int fontSize = 18)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(canvas.transform, false);
            var rt   = go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();

            text.text      = defaultText;
            text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                          ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize  = fontSize;
            text.color     = Color.white;
            text.alignment = alignment;

            if (centred)
            {
                rt.anchorMin        = new Vector2(0.5f, 0f);
                rt.anchorMax        = new Vector2(0.5f, 0f);
                rt.pivot            = new Vector2(0.5f, 0f);
                rt.anchoredPosition = offset;
                rt.sizeDelta        = size;
            }
            else if (anchorRight)
            {
                rt.anchorMin        = new Vector2(1f, 1f);
                rt.anchorMax        = new Vector2(1f, 1f);
                rt.pivot            = new Vector2(1f, 1f);
                rt.anchoredPosition = offset;
                rt.sizeDelta        = size;
            }
            else
            {
                rt.anchorMin        = new Vector2(0f, 1f);
                rt.anchorMax        = new Vector2(0f, 1f);
                rt.pivot            = new Vector2(0f, 1f);
                rt.anchoredPosition = offset;
                rt.sizeDelta        = size;
            }

            return text;
        }

        // ── Utility ───────────────────────────────────────────────────────────────

        private static void EnsureDir(string assetPath)
        {
            string abs = Path.Combine(Application.dataPath,
                                      assetPath.Replace("Assets/", ""));
            if (!Directory.Exists(abs))
                Directory.CreateDirectory(abs);
        }

        /// <summary>
        /// Attempts to add a Global Light 2D component via reflection so the script
        /// compiles even when the 2D Renderer package is not installed.
        /// </summary>
        private static void TryAddGlobalLight2D()
        {
            var lightType = Type.GetType(
                "UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
            if (lightType == null) return;

            var lightGo = new GameObject("Global Light 2D");
            var light   = lightGo.AddComponent(lightType);
            if (light == null) return;

            // Set LightType to Global (enum value 3 in URP Light2D)
            var lightTypeProp = lightType.GetProperty("lightType");
            if (lightTypeProp != null)
            {
                var lightTypeEnum = lightTypeProp.PropertyType;
                try { lightTypeProp.SetValue(light, Enum.ToObject(lightTypeEnum, 3)); }
                catch { /* ignore if enum layout differs */ }
            }

            var intensityProp = lightType.GetProperty("intensity");
            intensityProp?.SetValue(light, 1f);
        }
    }
}
#endif
