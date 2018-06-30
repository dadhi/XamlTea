namespace Tea.Sample.ToDo
{
    using ImTools;
    using static UIElements;
    using M = IMessage<ToDoAppWithHistory>;

    public class ToDoAppWithHistory : IComponent<ToDoAppWithHistory>
    {
        public readonly ImList<ToDoApp> History;
        public readonly ToDoApp App;

        public ToDoAppWithHistory(ImList<ToDoApp> history, ToDoApp app)
        {
            History = history;
            App = app;
        }

        public static ToDoAppWithHistory Init() => 
            new ToDoAppWithHistory(ImList<ToDoApp>.Empty, ToDoApp.Init());

        public struct Restore : M
        {
            public ToDoApp Model;
            public static M It(ToDoApp model) => new Restore { Model = model };
        }

        public ToDoAppWithHistory Update(M message)
        {
            if (message is Restore applyModel)
                return new ToDoAppWithHistory(History, applyModel.Model);

            if (message is ChildChanged<ToDoApp, ToDoAppWithHistory> modelChanged)
                return new ToDoAppWithHistory(History.Prepend(App), App.Update(modelChanged.Message));

            return this;
        }

        public UI<M> View() => 
            column(
                App.In(this),
                column(History.Map(app => row(button("apply", Restore.It(app)), text<M>(app)))));
    }
}
