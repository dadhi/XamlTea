using ImTools;
using static Tea.UIParts;
using static Tea.Props;

namespace Tea.Sample.CounterList
{
    public sealed class CounterList : IComponent<CounterList>
    {
        public abstract class Msg : IMsg<CounterList>
        {
            public sealed class Insert : Msg { public static readonly IMsg<CounterList> It = new Insert(); }
            public sealed class Remove : Msg { public static readonly IMsg<CounterList> It = new Remove(); }
        }

        public static readonly CounterList Initial = new CounterList(ImList<Counter>.Empty);

        public readonly ImList<Counter> Counters;
        public CounterList(ImList<Counter> counters) { Counters = counters; }

        public CounterList Update(IMsg<CounterList> msg)
        {
            if (msg is Msg.Insert)
                return new CounterList(Counters.Prep(new Counter(0)));

            if (Counters.IsEmpty)
                return this;

            if (msg is Msg.Remove)
                return new CounterList(Counters.Tail);

            if (msg is ItemChanged<Counter, CounterList> modify)
                return new CounterList(Counters.With(modify.Index, c => c.Update(modify.Msg)));

            return this;
        }

        public UI<IMsg<CounterList>> View()
        {
            var counterViews = Counters
                .Map((c, i) => c.View<Counter, CounterList>(i))
                .ToArray();

            return panel(Layout.Vertical, props(), new[]
            {
                button("Add", Msg.Insert.It),
                button("Remove", Msg.Remove.It)
            }.Append(counterViews));
        }
    }
}
