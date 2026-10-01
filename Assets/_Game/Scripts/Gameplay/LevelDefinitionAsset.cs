using System;
using System.Collections.Generic;
using UnityEngine;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    [Serializable]
    public class PackageDefinition
    {
        public int id;
        public int x;
        public int y;
        public PackageDirection direction;
        public PackageColor color;
    }

    [Serializable]
    public class BlockerDefinition
    {
        public int id;
        public int x;
        public int y;
    }

    [Serializable]
    public class TruckDefinition
    {
        public PackageColor requiredColor;
        public int capacity = 1;
    }

    [CreateAssetMenu(fileName = "NewLevelDefinition", menuName = "Parcel Escape/Level Definition")]
    public class LevelDefinitionAsset : ScriptableObject
    {
        public int width = 5;
        public int height = 5;
        public List<PackageDefinition> packageDefinitions = new List<PackageDefinition>();
        public List<BlockerDefinition> blockerDefinitions = new List<BlockerDefinition>();
        public List<TruckDefinition> truckDefinitions = new List<TruckDefinition>();
    }
}
