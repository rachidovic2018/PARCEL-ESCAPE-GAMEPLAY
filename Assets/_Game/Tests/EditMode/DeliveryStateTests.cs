using System.Collections.Generic;
using NUnit.Framework;
using ParcelEscape.Core;

namespace ParcelEscape.Tests
{
    [TestFixture]
    public class DeliveryStateTests
    {
        [Test]
        public void Route_MatchingEscapedPackage_LoadsActiveTruck()
        {
            var state = CreateDeliveryState(PackageColor.Blue, 2, PackageColor.Red, 3);
            var escapedPackage = Package(1, PackageColor.Blue);
            var board = new BoardState(
                5,
                5,
                new List<PackageState> { escapedPackage },
                new List<BlockerState>());
            var moveResult = MoveExecutor.Execute(board, new MoveCommand(escapedPackage.Id));

            var result = state.RouteEscapedPackage(moveResult);

            Assert.AreEqual(DeliveryRoutingStatus.LoadedActiveTruck, result.Status);
            Assert.IsTrue(result.WasAccepted);
            Assert.AreEqual(1, result.ResultingState.ActiveTruck.LoadCount);
            Assert.AreEqual(escapedPackage.Id, result.ResultingState.ActiveTruck.LoadedPackages[0].Id);
            Assert.AreEqual(0, result.ResultingState.HoldingQueue.Count);
        }

        [Test]
        public void Route_NonMatchingPackage_AddsToHolding()
        {
            var state = CreateDeliveryState(PackageColor.Blue, 2, PackageColor.Red, 3);
            var package = Package(1, PackageColor.Yellow);

            var result = state.RouteEscapedPackage(package);

            Assert.AreEqual(DeliveryRoutingStatus.AddedToHolding, result.Status);
            Assert.IsTrue(result.WasAccepted);
            Assert.AreEqual(0, result.ResultingState.ActiveTruck.LoadCount);
            Assert.AreEqual(package.Id, result.ResultingState.HoldingQueue.Packages[0].Id);
        }

        [Test]
        public void Route_MultipleHeldPackages_PreservesFifoOrder()
        {
            var initial = CreateDeliveryState(PackageColor.Blue, 2, PackageColor.Red, 3);

            var afterFirst = initial.RouteEscapedPackage(Package(10, PackageColor.Red)).ResultingState;
            var afterSecond = afterFirst.RouteEscapedPackage(Package(20, PackageColor.Green)).ResultingState;

            Assert.AreEqual(2, afterSecond.HoldingQueue.Count);
            Assert.AreEqual(10, afterSecond.HoldingQueue.Packages[0].Id);
            Assert.AreEqual(20, afterSecond.HoldingQueue.Packages[1].Id);
        }

        [Test]
        public void Route_WhenHoldingIsFull_RejectsAndReturnsUnroutedPackage()
        {
            var heldPackage = Package(1, PackageColor.Red);
            var state = new DeliveryState(
                new TruckState(PackageColor.Blue, 2),
                new TruckState(PackageColor.Green, 2),
                new HoldingQueueState(1, new List<PackageState> { heldPackage }));
            var package = Package(2, PackageColor.Yellow);

            var result = state.RouteEscapedPackage(package);

            Assert.AreEqual(DeliveryRoutingStatus.HoldingFull, result.Status);
            Assert.IsFalse(result.WasAccepted);
            Assert.AreEqual(package.Id, result.UnroutedPackage.Value.Id);
            Assert.AreSame(state, result.ResultingState);
            Assert.AreEqual(1, state.HoldingQueue.Count);
        }

        [Test]
        public void Route_WhenActiveTruckFills_LeavesCompletionForExplicitPromotion()
        {
            var state = new DeliveryState(
                new TruckState(PackageColor.Blue, 1),
                new TruckState(PackageColor.Green, 2),
                new HoldingQueueState(2));

            var completed = state.RouteEscapedPackage(Package(1, PackageColor.Blue));
            var overflow = completed.ResultingState.RouteEscapedPackage(Package(2, PackageColor.Blue));

            Assert.AreEqual(DeliveryRoutingStatus.LoadedActiveTruck, completed.Status);
            Assert.IsTrue(completed.ResultingState.ActiveTruck.IsComplete);
            Assert.AreEqual(1, completed.ResultingState.ActiveTruck.LoadCount);
            Assert.AreSame(state.NextTruck, completed.ResultingState.NextTruck);

            Assert.AreEqual(DeliveryRoutingStatus.AddedToHolding, overflow.Status);
            Assert.AreEqual(1, overflow.ResultingState.ActiveTruck.LoadCount);
            Assert.IsTrue(overflow.ResultingState.ActiveTruck.IsComplete);
            Assert.AreEqual(2, overflow.ResultingState.HoldingQueue.Packages[0].Id);
            Assert.AreSame(state.NextTruck, overflow.ResultingState.NextTruck);
        }

