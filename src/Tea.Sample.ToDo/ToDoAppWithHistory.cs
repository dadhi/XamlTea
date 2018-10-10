namespace Tea.Sample.ToDo
{
    using ImTools;
    using static UIElements;
    using M = IMessage<ToDoAppWithHistory>;

    public class ToDoAppWithHistory : IComponent<ToDoAppWithHistory>
    {
        public readonly ImZipper<ToDoApp> History;
        public readonly ToDoApp App;

        public ToDoAppWithHistory(ImZipper<ToDoApp> history, ToDoApp app)
        {
            History = history;
            App = app;
        }

        public static ToDoAppWithHistory Init() => 
            new ToDoAppWithHistory(ImZipper<ToDoApp>.Empty, ToDoApp.Init());

        public struct Restore : M
        {
            public ToDoApp Model;
            public static M It(ToDoApp model) => new Restore { Model = model };
        }

        public ToDoAppWithHistory Update(M message)
        {
            switch (message)
            {
                case Restore restore: 
                    return new ToDoAppWithHistory(History, restore.Model);
                case ChildChanged<ToDoApp, ToDoAppWithHistory> todoAppChanged:
                    return new ToDoAppWithHistory(History.Append(App), App.Update(todoAppChanged.Message));
                default:
                    return this;
            }
        }

        public UI<M> View() => 
            column(
                App.View(this),
                column(History.Map(app => row(button("Restore", Restore.It(app)), text<M>(app)))));
    }
}
