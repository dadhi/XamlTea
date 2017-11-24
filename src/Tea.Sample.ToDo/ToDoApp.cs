using ImTools;
using static Tea.UIParts;

namespace Tea.Sample.ToDo
{
    public class ToDoApp : IComponent<ToDoApp>
    {
        public readonly ImList<ToDoCards> History;
        public readonly ToDoCards Cards;

        public ToDoApp(ImList<ToDoCards> history, ToDoCards cards)
        {
            History = history;
            Cards = cards;
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
                column(
                    Cards.ViewIn(this),
                    column(History.Map(model => 
                        row(
                            button("apply", ApplyModelFromHistory.It(model)),
                            text<IMsg<ToDoApp>>(model.ToString()))
                        )
                    )
                );
        }

        public ToDoApp Update(IMsg<ToDoApp> msg)
        {
            if (msg is ApplyModelFromHistory applyModel)
                return new ToDoApp(History, applyModel.Model);

            if (msg is ItemChanged<ToDoCards, ToDoApp> modelChanged)
                return new ToDoApp(History.Prepend(Cards), Cards.Update(modelChanged.Msg));

            return this;
        }
    }
}