        [Test]
        public void Route_WhileActiveTruckIncomplete_LeavesNextTruckUnchanged()
        {
            var state = CreateDeliveryState(PackageColor.Blue, 2, PackageColor.Red, 3);
            var nextTruck = state.NextTruck;

            var result = state.RouteEscapedPackage(Package(1, PackageColor.Blue));

            Assert.AreSame(nextTruck, result.ResultingState.NextTruck);
            Assert.AreEqual(PackageColor.Red, result.ResultingState.NextTruck.RequiredColor);
            Assert.AreEqual(0, result.ResultingState.NextTruck.LoadCount);
        }

        [Test]
        public void Route_AcceptedOrRejected_AccountsForPackageExactlyOnce()
        {
            var acceptedState = CreateDeliveryState(PackageColor.Blue, 1, PackageColor.Red, 1);
            var accepted = acceptedState.RouteEscapedPackage(Package(1, PackageColor.Blue));

            var fullState = new DeliveryState(
                accepted.ResultingState.ActiveTruck,
                accepted.ResultingState.NextTruck,
                new HoldingQueueState(1, new List<PackageState> { Package(2, PackageColor.Red) }));
            var escapedPackage = Package(3, PackageColor.Green);
            var board = new BoardState(
                5,
                5,
                new List<PackageState> { escapedPackage },
                new List<BlockerState>());
            var moveResult = MoveExecutor.Execute(board, new MoveCommand(escapedPackage.Id));

            var rejected = fullState.RouteEscapedPackage(moveResult);

            Assert.AreEqual(1, CountPackageOccurrences(accepted, 1));
            Assert.IsNull(accepted.UnroutedPackage);
            Assert.IsFalse(moveResult.ResultingBoard.TryGetPackageById(escapedPackage.Id, out _));
            Assert.AreEqual(1, CountPackageOccurrences(rejected, 3));
            Assert.AreEqual(3, rejected.UnroutedPackage.Value.Id);
            Assert.AreSame(fullState, rejected.ResultingState);
        }

        [Test]
        public void Route_IdenticalStateAndPackage_ProducesIdenticalResult()
        {
            var firstState = CreateDeliveryState(PackageColor.Blue, 2, PackageColor.Red, 3);
            var secondState = CreateDeliveryState(PackageColor.Blue, 2, PackageColor.Red, 3);
            var package = Package(12, PackageColor.Yellow);

            var first = firstState.RouteEscapedPackage(package);
            var second = secondState.RouteEscapedPackage(package);

            AssertRoutingResultsEqual(first, second);
        }

        [Test]
        public void ResolveCompletion_CompletedActiveTruck_PromotesNextTruck()
        {
            var completedTruck = new TruckState(
                PackageColor.Blue,
                1,
                new List<PackageState> { Package(1, PackageColor.Blue) });
            var nextTruck = new TruckState(PackageColor.Red, 3);
            var state = new DeliveryState(completedTruck, nextTruck, new HoldingQueueState(3));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.Promoted, result.Status);
            Assert.IsTrue(result.WasPromoted);
            Assert.AreSame(completedTruck, result.CompletedTruck);
            Assert.AreSame(nextTruck, result.ResultingState.ActiveTruck);
            Assert.IsFalse(result.ResultingState.HasNextTruck);
            Assert.IsNull(result.ResultingState.NextTruck);
            Assert.AreEqual(0, result.AutoLoadedPackages.Count);
        }

