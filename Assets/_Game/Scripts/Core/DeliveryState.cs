using System;
using System.Collections.Generic;

namespace ParcelEscape.Core
{
    public sealed class DeliveryState
    {
        public TruckState ActiveTruck { get; }
        public TruckState NextTruck { get; }
        public HoldingQueueState HoldingQueue { get; }

        public DeliveryState(
            TruckState activeTruck,
            TruckState nextTruck,
            HoldingQueueState holdingQueue)
        {
            ActiveTruck = activeTruck ?? throw new ArgumentNullException(nameof(activeTruck));
            NextTruck = nextTruck ?? throw new ArgumentNullException(nameof(nextTruck));
            HoldingQueue = holdingQueue ?? throw new ArgumentNullException(nameof(holdingQueue));

            if (nextTruck.LoadCount != 0)
            {
                throw new ArgumentException("The next truck cannot receive packages before promotion.", nameof(nextTruck));
            }

            ValidateUniquePackageIds(activeTruck.LoadedPackages, holdingQueue.Packages);
        }

        public DeliveryRoutingResult RouteEscapedPackage(MoveExecutionResult moveResult)
        {
            if (moveResult == null)
            {
                throw new ArgumentNullException(nameof(moveResult));
            }

            if (moveResult.Status != MoveExecutionStatus.Success || !moveResult.EscapedPackage.HasValue)
            {
                throw new ArgumentException("Only a successful move with an escaped package can be routed.", nameof(moveResult));
            }

            return RouteEscapedPackage(moveResult.EscapedPackage.Value);
        }

        public DeliveryRoutingResult RouteEscapedPackage(PackageState package)
        {
            if (ContainsPackage(package.Id))
            {
                return new DeliveryRoutingResult(DeliveryRoutingStatus.PackageAlreadyRouted, package, this);
            }

            if (ActiveTruck.TryLoad(package, out var loadedTruck))
            {
                var state = new DeliveryState(loadedTruck, NextTruck, HoldingQueue);
                return new DeliveryRoutingResult(DeliveryRoutingStatus.LoadedActiveTruck, package, state);
            }

            if (HoldingQueue.TryEnqueue(package, out var holdingQueue))
            {
                var state = new DeliveryState(ActiveTruck, NextTruck, holdingQueue);
                return new DeliveryRoutingResult(DeliveryRoutingStatus.AddedToHolding, package, state);
            }

            return new DeliveryRoutingResult(DeliveryRoutingStatus.HoldingFull, package, this);
        }

        private bool ContainsPackage(int packageId)
        {
            foreach (var package in ActiveTruck.LoadedPackages)
            {
                if (package.Id == packageId)
                {
                    return true;
                }
            }

            foreach (var package in HoldingQueue.Packages)
            {
                if (package.Id == packageId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateUniquePackageIds(
            IReadOnlyList<PackageState> loadedPackages,
            IReadOnlyList<PackageState> heldPackages)
        {
            var packageIds = new HashSet<int>();
            foreach (var package in loadedPackages)
            {
                packageIds.Add(package.Id);
            }

            foreach (var package in heldPackages)
            {
                if (!packageIds.Add(package.Id))
                {
                    throw new ArgumentException($"Package ID {package.Id} cannot exist in both delivery destinations.");
                }
            }
        }
    }
}
