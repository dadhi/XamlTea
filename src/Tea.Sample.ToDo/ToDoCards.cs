
namespace Tea.Sample.ToDo
{
    using System;
    using ImTools;
    using static UIParts;

    public static class ToDoCards
    {
        public class Model
        {
            public readonly ImList<ToDoList.Model> Items;

            public Model(ImList<ToDoList.Model> items)
            {
                Items = items;
            }
        }

        public abstract class Msg
        {
            public class ItemChanged : Msg
            {
                public int ItemIndex { get; private set; }
                public ToDoList.Msg ItemMsg { get; private set; }
                public static Func<ToDoList.Msg, Msg> It(int index)
                {
                    return msg => new ItemChanged { ItemIndex = index, ItemMsg = msg };
                }
            }
        }

        public static Model Update(this Model model, Msg msg)
        {
            var itemChanged = msg as Msg.ItemChanged;
            if (itemChanged != null)
                return new Model(model.Items.With(itemChanged.ItemIndex, it => it.Update(itemChanged.ItemMsg)));

            return model;
        }

        public static UI<Msg> View(this Model model)
        {
            return panel(Layout.Horizontal, 
                model.Items.Map((it, i) => it.View().MapMsg(Msg.ItemChanged.It(i))).ToArray());
        }

        public static Model Init()
        {
            return new Model(ImList<ToDoList.Model>
                .Empty
                .Prep(ToDoList.Init())
                .Prep(ToDoList.Init())
                .Prep(ToDoList.Init()));
        }

        public static App<Msg, Model> App()
        {
            return UIApp.App(Init(), Update, View);
        }
    }
}