        [Test]
        public void ResolveCompletion_MatchingHoldingFront_AutoLoadsInFifoOrder()
        {
            var state = CompletedState(
                new TruckState(PackageColor.Red, 3),
                Package(10, PackageColor.Red),
                Package(20, PackageColor.Red),
                Package(30, PackageColor.Green));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.Promoted, result.Status);
            AssertPackageIds(result.AutoLoadedPackages, 10, 20);
            AssertPackageIds(result.ResultingState.ActiveTruck.LoadedPackages, 10, 20);
            AssertPackageIds(result.ResultingState.HoldingQueue.Packages, 30);
        }

        [Test]
        public void ResolveCompletion_NonMatchingHoldingFront_StopsAutoLoad()
        {
            var state = CompletedState(
                new TruckState(PackageColor.Red, 3),
                Package(10, PackageColor.Green),
                Package(20, PackageColor.Red));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.Promoted, result.Status);
            Assert.AreEqual(0, result.AutoLoadedPackages.Count);
            Assert.AreEqual(0, result.ResultingState.ActiveTruck.LoadCount);
            AssertPackageIds(result.ResultingState.HoldingQueue.Packages, 10, 20);
        }

        [Test]
        public void ResolveCompletion_AutoLoad_RespectsTruckCapacity()
        {
            var state = CompletedState(
                new TruckState(PackageColor.Red, 2),
                Package(10, PackageColor.Red),
                Package(20, PackageColor.Red),
                Package(30, PackageColor.Red));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.PromotedTruckCompleted, result.Status);
            Assert.IsTrue(result.ResultingState.ActiveTruck.IsComplete);
            AssertPackageIds(result.ResultingState.ActiveTruck.LoadedPackages, 10, 20);
            AssertPackageIds(result.ResultingState.HoldingQueue.Packages, 30);
        }

        [Test]
        public void ResolveCompletion_AutoLoad_RemovesOnlyLoadedHoldingPackages()
        {
            var state = CompletedState(
                new TruckState(PackageColor.Red, 3),
                Package(10, PackageColor.Red),
                Package(20, PackageColor.Red),
                Package(30, PackageColor.Green),
                Package(40, PackageColor.Red));

            var result = state.ResolveCompletedActiveTruck();

            AssertPackageIds(result.AutoLoadedPackages, 10, 20);
            AssertPackageIds(result.ResultingState.ActiveTruck.LoadedPackages, 10, 20);
            AssertPackageIds(result.ResultingState.HoldingQueue.Packages, 30, 40);
        }

        [Test]
        public void ResolveCompletion_AutoLoadCanCompletePromotedTruckSafely()
        {
            var state = CompletedState(
                new TruckState(PackageColor.Red, 1),
                Package(10, PackageColor.Red),
                Package(20, PackageColor.Green));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.PromotedTruckCompleted, result.Status);
            Assert.IsTrue(result.ResultingState.ActiveTruck.IsComplete);
            Assert.IsTrue(result.IsAwaitingNextTruck);
            Assert.IsFalse(result.ResultingState.HasNextTruck);
            AssertPackageIds(result.AutoLoadedPackages, 10);
            AssertPackageIds(result.ResultingState.HoldingQueue.Packages, 20);
        }

        [Test]
        public void ResolveCompletion_WhenPromotedTruckCompletes_DoesNotLoopWithoutNextTruck()
        {
            var state = CompletedState(
                new TruckState(PackageColor.Red, 1),
                Package(10, PackageColor.Red));
            var first = state.ResolveCompletedActiveTruck();

            var second = first.ResultingState.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.PromotedTruckCompleted, first.Status);
            Assert.AreEqual(TruckPromotionStatus.NextTruckUnavailable, second.Status);
            Assert.IsFalse(second.WasPromoted);
            Assert.IsNull(second.CompletedTruck);
            Assert.AreEqual(0, second.AutoLoadedPackages.Count);
            Assert.AreSame(first.ResultingState, second.ResultingState);
        }

        [Test]
        public void ResolveCompletion_PreservesEveryPackageExactlyOnce()
        {
            var completedPackage = Package(1, PackageColor.Blue);
            var firstHeldPackage = Package(10, PackageColor.Red);
            var secondHeldPackage = Package(20, PackageColor.Red);
            var blockedHeldPackage = Package(30, PackageColor.Green);
            var state = new DeliveryState(
                new TruckState(
                    PackageColor.Blue,
                    1,
                    new List<PackageState> { completedPackage }),
                new TruckState(PackageColor.Red, 3),
                new HoldingQueueState(
                    4,
                    new List<PackageState>
                    {
                        firstHeldPackage,
                        secondHeldPackage,
                        blockedHeldPackage
                    }));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(1, CountPackageOccurrences(result, completedPackage.Id));
            Assert.AreEqual(1, CountPackageOccurrences(result, firstHeldPackage.Id));
            Assert.AreEqual(1, CountPackageOccurrences(result, secondHeldPackage.Id));
            Assert.AreEqual(1, CountPackageOccurrences(result, blockedHeldPackage.Id));
            AssertPackagesEqual(completedPackage, result.CompletedTruck.LoadedPackages[0]);
            AssertPackagesEqual(firstHeldPackage, result.ResultingState.ActiveTruck.LoadedPackages[0]);
            AssertPackagesEqual(secondHeldPackage, result.ResultingState.ActiveTruck.LoadedPackages[1]);
            AssertPackagesEqual(blockedHeldPackage, result.ResultingState.HoldingQueue.Packages[0]);
        }

        [Test]
        public void ResolveCompletion_IdenticalState_ProducesIdenticalResult()
        {
            var firstState = CompletedState(
                new TruckState(PackageColor.Red, 3),
                Package(10, PackageColor.Red),
                Package(20, PackageColor.Red),
                Package(30, PackageColor.Green));
            var secondState = CompletedState(
                new TruckState(PackageColor.Red, 3),
                Package(10, PackageColor.Red),
                Package(20, PackageColor.Red),
                Package(30, PackageColor.Green));

            var first = firstState.ResolveCompletedActiveTruck();
            var second = secondState.ResolveCompletedActiveTruck();

            AssertPromotionResultsEqual(first, second);
        }

        [Test]
        public void ResolveCompletion_WhenActiveTruckIsIncomplete_DoesNotChangeState()
        {
            var state = CreateDeliveryState(PackageColor.Blue, 2, PackageColor.Red, 3);

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.ActiveTruckIncomplete, result.Status);
            Assert.IsFalse(result.WasPromoted);
            Assert.IsNull(result.CompletedTruck);
            Assert.AreEqual(0, result.AutoLoadedPackages.Count);
            Assert.AreSame(state, result.ResultingState);
        }

        [Test]
        public void ResolveCompletion_WhenNextTruckIsUnavailable_ReturnsExplicitState()
        {
            var completedTruck = new TruckState(
                PackageColor.Blue,
                1,
                new List<PackageState> { Package(1, PackageColor.Blue) });
            var state = new DeliveryState(completedTruck, null, new HoldingQueueState(2));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.NextTruckUnavailable, result.Status);
            Assert.IsFalse(result.WasPromoted);
            Assert.IsTrue(result.IsAwaitingNextTruck);
            Assert.IsNull(result.CompletedTruck);
            Assert.AreSame(state, result.ResultingState);
        }

        private static int CountPackageOccurrences(DeliveryRoutingResult result, int packageId)
        {
            var count = 0;
            count += CountPackages(result.ResultingState.ActiveTruck.LoadedPackages, packageId);
            count += CountPackages(result.ResultingState.HoldingQueue.Packages, packageId);

            if (result.UnroutedPackage.HasValue && result.UnroutedPackage.Value.Id == packageId)
            {
                count++;
            }

            return count;
        }

        private static int CountPackageOccurrences(TruckPromotionResult result, int packageId)
        {
            var count = 0;
            if (result.CompletedTruck != null)
            {
                count += CountPackages(result.CompletedTruck.LoadedPackages, packageId);
            }

            count += CountPackages(result.ResultingState.ActiveTruck.LoadedPackages, packageId);
            count += CountPackages(result.ResultingState.HoldingQueue.Packages, packageId);
            return count;
        }

        private static int CountPackages(IReadOnlyList<PackageState> packages, int packageId)
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

        private static void AssertRoutingResultsEqual(
            DeliveryRoutingResult expected,
            DeliveryRoutingResult actual)
        {
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.WasAccepted, actual.WasAccepted);
            AssertPackagesEqual(expected.Package, actual.Package);

            Assert.AreEqual(expected.UnroutedPackage.HasValue, actual.UnroutedPackage.HasValue);
            if (expected.UnroutedPackage.HasValue)
            {
                AssertPackagesEqual(expected.UnroutedPackage.Value, actual.UnroutedPackage.Value);
            }

            AssertTrucksEqual(expected.ResultingState.ActiveTruck, actual.ResultingState.ActiveTruck);
            AssertTrucksEqual(expected.ResultingState.NextTruck, actual.ResultingState.NextTruck);

            Assert.AreEqual(
                expected.ResultingState.HoldingQueue.Capacity,
                actual.ResultingState.HoldingQueue.Capacity);
            AssertPackageListsEqual(
                expected.ResultingState.HoldingQueue.Packages,
                actual.ResultingState.HoldingQueue.Packages);
        }

        private static void AssertPromotionResultsEqual(
            TruckPromotionResult expected,
            TruckPromotionResult actual)
        {
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.WasPromoted, actual.WasPromoted);
            Assert.AreEqual(expected.IsAwaitingNextTruck, actual.IsAwaitingNextTruck);

            Assert.AreEqual(expected.CompletedTruck == null, actual.CompletedTruck == null);
            if (expected.CompletedTruck != null)
            {
                AssertTrucksEqual(expected.CompletedTruck, actual.CompletedTruck);
            }

            AssertPackageListsEqual(expected.AutoLoadedPackages, actual.AutoLoadedPackages);
            AssertTrucksEqual(expected.ResultingState.ActiveTruck, actual.ResultingState.ActiveTruck);
            Assert.AreEqual(expected.ResultingState.HasNextTruck, actual.ResultingState.HasNextTruck);
            if (expected.ResultingState.HasNextTruck)
            {
                AssertTrucksEqual(expected.ResultingState.NextTruck, actual.ResultingState.NextTruck);
            }

            Assert.AreEqual(
                expected.ResultingState.HoldingQueue.Capacity,
                actual.ResultingState.HoldingQueue.Capacity);
            AssertPackageListsEqual(
                expected.ResultingState.HoldingQueue.Packages,
                actual.ResultingState.HoldingQueue.Packages);
        }

        private static void AssertPackageIds(
            IReadOnlyList<PackageState> packages,
            params int[] expectedIds)
        {
            Assert.AreEqual(expectedIds.Length, packages.Count);
            for (var i = 0; i < expectedIds.Length; i++)
            {
                Assert.AreEqual(expectedIds[i], packages[i].Id);
            }
        }

        private static void AssertTrucksEqual(TruckState expected, TruckState actual)
        {
            Assert.AreEqual(expected.RequiredColor, actual.RequiredColor);
            Assert.AreEqual(expected.Capacity, actual.Capacity);
            Assert.AreEqual(expected.IsComplete, actual.IsComplete);
            AssertPackageListsEqual(expected.LoadedPackages, actual.LoadedPackages);
        }

        private static void AssertPackageListsEqual(
            IReadOnlyList<PackageState> expected,
            IReadOnlyList<PackageState> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                AssertPackagesEqual(expected[i], actual[i]);
            }
        }

        private static void AssertPackagesEqual(PackageState expected, PackageState actual)
        {
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Position, actual.Position);
            Assert.AreEqual(expected.Direction, actual.Direction);
            Assert.AreEqual(expected.Color, actual.Color);
        }

        private static DeliveryState CreateDeliveryState(
            PackageColor activeColor,
            int activeCapacity,
            PackageColor nextColor,
            int holdingCapacity)
        {
            return new DeliveryState(
                new TruckState(activeColor, activeCapacity),
                new TruckState(nextColor, activeCapacity),
                new HoldingQueueState(holdingCapacity));
        }

        private static DeliveryState CompletedState(
            TruckState nextTruck,
            params PackageState[] holdingPackages)
        {
            return new DeliveryState(
                new TruckState(
                    PackageColor.Blue,
                    1,
                    new List<PackageState> { Package(1, PackageColor.Blue) }),
                nextTruck,
                new HoldingQueueState(holdingPackages.Length + 1, holdingPackages));
        }

        private static PackageState Package(int id, PackageColor color)
        {
            return new PackageState(id, new GridPosition(0, 0), PackageDirection.Up, color);
        }
    }
}
