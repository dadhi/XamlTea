using ImTools;
using static Tea.UIParts;
using static Tea.Props;

namespace Tea.Sample.CounterList
{
    public sealed class CounterList : IComponent<CounterList, CounterList.Msg>
    {
        public abstract class Msg
        {
            public sealed class Insert : Msg { public static readonly Msg It = new Insert(); }
            public sealed class Remove : Msg { public static readonly Msg It = new Remove(); }
            public sealed class Modify : Msg
            {
                public int Index { get; private set; }
                public Counter.Msg CounterMsg { get; private set; }
                public static Msg It(int index, Counter.Msg msg) { return new Modify { Index = index, CounterMsg = msg }; }
            }
        }

        public static readonly CounterList Initial = new CounterList(ImList<Counter>.Empty);

        public readonly ImList<Counter> Counters;
        public CounterList(ImList<Counter> counters) { Counters = counters; }

        public CounterList Update(Msg msg)
        {
            if (msg is Msg.Insert)
                return new CounterList(Counters.Prep(new Counter(0)));

            if (Counters.IsEmpty)
                return this;

            if (msg is Msg.Remove)
                return new CounterList(Counters.Tail);

            if (msg is Msg.Modify modify)
                return new CounterList(Counters.With(modify.Index, c => c.Update(modify.CounterMsg)));

            return this;
        }

        public UI<Msg> View()
        {
            var counterViews = Counters
                .Map((c, i) => c.View().Wrap(msg => Msg.Modify.It(i, msg)))
                .ToArray();

            return panel(Layout.Vertical, props(), new[]
            { button("Add", Msg.Insert.It)
                    , button("Remove", Msg.Remove.It)
                }.Append(counterViews));
        }
    }
}
