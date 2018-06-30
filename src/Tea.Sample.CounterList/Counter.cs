namespace Tea.Sample.CounterList
{
    using static UIElements;
    using M = IMessage<Counter>;

    public class Counter : IComponent<Counter>
    {
        public readonly int Count;
        public Counter(int count) => Count = count;

        public class Message : M
        {
            public static readonly M Increment = new Message();
            public static readonly M Decrement = new Message();
        }

        public Counter Update(M message) =>
            message == Message.Increment ? new Counter(Count + 1) :
            message == Message.Decrement ? new Counter(Count - 1) :
            this;

        public UI<M> View() =>
            row(button("+", Message.Increment), button("-", Message.Decrement), text<M>(Count));
    }
}