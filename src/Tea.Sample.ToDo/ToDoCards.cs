using System.Text;
using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoCards : IComponent<ToDoCards>
    {
        public readonly ImList<ToDoList> Cards;

        public ToDoCards(ImList<ToDoList> cards)
        {
            Cards = cards;
        }

        public static ToDoCards Init()
        {
            return new ToDoCards(ImList<ToDoList>.Empty
                .Prepend(ToDoList.Init())
                .Prepend(ToDoList.Init()));
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{Cards=[");
            Cards.To(s, (it, i, _) => (i == 0 ? _ : _.Append(",")).Append(it.ToString()));
            s.Append("]}");
            return s.ToString();
        }

        public ToDoCards Update(IMsg<ToDoCards> msg)
        {
            if (msg is ItemChanged<ToDoList, ToDoCards> card)
                return new ToDoCards(
                    Cards.With(card.Index, it => it.Update(card.Msg)));
            return this;
        }

        public UI<IMsg<ToDoCards>> View()
        {
            return row(Cards.Map(Component.ViewIn<ToDoList, ToDoCards>));
        }
    }
}