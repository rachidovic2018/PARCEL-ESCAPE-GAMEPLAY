namespace ParcelEscape.Core
{
    public readonly struct BlockerState
    {
        public readonly int Id;
        public readonly GridPosition Position;

        public BlockerState(int id, GridPosition position)
        {
            Id = id;
            Position = position;
        }
    }
}
