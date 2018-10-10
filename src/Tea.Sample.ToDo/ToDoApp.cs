namespace Tea.Sample.ToDo
{
    using System.Text;
    using ImTools;
    using static UIElements;
    using M = IMessage<ToDoApp>;

    public class ToDoApp : IComponent<ToDoApp>
    {
        public readonly ImZipper<ToDoList> Cards;
        public ToDoApp(ImZipper<ToDoList> cards) => Cards = cards;

        public static ToDoApp Init() => 
            new ToDoApp(ImZipper<ToDoList>.Empty.Append(ToDoList.Init()).Append(ToDoList.Init()));

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{Cards=[");
            Cards.Fold(s, (it, i, _) => (i == 0 ? _ : _.Append(",")).Append(it.ToString()));
            s.Append("]}");
            return s.ToString();
        }

        public ToDoApp Update(M message)
        {
            if (message is ChildChanged<ToDoList, ToDoApp> card)
                return new ToDoApp(Cards.UpdateAt(card.Index, x => x.Update(card.Message)));
            return this;
        }

        public UI<M> View() => row(Cards.Map(Component.View<ToDoList, ToDoApp>));
    }
}
