namespace ParcelEscape.Core
{
    public readonly struct PackageState
    {
        public readonly int Id;
        public readonly GridPosition Position;
        public readonly PackageDirection Direction;
        public readonly PackageColor Color;

        public PackageState(int id, GridPosition position, PackageDirection direction, PackageColor color)
        {
            Id = id;
            Position = position;
            Direction = direction;
            Color = color;
        }
    }
}
