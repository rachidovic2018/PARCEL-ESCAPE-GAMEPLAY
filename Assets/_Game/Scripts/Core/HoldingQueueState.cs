using System;
using System.Collections.Generic;

namespace ParcelEscape.Core
{
    public sealed class HoldingQueueState
    {
        public int Capacity { get; }
        public IReadOnlyList<PackageState> Packages { get; }
        public int Count => Packages.Count;
        public bool IsFull => Count == Capacity;

        public HoldingQueueState(int capacity, IReadOnlyList<PackageState> packages = null)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Holding capacity must be greater than zero.");
            }

            var queuedPackages = packages == null
                ? new List<PackageState>()
                : new List<PackageState>(packages);

            if (queuedPackages.Count > capacity)
            {
                throw new ArgumentException("Held packages cannot exceed holding capacity.", nameof(packages));
            }

            var packageIds = new HashSet<int>();
            foreach (var package in queuedPackages)
            {
                if (!packageIds.Add(package.Id))
                {
                    throw new ArgumentException($"Duplicate held package ID {package.Id}.", nameof(packages));
                }
            }

            Capacity = capacity;
            Packages = queuedPackages.AsReadOnly();
        }

        public bool TryEnqueue(PackageState package, out HoldingQueueState resultingQueue)
        {
            if (IsFull)
            {
                resultingQueue = this;
                return false;
            }

            foreach (var heldPackage in Packages)
            {
                if (heldPackage.Id == package.Id)
                {
                    resultingQueue = this;
                    return false;
                }
            }

            var packages = new List<PackageState>(Packages) { package };
            resultingQueue = new HoldingQueueState(Capacity, packages);
            return true;
        }
    }
}
