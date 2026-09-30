using NUnit.Framework;
using ParcelEscape.Core;
using System.Collections.Generic;

namespace ParcelEscape.Tests
{
    [TestFixture]
    public class MoveExecutionTests
    {
        [Test]
        public void Execution_InvalidPackageId_ReturnsPackageNotFound()
        {
            var board = new BoardState(5, 5, new List<PackageState>(), new List<BlockerState>());
            var command = new MoveCommand(999);
            
            var result = MoveExecutor.Execute(board, command);
            
            Assert.AreEqual(MoveExecutionStatus.ValidationFailed, result.Status);
            Assert.AreEqual(MoveValidationStatus.PackageNotFound, result.ValidationResult.Status);
            Assert.IsNull(result.EscapedPackage);
            Assert.AreSame(board, result.ResultingBoard);
        }

        [Test]
        public void Execution_BlockedMove_ReturnsBlockedAndUnchangedBoard()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red),
                new PackageState(2, new GridPosition(2, 3), PackageDirection.Right, PackageColor.Blue)
            };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            var command = new MoveCommand(1);

            var result = MoveExecutor.Execute(board, command);
            
            Assert.AreEqual(MoveExecutionStatus.ValidationFailed, result.Status);
            Assert.AreEqual(MoveValidationStatus.Blocked, result.ValidationResult.Status);
            Assert.IsNull(result.EscapedPackage);
            Assert.AreSame(board, result.ResultingBoard);
            Assert.AreEqual(2, board.Packages.Count);
        }

        [Test]
        public void Execution_ValidMove_RemovesPackageAndDecrementsCount()
        {
            var packageToEscape = new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red);
            var packages = new List<PackageState> { packageToEscape };
            var board = new BoardState(5, 5, packages, new List<BlockerState>());
            var command = new MoveCommand(1);

            var result = MoveExecutor.Execute(board, command);
            
            Assert.AreEqual(MoveExecutionStatus.Success, result.Status);
            Assert.AreEqual(MoveValidationStatus.Valid, result.ValidationResult.Status);
            
            Assert.IsNotNull(result.EscapedPackage);
            Assert.AreEqual(packageToEscape.Id, result.EscapedPackage.Value.Id);
            
            Assert.AreNotSame(board, result.ResultingBoard);
            Assert.AreEqual(0, result.ResultingBoard.Packages.Count);
            Assert.AreEqual(1, board.Packages.Count); // Original board unchanged
        }
    }
}
