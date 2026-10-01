namespace ParcelEscape.Core
{
    public enum DeliveryRoutingStatus
    {
        LoadedActiveTruck,
        AddedToHolding,
        HoldingFull,
        PackageAlreadyRouted
    }

    public sealed class DeliveryRoutingResult
    {
        public DeliveryRoutingStatus Status { get; }
        public PackageState Package { get; }
        public DeliveryState ResultingState { get; }
        public bool WasAccepted =>
            Status == DeliveryRoutingStatus.LoadedActiveTruck ||
            Status == DeliveryRoutingStatus.AddedToHolding;
        public PackageState? UnroutedPackage => WasAccepted ? (PackageState?)null : Package;

        internal DeliveryRoutingResult(
            DeliveryRoutingStatus status,
            PackageState package,
            DeliveryState resultingState)
        {
            Status = status;
            Package = package;
            ResultingState = resultingState;
        }
    }
}
