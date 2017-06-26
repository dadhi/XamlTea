using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public static class ToDoItem
    {
        public class Model : IComponent<Model, Msg>
        {
            public readonly string Text;
            public readonly bool IsDone;

            public Model(string text, bool isDone = false)
            {
                Text = text;
                IsDone = isDone;
            }

            public override string ToString()
            {
                return $"{{Text={Text},IsDone={IsDone}}}";
            }

            public Model Update(Msg msg)
            {
                if (msg is Msg.StateChanged stateChanged)
                    return new Model(Text, stateChanged.IsDone);
                return this;
            }

            public UI<Msg> View()
            {
                return checkbox(Text, IsDone, Msg.StateChanged.It);
            }
        }

        public abstract class Msg
        {
            public class StateChanged : Msg
            {
                public bool IsDone { get; private set; }
                public static Msg It(bool isDone) { return new StateChanged { IsDone = isDone }; }
            }
        }
    }
}