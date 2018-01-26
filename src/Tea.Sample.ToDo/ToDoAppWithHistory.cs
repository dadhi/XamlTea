using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoAppWithHistory : IComponent<ToDoAppWithHistory>
    {
        public readonly ImList<ToDoApp> History;
        public readonly ToDoApp App;

        public ToDoAppWithHistory(ImList<ToDoApp> history, ToDoApp app)
        {
            History = history;
            App = app;
        }

        public static ToDoAppWithHistory Init() => new ToDoAppWithHistory(ImList<ToDoApp>.Empty, ToDoApp.Init());

        public class ApplyFromHistory : IMsg<ToDoAppWithHistory>
        {
            public ToDoApp Model { get; private set; }
            public static IMsg<ToDoAppWithHistory> It(ToDoApp model) => new ApplyFromHistory { Model = model };
        }

        public UI<IMsg<ToDoAppWithHistory>> View()
        {
            return
                column(
                    App.ViewIn(this),
                    column(History.Map(model =>
                        row(button("apply", ApplyFromHistory.It(model)),
                            text<IMsg<ToDoAppWithHistory>>(model.ToString()))
                        )
                    )
                );
        }

        public ToDoAppWithHistory Update(IMsg<ToDoAppWithHistory> msg)
        {
            if (msg is ApplyFromHistory applyModel)
                return new ToDoAppWithHistory(History, applyModel.Model);

            if (msg is ItemChanged<ToDoApp, ToDoAppWithHistory> modelChanged)
                return new ToDoAppWithHistory(History.Prepend(App), App.Update(modelChanged.Msg));

            return this;
        }
    }
}
