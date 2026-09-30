using NUnit.Framework;
using ParcelEscape.Core;
using System.Collections.Generic;

namespace ParcelEscape.Tests
{
    [TestFixture]
    public class EscapeTests
    {
        [Test]
        public void EscapeUp_ClearPath_IsValid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
            var result = MoveValidator.Validate(board, new MoveCommand(1));
            Assert.AreEqual(MoveValidationStatus.Valid, result.Status);
        }

        [Test]
        public void EscapeUp_BlockedByPackage_IsInvalid()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red),
                new PackageState(2, new GridPosition(2, 3), PackageDirection.Right, PackageColor.Blue)
            };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsFalse(board.IsPathClear(1));
            var result = MoveValidator.Validate(board, new MoveCommand(1));
            Assert.AreEqual(MoveValidationStatus.Blocked, result.Status);
        }

        [Test]
        public void EscapeUp_BlockedByBlocker_IsInvalid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red) };
            var blockers = new List<BlockerState> { new BlockerState(1, new GridPosition(2, 4)) };
            var board = new BoardState(5, 5, packages, blockers);
            Assert.IsFalse(board.IsPathClear(1));
        }

        [Test]
        public void EscapeDown_ClearPath_IsValid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Down, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void EscapeDown_BlockedByPackage_IsInvalid()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Down, PackageColor.Red),
                new PackageState(2, new GridPosition(2, 1), PackageDirection.Right, PackageColor.Blue)
            };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsFalse(board.IsPathClear(1));
        }

        [Test]
        public void EscapeDown_BlockedByBlocker_IsInvalid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Down, PackageColor.Red) };
            var blockers = new List<BlockerState> { new BlockerState(1, new GridPosition(2, 0)) };
            var board = new BoardState(5, 5, packages, blockers);
            Assert.IsFalse(board.IsPathClear(1));
        }

        [Test]
        public void EscapeLeft_ClearPath_IsValid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Left, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void EscapeLeft_BlockedByPackage_IsInvalid()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Left, PackageColor.Red),
                new PackageState(2, new GridPosition(1, 2), PackageDirection.Up, PackageColor.Blue)
            };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsFalse(board.IsPathClear(1));
        }

        [Test]
        public void EscapeLeft_BlockedByBlocker_IsInvalid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Left, PackageColor.Red) };
            var blockers = new List<BlockerState> { new BlockerState(1, new GridPosition(0, 2)) };
            var board = new BoardState(5, 5, packages, blockers);
            Assert.IsFalse(board.IsPathClear(1));
        }

        [Test]
        public void EscapeRight_ClearPath_IsValid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Right, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void EscapeRight_BlockedByPackage_IsInvalid()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Right, PackageColor.Red),
                new PackageState(2, new GridPosition(3, 2), PackageDirection.Up, PackageColor.Blue)
            };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsFalse(board.IsPathClear(1));
        }

        [Test]
        public void EscapeRight_BlockedByBlocker_IsInvalid()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 2), PackageDirection.Right, PackageColor.Red) };
            var blockers = new List<BlockerState> { new BlockerState(1, new GridPosition(4, 2)) };
            var board = new BoardState(5, 5, packages, blockers);
            Assert.IsFalse(board.IsPathClear(1));
        }

        [Test]
        public void EdgePackage_TopEdgeFacingUp_IsValidEscape()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 4), PackageDirection.Up, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void EdgePackage_BottomEdgeFacingDown_IsValidEscape()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(2, 0), PackageDirection.Down, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void EdgePackage_LeftEdgeFacingLeft_IsValidEscape()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(0, 2), PackageDirection.Left, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void EdgePackage_RightEdgeFacingRight_IsValidEscape()
        {
            var packages = new List<PackageState> { new PackageState(1, new GridPosition(4, 2), PackageDirection.Right, PackageColor.Red) };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void Obstruction_BehindPackage_DoesNotBlock()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red),
                new PackageState(2, new GridPosition(2, 1), PackageDirection.Left, PackageColor.Blue)
            };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }

        [Test]
        public void Obstruction_SameRowButOutsideRay_DoesNotBlock()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Right, PackageColor.Red),
                new PackageState(2, new GridPosition(1, 2), PackageDirection.Left, PackageColor.Blue)
            };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            Assert.IsTrue(board.IsPathClear(1));
        }
    }
}
