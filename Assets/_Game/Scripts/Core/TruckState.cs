using System;
using System.Collections.Generic;

namespace ParcelEscape.Core
{
    public sealed class TruckState
    {
        public PackageColor RequiredColor { get; }
        public int Capacity { get; }
        public IReadOnlyList<PackageState> LoadedPackages { get; }
        public int LoadCount => LoadedPackages.Count;
        public bool IsComplete => LoadCount == Capacity;

        public TruckState(
            PackageColor requiredColor,
            int capacity,
            IReadOnlyList<PackageState> loadedPackages = null)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Truck capacity must be greater than zero.");
            }

            var packages = loadedPackages == null
                ? new List<PackageState>()
                : new List<PackageState>(loadedPackages);

            if (packages.Count > capacity)
            {
                throw new ArgumentException("Loaded packages cannot exceed truck capacity.", nameof(loadedPackages));
            }

            var packageIds = new HashSet<int>();
            foreach (var package in packages)
            {
                if (package.Color != requiredColor)
                {
                    throw new ArgumentException("Every loaded package must match the truck color.", nameof(loadedPackages));
                }

                if (!packageIds.Add(package.Id))
                {
                    throw new ArgumentException($"Duplicate loaded package ID {package.Id}.", nameof(loadedPackages));
                }
            }

            RequiredColor = requiredColor;
            Capacity = capacity;
            LoadedPackages = packages.AsReadOnly();
        }

        public bool TryLoad(PackageState package, out TruckState resultingTruck)
        {
            if (package.Color != RequiredColor || IsComplete)
            {
                resultingTruck = this;
                return false;
            }

            foreach (var loadedPackage in LoadedPackages)
            {
                if (loadedPackage.Id == package.Id)
                {
                    resultingTruck = this;
                    return false;
                }
            }

            var packages = new List<PackageState>(LoadedPackages) { package };
            resultingTruck = new TruckState(RequiredColor, Capacity, packages);
            return true;
        }
    }
}
