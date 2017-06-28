using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoItem : IComponent<ToDoItem, IMsg<ToDoItem.Msg>>
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

        public enum Msg
        {
            StateChanged
        }

        public class StateChanged : IMsg<Msg>
        {
            public Msg Type => Msg.StateChanged;
            public bool IsDone { get; private set; }
            public static IMsg<Msg> It(bool isDone) => new StateChanged { IsDone = isDone };
        }

        public ToDoItem Update(IMsg<Msg> msg)
        {
            if (msg is StateChanged stateChanged)
                return new ToDoItem(Text, stateChanged.IsDone);
            return this;
        }

        public UI<IMsg<Msg>> View()
        {
            return checkbox(Text, IsDone, StateChanged.It);
        }
    }
}