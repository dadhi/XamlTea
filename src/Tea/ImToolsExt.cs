using System;
using ImTools;

namespace Tea
{
    public sealed class unit
    {
        public static readonly unit _ = new unit();
        private unit() { }
    }

    public struct Pair<A, B>
    {
        public A a;
        public B b;

        public Pair(A a, B b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public static class pair
    {
        public static Pair<A, B> of<A, B>(A a, B b)
        {
            return new Pair<A, B>(a, b);
        }
    }

    public static class ImToolsExt
    {
        public static ImList<T> AsList<T>(this T[] values)
        {
            if (values.IsNullOrEmpty())
                return ImList<T>.Empty;
            var result = ImList<T>.Empty;
            for (var i = values.Length - 1; i >= 0; i--)
                result = result.Prepend(values[i]);
            return result;
        }

        public static T GetAt<T>(this ImList<T> source, int index)
        {
            if (source.IsEmpty)
                return default(T);
            for (var i = 0; !source.IsEmpty; source = source.Tail, ++i)
                if (i == index)
                    return source.Head;
            return default(T);
        }

        public static T GetOrDefault<T>(this ImList<T> source, Func<T, bool> condition)
        {
            if (source.IsEmpty)
                return default(T);
            for (; !source.IsEmpty; source = source.Tail)
                if (condition(source.Head))
                    return source.Head;
            return default(T);
        }

        public static ImList<T> Prepend<T>(this ImList<T> source, ImList<T> prefix)
        {
            if (source.IsEmpty)
                return prefix;
            for (; !prefix.IsEmpty; prefix = prefix.Tail)
                source = source.Prepend(prefix.Head);
            return source;
        }

        public static ImList<T> With<T>(this ImList<T> source, int index, Func<T, T> update)
        {
            if (source.IsEmpty || index < 0)
                return source;

            if (index == 0)
                return source.Tail.Prepend(update(source.Head));

            // start from index 1
            var reversedPrefix = ImList<T>.Empty.Prepend(source.Head);
            var suffix = source.Tail;
            for (var i = 1; !suffix.IsEmpty; suffix = suffix.Tail, ++i)
            {
                if (i == index)
                {
                    var sourceItem = suffix.Head;
                    var updatedItem = update(sourceItem);
                    if (ReferenceEquals(updatedItem, sourceItem) || 
                        updatedItem != null && updatedItem.Equals(sourceItem))
                        return source; // if item did not change, return the original source
                    return suffix.Tail.Prepend(updatedItem).Prepend(reversedPrefix);
                }
                reversedPrefix = reversedPrefix.Prepend(suffix.Head);
            }

            // if index is outside of the bounds, return original array
            return source;
        }

        public static ImList<T> Without<T>(this ImList<T> source, int index)
        {
            if (source.IsEmpty || index < 0)
                return source;

            if (index == 0)
                return source.Tail;

            // start from index 1
            var reversedPrefix = ImList<T>.Empty.Prepend(source.Head);
            var remaining = source.Tail;
            for (var i = 1; !remaining.IsEmpty; remaining = remaining.Tail, ++i)
            {
                if (i == index)
                    return remaining.Tail.Prepend(reversedPrefix);
                reversedPrefix = reversedPrefix.Prepend(remaining.Head);
            }

            // if index is outside of the bounds, return original array
            return source;
        }
    }
}
