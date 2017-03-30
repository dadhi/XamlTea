namespace Tea.Sample.CounterList
{
    using static UIParts;

    public static class Counter
    {
        public sealed class Model
        {
            public readonly int Count;
            public Model(int count) { Count = count; }
        }

        public enum Msg { Increment, Decrement }

        public static Model Update(this Model model, Msg msg)
        {
            switch (msg)
            {
                case Msg.Increment: return new Model(model.Count + 1);
                case Msg.Decrement: return new Model(model.Count - 1);
            }
            return model;
        }

        public static UI<Msg> View(Model model)
        {
            return div(Layout.Horizontal
                , button("+", Msg.Increment)
                , button("-", Msg.Decrement)
                , text<Msg>($"{model.Count}")); // todo: don't like Msg here
        }
    }
}