using System.Linq;
using ImTools;

namespace Tea.Sample.CounterList
{
    using static UIParts;

    public static class CounterList
    {
        public abstract class Msg
        {
            public sealed class Insert : Msg { public static readonly Msg It = new Insert(); }
            public sealed class Remove : Msg { public static readonly Msg It = new Remove(); }
            public sealed class Modify : Msg
            {
                public readonly int Index;
                public readonly Counter.Msg CounterMsg;
                public Modify(int index, Counter.Msg msg) { Index = index; CounterMsg = msg; }
                public static Msg It(int index, Counter.Msg msg) { return new Modify(index, msg); }
            }
        }

        public sealed class Model
        {
            public readonly ImList<Counter.Model> Counters;
            public Model(ImList<Counter.Model> counters) { Counters = counters; }
        }

        public static Model Update(Msg msg, Model model)
        {
            if (msg is Msg.Insert)
                return new Model(model.Counters.Push(new Counter.Model(0)));

            if (model.Counters.IsEmpty)
                return model;

            if (msg is Msg.Remove)
                return new Model(model.Counters.Tail);

            var modify = msg as Msg.Modify;
            if (modify != null)
            {
                var counterWithUpdatedOne = model.Counters.To(0, ImList<Counter.Model>.Empty, 
                    (counter, i, _) => _.Push(i != modify.Index ? counter
                        : Counter.Update(modify.CounterMsg, counter)));
                return new Model(counterWithUpdatedOne);
            }

            return model;
        }

        public static UI<Msg> View(Model model)
        {
            return div(Layout.Vertical, new[]
                { button("Add", Msg.Insert.It)
                , button("Remove", Msg.Remove.It)
                }.Append(model.Counters.To(0, ImList<UI<Msg>>.Empty, (c, i, _) => _.Push(
                    Counter.View(c).Map(m => Msg.Modify.It(i, m)))).Enumerate().ToArray()));
        }

        public static App<Msg, Model> App()
        {
            return UIApp.App(new Model(ImList<Counter.Model>.Empty), Update, View);
        }
    }
}
