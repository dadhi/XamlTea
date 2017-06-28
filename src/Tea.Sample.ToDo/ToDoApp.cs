using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoApp : IComponent<ToDoApp, IMsg<ToDoApp.Msg>>
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

        public enum Msg
        {
            ApplyModelFromHistory,
            CardsChanged
        }

        public class ApplyModelFromHistory : IMsg<Msg>
        {
            public Msg Type => Msg.ApplyModelFromHistory;
            public ToDoCards Model { get; private set; }
            public static IMsg<Msg> It(ToDoCards model) => new ApplyModelFromHistory { Model = model };
        }

        public UI<IMsg<Msg>> View()
        {
            return
                panel(Layout.Vertical,
                    Model.View(0, Msg.CardsChanged),
                    panel(Layout.Vertical,
                        History.Map(model =>
                            panel(Layout.Horizontal,
                                button("apply", ApplyModelFromHistory.It(model)),
                                text<IMsg<Msg>>(model.ToString())))
                    )
                );
        }

        public ToDoApp Update(IMsg<Msg> msg)
        {
            if (msg is ApplyModelFromHistory applyModel)
                return new ToDoApp(History, applyModel.Model);

            if (msg is ItemChanged<IMsg<ToDoCards.Msg>, Msg> modelChanged)
                return new ToDoApp(History.Prep(Model), Model.Update(modelChanged.Msg));

            return this;
        }
    }
}
