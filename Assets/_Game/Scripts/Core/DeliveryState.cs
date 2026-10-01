using System;
using System.Collections.Generic;

namespace ParcelEscape.Core
{
    public sealed class DeliveryState
    {
        public TruckState ActiveTruck { get; }
        public TruckState NextTruck { get; }
        public TruckSequenceState RemainingTruckSequence { get; }
        public HoldingQueueState HoldingQueue { get; }
        public bool HasNextTruck => NextTruck != null;

        public DeliveryState(
            TruckState activeTruck,
            TruckState nextTruck,
            HoldingQueueState holdingQueue)
            : this(activeTruck, nextTruck, new TruckSequenceState(), holdingQueue)
        {
        }

        public DeliveryState(
            TruckState activeTruck,
            TruckState nextTruck,
            TruckSequenceState remainingTruckSequence,
            HoldingQueueState holdingQueue)
        {
            ActiveTruck = activeTruck ?? throw new ArgumentNullException(nameof(activeTruck));
            NextTruck = nextTruck;
            RemainingTruckSequence = remainingTruckSequence ??
                throw new ArgumentNullException(nameof(remainingTruckSequence));
            HoldingQueue = holdingQueue ?? throw new ArgumentNullException(nameof(holdingQueue));

            if (nextTruck != null && nextTruck.LoadCount != 0)
            {
                throw new ArgumentException("The next truck cannot receive packages before promotion.", nameof(nextTruck));
            }

            if (nextTruck == null && !remainingTruckSequence.IsEmpty)
            {
                throw new ArgumentException(
                    "A remaining truck sequence requires an explicit next truck.",
                    nameof(remainingTruckSequence));
            }

            ValidateUniquePackageIds(activeTruck.LoadedPackages, holdingQueue.Packages);
        }

        public static DeliveryState CreateInitial(
            TruckSequenceState truckSequence,
            HoldingQueueState holdingQueue)
        {
            if (truckSequence == null)
            {
                throw new ArgumentNullException(nameof(truckSequence));
            }

            if (holdingQueue == null)
            {
                throw new ArgumentNullException(nameof(holdingQueue));
            }

            if (!truckSequence.TryDequeue(out var activeTruck, out var afterActive) ||
                !afterActive.TryDequeue(out var nextTruck, out var remainingSequence))
            {
                throw new ArgumentException(
                    "An authored truck sequence must contain at least an active and next truck.",
                    nameof(truckSequence));
            }

            return new DeliveryState(activeTruck, nextTruck, remainingSequence, holdingQueue);
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
                var state = new DeliveryState(
                    loadedTruck,
                    NextTruck,
                    RemainingTruckSequence,
                    HoldingQueue);
                return new DeliveryRoutingResult(DeliveryRoutingStatus.LoadedActiveTruck, package, state);
            }

            if (HoldingQueue.TryEnqueue(package, out var holdingQueue))
            {
                var state = new DeliveryState(
                    ActiveTruck,
                    NextTruck,
                    RemainingTruckSequence,
                    holdingQueue);
                return new DeliveryRoutingResult(DeliveryRoutingStatus.AddedToHolding, package, state);
            }

            return new DeliveryRoutingResult(DeliveryRoutingStatus.HoldingFull, package, this);
        }

        public TruckPromotionResult ResolveCompletedActiveTruck()
        {
            var noCompletedTrucks = new List<TruckState>().AsReadOnly();
            var noAutoLoadedPackages = new List<PackageState>().AsReadOnly();

            if (!ActiveTruck.IsComplete)
            {
                return new TruckPromotionResult(
                    TruckPromotionStatus.ActiveTruckIncomplete,
                    noCompletedTrucks,
                    noAutoLoadedPackages,
                    this);
            }

            if (!HasNextTruck)
            {
                return new TruckPromotionResult(
                    TruckPromotionStatus.NextTruckUnavailable,
                    noCompletedTrucks,
                    noAutoLoadedPackages,
                    this);
            }

            var completedTrucks = new List<TruckState>();
            var activeTruck = ActiveTruck;
            var nextTruck = NextTruck;
            var remainingSequence = RemainingTruckSequence;
            var holdingQueue = HoldingQueue;
            var autoLoadedPackages = new List<PackageState>();
            var maximumPromotions = RemainingTruckSequence.Count + 1;

            for (var promotionIndex = 0; promotionIndex < maximumPromotions; promotionIndex++)
            {
                completedTrucks.Add(activeTruck);
                activeTruck = nextTruck;

                if (remainingSequence.TryDequeue(out var followingTruck, out var afterFollowing))
                {
                    nextTruck = followingTruck;
                    remainingSequence = afterFollowing;
                }
                else
                {
                    nextTruck = null;
                }

                var maximumTransfers = Math.Min(
                    holdingQueue.Count,
                    activeTruck.Capacity - activeTruck.LoadCount);

                for (var transferIndex = 0; transferIndex < maximumTransfers; transferIndex++)
                {
                    if (!holdingQueue.TryPeek(out var nextPackage) ||
                        nextPackage.Color != activeTruck.RequiredColor)
                    {
                        break;
                    }

                    if (!holdingQueue.TryDequeue(out var dequeuedPackage, out var remainingHolding))
                    {
                        throw new InvalidOperationException("A peeked holding package could not be dequeued.");
                    }

                    if (dequeuedPackage.Id != nextPackage.Id)
                    {
                        throw new InvalidOperationException("Holding queue order changed during auto-load.");
                    }

                    if (!activeTruck.TryLoad(dequeuedPackage, out var loadedTruck))
                    {
                        throw new InvalidOperationException("An eligible holding package could not be loaded.");
                    }

                    activeTruck = loadedTruck;
                    holdingQueue = remainingHolding;
                    autoLoadedPackages.Add(dequeuedPackage);
                }

                if (!activeTruck.IsComplete || nextTruck == null)
                {
                    break;
                }
            }

            var resultingState = new DeliveryState(
                activeTruck,
                nextTruck,
                remainingSequence,
                holdingQueue);
            var status = activeTruck.IsComplete
                ? TruckPromotionStatus.PromotedTruckCompleted
                : TruckPromotionStatus.Promoted;

            return new TruckPromotionResult(
                status,
                completedTrucks,
                autoLoadedPackages,
                resultingState);
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
