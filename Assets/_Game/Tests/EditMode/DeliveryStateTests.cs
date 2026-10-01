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
        public void Route_WhenActiveTruckFills_DoesNotExceedCapacityOrPromoteNextTruck()
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

        private static PackageState Package(int id, PackageColor color)
        {
            return new PackageState(id, new GridPosition(0, 0), PackageDirection.Up, color);
        }
    }
}
