using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Tea;

namespace Tea
{
    /// Action type
    public enum DiffActionType
    {
        /// Changed item
        Update,
        /// Added
        Add,
        /// Removed
        Remove,
    }

    /// Action to do on list element to make it like in the target
    public struct DiffAction<T>
    {
        /// Action type
        public readonly DiffActionType ActionType;

        /// Source
        public readonly T Source;

        /// Target
        public readonly T Target;

        /// Constructor
        public DiffAction(DiffActionType type, T source, T target) =>
            (ActionType, Source, Target) = (type, source, target);

        /// Pretty prints the action
        public override string ToString() => $"{ActionType}: {Source} -> {Target}";
    }

    /// Optimized CLS from : http://en.wikipedia.org/wiki/Longest_common_subsequence_problem
    public static class Cls
    {
        /// Calculates list of actions to make a target list from source list
        public static List<DiffAction<T>> Diff<T>(this IEnumerable<T> source, IEnumerable<T> target) =>
            (source as IList<T> ?? source.ToList()).Diff(target as IList<T> ?? target.ToList());

        /// Calculates list of actions to make a target list from source list
        public static List<DiffAction<T>> Diff<T>(this IList<T> source, IList<T> target)
        {
            var m = source.Count;
            var n = target.Count;

            var c = new int[m + 1, n + 1];
            for (var i = 1; i <= m; i++)
            {
                for (var j = 1; j <= n; j++)
                {
                    if (Equals(source[i - 1], target[j - 1]))
                    {
                        c[i, j] = c[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        c[i, j] = Math.Max(c[i, j - 1], c[i - 1, j]);
                    }
                }
            }

            return Diff(new List<DiffAction<T>>(), c, source, target, m, n);
        }

        private static List<DiffAction<T>> Diff<T>(this List<DiffAction<T>> actions,
            int[,] c, IList<T> s, IList<T> t, int m, int n)
        {
            if (m > 0 && n > 0 && Equals(s[m - 1], t[n - 1]))
            {
                actions = actions.Diff(c, s, t, m - 1, n - 1);
                actions.Add(new DiffAction<T>(DiffActionType.Update, s[m - 1], t[n - 1]));
            }
            else if (n > 0 && (m == 0 || c[m, n - 1] >= c[m - 1, n]))
            {
                actions = actions.Diff(c, s, t, m, n - 1);
                actions.Add(new DiffAction<T>(DiffActionType.Add, default, t[n - 1]));
            }
            else if (m > 0 && (n == 0 || c[m, n - 1] < c[m - 1, n]))
            {
                actions = actions.Diff(c, s, t, m - 1, n);
                actions.Add(new DiffAction<T>(DiffActionType.Remove, s[m - 1], default));
            }

            return actions;
        }

        /// <summary>Pretty printing the diff action list.</summary>
        public static string Print<T>(this List<DiffAction<T>> actions) =>
            new StringBuilder().Print(actions).ToString();

        /// <summary>Pretty printing the diff action list.</summary>
        public static StringBuilder Print<T>(this StringBuilder sb, List<DiffAction<T>> actions)
        {
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action.ActionType == DiffActionType.Update)
                    sb.Append(action.Source);
                else if (action.ActionType == DiffActionType.Add)
                    sb.Append("+(").Append(action.Target).Append(")");
                else if (action.ActionType == DiffActionType.Remove)
                    sb.Append("-(").Append(action.Source).Append(")");
            }

            return sb;
        }
    }
}

namespace ClsTests
{
    public class Tests
    {
        [TestCase("", "", "")]
        [TestCase("", "a", "+(a)")]
        [TestCase("a", "", "-(a)")]
        [TestCase("a", "a", "a")]
        [TestCase("a", "b", "-(a)+(b)")]
        [TestCase("ab", "ab", "ab")]
        [TestCase("abc", "ab", "ab-(c)")]
        [TestCase("ab", "abc", "ab+(c)")]
        [TestCase("ab", "zab", "+(z)ab")]
        [TestCase("ab", "b", "-(a)b")]
        [TestCase("abc", "ac", "a-(b)c")]
        [TestCase("abc", "a", "a-(b)-(c)")]
        [TestCase("abc", "c", "-(a)-(b)c")]
        [TestCase("abc", "", "-(a)-(b)-(c)")]
        public void SimpleCases(string s, string t, string expected)
        {
            var diff = s.Diff(t);
            Assert.AreEqual(expected, diff.Print());
        }

        [Test]
        public void Insert()
        {
            var s = "ab";
            var t = "abc";
            var diff = s.Diff(t);
            Assert.AreEqual("ab+(c)", diff.Print());
        }
    }
}
