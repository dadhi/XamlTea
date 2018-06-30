namespace Tea.Sample.ToDo
{
    using System.Text;
    using ImTools;
    using static UIElements;
    using static Component;
    using M = IMessage<ToDoApp>;

    public class ToDoApp : IComponent<ToDoApp>
    {
        public readonly ImList<ToDoList> Cards;

        public ToDoApp(ImList<ToDoList> cards)
        {
            Cards = cards;
        }

        public static ToDoApp Init() => 
            new ToDoApp(ToDoList.Init().Cons(ToDoList.Init()));

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
                return new ToDoApp(Cards.UpdateAt(card.Index, it => it.Update(card.Message)));
            return this;
        }

        public UI<M> View() => 
            row(Cards.Map(Component.In<ToDoList, ToDoApp>));
    }
}