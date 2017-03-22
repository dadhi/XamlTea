namespace Tea.Sample.CounterList
{
    using static UIParts;

    public static class Counter
    {
        public enum Msg { Increment, Decrement }

        public sealed class Model
        {
            public readonly int Count;
            public Model(int count) { Count = count; }
        }

        public static Model Update(Msg msg, Model model)
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
            return
                div(Layout.Horizontal
                    , button("+", Msg.Increment)
                    , button("-", Msg.Decrement)
                    , text<Msg>($"{model.Count}")); // todo: don't like Msg here
        }

        public static App<Msg, Model> App(int i)
        {
            return UIApp.App(new Model(i), Update, View);
        }
    }
}