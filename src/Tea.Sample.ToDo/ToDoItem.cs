using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoItem : IComponent<ToDoItem, ToDoItem.Msg>
    {
        public readonly string Text;
        public readonly bool IsDone;

        public ToDoItem(string text, bool isDone = false)
        {
            Text = text;
            IsDone = isDone;
        }

        public override string ToString()
        {
            return $"{{Text={Text},IsDone={IsDone}}}";
        }

        public abstract class Msg
        {
            public class StateChanged : Msg
            {
                public bool IsDone { get; private set; }
                public static Msg It(bool isDone) { return new StateChanged { IsDone = isDone }; }
            }
        }

        public ToDoItem Update(Msg msg)
        {
            if (msg is Msg.StateChanged stateChanged)
                return new ToDoItem(Text, stateChanged.IsDone);
            return this;
        }

        public UI<Msg> View()
        {
            return checkbox(Text, IsDone, Msg.StateChanged.It);
        }
    }
}