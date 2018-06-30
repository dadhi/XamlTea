namespace Tea.Sample.ToDo
{
    using static UIElements;
    using M = IMessage<ToDoItem>;

    public class ToDoItem : IComponent<ToDoItem>
    {
        public readonly string Text;
        public readonly bool IsDone;

        public ToDoItem(string text, bool isDone = false)
        {
            Text = text;
            IsDone = isDone;
        }

        public override string ToString() => "'" + Text + (IsDone ? "';done" : "'");

        public class IsDoneChanged : M
        {
            public bool IsDone { get; private set; }
            public static M It(bool isDone) => new IsDoneChanged { IsDone = isDone };
        }

        public class TextChanged : M
        {
            public string Text { get; private set; }
            public static M It(string text) => new TextChanged { Text = text };
        }

        public ToDoItem Update(M message)
        {
            if (message is IsDoneChanged isDoneChanged)
                return new ToDoItem(Text, isDoneChanged.IsDone);
            if (message is TextChanged textChanged)
                return new ToDoItem(textChanged.Text, IsDone);
            return this;
        }

        public UI<M> View() => 
            IsDone
            ? check(Text, IsDone, IsDoneChanged.It)
            : row(check("", IsDone, IsDoneChanged.It), input(Text, TextChanged.It));
    }
}