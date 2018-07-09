using System;
using ImTools;

namespace Tea
{
    public static class ImToolsExt
    {
        public static ImList<T> list<T>(params T[] items)
        {
            if (items.IsNullOrEmpty())
                return ImList<T>.Empty;
            var list = ImList<T>.Empty;
            for (var i = items.Length - 1; i >= 0; --i)
                list = items[i].Cons(list);
            return list;
        }

        public static void Deconstruct<T>(this ImList<T> list, out T head, out ImList<T> tail, out bool isEmpty)
        {
            head = list.Head;
            tail = list.Tail;
            isEmpty = list.IsEmpty;
        }

        public static T GetAt<T>(this ImList<T> list, int i, T @default = default)
        {
            if (list.IsEmpty)
                return @default;
            for (var j = 0; !list.IsEmpty; list = list.Tail, ++j)
                if (j == i)
                    return list.Head;
            return @default;
        }

        public static T Get<T>(this ImList<T> list, Func<T, bool> condition, T @default = default)
        {
            if (list.IsEmpty)
                return @default;
            for (; !list.IsEmpty; list = list.Tail)
                if (condition(list.Head))
                    return list.Head;
            return @default;
        }

        /// <summary> Splits the list into two parts, first part is up to but no including the index.
        /// BUT the first part will be returned in REVERSE order. That's why it is called Unzip and not Split.
        /// <code>list(1, 2, 3, 4, 5).Unzip(2) =>> (list(2, 1), list(3, 4, 5)) </code></summary>
        public static (ImList<T> revHead, ImList<T> tail) Unzip<T>(this ImList<T> list, int i)
        {
            if (list.IsEmpty)
                return (list, list);
            if (i <= 0)
                return (ImList<T>.Empty, list);

            var head = ImList<T>.Empty;
            for (var j = 0; j < i && !list.IsEmpty; ++j, list = list.Tail)
                head = head.Prepend(list.Head);

            return (head, list);
        }

        /// <summary>Joins reversed head with tail</summary>
        public static ImList<T> Zip<T>(this ImList<T> revHead, ImList<T> tail)
        {
            if (revHead.IsEmpty)
                return tail;
            for (; !revHead.IsEmpty; revHead = revHead.Tail)
                tail = tail.Prepend(revHead.Head);
            return tail;
        }

        /// <summary> <code>list(1, 2, 3).Cons(list(4, 5)) => list(1, 2, 3, 4, 5)</code></summary>
        public static ImList<T> Cons<T>(this ImList<T> head, ImList<T> tail) =>
            head.Reverse().Zip(tail);

        /// <summary> <code>list(1, 2, 3).UpdateAt(1, x => x*2) => list(1, 4, 3)</code>
        /// The method will return original list for out of bound index. So you can check that.
        /// This also will return original list when updated element is equal to old one.</summary>
        public static ImList<T> UpdateAt<T>(this ImList<T> list, int i, Func<T, T> update)
        {
            if (list.IsEmpty || i < 0)
                return list;
            var (revHead, tail) = list.Unzip(i);
            if (tail.IsEmpty)
                return list;
            var it = update(tail.Head);
            if (ReferenceEquals(it, tail.Head) || it != null && it.Equals(tail.Head))
                return list;
            return revHead.Zip(it.Cons(tail.Tail));
        }

        /// <summary> <code>list(1, 2, 3).RemoveAt(1) => list(1, 3)</code>
        /// The method will return original list for out of bound index. So you can check that.</summary>
        public static ImList<T> RemoveAt<T>(this ImList<T> list, int i)
        {
            if (list.IsEmpty || i < 0)
                return list;
            var (revHead, tail) = list.Unzip(i);
            return tail.IsEmpty ? list : revHead.Zip(tail.Tail);
        }
    }
}
