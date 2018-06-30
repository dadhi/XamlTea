namespace Tea.Sample.CounterList
{
    using ImTools;
    using static UIElements;
    using M = IMessage<CounterList>;

    public class CounterList : IComponent<CounterList>
    {
        public readonly ImList<Counter> Counters;
        public CounterList(ImList<Counter> counters) => Counters = counters;

        public static CounterList Init() => new CounterList(ImList<Counter>.Empty);

        public abstract class Message : M
        {
            public class Insert : Message { public static readonly M It = new Insert(); }
            public class Remove : Message { public static readonly M It = new Remove(); }
        }

        public CounterList Update(M message)
        {
            if (message is Message.Insert)
                return new CounterList(Counters.Prepend(new Counter(0)));

            if (Counters.IsEmpty)
                return this;

            if (message is Message.Remove)
                return new CounterList(Counters.Tail);

            if (message is ChildChanged<Counter, CounterList> modify)
                return new CounterList(Counters.UpdateAt(modify.Index, c => c.Update(modify.Message)));

            return this;
        }

        public UI<M> View() => 
            column(
                button("Add", Message.Insert.It),
                button("Remove", Message.Remove.It),
                column(Counters.Map((c, i) => c.In<Counter, CounterList>(i))));
    }
}
