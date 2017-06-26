using static Tea.UIParts;

namespace Tea.Sample.CounterList
{
    public sealed class Counter : IComponent<Counter, Counter.Msg>
    {
        public enum Msg { Increment, Decrement }

        public readonly int Count;
        public Counter(int count) { Count = count; }

        public Counter Update(Msg msg)
        {
            switch (msg)
            {
                case Msg.Increment: return new Counter(Count + 1);
                case Msg.Decrement: return new Counter(Count - 1);
            }
            return this;
        }

        public UI<Msg> View()
        {
            return panel(Layout.Horizontal
                , button("+", Msg.Increment)
                , button("-", Msg.Decrement)
                , text<Msg>(Count.ToString()));
        }
    }
}