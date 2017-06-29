using System.Text;
using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoCards : IComponent<ToDoCards, IMsg<ToDoCards>>
    {
        public readonly ImList<ToDoList> Cards;

        public ToDoCards(ImList<ToDoList> cards)
        {
            Cards = cards;
        }

        public static ToDoCards Init()
        {
            return new ToDoCards(ImList<ToDoList>.Empty.Prep(ToDoList.Init()).Prep(ToDoList.Init()));
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{Cards=[");
            Cards.To(s, (it, i, _) => (i == 0 ? _ : _.Append(",")).Append(it.ToString()));
            s.Append("]}");
            return s.ToString();
        }

        public class SetModel : IMsg<ToDoCards>
        {
            public ToDoCards Model { get; private set; }
            public static IMsg<ToDoCards> It(ToDoCards model) { return new SetModel { Model = model }; }
        }

        public ToDoCards Update(IMsg<ToDoCards> msg)
        {
            if (msg is ItemChanged<IMsg<ToDoList>, ToDoCards> itemChanged)
                return new ToDoCards(
                    Cards.With(itemChanged.Index, it => it.Update(itemChanged.Msg)));
            return this;
        }

        public UI<IMsg<ToDoCards>> View()
        {
            return
                panel(Layout.Horizontal,
                    Cards.Map((it, i) => it.View<IMsg<ToDoList>, ToDoCards>(i))
                );
        }
    }
}