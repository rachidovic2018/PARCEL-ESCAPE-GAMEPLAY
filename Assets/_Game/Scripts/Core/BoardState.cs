using System;
using System.Collections.Generic;

namespace ParcelEscape.Core
{
    public class BoardState
    {
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<PackageState> Packages { get; }
        public IReadOnlyList<BlockerState> Blockers { get; }

        public BoardState(int width, int height, IReadOnlyList<PackageState> packages, IReadOnlyList<BlockerState> blockers)
        {
            if (width <= 0) throw new ArgumentException("Width must be greater than 0");
            if (height <= 0) throw new ArgumentException("Height must be greater than 0");

            var packageIds = new HashSet<int>();
            var blockerIds = new HashSet<int>();
            var occupiedPositions = new HashSet<GridPosition>();

            foreach (var package in packages)
            {
                if (package.Position.X < 0 || package.Position.X >= width || package.Position.Y < 0 || package.Position.Y >= height)
                    throw new ArgumentException($"Package {package.Id} is out of bounds");
                if (!packageIds.Add(package.Id))
                    throw new ArgumentException($"Duplicate package ID {package.Id}");
                if (!occupiedPositions.Add(package.Position))
                    throw new ArgumentException($"Multiple items at position {package.Position}");
            }

            foreach (var blocker in blockers)
            {
                if (blocker.Position.X < 0 || blocker.Position.X >= width || blocker.Position.Y < 0 || blocker.Position.Y >= height)
                    throw new ArgumentException($"Blocker {blocker.Id} is out of bounds");
                if (!blockerIds.Add(blocker.Id))
                    throw new ArgumentException($"Duplicate blocker ID {blocker.Id}");
                if (!occupiedPositions.Add(blocker.Position))
                    throw new ArgumentException($"Multiple items at position {blocker.Position}");
            }

            Width = width;
            Height = height;
            
            // Create internal copies to ensure immutability
            Packages = new List<PackageState>(packages).AsReadOnly();
            Blockers = new List<BlockerState>(blockers).AsReadOnly();
        }

        public bool IsInside(GridPosition pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        public bool IsOccupied(GridPosition pos)
        {
            return IsOccupiedByPackage(pos) || IsOccupiedByBlocker(pos);
        }

        public bool IsOccupiedByPackage(GridPosition pos)
        {
            foreach (var p in Packages)
            {
                if (p.Position == pos) return true;
            }
            return false;
        }

        public bool IsOccupiedByBlocker(GridPosition pos)
        {
            foreach (var b in Blockers)
            {
                if (b.Position == pos) return true;
            }
            return false;
        }

        public bool TryGetPackageAt(GridPosition pos, out PackageState package)
        {
            foreach (var p in Packages)
            {
                if (p.Position == pos)
                {
                    package = p;
                    return true;
                }
            }
            package = default;
            return false;
        }

        public bool TryGetPackageById(int packageId, out PackageState package)
        {
            foreach (var p in Packages)
            {
                if (p.Id == packageId)
                {
                    package = p;
                    return true;
                }
            }
            package = default;
            return false;
        }

        public bool IsPathClear(int packageId)
        {
            if (!TryGetPackageById(packageId, out var package))
            {
                return false;
            }

            var delta = package.Direction.ToGridDelta();
            var currentPos = new GridPosition(package.Position.X + delta.X, package.Position.Y + delta.Y);

            while (IsInside(currentPos))
            {
                if (IsOccupied(currentPos))
                {
                    return false;
                }
                currentPos = new GridPosition(currentPos.X + delta.X, currentPos.Y + delta.Y);
            }

            return true;
        }

        public BoardState RemovePackage(int packageId)
        {
            var newPackages = new List<PackageState>();
            foreach (var p in Packages)
            {
                if (p.Id != packageId)
                {
                    newPackages.Add(p);
                }
            }
            return new BoardState(Width, Height, newPackages, Blockers);
        }
    }
}
