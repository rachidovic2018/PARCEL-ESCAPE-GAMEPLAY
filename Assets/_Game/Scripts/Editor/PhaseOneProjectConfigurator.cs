using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ParcelEscape.Editor
{
    public static class PhaseOneProjectConfigurator
    {
        private const string SettingsFolder = "Assets/_Game/Settings";
        private const string RendererPath = SettingsFolder + "/ParcelEscapeMobileRenderer.asset";
        private const string PipelinePath = SettingsFolder + "/ParcelEscapeMobileURP.asset";
        private const string MaterialsFolder = "Assets/_Game/Materials";
        private const string BoardMaterialPath = MaterialsFolder + "/BoardCell_URP.mat";
        private const string GameplayScenePath = "Assets/_Game/Scenes/Gameplay.unity";
        private const string PackageIdentifier = "com.parcelescape.sortanddeliver";

        [MenuItem("Parcel Escape/Phase 1/Configure URP and Android")]
        public static void Configure()
        {
            UniversalRenderPipelineAsset pipelineAsset = CreateOrReusePipelineAsset();
            ConfigurePipeline(pipelineAsset);
            AssignPipelineToGraphicsAndQuality(pipelineAsset);
            ConfigureGameplaySceneMaterials();

            bool androidTargetAvailable = ConfigureAndroidSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                "Phase 1 project configuration applied.\n" +
                BuildValidationReport(pipelineAsset, androidTargetAvailable));
        }

        [MenuItem("Parcel Escape/Phase 1/Validate URP and Android")]
        public static void Validate()
        {
            UniversalRenderPipelineAsset pipelineAsset =
                GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            bool androidTargetAvailable = BuildPipeline.IsBuildTargetSupported(
                BuildTargetGroup.Android,
                BuildTarget.Android);

            Debug.Log(BuildValidationReport(pipelineAsset, androidTargetAvailable));
        }

        private static UniversalRenderPipelineAsset CreateOrReusePipelineAsset()
        {
            EnsureFolder(SettingsFolder);

            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                rendererData.postProcessData = AssetDatabase.LoadAssetAtPath<PostProcessData>(
                    "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            UniversalRenderPipelineAsset pipelineAsset =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
            }

            if (pipelineAsset.rendererDataList.Length == 0 || pipelineAsset.rendererDataList[0] == null)
            {
                throw new InvalidOperationException("The URP asset has no valid default renderer.");
            }

            return pipelineAsset;
        }

        private static void ConfigurePipeline(UniversalRenderPipelineAsset pipelineAsset)
        {
            pipelineAsset.renderScale = 1f;
            pipelineAsset.msaaSampleCount = 2;
            pipelineAsset.supportsHDR = false;
            pipelineAsset.supportsCameraDepthTexture = false;
            pipelineAsset.supportsCameraOpaqueTexture = false;
            pipelineAsset.maxAdditionalLightsCount = 0;
            pipelineAsset.shadowDistance = 20f;
            pipelineAsset.shadowCascadeCount = 1;
            EditorUtility.SetDirty(pipelineAsset);
        }

        private static void AssignPipelineToGraphicsAndQuality(
            UniversalRenderPipelineAsset pipelineAsset)
        {
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            int originalQualityLevel = QualitySettings.GetQualityLevel();
            try
            {
                for (int index = 0; index < QualitySettings.names.Length; index++)
                {
                    QualitySettings.SetQualityLevel(index, false);
                    QualitySettings.renderPipeline = pipelineAsset;
                }
            }
            finally
            {
                QualitySettings.SetQualityLevel(originalQualityLevel, false);
            }
        }

        private static void ConfigureGameplaySceneMaterials()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("URP Simple Lit shader is unavailable.");
            }

            EnsureFolder(MaterialsFolder);
            Material boardMaterial = AssetDatabase.LoadAssetAtPath<Material>(BoardMaterialPath);
            if (boardMaterial == null)
            {
                boardMaterial = new Material(shader)
                {
                    name = "BoardCell_URP",
                    color = new Color(0.22f, 0.26f, 0.30f)
                };
                AssetDatabase.CreateAsset(boardMaterial, BoardMaterialPath);
            }
            else
            {
                boardMaterial.shader = shader;
                boardMaterial.color = new Color(0.22f, 0.26f, 0.30f);
                EditorUtility.SetDirty(boardMaterial);
            }

            Scene scene = SceneManager.GetSceneByPath(GameplayScenePath);
            bool openedForConfiguration = !scene.IsValid() || !scene.isLoaded;
            if (!openedForConfiguration && scene.isDirty)
            {
                throw new InvalidOperationException(
                    "Gameplay scene has unsaved changes. Save or discard them before configuration.");
            }

            try
            {
                if (openedForConfiguration)
                {
                    scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Additive);
                }

                GameObject boardSurface = scene.GetRootGameObjects()
                    .FirstOrDefault(root => root.name == "BoardSurface");
                if (boardSurface == null)
                {
                    throw new InvalidOperationException("Gameplay scene is missing BoardSurface.");
                }

                MeshRenderer[] cellRenderers = boardSurface.GetComponentsInChildren<MeshRenderer>(true);
                if (cellRenderers.Length != 25)
                {
                    throw new InvalidOperationException(
                        $"Expected 25 board cell renderers but found {cellRenderers.Length}.");
                }

                foreach (MeshRenderer cellRenderer in cellRenderers)
                {
                    cellRenderer.sharedMaterial = boardMaterial;
                    EditorUtility.SetDirty(cellRenderer);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                {
                    throw new InvalidOperationException(
                        "Failed to save Gameplay scene material changes.");
                }
            }
            finally
            {
                if (openedForConfiguration && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static bool ConfigureAndroidSettings()
        {
            NamedBuildTarget android = NamedBuildTarget.Android;

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel36;
            PlayerSettings.SetApplicationIdentifier(android, PackageIdentifier);
            EditorUserBuildSettings.buildAppBundle = true;

            bool androidTargetAvailable = BuildPipeline.IsBuildTargetSupported(
                BuildTargetGroup.Android,
                BuildTarget.Android);
            if (androidTargetAvailable && EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                        BuildTargetGroup.Android,
                        BuildTarget.Android))
                {
                    Debug.LogWarning("Android support is installed, but Unity did not switch build target.");
                }
            }
            else if (!androidTargetAvailable)
            {
                Debug.LogWarning(
                    "Android Build Support is not installed for this Unity Editor. " +
                    "Project settings were saved, but the build target was not switched.");
            }

            return androidTargetAvailable;
        }

        private static string BuildValidationReport(
            UniversalRenderPipelineAsset pipelineAsset,
            bool androidTargetAvailable)
        {
            string graphicsPipeline = GraphicsSettings.defaultRenderPipeline != null
                ? GraphicsSettings.defaultRenderPipeline.name
                : "Built-in";
            string qualityPipelines = string.Join(", ", Enumerable.Range(0, QualitySettings.names.Length)
                .Select(index =>
                {
                    RenderPipelineAsset asset = QualitySettings.GetRenderPipelineAssetAt(index);
                    return $"{QualitySettings.names[index]}={(asset != null ? asset.name : "None")}";
                }));

            bool rendererValid = pipelineAsset != null &&
                                 pipelineAsset.rendererDataList.Length > 0 &&
                                 pipelineAsset.rendererDataList[0] != null;

            return
                $"GraphicsPipeline={graphicsPipeline}\n" +
                $"QualityPipelines={qualityPipelines}\n" +
                $"RendererValid={rendererValid}\n" +
                $"Orientation={PlayerSettings.defaultInterfaceOrientation}\n" +
                $"PackageIdentifier={PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)}\n" +
                $"ScriptingBackend={PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}\n" +
                $"Architectures={PlayerSettings.Android.targetArchitectures}\n" +
                $"MinimumSdk={(int)PlayerSettings.Android.minSdkVersion}\n" +
                $"TargetSdk={(int)PlayerSettings.Android.targetSdkVersion}\n" +
                $"BuildAppBundle={EditorUserBuildSettings.buildAppBundle}\n" +
                $"AndroidTargetAvailable={androidTargetAvailable}\n" +
                $"ActiveBuildTarget={EditorUserBuildSettings.activeBuildTarget}";
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
    }
}
