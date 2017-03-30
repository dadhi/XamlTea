using ImTools;

namespace Tea.Sample.CounterList
{
    using static UIParts;

    public static class CounterList
    {
        public sealed class Model
        {
            public readonly ImList<Counter.Model> Counters;
            public Model(ImList<Counter.Model> counters) { Counters = counters; }
        }

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

        public static Model Update(Model model, Msg msg)
        {
            if (msg is Msg.Insert)
                return new Model(model.Counters.Prep(new Counter.Model(0)));

            if (model.Counters.IsEmpty)
                return model;

            if (msg is Msg.Remove)
                return new Model(model.Counters.Tail);

            var modify = msg as Msg.Modify;
            if (modify != null)
                return new Model(model.Counters.Map((c, i) =>
                    i != modify.Index ? c : c.Update(modify.CounterMsg)));

            return model;
        }

        public static UI<Msg> View(Model model)
        {
            var counterViews = model.Counters
                .Map((c, i) => Counter.View(c).MapMsg(msg => Msg.Modify.It(i, msg)))
                .ToArray();

            return div(Layout.Vertical, new[]
                { button("Add", Msg.Insert.It)
                , button("Remove", Msg.Remove.It)
                }.Append(counterViews));
        }

        public static App<Msg, Model> App()
        {
            return UIApp.App(new Model(ImList<Counter.Model>.Empty), Update, View);
        }
    }
}
