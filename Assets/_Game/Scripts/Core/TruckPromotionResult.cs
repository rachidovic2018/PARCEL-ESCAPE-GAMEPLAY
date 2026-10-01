using System.Collections.Generic;

namespace ParcelEscape.Core
{
    public enum TruckPromotionStatus
    {
        ActiveTruckIncomplete,
        NextTruckUnavailable,
        Promoted,
        PromotedTruckCompleted
    }

    public sealed class TruckPromotionResult
    {
        public TruckPromotionStatus Status { get; }
        public IReadOnlyList<TruckState> CompletedTrucks { get; }
        public TruckState CompletedTruck => CompletedTrucks.Count == 0 ? null : CompletedTrucks[0];
        public IReadOnlyList<PackageState> AutoLoadedPackages { get; }
        public DeliveryState ResultingState { get; }
        public bool WasPromoted =>
            Status == TruckPromotionStatus.Promoted ||
            Status == TruckPromotionStatus.PromotedTruckCompleted;
        public bool IsAwaitingNextTruck =>
            ResultingState.ActiveTruck.IsComplete &&
            !ResultingState.HasNextTruck;

        internal TruckPromotionResult(
            TruckPromotionStatus status,
            IReadOnlyList<TruckState> completedTrucks,
            IReadOnlyList<PackageState> autoLoadedPackages,
            DeliveryState resultingState)
        {
            Status = status;
            CompletedTrucks = new List<TruckState>(completedTrucks).AsReadOnly();
            AutoLoadedPackages = new List<PackageState>(autoLoadedPackages).AsReadOnly();
            ResultingState = resultingState;
        }
    }
}
