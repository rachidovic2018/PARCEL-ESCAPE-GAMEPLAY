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
        public TruckState CompletedTruck { get; }
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
            TruckState completedTruck,
            IReadOnlyList<PackageState> autoLoadedPackages,
            DeliveryState resultingState)
        {
            Status = status;
            CompletedTruck = completedTruck;
            AutoLoadedPackages = new List<PackageState>(autoLoadedPackages).AsReadOnly();
            ResultingState = resultingState;
        }
    }
}
