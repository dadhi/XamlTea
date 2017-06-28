using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoItem : IComponent<ToDoItem, IMsg<ToDoItem.MsgType>>
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

        public enum MsgType
        {
            StateChanged
        }

        public class StateChanged : IMsg<MsgType>
        {
            public MsgType Type => MsgType.StateChanged;
            public bool IsDone { get; private set; }
            public static IMsg<MsgType> It(bool isDone) => new StateChanged { IsDone = isDone };
        }

        public ToDoItem Update(IMsg<MsgType> msg)
        {
            if (msg is StateChanged stateChanged)
                return new ToDoItem(Text, stateChanged.IsDone);
            return this;
        }

        public UI<IMsg<MsgType>> View()
        {
            return checkbox(Text, IsDone, StateChanged.It);
        }
    }
}