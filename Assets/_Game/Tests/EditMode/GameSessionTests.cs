using System.Collections.Generic;
using NUnit.Framework;
using ParcelEscape.Core;
using ParcelEscape.Gameplay;

namespace ParcelEscape.Tests
{
    [TestFixture]
    public class GameSessionTests
    {
        [Test]
        public void TryMovePackage_ValidMove_UpdatesBoardAndBeginsPresentation()
        {
            var initialBoard = CreateBoard(
                new PackageState(1, new GridPosition(2, 3), PackageDirection.Up, PackageColor.Blue));
            var session = new GameSession(initialBoard);

            var accepted = session.TryMovePackage(1, out var result);

            Assert.IsTrue(accepted);
            Assert.IsNotNull(result);
            Assert.AreEqual(MoveExecutionStatus.Success, result.Status);
            Assert.AreEqual(1, result.EscapedPackage.Value.Id);
            Assert.AreSame(result.ResultingBoard, session.CurrentBoard);
            Assert.AreEqual(0, session.CurrentBoard.Packages.Count);
            Assert.AreEqual(InteractionState.ResolvingMove, session.State);
        }

        [Test]
        public void TryMovePackage_BlockedMove_LeavesBoardAndSessionReady()
        {
            var initialBoard = CreateBoard(
                new PackageState(1, new GridPosition(2, 2), PackageDirection.Up, PackageColor.Red),
                new PackageState(2, new GridPosition(2, 3), PackageDirection.Right, PackageColor.Blue));
            var session = new GameSession(initialBoard);

            var accepted = session.TryMovePackage(1, out var result);

            Assert.IsTrue(accepted);
            Assert.IsNotNull(result);
            Assert.AreEqual(MoveExecutionStatus.ValidationFailed, result.Status);
            Assert.AreEqual(MoveValidationStatus.Blocked, result.ValidationResult.Status);
            Assert.AreSame(initialBoard, session.CurrentBoard);
            Assert.AreEqual(2, session.CurrentBoard.Packages.Count);
            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        [Test]
        public void TryMovePackage_WhileResolving_RejectsDuplicateWithoutAnotherMutationOrResult()
        {
            var initialBoard = CreateBoard(
                new PackageState(1, new GridPosition(0, 2), PackageDirection.Left, PackageColor.Red),
                new PackageState(2, new GridPosition(4, 2), PackageDirection.Right, PackageColor.Blue));
            var session = new GameSession(initialBoard);

            Assert.IsTrue(session.TryMovePackage(1, out var firstResult));
            var boardAfterFirstMove = session.CurrentBoard;

            var accepted = session.TryMovePackage(2, out var duplicateResult);

            Assert.IsFalse(accepted);
            Assert.IsNull(duplicateResult);
            Assert.AreSame(boardAfterFirstMove, session.CurrentBoard);
            Assert.AreEqual(1, session.CurrentBoard.Packages.Count);
            Assert.IsFalse(session.CurrentBoard.TryGetPackageById(1, out _));
            Assert.IsTrue(session.CurrentBoard.TryGetPackageById(2, out _));
            Assert.AreEqual(InteractionState.ResolvingMove, session.State);
            Assert.AreEqual(MoveExecutionStatus.Success, firstResult.Status);
        }

        [Test]
        public void CompleteMovePresentation_AfterSuccessfulMove_ReturnsSessionToReady()
        {
            var session = new GameSession(CreateBoard(
                new PackageState(1, new GridPosition(2, 4), PackageDirection.Up, PackageColor.Green)));
            session.TryMovePackage(1, out _);

            session.CompleteMovePresentation();

            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        [Test]
        public void TryMovePackage_PackageNotFound_LeavesBoardAndSessionReady()
        {
            var initialBoard = CreateBoard(
                new PackageState(1, new GridPosition(2, 4), PackageDirection.Up, PackageColor.Green));
            var session = new GameSession(initialBoard);

            var accepted = session.TryMovePackage(999, out var result);

            Assert.IsTrue(accepted);
            Assert.AreEqual(MoveExecutionStatus.ValidationFailed, result.Status);
            Assert.AreEqual(MoveValidationStatus.PackageNotFound, result.ValidationResult.Status);
            Assert.AreSame(initialBoard, session.CurrentBoard);
            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        [Test]
        public void Restart_AfterSuccessfulMove_RestoresInitialBoardAndReadyState()
        {
            var initialBoard = CreateBoard(
                new PackageState(1, new GridPosition(2, 4), PackageDirection.Up, PackageColor.Yellow));
            var session = new GameSession(initialBoard);
            session.TryMovePackage(1, out _);

            session.Restart();

            Assert.AreSame(initialBoard, session.CurrentBoard);
            Assert.AreEqual(1, session.CurrentBoard.Packages.Count);
            Assert.IsTrue(session.CurrentBoard.TryGetPackageById(1, out _));
            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        private static BoardState CreateBoard(params PackageState[] packages)
        {
            return new BoardState(
                5,
                5,
                new List<PackageState>(packages),
                new List<BlockerState>());
        }
    }
}
