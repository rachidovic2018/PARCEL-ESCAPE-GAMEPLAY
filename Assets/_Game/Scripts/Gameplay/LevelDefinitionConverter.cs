using System.Collections.Generic;
using ParcelEscape.Core;

namespace ParcelEscape.Gameplay
{
    public static class LevelDefinitionConverter
    {
        public static BoardState Convert(LevelDefinitionAsset asset)
        {
            var packages = new List<PackageState>();
            foreach (var p in asset.packageDefinitions)
            {
                packages.Add(new PackageState(p.id, new GridPosition(p.x, p.y), p.direction, p.color));
            }

            var blockers = new List<BlockerState>();
            foreach (var b in asset.blockerDefinitions)
            {
                blockers.Add(new BlockerState(b.id, new GridPosition(b.x, b.y)));
            }

            return new BoardState(asset.width, asset.height, packages, blockers);
        }
    }
}
