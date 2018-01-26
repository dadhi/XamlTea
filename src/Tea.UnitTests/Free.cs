using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

using static System.Console;
using static System.Linq.Enumerable;

using static Free.Unit;
using static Free.IOExt;
using static Free.MyIO;

namespace Free
{
    [TestFixture]
    class Program
    {
        [Test]
        public static async Task Main()
        {
            // Compose, e.g. describe program without running it
            var program = PrefixLines("d:/some_text_file.txt");

            // Run program by interpreting its operations
            MockInterpreter.Interpet(program);
            MockInterpreter.Interpet(program, skipLogging: true);
            LiveInterpreter.Interpet(program);
            await LiveInterpreterAsync.Interpet(program);
        }

        // Program description
        private static IO<Unit> PrefixLines(string path) =>
              from lines in ReadAllLines(path)
              from a in Log($"There are {lines.Count()} lines")
              from b in Log("Prepend line numbers")
              let newLines = Range(1, int.MaxValue).Zip(lines, (i, line) => $"{i}: {line}")
              let newFile = path + ".prefixed"
              from c in WriteAllLines(newFile, newLines)
              from d in Log($"Lines prepended and file saved successfully to \"{newFile}\"")
              select unit;
    }

    public struct WriteAllLines
    {
        public string Path;
        public IEnumerable<string> Lines;
        public WriteAllLines(string path, IEnumerable<string> lines)
        {
            Path = path;
            Lines = lines;
        }
    }

    public struct ReadAllLines
    {
        public string Path;
        public ReadAllLines(string path) => Path = path;
    }

    public struct Log
    {
        public string Message;
        public Log(string message) => Message = message;
    }

    public static class MyIO
    {
        public static IO<Unit> WriteAllLines(string path, IEnumerable<string> lines) =>
            NewIO(new WriteAllLines(path, lines));

        public static IO<IEnumerable<string>> ReadAllLines(string path) =>
            NewIO<ReadAllLines, IEnumerable<string>>(new ReadAllLines(path));

        public static IO<Unit> Log(string message) =>
            NewIO(new Log(message));
    }

    public static class LiveInterpreter
    {
        public static A Interpet<A>(IO<A> m) =>
            m is Return<A> r ? r.Result
            : m is IO<ReadAllLines, IEnumerable<string>, A> ra ? Interpet(ra.Do(File.ReadAllLines(ra.Op.Path)))
            : m is IO<WriteAllLines, Unit, A> wa ? Interpet(wa.Do(f(() => File.WriteAllLines(wa.Op.Path, wa.Op.Lines))))
            : m is IO<Log, Unit, A> log ? Interpet(log.Do(f(() => WriteLine(log.Op.Message))))
            : throw new NotSupportedException($"Not supported operation {m}");
    }

    public static class LiveInterpreterAsync
    {
        public static async Task<A> Interpet<A>(IO<A> m) =>
            m is Return<A> r ? r.Result
            : m is IO<ReadAllLines, IEnumerable<string>, A> ra ? await Interpet(ra.Do(await ReadAllLines(ra.Op.Path)))
            : m is IO<WriteAllLines, Unit, A> wa ? await Interpet(wa.Do(await WriteAllLines(wa.Op.Path, wa.Op.Lines)))
            : m is IO<Log, Unit, A> log ? await Interpet(log.Do(f(() => WriteLine(log.Op.Message))))
            : throw new NotSupportedException();

        static Task<Unit> WriteAllLines(string path, IEnumerable<string> output) => 
            Task.Run(() => f(() => File.WriteAllLines(path, output)));

        static Task<IEnumerable<string>> ReadAllLines(string path) =>
            Task.Run<IEnumerable<string>>(() => File.ReadAllLines(path));
    }

    public static class MockInterpreter
    {
        // Example of non-recursive (stack-safe) interpreter
        public static A Interpet<A>(IO<A> m, bool skipLogging = false)
        {
            while (true)
                switch (m)
                {
                    case Return<A> x:
                        return x.Result;
                    case IO<ReadAllLines, IEnumerable<string>, A> x:
                        m = x.Do(MockReadAllLines(x.Op.Path));
                        break;
                    case IO<WriteAllLines, Unit, A> x:
                        m = x.Do(unit); // do nothing, not interested in output
                        break;
                    case IO<Log, Unit, A> log:
                        m = skipLogging ? log.Do(unit) : log.Do(f(() => WriteLine(log.Op.Message)));
                        break;
                    default: throw new NotSupportedException($"Not supported operation {m}");
                }
        }

        static IEnumerable<string> MockReadAllLines(string path) =>
            new[] { "Hello", "World", path };
    }

    // Monadic IO implementation, can be reused, published to NuGet, etc.
    //-------------------------------------------------------------------

    public interface IO<A>
    {
        IO<B> Bind<B>(Func<A, IO<B>> f);
    }

    public sealed class Return<A> : IO<A>
    {
        public readonly A Result;
        public Return(A a) { Result = a; }
        public IO<B> Bind<B>(Func<A, IO<B>> f) => f(Result);
    }

    public class IO<O, R, A> : IO<A>
    {
        public readonly O Op;
        public readonly Func<R, IO<A>> Do;
        public IO(O op, Func<R, IO<A>> @do)
        {
            Op = op;
            Do = @do;
        }

        public IO<B> Bind<B>(Func<A, IO<B>> f) => new IO<O, R, B>(Op, r => Do(r).Bind(f));
    }

    public static class IOExt
    {
        public static IO<A> IO<A>(this A a) =>
            new Return<A>(a);

        public static IO<B> Select<A, B>(this IO<A> m, Func<A, B> f) =>
            m.Bind(a => f(a).IO());

        public static IO<C> SelectMany<A, B, C>(this IO<A> m, Func<A, IO<B>> f, Func<A, B, C> project) =>
            m.Bind(a => f(a).Bind(b => project(a, b).IO()));

        // Simplify IO operations creation a bit
        public static IO<Unit> NewIO<O>(O op) => new IO<O, Unit, Unit>(op, IO);
        public static IO<R> NewIO<O, R>(O op) => new IO<O, R, R>(op, IO);
    }

    public sealed class Unit
    {
        public static readonly Unit unit = new Unit();

        public static Unit f(Action a) { a(); return unit; }

        private Unit() { }
    }
}
