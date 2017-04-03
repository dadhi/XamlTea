using System;
using ImTools;
using NUnit.Framework;

namespace Tea.UnitTests
{
    [TestFixture]
    public class ImToolsExtTests
    {
        [Test]
        public void Can_get_item_at_specific_index()
        {
            var list = ImList<int>.Empty
                .Prep(1)
                .Prep(5)
                .Prep(10)
                .Prep(15);

            Assert.AreEqual(10, list.GetOrDefault(1));
        }

        [Test]
        public void Can_efficiently_map_at_specific_index()
        {
            var list = ImList<int>.Empty
                .Prep(1)
                .Prep(5)
                .Prep(10)
                .Prep(15);

            var newList = list.With(1, i => i + 1);

            CollectionAssert.AreEqual(new[] { 15, 11, 5, 1 }, newList.Enumerate());
        }

        [Test]
        public void Can_efficiently_map_at_index_0()
        {
            var list = ImList<int>.Empty
                .Prep(1)
                .Prep(5)
                .Prep(10)
                .Prep(15);

            var newList = list.With(0, i => i + 1);

            CollectionAssert.AreEqual(new[] { 16, 10, 5, 1 }, newList.Enumerate());
        }

        [Test]
        public void Can_efficiently_map_at_last_index()
        {
            var list = ImList<int>.Empty
                .Prep(1)
                .Prep(5)
                .Prep(10)
                .Prep(15);

            var newList = list.With(3, i => i + 1);

            CollectionAssert.AreEqual(new[] { 15, 10, 5, 2 }, newList.Enumerate());
        }
    }
}
