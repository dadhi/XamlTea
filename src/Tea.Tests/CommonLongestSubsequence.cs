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
        None,
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
            var s = source.Count;
            var t = target.Count;

            var cls = new int[s + 1, t + 1];
            for (var i = 1; i <= s; i++)
            {
                for (var j = 1; j <= t; j++)
                {
                    if (Equals(source[i - 1], target[j - 1]))
                        cls[i, j] = cls[i - 1, j - 1] + 1;
                    else
                        cls[i, j] = Math.Max(cls[i, j - 1], cls[i - 1, j]);
                }
            }

            return Diff(new List<DiffAction<T>>(), cls, source, target, s, t);
        }

        /// Calculates list of actions to make a target list from source list
        public static List<DiffAction<T>> Diff2<T>(this IList<T> source, IList<T> target) 
        {
            //function LCSLength(X[1..m], Y[1..n])
            //C = array(0..m, 0..n)
            //for i := 0..m
            //C[i, 0] = 0
            //for j := 0..n
            //C[0, j] = 0
            //for i := 1..m
            //for j := 1..n
            //if X[i] = Y[j]
            //C[i, j] := C[i - 1, j - 1] + 1
            //else
            //C[i, j] := max(C[i, j - 1], C[i - 1, j])
            //return C[m, n]


            //function LCS(X[1..m], Y[1..n])
            //start:= 1
            //m_end:= m
            //n_end:= n
            //trim off the matching items at the beginning
            //while start ≤ m_end and start ≤ n_end and X[start] = Y[start]
            //start:= start + 1
            //trim off the matching items at the end
            //while start ≤ m_end and start ≤ n_end and X[m_end] = Y[n_end]
            //m_end:= m_end - 1
            //n_end:= n_end - 1
            //C = array(start - 1..m_end, start - 1..n_end)

            //only loop over the items that have changed
            //for i := start..m_end
            //for j := start..n_end

            //the algorithm continues as before...

            // Shrink down the equal elements from the both sides of sequences
            var n = 0;
            var s = source.Count;
            var t = target.Count;
            for (; n < s && n < t && Equals(source[n], target[n]); ++n) { }
            for (; n < s && n < t && Equals(source[s], target[t]); --s, --t) { }
            if (n == s && n == t)
                return new List<DiffAction<T>>(0);

            var cls = new int[s - n + 1, t - n + 1];

            for (var i = n; i <= s; i++)
            for (var j = n; j <= t; j++)
                if (Equals(source[i - 1], target[j - 1]))
                    cls[i - n, j - n] = cls[i - n - 1, j - n - 1] + 1;
                else
                    cls[i - n, j - n] = Math.Max(cls[i - n, j - n - 1], cls[i - n - 1, j - n]);

            return Diff(new List<DiffAction<T>>(), cls, source, target, s, t, n);
        }

        private static List<DiffAction<T>> Diff<T>(this List<DiffAction<T>> actions,
            int[,] cls, IList<T> source, IList<T> target, int s, int t, int start = 0)
        {
            if (s > start && t > start && Equals(source[s - 1], target[t - 1]))
            {
                actions = actions.Diff(cls, source, target, s - 1, t - 1, start);
                actions.Add(new DiffAction<T>(DiffActionType.None, source[s - 1], target[t - 1]));
            }
            else if (t > start && (s == start || cls[s, t - 1] >= cls[s - 1, t]))
            {
                actions = actions.Diff(cls, source, target, s, t - 1, start);
                actions.Add(new DiffAction<T>(DiffActionType.Add, default, target[t - 1]));
            }
            else if (s > start && (t == start || cls[s, t - 1] < cls[s - 1, t]))
            {
                actions = actions.Diff(cls, source, target, s - 1, t, start);
                actions.Add(new DiffAction<T>(DiffActionType.Remove, source[s - 1], default));
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
                if (action.ActionType == DiffActionType.None)
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
