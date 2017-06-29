using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoItem : IComponent<ToDoItem, IMsg<ToDoItem>>
    {
        public readonly string Text;
        public readonly bool IsDone;

        public ToDoItem(string text, bool isDone = false)
        {
            Text = text;
            IsDone = isDone;
        }

        public override string ToString() => $"{{Text={Text},IsDone={IsDone}}}";

        public class StateChanged : IMsg<ToDoItem>
        {
            public bool IsDone { get; private set; }
            public static IMsg<ToDoItem> It(bool isDone) => new StateChanged { IsDone = isDone };
        }

        public ToDoItem Update(IMsg<ToDoItem> msg)
        {
            if (msg is StateChanged stateChanged)
                return new ToDoItem(Text, stateChanged.IsDone);
            return this;
        }

        public UI<IMsg<ToDoItem>> View()
        {
            return checkbox(Text, IsDone, StateChanged.It);
        }
    }
}