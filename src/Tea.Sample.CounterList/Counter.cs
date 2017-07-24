using static Tea.UIParts;

namespace Tea.Sample.CounterList
{
    public sealed class Counter : IComponent<Counter>
    {
        public class Msg : IMsg<Counter>
        {
            public static readonly IMsg<Counter> Increment = new Msg();
            public static readonly IMsg<Counter> Decrement = new Msg();
        }

        public readonly int Count;
        public Counter(int count) { Count = count; }

        public Counter Update(IMsg<Counter> msg)
        {
            if (msg == Msg.Increment)
                return new Counter(Count + 1);

            if (msg == Msg.Decrement)
                return new Counter(Count - 1);

            return this;
        }

        public UI<IMsg<Counter>> View()
        {
            return row(
                button("+", Msg.Increment), 
                button("-", Msg.Decrement), 
                text<IMsg<Counter>>(Count.ToString()));
        }
    }
}