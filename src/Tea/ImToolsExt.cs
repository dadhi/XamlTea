using System;
using ImTools;

namespace Tea
{
    public sealed class unit
    {
        public static readonly unit _ = new unit();

        public static unit Ignore<T>(T it) { return _; }

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
        public static ImList<T> FromArray<T>(params T[] values)
        {
            if (values.IsNullOrEmpty())
                return ImList<T>.Empty;
            var result = ImList<T>.Empty;
            for (var i = 0; i < values.Length; i++)
                result = result.Prep(values[i]);
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

        public static ImList<T> With<T>(this ImList<T> source, int index, Func<T, T> update)
        {
            if (source.IsEmpty || index < 0)
                return source;

            if (index == 0)
                return source.Tail.Prep(update(source.Head));

            // start from index 1
            var beginning = ImList<T>.Empty.Prep(source.Head);
            var remaining = source.Tail;
            for (var i = 1; !remaining.IsEmpty; remaining = remaining.Tail, ++i)
            {
                if (i == index)
                {
                    var value = remaining.Head;
                    var updatedValue = update(value);
                    if (ReferenceEquals(updatedValue, value) || Equals(updatedValue, value))
                        return source; // if item did not change, return the source

                    remaining = remaining.Tail.Prep(updatedValue);
                    return beginning.To(remaining, (it, _) => _.Prep(it));
                }
                beginning = beginning.Prep(remaining.Head);
            }

            // if index is ouside of the bounds, return original array
            return source;
        }

        public static ImList<T> Without<T>(this ImList<T> source, int index)
        {
            if (source.IsEmpty || index < 0)
                return source;

            if (index == 0)
                return source.Tail;

            // start from index 1
            var beginning = ImList<T>.Empty.Prep(source.Head);
            var remaining = source.Tail;
            for (var i = 1; !remaining.IsEmpty; remaining = remaining.Tail, ++i)
            {
                if (i == index)
                    return beginning.To(remaining.Tail, (it, _) => _.Prep(it));
                beginning = beginning.Prep(remaining.Head);
            }

            // if index is ouside of the bounds, return original array
            return source;
        }
    }
}
