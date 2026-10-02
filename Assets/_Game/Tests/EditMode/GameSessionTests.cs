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
        public void TryMovePackage_MatchingEscapeLoadsActiveTruckAndCommitsBoard()
        {
            var session = CreateSession(
                CreateBoard(Package(1, 2, 4, PackageDirection.Up, PackageColor.Blue)),
                CreateDelivery(
                    3,
                    Truck(PackageColor.Blue, 2),
                    Truck(PackageColor.Red, 1)));

            var accepted = session.TryMovePackage(1, out var result);

            Assert.IsTrue(accepted);
            Assert.AreEqual(GameSessionMoveStatus.Committed, result.Status);
            Assert.IsTrue(result.WasCommitted);
            Assert.AreEqual(MoveExecutionStatus.Success, result.BoardMoveResult.Status);
            Assert.AreEqual(DeliveryRoutingStatus.LoadedActiveTruck, result.DeliveryRoutingResult.Status);
            Assert.AreEqual(TruckPromotionStatus.ActiveTruckIncomplete, result.TruckPromotionResult.Status);
            Assert.AreEqual(1, result.EscapedPackage.Value.Id);
            Assert.AreSame(result.ResultingBoard, session.CurrentBoard);
            Assert.AreSame(result.ResultingDeliveryState, session.CurrentDeliveryState);
            Assert.AreEqual(0, session.CurrentBoard.Packages.Count);
            AssertPackageIds(session.CurrentDeliveryState.ActiveTruck.LoadedPackages, 1);
            Assert.AreEqual(InteractionState.ResolvingMove, session.State);
        }

        [Test]
        public void TryMovePackage_NonMatchingEscapeEntersHolding()
        {
            var session = CreateSession(
                CreateBoard(Package(1, 0, 2, PackageDirection.Left, PackageColor.Red)),
                CreateDelivery(
                    3,
                    Truck(PackageColor.Blue, 2),
                    Truck(PackageColor.Red, 2)));

            session.TryMovePackage(1, out var result);

            Assert.AreEqual(GameSessionMoveStatus.Committed, result.Status);
            Assert.AreEqual(DeliveryRoutingStatus.AddedToHolding, result.DeliveryRoutingResult.Status);
            AssertPackageIds(session.CurrentDeliveryState.HoldingQueue.Packages, 1);
            Assert.AreEqual(0, session.CurrentDeliveryState.ActiveTruck.LoadCount);
            Assert.AreEqual(0, session.CurrentBoard.Packages.Count);
        }

        [Test]
        public void TryMovePackage_BlockedMoveDoesNotMutateDeliveryState()
        {
            var initialBoard = CreateBoard(
                Package(1, 2, 2, PackageDirection.Up, PackageColor.Red),
                Package(2, 2, 3, PackageDirection.Right, PackageColor.Blue));
            var initialDelivery = CreateDelivery(
                3,
                Truck(PackageColor.Red, 2),
                Truck(PackageColor.Blue, 2));
            var session = CreateSession(initialBoard, initialDelivery);

            session.TryMovePackage(1, out var result);

            Assert.AreEqual(GameSessionMoveStatus.BoardMoveRejected, result.Status);
            Assert.AreEqual(MoveExecutionStatus.ValidationFailed, result.BoardMoveResult.Status);
            Assert.AreEqual(MoveValidationStatus.Blocked, result.BoardMoveResult.ValidationResult.Status);
            Assert.IsNull(result.DeliveryRoutingResult);
            Assert.IsNull(result.TruckPromotionResult);
            Assert.IsNull(result.EscapedPackage);
            Assert.AreSame(initialBoard, session.CurrentBoard);
            Assert.AreSame(initialDelivery, session.CurrentDeliveryState);
            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        [Test]
        public void TryMovePackage_InvalidPackageIdDoesNotMutateDeliveryState()
        {
            var initialBoard = CreateBoard(
                Package(1, 2, 4, PackageDirection.Up, PackageColor.Green));
            var initialDelivery = CreateDelivery(
                3,
                Truck(PackageColor.Green, 2),
                Truck(PackageColor.Red, 1));
            var session = CreateSession(initialBoard, initialDelivery);

            session.TryMovePackage(999, out var result);

            Assert.AreEqual(GameSessionMoveStatus.BoardMoveRejected, result.Status);
            Assert.AreEqual(MoveValidationStatus.PackageNotFound, result.BoardMoveResult.ValidationResult.Status);
            Assert.IsNull(result.DeliveryRoutingResult);
            Assert.AreSame(initialBoard, session.CurrentBoard);
            Assert.AreSame(initialDelivery, session.CurrentDeliveryState);
            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        [Test]
        public void TryMovePackage_ActiveCompletionPromotesNextTruck()
        {
            var session = CreateSession(
                CreateBoard(Package(1, 2, 4, PackageDirection.Up, PackageColor.Blue)),
                CreateDelivery(
                    3,
                    Truck(PackageColor.Blue, 1),
                    Truck(PackageColor.Red, 2),
                    Truck(PackageColor.Green, 1)));

            session.TryMovePackage(1, out var result);

            Assert.AreEqual(GameSessionMoveStatus.Committed, result.Status);
            Assert.AreEqual(TruckPromotionStatus.Promoted, result.TruckPromotionResult.Status);
            Assert.AreEqual(1, result.TruckPromotionResult.CompletedTrucks.Count);
            AssertTruck(result.TruckPromotionResult.CompletedTrucks[0], PackageColor.Blue, 1, 1);
            AssertTruck(session.CurrentDeliveryState.ActiveTruck, PackageColor.Red, 2, 0);
            AssertTruck(session.CurrentDeliveryState.NextTruck, PackageColor.Green, 1, 0);
        }

        [Test]
        public void TryMovePackage_PromotionAutoLoadsHoldingThroughSession()
        {
            var session = CreateAutoLoadSession();

            session.TryMovePackage(1, out var heldResult);
            Assert.AreEqual(DeliveryRoutingStatus.AddedToHolding, heldResult.DeliveryRoutingResult.Status);
            session.CompleteMovePresentation();

            session.TryMovePackage(2, out var completionResult);

            Assert.AreEqual(GameSessionMoveStatus.Committed, completionResult.Status);
            Assert.AreEqual(TruckPromotionStatus.Promoted, completionResult.TruckPromotionResult.Status);
            AssertPackageIds(completionResult.TruckPromotionResult.AutoLoadedPackages, 1);
            AssertTruck(session.CurrentDeliveryState.ActiveTruck, PackageColor.Red, 2, 1);
            Assert.AreEqual(0, session.CurrentDeliveryState.HoldingQueue.Count);
        }

        [Test]
        public void TryMovePackage_HoldingFullRejectsWholeTransitionAtomically()
        {
            var initialBoard = CreateBoard(
                Package(1, 0, 2, PackageDirection.Left, PackageColor.Red));
            var heldPackage = Package(99, 0, 0, PackageDirection.Up, PackageColor.Green);
            var initialDelivery = new DeliveryState(
                Truck(PackageColor.Blue, 2),
                Truck(PackageColor.Green, 1),
                new HoldingQueueState(1, new[] { heldPackage }));
            var session = CreateSession(initialBoard, initialDelivery);

            session.TryMovePackage(1, out var result);

            Assert.AreEqual(GameSessionMoveStatus.DeliveryRejected, result.Status);
            Assert.AreEqual(MoveExecutionStatus.Success, result.BoardMoveResult.Status);
            Assert.AreEqual(DeliveryRoutingStatus.HoldingFull, result.DeliveryRoutingResult.Status);
            Assert.IsNull(result.TruckPromotionResult);
            Assert.IsNull(result.EscapedPackage);
            Assert.AreSame(initialBoard, session.CurrentBoard);
            Assert.AreSame(initialDelivery, session.CurrentDeliveryState);
            Assert.IsTrue(session.CurrentBoard.TryGetPackageById(1, out _));
            AssertPackageIds(session.CurrentDeliveryState.HoldingQueue.Packages, 99);
            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        [Test]
        public void TryMovePackage_RepeatedRequestCannotRoutePackageTwice()
        {
            var session = CreateSession(
                CreateBoard(Package(1, 2, 4, PackageDirection.Up, PackageColor.Blue)),
                CreateDelivery(
                    3,
                    Truck(PackageColor.Blue, 2),
                    Truck(PackageColor.Red, 1)));

            Assert.IsTrue(session.TryMovePackage(1, out var firstResult));
            var boardAfterFirstMove = session.CurrentBoard;
            var deliveryAfterFirstMove = session.CurrentDeliveryState;

            Assert.IsFalse(session.TryMovePackage(1, out var lockedResult));
            Assert.IsNull(lockedResult);
            Assert.AreSame(boardAfterFirstMove, session.CurrentBoard);
            Assert.AreSame(deliveryAfterFirstMove, session.CurrentDeliveryState);

            session.CompleteMovePresentation();
            Assert.IsTrue(session.TryMovePackage(1, out var repeatedResult));

            Assert.AreEqual(GameSessionMoveStatus.BoardMoveRejected, repeatedResult.Status);
            Assert.AreEqual(MoveValidationStatus.PackageNotFound, repeatedResult.BoardMoveResult.ValidationResult.Status);
            Assert.AreSame(deliveryAfterFirstMove, session.CurrentDeliveryState);
            AssertPackageIds(session.CurrentDeliveryState.ActiveTruck.LoadedPackages, 1);
            Assert.AreEqual(MoveExecutionStatus.Success, firstResult.BoardMoveResult.Status);
        }

        [Test]
        public void Restart_RestoresOriginalBoardAndDeliveryState()
        {
            var session = CreateAutoLoadSession();
            var initialBoard = session.CurrentBoard;
            var initialDelivery = session.CurrentDeliveryState;

            session.TryMovePackage(1, out _);
            session.CompleteMovePresentation();
            session.TryMovePackage(2, out _);

            session.Restart();

            Assert.AreSame(initialBoard, session.CurrentBoard);
            Assert.AreSame(initialDelivery, session.CurrentDeliveryState);
            Assert.AreEqual(2, session.CurrentBoard.Packages.Count);
            AssertTruck(session.CurrentDeliveryState.ActiveTruck, PackageColor.Blue, 1, 0);
            AssertTruck(session.CurrentDeliveryState.NextTruck, PackageColor.Red, 2, 0);
            Assert.AreEqual(0, session.CurrentDeliveryState.HoldingQueue.Count);
            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        [Test]
        public void TryMovePackage_ConservesPackagesAcrossBoardAndDeliveryTransition()
        {
            var session = CreateSession(
                CreateBoard(
                    Package(1, 2, 4, PackageDirection.Up, PackageColor.Blue),
                    Package(2, 0, 0, PackageDirection.Left, PackageColor.Red)),
                CreateDelivery(
                    3,
                    Truck(PackageColor.Blue, 1),
                    Truck(PackageColor.Red, 2)));

            session.TryMovePackage(1, out var result);

            Assert.AreEqual(2, CountPackages(session, result.TruckPromotionResult));
            Assert.AreEqual(1, CountPackageOccurrences(session, result.TruckPromotionResult, 1));
            Assert.AreEqual(1, CountPackageOccurrences(session, result.TruckPromotionResult, 2));
        }

        [Test]
        public void TryMovePackage_IdenticalSessionsProduceIdenticalResults()
        {
            var first = CreateAutoLoadSession();
            var second = CreateAutoLoadSession();

            first.TryMovePackage(1, out _);
            second.TryMovePackage(1, out _);
            first.CompleteMovePresentation();
            second.CompleteMovePresentation();

            first.TryMovePackage(2, out var firstResult);
            second.TryMovePackage(2, out var secondResult);

            AssertSessionResultsEqual(firstResult, secondResult);
            AssertBoardsEqual(first.CurrentBoard, second.CurrentBoard);
            AssertDeliveryStatesEqual(first.CurrentDeliveryState, second.CurrentDeliveryState);
        }

        [Test]
        public void CompleteMovePresentation_AfterCommittedMoveReturnsSessionToReady()
        {
            var session = CreateSession(
                CreateBoard(Package(1, 2, 4, PackageDirection.Up, PackageColor.Green)),
                CreateDelivery(
                    2,
                    Truck(PackageColor.Green, 2),
                    Truck(PackageColor.Red, 1)));
            session.TryMovePackage(1, out _);

            session.CompleteMovePresentation();

            Assert.AreEqual(InteractionState.Ready, session.State);
        }

        private static GameSession CreateAutoLoadSession()
        {
            return CreateSession(
                CreateBoard(
                    Package(1, 0, 1, PackageDirection.Left, PackageColor.Red),
                    Package(2, 4, 3, PackageDirection.Right, PackageColor.Blue)),
                CreateDelivery(
                    3,
                    Truck(PackageColor.Blue, 1),
                    Truck(PackageColor.Red, 2),
                    Truck(PackageColor.Green, 1)));
        }

        private static GameSession CreateSession(
            BoardState board,
            DeliveryState deliveryState)
        {
            return new GameSession(board, deliveryState);
        }

        private static BoardState CreateBoard(params PackageState[] packages)
        {
            return new BoardState(
                5,
                5,
                new List<PackageState>(packages),
                new List<BlockerState>());
        }

        private static DeliveryState CreateDelivery(
            int holdingCapacity,
            params TruckState[] trucks)
        {
            return DeliveryState.CreateInitial(
                new TruckSequenceState(trucks),
                new HoldingQueueState(holdingCapacity));
        }

        private static TruckState Truck(PackageColor color, int capacity)
        {
            return new TruckState(color, capacity);
        }

        private static PackageState Package(
            int id,
            int x,
            int y,
            PackageDirection direction,
            PackageColor color)
        {
            return new PackageState(id, new GridPosition(x, y), direction, color);
        }

        private static int CountPackages(
            GameSession session,
            TruckPromotionResult promotionResult)
        {
            var count = session.CurrentBoard.Packages.Count;
            count += session.CurrentDeliveryState.ActiveTruck.LoadCount;
            count += session.CurrentDeliveryState.HoldingQueue.Count;
            if (session.CurrentDeliveryState.HasNextTruck)
            {
                count += session.CurrentDeliveryState.NextTruck.LoadCount;
            }

            foreach (var truck in session.CurrentDeliveryState.RemainingTruckSequence.Trucks)
            {
                count += truck.LoadCount;
            }

            foreach (var truck in promotionResult.CompletedTrucks)
            {
                count += truck.LoadCount;
            }

            return count;
        }

        private static int CountPackageOccurrences(
            GameSession session,
            TruckPromotionResult promotionResult,
            int packageId)
        {
            var count = CountPackageOccurrences(session.CurrentBoard.Packages, packageId);
            count += CountPackageOccurrences(
                session.CurrentDeliveryState.ActiveTruck.LoadedPackages,
                packageId);
            count += CountPackageOccurrences(
                session.CurrentDeliveryState.HoldingQueue.Packages,
                packageId);
            if (session.CurrentDeliveryState.HasNextTruck)
            {
                count += CountPackageOccurrences(
                    session.CurrentDeliveryState.NextTruck.LoadedPackages,
                    packageId);
            }

            foreach (var truck in session.CurrentDeliveryState.RemainingTruckSequence.Trucks)
            {
                count += CountPackageOccurrences(truck.LoadedPackages, packageId);
            }

            foreach (var truck in promotionResult.CompletedTrucks)
            {
                count += CountPackageOccurrences(truck.LoadedPackages, packageId);
            }

            return count;
        }

        private static int CountPackageOccurrences(
            IReadOnlyList<PackageState> packages,
            int packageId)
        {
            var count = 0;
            foreach (var package in packages)
            {
                if (package.Id == packageId)
                {
                    count++;
                }
            }

            return count;
        }

        private static void AssertSessionResultsEqual(
            GameSessionMoveResult expected,
            GameSessionMoveResult actual)
        {
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.WasCommitted, actual.WasCommitted);
            Assert.AreEqual(expected.BoardMoveResult.Status, actual.BoardMoveResult.Status);
            Assert.AreEqual(
                expected.DeliveryRoutingResult.Status,
                actual.DeliveryRoutingResult.Status);
            Assert.AreEqual(
                expected.TruckPromotionResult.Status,
                actual.TruckPromotionResult.Status);
            AssertPackageListsEqual(
                expected.TruckPromotionResult.AutoLoadedPackages,
                actual.TruckPromotionResult.AutoLoadedPackages);
            Assert.AreEqual(
                expected.TruckPromotionResult.CompletedTrucks.Count,
                actual.TruckPromotionResult.CompletedTrucks.Count);
            for (var index = 0;
                 index < expected.TruckPromotionResult.CompletedTrucks.Count;
                 index++)
            {
                AssertTrucksEqual(
                    expected.TruckPromotionResult.CompletedTrucks[index],
                    actual.TruckPromotionResult.CompletedTrucks[index]);
            }

            AssertBoardsEqual(expected.ResultingBoard, actual.ResultingBoard);
            AssertDeliveryStatesEqual(
                expected.ResultingDeliveryState,
                actual.ResultingDeliveryState);
        }

        private static void AssertBoardsEqual(BoardState expected, BoardState actual)
        {
            Assert.AreEqual(expected.Width, actual.Width);
            Assert.AreEqual(expected.Height, actual.Height);
            AssertPackageListsEqual(expected.Packages, actual.Packages);
            Assert.AreEqual(expected.Blockers.Count, actual.Blockers.Count);
            for (var index = 0; index < expected.Blockers.Count; index++)
            {
                Assert.AreEqual(expected.Blockers[index].Id, actual.Blockers[index].Id);
                Assert.AreEqual(expected.Blockers[index].Position, actual.Blockers[index].Position);
            }
        }

        private static void AssertDeliveryStatesEqual(
            DeliveryState expected,
            DeliveryState actual)
        {
            AssertTrucksEqual(expected.ActiveTruck, actual.ActiveTruck);
            Assert.AreEqual(expected.HasNextTruck, actual.HasNextTruck);
            if (expected.HasNextTruck)
            {
                AssertTrucksEqual(expected.NextTruck, actual.NextTruck);
            }

            Assert.AreEqual(
                expected.RemainingTruckSequence.Count,
                actual.RemainingTruckSequence.Count);
            for (var index = 0; index < expected.RemainingTruckSequence.Count; index++)
            {
                AssertTrucksEqual(
                    expected.RemainingTruckSequence.Trucks[index],
                    actual.RemainingTruckSequence.Trucks[index]);
            }

            Assert.AreEqual(expected.HoldingQueue.Capacity, actual.HoldingQueue.Capacity);
            AssertPackageListsEqual(expected.HoldingQueue.Packages, actual.HoldingQueue.Packages);
        }

        private static void AssertTruck(
            TruckState truck,
            PackageColor color,
            int capacity,
            int loadCount)
        {
            Assert.IsNotNull(truck);
            Assert.AreEqual(color, truck.RequiredColor);
            Assert.AreEqual(capacity, truck.Capacity);
            Assert.AreEqual(loadCount, truck.LoadCount);
        }

        private static void AssertTrucksEqual(TruckState expected, TruckState actual)
        {
            AssertTruck(
                actual,
                expected.RequiredColor,
                expected.Capacity,
                expected.LoadCount);
            AssertPackageListsEqual(expected.LoadedPackages, actual.LoadedPackages);
        }

        private static void AssertPackageListsEqual(
            IReadOnlyList<PackageState> expected,
            IReadOnlyList<PackageState> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for (var index = 0; index < expected.Count; index++)
            {
                Assert.AreEqual(expected[index].Id, actual[index].Id);
                Assert.AreEqual(expected[index].Position, actual[index].Position);
                Assert.AreEqual(expected[index].Direction, actual[index].Direction);
                Assert.AreEqual(expected[index].Color, actual[index].Color);
            }
        }

        private static void AssertPackageIds(
            IReadOnlyList<PackageState> packages,
            params int[] expectedIds)
        {
            Assert.AreEqual(expectedIds.Length, packages.Count);
            for (var index = 0; index < expectedIds.Length; index++)
            {
                Assert.AreEqual(expectedIds[index], packages[index].Id);
            }
        }
    }
}
