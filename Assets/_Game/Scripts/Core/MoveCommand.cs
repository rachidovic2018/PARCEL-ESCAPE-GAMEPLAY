namespace ParcelEscape.Core
{
    public readonly struct MoveCommand
    {
        public readonly int PackageId;

        public MoveCommand(int packageId)
        {
            PackageId = packageId;
        }
    }
}
