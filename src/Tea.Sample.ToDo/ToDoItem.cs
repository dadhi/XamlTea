namespace Tea.Sample.ToDo
{
    public static class ToDoItem
    {
        public class Model
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
        }

        public abstract class Msg
        {
            public class StateChanged : Msg
            {
                public bool IsDone { get; private set; }
                public static Msg It(bool isDone) { return new StateChanged { IsDone = isDone }; }
            }

            public class Remove : Msg
            {
                public static readonly Msg It = new Remove();
            }
        }

        public static Model Update(this Model model, Msg msg)
        {
            var stateChanged = msg as Msg.StateChanged;
            if (stateChanged != null)
                return new Model(model.Text, stateChanged.IsDone);
            return model;
        }

        public static UI<Msg> View(this Model model)
        {
            return UIParts.panel(Layout.Horizontal,
                UIParts.checkbox(model.Text, model.IsDone, Msg.StateChanged.It),
                UIParts.button("remove", Msg.Remove.It));
        }
    }
}