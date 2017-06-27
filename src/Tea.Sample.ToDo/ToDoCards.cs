using System;
using System.Text;
using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoCards : IComponent<ToDoCards, ToDoCards.Msg>
    {
        public readonly ImList<ToDoList> Cards;
        public readonly ImList<ToDoCards> History;

        public ToDoCards(ImList<ToDoList> cards, ImList<ToDoCards> history)
        {
            Cards = cards;
            History = history;
        }

        public static ToDoCards Init()
        {
            return new ToDoCards(
                ImList<ToDoList>.Empty.Prep(ToDoList.Init()).Prep(ToDoList.Init()),
                ImList<ToDoCards>.Empty);
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{Cards=[");
            Cards.To(s, (it, i, _) => (i == 0 ? _ : _.Append(",")).Append(it.ToString()));
            s.Append("]}");
            return s.ToString();
        }

        public abstract class Msg
        {
            public class SetModel : Msg
            {
                public ToDoCards Model { get; private set; }
                public static Msg It(ToDoCards model) { return new SetModel { Model = model }; }
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

        public ToDoCards Update(Msg msg)
        {
            if (msg is Msg.SetModel setModel)
                return setModel.Model;

            if (msg is Msg.ItemChanged itemChanged)
                return new ToDoCards(
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
}