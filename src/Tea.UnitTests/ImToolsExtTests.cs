using ImTools;
using NUnit.Framework;

namespace Tea.UnitTests
{
    [TestFixture]
    public class ImToolsExtTests
    {
        [Test]
        public void Can_get_item_at_specific_index() =>
            Assert.AreEqual(4, 1.Cons(2.Cons(3.Cons(4.Cons(5.Cons())))).GetAt(3));

        [Test]
        public void Can_efficiently_update_at_specific_index()
        {
            var list = 15.Cons(10.Cons(5.Cons(1.Cons())));

            list = list.UpdateAt(1, i => i + 1);
            CollectionAssert.AreEqual(new[] { 15, 11, 5, 1 }, list.ToArray());

            list = list.UpdateAt(0, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 1 }, list.ToArray());

            list = list.UpdateAt(3, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 2 }, list.ToArray());

            list = list.UpdateAt(-5, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 2 }, list.ToArray());

            list = list.UpdateAt(5, i => i + 1);
            CollectionAssert.AreEqual(new[] { 16, 11, 5, 2 }, list.ToArray());

            var newList = list.UpdateAt(1, i => i);
            Assert.AreSame(list, newList);
        }

        [Test]
        public void Can_remove_item_at_index()
        {
            var list = 15.Cons(10.Cons(5.Cons(1.Cons())));

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

        [Test]
        public void Can_map() =>
            CollectionAssert.AreEqual(
                new[] {2, 4, 6, 8},
                1.Cons(2.Cons(3.Cons(4.Cons()))).Map(i => i * 2).ToArray());

        [Test]
        public void Can_map_with_index() =>
            CollectionAssert.AreEqual(
                new[] { 1, 3, 5, 7 },
                1.Cons(2.Cons(3.Cons(4.Cons()))).Map((x, i)  => x + i).ToArray());
    }
}
