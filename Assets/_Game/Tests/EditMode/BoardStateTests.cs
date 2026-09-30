using NUnit.Framework;
using ParcelEscape.Core;
using System;
using System.Collections.Generic;

namespace ParcelEscape.Tests
{
    [TestFixture]
    public class BoardStateTests
    {
        [Test]
        public void IsInside_InsideBoard_ReturnsTrue()
        {
            var board = new BoardState(5, 5, new List<PackageState>(), new List<BlockerState>());
            var pos = new GridPosition(2, 3);
            Assert.IsTrue(board.IsInside(pos));
        }

        [Test]
        public void IsInside_OutsideBoardNegative_ReturnsFalse()
        {
            var board = new BoardState(5, 5, new List<PackageState>(), new List<BlockerState>());
            Assert.IsFalse(board.IsInside(new GridPosition(-1, 2)));
            Assert.IsFalse(board.IsInside(new GridPosition(2, -1)));
        }

        [Test]
        public void IsInside_OutsideBoardUpper_ReturnsFalse()
        {
            var board = new BoardState(5, 5, new List<PackageState>(), new List<BlockerState>());
            Assert.IsFalse(board.IsInside(new GridPosition(5, 2)));
            Assert.IsFalse(board.IsInside(new GridPosition(2, 5)));
        }

        [Test]
        public void IsInside_ExactBoundary_ReturnsBehavior()
        {
            var board = new BoardState(5, 5, new List<PackageState>(), new List<BlockerState>());
            Assert.IsTrue(board.IsInside(new GridPosition(0, 0)));
            Assert.IsTrue(board.IsInside(new GridPosition(4, 4)));
        }

        [Test]
        public void Occupancy_PackageOccupiesCell_ReturnsTrue()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(1, 1), PackageDirection.Up, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsOccupied(new GridPosition(1, 1)));
            Assert.IsTrue(board.IsOccupiedByPackage(new GridPosition(1, 1)));
        }

        [Test]
        public void Occupancy_BlockerOccupiesCell_ReturnsTrue()
        {
            var blockers = new List<BlockerState> { new BlockerState(1, new GridPosition(2, 2)) };
            var board = new BoardState(5, 5, new List<PackageState>(), blockers);
            Assert.IsTrue(board.IsOccupied(new GridPosition(2, 2)));
            Assert.IsTrue(board.IsOccupiedByBlocker(new GridPosition(2, 2)));
        }

        [Test]
        public void Occupancy_EmptyCell_ReturnsFalse()
        {
            var board = new BoardState(5, 5, new List<PackageState>(), new List<BlockerState>());
            Assert.IsFalse(board.IsOccupied(new GridPosition(3, 3)));
        }

        [Test]
        public void Invariants_WidthLessOrEqualZero_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new BoardState(0, 5, new List<PackageState>(), new List<BlockerState>()));
            Assert.Throws<ArgumentException>(() => new BoardState(-1, 5, new List<PackageState>(), new List<BlockerState>()));
        }

        [Test]
        public void Invariants_HeightLessOrEqualZero_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => new BoardState(5, 0, new List<PackageState>(), new List<BlockerState>()));
            Assert.Throws<ArgumentException>(() => new BoardState(5, -1, new List<PackageState>(), new List<BlockerState>()));
        }

        [Test]
        public void Invariants_PackageOutsideBounds_ThrowsException()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(5, 5), PackageDirection.Up, PackageColor.Red) };
            Assert.Throws<ArgumentException>(() => new BoardState(5, 5, packages, new List<BlockerState>()));
        }

        [Test]
        public void Invariants_BlockerOutsideBounds_ThrowsException()
        {
            var blockers = new List<BlockerState> { new BlockerState(1, new GridPosition(-1, 2)) };
            Assert.Throws<ArgumentException>(() => new BoardState(5, 5, new List<PackageState>(), blockers));
        }

        [Test]
        public void Invariants_DuplicatePackageIds_ThrowsException()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(0, 0), PackageDirection.Up, PackageColor.Red),
                new PackageState(1, new GridPosition(1, 1), PackageDirection.Up, PackageColor.Blue) 
            };
            Assert.Throws<ArgumentException>(() => new BoardState(5, 5, packages, new List<BlockerState>()));
        }

        [Test]
        public void Invariants_DuplicatePositions_ThrowsException()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red),
                new PackageState(2, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Blue) 
            };
            Assert.Throws<ArgumentException>(() => new BoardState(5, 5, packages, new List<BlockerState>()));
        }

        [Test]
        public void Invariants_PackageBlockerOverlap_ThrowsException()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(3, 3), PackageDirection.Up, PackageColor.Red) };
            var blockers = new List<BlockerState> { new BlockerState(2, new GridPosition(3, 3)) };
            Assert.Throws<ArgumentException>(() => new BoardState(5, 5, packages, blockers));
        }
    }
}
