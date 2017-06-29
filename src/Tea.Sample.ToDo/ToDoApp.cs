using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoApp : IComponent<ToDoApp>
    {
        public readonly ImList<ToDoCards> History;
        public readonly ToDoCards Model;

        public ToDoApp(ImList<ToDoCards> history, ToDoCards model)
        {
            History = history;
            Model = model;
        }

        public static ToDoApp Init()
        {
            return new ToDoApp(ImList<ToDoCards>.Empty, ToDoCards.Init());
        }

        public class ApplyModelFromHistory : IMsg<ToDoApp>
        {
            public ToDoCards Model { get; private set; }
            public static IMsg<ToDoApp> It(ToDoCards model) => new ApplyModelFromHistory { Model = model };
        }

        public UI<IMsg<ToDoApp>> View()
        {
            return
                panel(Layout.Vertical,
                    Model.View<ToDoCards, ToDoApp>(0),
                    panel(Layout.Vertical,
                        History.Map(model =>
                            panel(Layout.Horizontal,
                                button("apply", ApplyModelFromHistory.It(model)),
                                text<IMsg<ToDoApp>>(model.ToString())))
                    )
                );
        }

        public ToDoApp Update(IMsg<ToDoApp> msg)
        {
            if (msg is ApplyModelFromHistory applyModel)
                return new ToDoApp(History, applyModel.Model);

            if (msg is ItemChanged<ToDoCards, ToDoApp> modelChanged)
                return new ToDoApp(History.Prep(Model), Model.Update(modelChanged.Msg));

            return this;
        }
    }
}
