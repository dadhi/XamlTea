using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoItem : IComponent<ToDoItem>
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

        public class IsDoneChanged : IMsg<ToDoItem>
        {
            public bool IsDone { get; private set; }
            public static IMsg<ToDoItem> It(bool isDone) => new IsDoneChanged { IsDone = isDone };
        }

        public class TextChanged : IMsg<ToDoItem>
        {
            public string Text { get; private set; }
            public static IMsg<ToDoItem> It(string text) => new TextChanged { Text = text };
        }

        public ToDoItem Update(IMsg<ToDoItem> msg)
        {
            if (msg is IsDoneChanged isDoneChanged)
                return new ToDoItem(Text, isDoneChanged.IsDone);
            if (msg is TextChanged textChanged)
                return new ToDoItem(textChanged.Text, IsDone);
            return this;
        }

        public UI<IMsg<ToDoItem>> View()
        {
            return IsDone 
                ? checkbox(Text, IsDone, IsDoneChanged.It)
                : row(
                    checkbox(string.Empty, IsDone, IsDoneChanged.It),
                    input(Text, TextChanged.It));
        }
    }
}