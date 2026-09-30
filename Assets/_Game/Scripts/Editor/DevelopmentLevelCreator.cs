using UnityEngine;
using UnityEditor;
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
        [MenuItem("Parcel Escape/Create Development Level")]
        public static void CreateDevelopmentLevel()
        {
            var asset = ScriptableObject.CreateInstance<LevelDefinitionAsset>();
            asset.width = 5;
            asset.height = 5;

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

            string folder = "Assets/_Game/ScriptableObjects";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "ScriptableObjects");
            }

            AssetDatabase.CreateAsset(asset, folder + "/DevLevel_5x5.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log("Development level 'DevLevel_5x5' created successfully.");
        }
    }
}

