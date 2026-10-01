using System;
using System.Collections.Generic;
using NUnit.Framework;
using ParcelEscape.Core;
using ParcelEscape.Gameplay;
using UnityEngine;

namespace ParcelEscape.Tests
{
    public class TruckSequenceTests
    {
        [Test]
        public void ConvertTruckSequence_PreservesAuthoredOrder()
        {
            var asset = CreateAsset(
                Truck(PackageColor.Blue, 2),
                Truck(PackageColor.Red, 3),
                Truck(PackageColor.Blue, 1));

            try
            {
                var sequence = LevelDefinitionConverter.ConvertTruckSequence(asset);

                Assert.AreEqual(3, sequence.Count);
                AssertTruck(sequence.Trucks[0], PackageColor.Blue, 2);
                AssertTruck(sequence.Trucks[1], PackageColor.Red, 3);
                AssertTruck(sequence.Trucks[2], PackageColor.Blue, 1);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void ConvertDelivery_FirstAuthoredTruckBecomesActive()
        {
            var asset = CreateAsset(
                Truck(PackageColor.Blue, 2),
                Truck(PackageColor.Red, 3));

            try
            {
                var state = LevelDefinitionConverter.ConvertDelivery(asset, 4);

                AssertTruck(state.ActiveTruck, PackageColor.Blue, 2);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void ConvertDelivery_SecondAuthoredTruckBecomesNext()
        {
            var asset = CreateAsset(
                Truck(PackageColor.Blue, 2),
                Truck(PackageColor.Red, 3),
                Truck(PackageColor.Green, 4));

            try
            {
                var state = LevelDefinitionConverter.ConvertDelivery(asset, 4);

                Assert.IsTrue(state.HasNextTruck);
                AssertTruck(state.NextTruck, PackageColor.Red, 3);
                Assert.AreEqual(1, state.RemainingTruckSequence.Count);
                AssertTruck(state.RemainingTruckSequence.Trucks[0], PackageColor.Green, 4);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void ResolveCompletion_PromotionAdvancesNextInAuthoredOrder()
        {
            var state = CreateDelivery(
                4,
                Truck(PackageColor.Blue, 1),
                Truck(PackageColor.Red, 2),
                Truck(PackageColor.Green, 3),
                Truck(PackageColor.Yellow, 4));
            state = CompleteActiveTruck(state, 1);

            var result = state.ResolveCompletedActiveTruck();

            AssertTruck(result.ResultingState.ActiveTruck, PackageColor.Red, 2);
            AssertTruck(result.ResultingState.NextTruck, PackageColor.Green, 3);
            Assert.AreEqual(1, result.ResultingState.RemainingTruckSequence.Count);
            AssertTruck(
                result.ResultingState.RemainingTruckSequence.Trucks[0],
                PackageColor.Yellow,
                4);
        }

        [Test]
        public void ResolveCompletion_SequenceExhaustionDoesNotFabricateTruck()
        {
            var state = CreateDelivery(
                2,
                Truck(PackageColor.Blue, 1),
                Truck(PackageColor.Red, 1));
            state = CompleteActiveTruck(state, 1);

            var first = state.ResolveCompletedActiveTruck();
            var completedFinalState = CompleteActiveTruck(first.ResultingState, 2);
            var second = completedFinalState.ResolveCompletedActiveTruck();

            Assert.IsFalse(first.ResultingState.HasNextTruck);
            Assert.IsTrue(first.ResultingState.RemainingTruckSequence.IsEmpty);
            Assert.AreEqual(TruckPromotionStatus.NextTruckUnavailable, second.Status);
            Assert.AreSame(completedFinalState, second.ResultingState);
        }

        [Test]
        public void ResolveCompletion_ChainsPromotionAndFifoAutoLoadAcrossAuthoredTrucks()
        {
            var state = ChainedState();

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(TruckPromotionStatus.Promoted, result.Status);
            Assert.AreEqual(3, result.CompletedTrucks.Count);
            AssertTruck(result.CompletedTrucks[0], PackageColor.Blue, 1, 1);
            AssertTruck(result.CompletedTrucks[1], PackageColor.Red, 1, 1);
            AssertTruck(result.CompletedTrucks[2], PackageColor.Red, 1, 1);
            AssertPackageIds(result.AutoLoadedPackages, 10, 20, 30);
            AssertTruck(result.ResultingState.ActiveTruck, PackageColor.Green, 2, 1);
            Assert.IsFalse(result.ResultingState.HasNextTruck);
            Assert.IsTrue(result.ResultingState.RemainingTruckSequence.IsEmpty);
            Assert.AreEqual(0, result.ResultingState.HoldingQueue.Count);
        }

        [Test]
        public void ConvertDelivery_PreservesEveryAuthoredCapacityThroughPromotion()
        {
            var state = CreateDelivery(
                4,
                Truck(PackageColor.Blue, 2),
                Truck(PackageColor.Red, 3),
                Truck(PackageColor.Green, 4),
                Truck(PackageColor.Yellow, 5));

            AssertTruck(state.ActiveTruck, PackageColor.Blue, 2);
            AssertTruck(state.NextTruck, PackageColor.Red, 3);
            AssertTruck(state.RemainingTruckSequence.Trucks[0], PackageColor.Green, 4);
            AssertTruck(state.RemainingTruckSequence.Trucks[1], PackageColor.Yellow, 5);

            state = CompleteActiveTruck(state, 1);
            var result = state.ResolveCompletedActiveTruck();

            AssertTruck(result.ResultingState.ActiveTruck, PackageColor.Red, 3);
            AssertTruck(result.ResultingState.NextTruck, PackageColor.Green, 4);
            AssertTruck(
                result.ResultingState.RemainingTruckSequence.Trucks[0],
                PackageColor.Yellow,
                5);
        }

        [Test]
        public void ResolveCompletion_ChainedPromotionPreservesEveryPackageExactlyOnce()
        {
            var state = ChainedState(Package(40, PackageColor.Yellow));

            var result = state.ResolveCompletedActiveTruck();

            Assert.AreEqual(1, CountPackageOccurrences(result, 1));
            Assert.AreEqual(1, CountPackageOccurrences(result, 10));
            Assert.AreEqual(1, CountPackageOccurrences(result, 20));
            Assert.AreEqual(1, CountPackageOccurrences(result, 30));
            Assert.AreEqual(1, CountPackageOccurrences(result, 40));
            AssertPackageIds(result.ResultingState.HoldingQueue.Packages, 40);
        }

        [Test]
        public void ResolveCompletion_IdenticalAuthoredInputProducesIdenticalFinalState()
        {
            var first = ChainedState(Package(40, PackageColor.Yellow))
                .ResolveCompletedActiveTruck();
            var second = ChainedState(Package(40, PackageColor.Yellow))
                .ResolveCompletedActiveTruck();

            AssertPromotionResultsEqual(first, second);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ConvertTruckSequence_NonPositiveCapacityIsRejected(int capacity)
        {
            var asset = CreateAsset(
                Truck(PackageColor.Blue, capacity),
                Truck(PackageColor.Red, 1));

            try
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => LevelDefinitionConverter.ConvertTruckSequence(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void ConvertTruckSequence_UndefinedColorIsRejected()
        {
            var asset = CreateAsset(
                Truck((PackageColor)999, 1),
                Truck(PackageColor.Red, 1));

            try
            {
                Assert.Throws<ArgumentOutOfRangeException>(
                    () => LevelDefinitionConverter.ConvertTruckSequence(asset));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void ConvertDelivery_SequenceWithoutNextTruckIsRejected()
        {
            var asset = CreateAsset(Truck(PackageColor.Blue, 1));

            try
            {
                Assert.Throws<ArgumentException>(
                    () => LevelDefinitionConverter.ConvertDelivery(asset, 2));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void DeliveryState_RemainingSequenceWithoutNextTruckIsRejected()
        {
            var activeTruck = new TruckState(PackageColor.Blue, 1);
            var remainingSequence = new TruckSequenceState(
                new[] { new TruckState(PackageColor.Red, 1) });

            Assert.Throws<ArgumentException>(
                () => new DeliveryState(
                    activeTruck,
                    null,
                    remainingSequence,
                    new HoldingQueueState(2)));
        }

        private static DeliveryState ChainedState(params PackageState[] trailingPackages)
        {
            var state = CreateDelivery(
                5,
                Truck(PackageColor.Blue, 1),
                Truck(PackageColor.Red, 1),
                Truck(PackageColor.Red, 1),
                Truck(PackageColor.Green, 2));
            state = CompleteActiveTruck(state, 1);

            state = Route(state, Package(10, PackageColor.Red));
            state = Route(state, Package(20, PackageColor.Red));
            state = Route(state, Package(30, PackageColor.Green));
            foreach (var package in trailingPackages)
            {
                state = Route(state, package);
            }

            return state;
        }

        private static DeliveryState CreateDelivery(
            int holdingCapacity,
            params TruckDefinition[] definitions)
        {
            var asset = CreateAsset(definitions);
            try
            {
                return LevelDefinitionConverter.ConvertDelivery(asset, holdingCapacity);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        private static DeliveryState CompleteActiveTruck(DeliveryState state, int firstPackageId)
        {
            for (var index = state.ActiveTruck.LoadCount; index < state.ActiveTruck.Capacity; index++)
            {
                state = Route(
                    state,
                    Package(firstPackageId + index, state.ActiveTruck.RequiredColor));
            }

            return state;
        }

        private static DeliveryState Route(DeliveryState state, PackageState package)
        {
            var result = state.RouteEscapedPackage(package);
            Assert.AreNotEqual(DeliveryRoutingStatus.HoldingFull, result.Status);
            Assert.AreNotEqual(DeliveryRoutingStatus.PackageAlreadyRouted, result.Status);
            return result.ResultingState;
        }

        private static int CountPackageOccurrences(TruckPromotionResult result, int packageId)
        {
            var count = 0;
            foreach (var truck in result.CompletedTrucks)
            {
                count += CountPackages(truck.LoadedPackages, packageId);
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

        private static void AssertPromotionResultsEqual(
            TruckPromotionResult expected,
            TruckPromotionResult actual)
        {
            Assert.AreEqual(expected.Status, actual.Status);
            Assert.AreEqual(expected.CompletedTrucks.Count, actual.CompletedTrucks.Count);
            for (var index = 0; index < expected.CompletedTrucks.Count; index++)
            {
                AssertTrucksEqual(expected.CompletedTrucks[index], actual.CompletedTrucks[index]);
            }

            AssertPackageListsEqual(expected.AutoLoadedPackages, actual.AutoLoadedPackages);
            AssertTrucksEqual(expected.ResultingState.ActiveTruck, actual.ResultingState.ActiveTruck);
            Assert.AreEqual(expected.ResultingState.HasNextTruck, actual.ResultingState.HasNextTruck);
            if (expected.ResultingState.HasNextTruck)
            {
                AssertTrucksEqual(expected.ResultingState.NextTruck, actual.ResultingState.NextTruck);
            }

            Assert.AreEqual(
                expected.ResultingState.RemainingTruckSequence.Count,
                actual.ResultingState.RemainingTruckSequence.Count);
            for (var index = 0;
                 index < expected.ResultingState.RemainingTruckSequence.Count;
                 index++)
            {
                AssertTrucksEqual(
                    expected.ResultingState.RemainingTruckSequence.Trucks[index],
                    actual.ResultingState.RemainingTruckSequence.Trucks[index]);
            }

            Assert.AreEqual(
                expected.ResultingState.HoldingQueue.Capacity,
                actual.ResultingState.HoldingQueue.Capacity);
            AssertPackageListsEqual(
                expected.ResultingState.HoldingQueue.Packages,
                actual.ResultingState.HoldingQueue.Packages);
        }

        private static void AssertTruck(
            TruckState truck,
            PackageColor color,
            int capacity,
            int loadCount = 0)
        {
            Assert.IsNotNull(truck);
            Assert.AreEqual(color, truck.RequiredColor);
            Assert.AreEqual(capacity, truck.Capacity);
            Assert.AreEqual(loadCount, truck.LoadCount);
        }

        private static void AssertTrucksEqual(TruckState expected, TruckState actual)
        {
            AssertTruck(actual, expected.RequiredColor, expected.Capacity, expected.LoadCount);
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
                Assert.AreEqual(expected[index].Color, actual[index].Color);
                Assert.AreEqual(expected[index].Position, actual[index].Position);
                Assert.AreEqual(expected[index].Direction, actual[index].Direction);
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

        private static LevelDefinitionAsset CreateAsset(params TruckDefinition[] definitions)
        {
            var asset = ScriptableObject.CreateInstance<LevelDefinitionAsset>();
            asset.truckDefinitions = new List<TruckDefinition>(definitions);
            return asset;
        }

        private static TruckDefinition Truck(PackageColor color, int capacity)
        {
            return new TruckDefinition
            {
                requiredColor = color,
                capacity = capacity
            };
        }

        private static PackageState Package(int id, PackageColor color)
        {
            return new PackageState(id, new GridPosition(0, 0), PackageDirection.Up, color);
        }
    }
}
