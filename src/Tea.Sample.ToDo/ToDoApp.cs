using System.Text;
using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoApp : IComponent<ToDoApp>
    {
        public readonly ImList<ToDoList> Cards;

        public ToDoApp(ImList<ToDoList> cards)
        {
            Cards = cards;
        }

        public static ToDoApp Init() => 
            new ToDoApp(ImList<ToDoList>.Empty.Prepend(ToDoList.Init()).Prepend(ToDoList.Init()));

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{Cards=[");
            Cards.Fold(s, (it, i, _) => (i == 0 ? _ : _.Append(",")).Append(it.ToString()));
            s.Append("]}");
            return s.ToString();
        }

        public ToDoApp Update(IMsg<ToDoApp> msg)
        {
            if (msg is ItemChanged<ToDoList, ToDoApp> card)
                return new ToDoApp(Cards.UpdateAt(card.Index, it => it.Update(card.Msg)));
            return this;
        }

        public UI<IMsg<ToDoApp>> View()
        {
            return row(Cards.Map(Component.ViewIn<ToDoList, ToDoApp>));
        }
    }
}