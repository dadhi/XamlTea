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
                .Prepend(1)
                .Prepend(5)
                .Prepend(10)
                .Prepend(15);

            Assert.AreEqual(10, list.GetAt(1));
        }

        [Test]
        public void Can_efficiently_map_at_specific_index()
        {
            var list = ImList<int>.Empty
                .Prepend(1)
                .Prepend(5)
                .Prepend(10)
                .Prepend(15);

            list = list.UpdateAt(1, i => i + 1);
            CollectionAssert.AreEqual(new[] { 15, 11, 5, 1 }, list.Enumerate());

            list = list.UpdateAt(0, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 1 }, list.Enumerate());

            list = list.UpdateAt(3, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 2 }, list.Enumerate());

            list = list.UpdateAt(-5, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 2 }, list.Enumerate());

            list = list.UpdateAt(5, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 2 }, list.Enumerate());

            var newList = list.UpdateAt(1, i => i);
            Assert.AreSame(list, newList);
        }

        [Test]
        public void Can_remove_item_with_index()
        {
            var list = ImList<int>.Empty
                .Prepend(1)
                .Prepend(5)
                .Prepend(10)
                .Prepend(15);

            list = list.RemoveAt(2);
            CollectionAssert.AreEqual(new[] { 15, 10, 1 }, list.Enumerate());

            list = list.RemoveAt(2);
            CollectionAssert.AreEqual(new[] { 15, 10 }, list.Enumerate());

            list = list.RemoveAt(0);
            CollectionAssert.AreEqual(new[] { 10 }, list.Enumerate());

            list = list.RemoveAt(5);
            CollectionAssert.AreEqual(new[] { 10 }, list.Enumerate());

            var newList = list.RemoveAt(-5);
            CollectionAssert.AreEqual(new[] { 10 }, list.Enumerate());
            Assert.AreSame(list, newList);

            list = list.RemoveAt(0);
            CollectionAssert.AreEqual(new int[] {}, list.Enumerate());

            list = list.RemoveAt(0);
            CollectionAssert.AreEqual(new int[] { }, list.Enumerate());
        }
    }
}
