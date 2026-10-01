using System;
using System.Collections.Generic;

namespace ParcelEscape.Core
{
    public sealed class TruckSequenceState
    {
        public IReadOnlyList<TruckState> Trucks { get; }
        public int Count => Trucks.Count;
        public bool IsEmpty => Count == 0;

        public TruckSequenceState(IReadOnlyList<TruckState> trucks = null)
        {
            var queuedTrucks = trucks == null
                ? new List<TruckState>()
                : new List<TruckState>(trucks);

            for (var index = 0; index < queuedTrucks.Count; index++)
            {
                var truck = queuedTrucks[index];
                if (truck == null)
                {
                    throw new ArgumentException("Truck sequences cannot contain null entries.", nameof(trucks));
                }

                if (truck.LoadCount != 0)
                {
                    throw new ArgumentException("Authored future trucks must start empty.", nameof(trucks));
                }
            }

            Trucks = queuedTrucks.AsReadOnly();
        }

        public bool TryDequeue(out TruckState truck, out TruckSequenceState remainingSequence)
        {
            if (IsEmpty)
            {
                truck = null;
                remainingSequence = this;
                return false;
            }

            truck = Trucks[0];
            var remainingTrucks = new List<TruckState>(Count - 1);
            for (var index = 1; index < Count; index++)
            {
                remainingTrucks.Add(Trucks[index]);
            }

            remainingSequence = new TruckSequenceState(remainingTrucks);
            return true;
        }
    }
}
