using System.Text;
using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoCards : IComponent<ToDoCards, IMsg<ToDoCards.MsgType>>
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

        public enum MsgType { SetModel, CardChanged }

        public class SetModel : IMsg<MsgType>
        {
            public MsgType Type => MsgType.SetModel;

            public ToDoCards Model { get; private set; }
            public static IMsg<MsgType> It(ToDoCards model) { return new SetModel { Model = model }; }
        }

        public ToDoCards Update(IMsg<MsgType> msg)
        {
            if (msg is SetModel setModel)
                return setModel.Model;

            if (msg is ItemChanged<IMsg<ToDoList.MsgType>, MsgType> itemChanged)
                return new ToDoCards(
                   Cards.With(itemChanged.ItemIndex, it => it.Update(itemChanged.ItemMsg)),
                   History.Prep(this));

            return this;
        }

        public UI<IMsg<MsgType>> View()
        {
            return
                panel(Layout.Vertical,
                    panel(Layout.Horizontal,
                        Cards.Map((it, i) => it.View(i, MsgType.CardChanged))
                    ),
                    panel(Layout.Vertical,
                        History.Map(m =>
                            panel(Layout.Horizontal,
                                button("apply", SetModel.It(m)),
                                text<IMsg<MsgType>>(m.ToString())))
                    )
                );
        }
    }
}