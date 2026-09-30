using NUnit.Framework;
using ParcelEscape.Core;
using System.Collections.Generic;

namespace ParcelEscape.Tests
{
    [TestFixture]
    public class DeterminismTests
    {
        private BoardState CreateInitialBoard()
        {
            var packages = new List<PackageState> 
            { 
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red),
                new PackageState(2, new GridPosition(3, 3), PackageDirection.Left, PackageColor.Blue)
            };
            var blockers = new List<BlockerState> { new BlockerState(1, new GridPosition(0, 0)) };
            return new BoardState(5, 5, packages, blockers);
        }

        [Test]
        public void Determinism_SameBoardAndCommand_SameValidationResult()
        {
            var board1 = CreateInitialBoard();
            var board2 = CreateInitialBoard();
            var command = new MoveCommand(1);

            var result1 = MoveValidator.Validate(board1, command);
            var result2 = MoveValidator.Validate(board2, command);

            Assert.AreEqual(result1.Status, result2.Status);
            Assert.AreEqual(result1.Reason, result2.Reason);
        }

        [Test]
        public void Determinism_SameBoardAndCommand_SameExecutionResult()
        {
            var board1 = CreateInitialBoard();
            var board2 = CreateInitialBoard();
            var command = new MoveCommand(1);

            var result1 = MoveExecutor.Execute(board1, command);
            var result2 = MoveExecutor.Execute(board2, command);

            Assert.AreEqual(result1.Status, result2.Status);
            Assert.AreEqual(result1.ValidationResult.Status, result2.ValidationResult.Status);
            Assert.AreEqual(result1.EscapedPackage?.Id, result2.EscapedPackage?.Id);
        }

        [Test]
        public void Determinism_SameBoardAndCommand_SameResultingBoardState()
        {
            var board1 = CreateInitialBoard();
            var board2 = CreateInitialBoard();
            var command = new MoveCommand(1);

            var newBoard1 = MoveExecutor.Execute(board1, command).ResultingBoard;
            var newBoard2 = MoveExecutor.Execute(board2, command).ResultingBoard;

            Assert.AreEqual(newBoard1.Width, newBoard2.Width);
            Assert.AreEqual(newBoard1.Height, newBoard2.Height);
            Assert.AreEqual(newBoard1.Packages.Count, newBoard2.Packages.Count);
            Assert.AreEqual(newBoard1.Blockers.Count, newBoard2.Blockers.Count);
            
            for (int i = 0; i < newBoard1.Packages.Count; i++)
            {
                Assert.AreEqual(newBoard1.Packages[i].Id, newBoard2.Packages[i].Id);
                Assert.AreEqual(newBoard1.Packages[i].Position, newBoard2.Packages[i].Position);
            }
        }
        
        [Test]
        public void Determinism_MultipleRuns_AllIdentical()
        {
            var command = new MoveCommand(1);
            var initialBoard = CreateInitialBoard();
            var referenceResult = MoveExecutor.Execute(initialBoard, command);
            
            for (int i = 0; i < 10; i++)
            {
                var newBoard = CreateInitialBoard();
                var result = MoveExecutor.Execute(newBoard, command);
                
                Assert.AreEqual(referenceResult.Status, result.Status);
                Assert.AreEqual(referenceResult.ResultingBoard.Packages.Count, result.ResultingBoard.Packages.Count);
            }
        }
    }
}
