using System;

namespace Tea.Sample.ToDo
{
    using ImTools;
    using static UIParts;

    public static class ToDoList
    {
        public class Model
        {
            public readonly ImList<ToDoItem.Model> Items;
            public readonly string NewItem;

            public Model(ImList<ToDoItem.Model> items, string newItem)
            {
                Items = items;
                NewItem = newItem;
            }
        }

        public abstract class Msg
        {
            public class AddNewItem : Msg
            {
                public string Text { get; private set; }
                public static Msg It(string text) { return new AddNewItem { Text = text }; }
            }

            public class ItemChanged : Msg
            {
                public int ItemIndex { get; private set; }
                public ToDoItem.Msg ItemMsg { get; private set; }

                public static Func<ToDoItem.Msg, Msg> It(int index)
                {
                    return msg => new ItemChanged { ItemIndex = index, ItemMsg = msg };
                }
            }
        }

        public static Model Update(this Model model, Msg msg)
        {
            var addNewItem = msg as Msg.AddNewItem;
            if (addNewItem != null)
                return new Model(model.Items, addNewItem.Text);

            var itemChanged = msg as Msg.ItemChanged;
            if (itemChanged != null)
                return new Model(
                    model.Items.Map((it, i) => i == itemChanged.ItemIndex ? it.Update(itemChanged.ItemMsg) : it),
                    model.NewItem);

            return model;
        }

        public static UI<Msg> View(this Model model)
        {
            return div(Layout.Vertical, 
                model.Items.Map((it, i) => it.View().MapMsg(Msg.ItemChanged.It(i))).ToArray().Append(
                input(model.NewItem, Msg.AddNewItem.It),
                text<Msg>(model.NewItem)
                ));
        }

        public static Model Init()
        {
            return new Model(ImList<ToDoItem.Model>.Empty
                .Prep(new ToDoItem.Model("foo", false))
                .Prep(new ToDoItem.Model("bar", true)),
                string.Empty);
        }

        public static App<Msg, Model> App()
        {
            return UIApp.App(Init(), Update, View);
        }
    }

    public static class ToDoItem
    {
        public class Model
        {
            public readonly string Text;
            public readonly bool IsDone;

            public Model(string text, bool isDone)
            {
                Text = text;
                IsDone = isDone;
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

        public static Model Update(this Model model, Msg msg)
        {
            var stateChanged = msg as Msg.StateChanged;
            if (stateChanged != null)
                return new Model(model.Text, stateChanged.IsDone);
            return model;
        }

        public static UI<Msg> View(this Model model)
        {
            return checkbox(model.Text, model.IsDone, Msg.StateChanged.It);
        }
    }
}
