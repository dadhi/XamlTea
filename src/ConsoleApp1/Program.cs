using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<ToListVsToArray>();
        }
    }

    [MemoryDiagnoser]
    public class ToListVsToArray
    {
        private static readonly IEnumerable<Type> _types = typeof(Enumerable).Assembly.DefinedTypes.TakeWhile((_, i) => i < 1000);

        [Benchmark(Baseline = true)]
        public object ToList() => _types.ToList();

        [Benchmark]
        public object ToArray() => _types.ToArray();
    }
}
