using System;
using System.Text;
using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public static class ToDoCards
    {
        public class Model : IComponent<Model, Msg>
        {
            public readonly ImList<ToDoList.Model> Cards;

            public readonly ImList<Model> History;

            public Model(ImList<ToDoList.Model> cards, ImList<Model> history)
            {
                Cards = cards;
                History = history;
            }

            public override string ToString()
            {
                var s = new StringBuilder();
                s.Append("{Cards=[");
                Cards.To(s, (it, i, _) => (i == 0 ? _ : _.Append(",")).Append(it.ToString()));
                s.Append("]}");
                return s.ToString();
            }

            public Model Update(Msg msg)
            {
                if (msg is Msg.SetModel setModel)
                    return setModel.Model;

                if (msg is Msg.ItemChanged itemChanged)
                    return new Model(
                       Cards.With(itemChanged.ItemIndex, it => it.Update(itemChanged.ItemMsg)),
                       History.Prep(this));

                return this;
            }

            public UI<Msg> View()
            {
                return
                    panel(Layout.Vertical,
                        panel(Layout.Horizontal,
                            Cards.Map((it, i) => it.View().MapMsg(Msg.ItemChanged.It(i)))
                        ),
                        panel(Layout.Vertical,
                            History.Map(m =>
                                panel(Layout.Horizontal,
                                    button("apply", Msg.SetModel.It(m)),
                                    text<Msg>(m.ToString())))
                        )
                    );

            }
        }

        public abstract class Msg
        {
            public class SetModel : Msg
            {
                public Model Model { get; private set; }
                public static Msg It(Model model) { return new SetModel { Model = model }; }
            }

            public class ItemChanged : Msg
            {
                public int ItemIndex { get; private set; }
                public ToDoList.Msg ItemMsg { get; private set; }

                public static Func<ToDoList.Msg, Msg> It(int index)
                {
                    return msg => new ItemChanged { ItemIndex = index, ItemMsg = msg };
                }
            }
        }

        public static Model Init()
        {
            return new Model(
                ImList<ToDoList.Model>.Empty.Prep(ToDoList.Init()).Prep(ToDoList.Init()),
                ImList<Model>.Empty);
        }
    }
}