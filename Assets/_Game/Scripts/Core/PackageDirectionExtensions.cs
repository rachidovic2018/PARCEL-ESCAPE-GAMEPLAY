using System;

namespace ParcelEscape.Core
{
    public static class PackageDirectionExtensions
    {
        public static GridPosition ToGridDelta(this PackageDirection direction)
        {
            switch (direction)
            {
                case PackageDirection.Up:
                    return new GridPosition(0, 1);
                case PackageDirection.Down:
                    return new GridPosition(0, -1);
                case PackageDirection.Left:
                    return new GridPosition(-1, 0);
                case PackageDirection.Right:
                    return new GridPosition(1, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
