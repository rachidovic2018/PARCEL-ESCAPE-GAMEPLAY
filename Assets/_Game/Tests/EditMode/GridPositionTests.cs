using NUnit.Framework;
using ParcelEscape.Core;

namespace ParcelEscape.Tests
{
    [TestFixture]
    public class GridPositionTests
    {
        [Test]
        public void Equality_SameCoordinates_ReturnsTrue()
        {
            var pos1 = new GridPosition(3, 4);
            var pos2 = new GridPosition(3, 4);

            Assert.IsTrue(pos1.Equals(pos2));
            Assert.IsTrue(pos1 == pos2);
            Assert.IsFalse(pos1 != pos2);
            Assert.AreEqual(pos1, pos2);
        }

        [Test]
        public void Equality_DifferentCoordinates_ReturnsFalse()
        {
            var pos1 = new GridPosition(3, 4);
            var pos2 = new GridPosition(4, 3);

            Assert.IsFalse(pos1.Equals(pos2));
            Assert.IsFalse(pos1 == pos2);
            Assert.IsTrue(pos1 != pos2);
            Assert.AreNotEqual(pos1, pos2);
        }

        [Test]
        public void GetHashCode_SameCoordinates_ReturnsSameHash()
        {
            var pos1 = new GridPosition(5, 5);
            var pos2 = new GridPosition(5, 5);

            Assert.AreEqual(pos1.GetHashCode(), pos2.GetHashCode());
        }

        [Test]
        public void ToString_Format_ReturnsCorrectString()
        {
            var pos = new GridPosition(-1, 10);
            Assert.AreEqual("(-1, 10)", pos.ToString());
        }
    }
}
