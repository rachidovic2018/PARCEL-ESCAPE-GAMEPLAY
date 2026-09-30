using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ParcelEscape.Core;
using ParcelEscape.Gameplay;

namespace ParcelEscape.Editor
{
    /// <summary>
    /// Editor utility to create the 5×5 development level ScriptableObject.
    /// Menu: Parcel Escape → Create Development Level
    /// </summary>
    public static class DevelopmentLevelCreator
    {
        private const string LevelFolder = "Assets/_Game/ScriptableObjects";
        private const string LevelAssetPath = LevelFolder + "/DevLevel_5x5.asset";
        private const string SceneFolder = "Assets/_Game/Scenes";
        private const string ScenePath = SceneFolder + "/Gameplay.unity";

        [MenuItem("Parcel Escape/Create Development Level")]
        public static void CreateDevelopmentLevel()
        {
            LevelDefinitionAsset asset = CreateOrUpdateDevelopmentLevel();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log($"Development level created or updated at '{LevelAssetPath}'.");
        }

        [MenuItem("Parcel Escape/Create Development Gameplay Scene")]
        public static void CreateDevelopmentGameplayScene()
        {
            LevelDefinitionAsset level = CreateOrUpdateDevelopmentLevel();
            EnsureFolder(SceneFolder);

            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            level = AssetDatabase.LoadAssetAtPath<LevelDefinitionAsset>(LevelAssetPath);
            if (level == null)
            {
                throw new InvalidOperationException($"Failed to reload development level at '{LevelAssetPath}'.");
            }

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.12f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();
            var cameraSetup = cameraObject.AddComponent<CameraSetup>();
            cameraSetup.SetupForBoard(level.width, level.height, 1f);

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.90f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            CreateBoardSurface(level.width, level.height, 1f);

            var boardObject = new GameObject("BoardPresenter");
            var boardPresenter = boardObject.AddComponent<BoardPresenter>();

            var bootstrapObject = new GameObject("GameplayBootstrap");
            var bootstrap = bootstrapObject.AddComponent<GameplaySceneBootstrap>();
            bootstrap.Configure(level, boardPresenter, cameraSetup);
            EditorUtility.SetDirty(bootstrap);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Failed to save development gameplay scene at '{ScenePath}'.");
            }

            AddGameplaySceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = bootstrapObject;
            Debug.Log($"Development gameplay scene created at '{ScenePath}' and enabled in Build Settings.");
        }

        private static LevelDefinitionAsset CreateOrUpdateDevelopmentLevel()
        {
            EnsureFolder(LevelFolder);

            var asset = AssetDatabase.LoadAssetAtPath<LevelDefinitionAsset>(LevelAssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LevelDefinitionAsset>();
                AssetDatabase.CreateAsset(asset, LevelAssetPath);
            }

            asset.width = 5;
            asset.height = 5;
            asset.packageDefinitions.Clear();
            asset.blockerDefinitions.Clear();

            // Blue at (2,3) facing Up — valid escape (2,4 is clear)
            asset.packageDefinitions.Add(new PackageDefinition
            {
                id = 1, x = 2, y = 3,
                direction = PackageDirection.Up,
                color = PackageColor.Blue
            });

            // Red at (0,2) facing Right — blocked by Green at (2,2)
            asset.packageDefinitions.Add(new PackageDefinition
            {
                id = 2, x = 0, y = 2,
                direction = PackageDirection.Right,
                color = PackageColor.Red
            });

            // Green at (2,2) facing Left — blocked by Red at (0,2) path
            asset.packageDefinitions.Add(new PackageDefinition
            {
                id = 3, x = 2, y = 2,
                direction = PackageDirection.Left,
                color = PackageColor.Green
            });

            // Yellow at (1,3) facing Down — blocked by blocker at (1,1)
            asset.packageDefinitions.Add(new PackageDefinition
            {
                id = 4, x = 1, y = 3,
                direction = PackageDirection.Down,
                color = PackageColor.Yellow
            });

            // Blocker at (1,1)
            asset.blockerDefinitions.Add(new BlockerDefinition
            {
                id = 5, x = 1, y = 1
            });

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(LevelAssetPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            LevelDefinitionAsset savedAsset = AssetDatabase.LoadAssetAtPath<LevelDefinitionAsset>(LevelAssetPath);
            if (savedAsset == null)
            {
                throw new InvalidOperationException($"Failed to load development level at '{LevelAssetPath}'.");
            }

            LevelDefinitionConverter.Convert(savedAsset);
            return savedAsset;
        }

        private static void CreateBoardSurface(int width, int height, float cellSize)
        {
            var boardSurface = new GameObject("BoardSurface");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cell.name = $"Cell_{x}_{y}";
                    cell.transform.SetParent(boardSurface.transform, false);
                    cell.transform.position = new Vector3(
                        x * cellSize - (width * cellSize * 0.5f) + (cellSize * 0.5f),
                        -0.05f,
                        y * cellSize - (height * cellSize * 0.5f) + (cellSize * 0.5f));
                    cell.transform.localScale = new Vector3(cellSize * 0.92f, 0.1f, cellSize * 0.92f);

                    Collider collider = cell.GetComponent<Collider>();
                    if (collider != null)
                    {
                        UnityEngine.Object.DestroyImmediate(collider);
                    }
                }
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(folderPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
            {
                throw new InvalidOperationException($"Invalid Unity asset folder path '{folderPath}'.");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void AddGameplaySceneToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            int existingIndex = scenes.FindIndex(scene => scene.path == ScenePath);
            var gameplayScene = new EditorBuildSettingsScene(ScenePath, true);

            if (existingIndex >= 0)
            {
                scenes[existingIndex] = gameplayScene;
            }
            else
            {
                scenes.Add(gameplayScene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
