using System;
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

        public static TruckSequenceState ConvertTruckSequence(LevelDefinitionAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (asset.truckDefinitions == null)
            {
                throw new ArgumentException("Level truck definitions cannot be null.", nameof(asset));
            }

            var trucks = new List<TruckState>(asset.truckDefinitions.Count);
            for (var index = 0; index < asset.truckDefinitions.Count; index++)
            {
                var definition = asset.truckDefinitions[index];
                if (definition == null)
                {
                    throw new ArgumentException(
                        $"Truck definition at index {index} cannot be null.",
                        nameof(asset));
                }

                trucks.Add(new TruckState(definition.requiredColor, definition.capacity));
            }

            return new TruckSequenceState(trucks);
        }

        public static DeliveryState ConvertDelivery(LevelDefinitionAsset asset, int holdingCapacity)
        {
            var truckSequence = ConvertTruckSequence(asset);
            return DeliveryState.CreateInitial(
                truckSequence,
                new HoldingQueueState(holdingCapacity));
        }
    }
}
