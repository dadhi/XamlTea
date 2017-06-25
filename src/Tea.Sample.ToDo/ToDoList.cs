namespace Tea.Sample.ToDo
{
    using System;
    using System.Text;
    using ImTools;
    using static UIParts;
    using static Props;

    public static class ToDoList
    {
        public class Model
        {
            public readonly ImList<ToDoItem.Model> Items;
            public readonly string NewItem;
            public readonly bool IsNewItemValid;

            public Model(ImList<ToDoItem.Model> items, string newItem)
            {
                Items = items;
                NewItem = newItem;
                IsNewItemValid = !string.IsNullOrWhiteSpace(newItem);
            }

            public Model With(ImList<ToDoItem.Model> items)
            {
                return new Model(items, NewItem);
            }

            public override string ToString()
            {
                var s = new StringBuilder();
                s.Append("{NewItem=").Append(NewItem);
                s.Append(",Items=[");
                Items.To(s, (it, i, _) => (i == 0 ? _ : _.Append(",")).Append(it.ToString()));
                s.Append("]}");
                return s.ToString();
            }
        }

        public abstract class Msg
        {
            public class EditNewItem : Msg
            {
                public string Text { get; private set; }
                public static Msg It(string text) { return new EditNewItem { Text = text }; }
            }

            public class AddNewItem : Msg
            {
                public static readonly Msg It = new AddNewItem();
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
            var editNewItem = msg as Msg.EditNewItem;
            if (editNewItem != null)
                return new Model(model.Items, editNewItem.Text);

            var addNewItem = msg as Msg.AddNewItem;
            if (addNewItem != null) // adds new item to list and reset new item text
                return model.IsNewItemValid
                    ? new Model(model.Items.Prep(new ToDoItem.Model(model.NewItem)), string.Empty)
                    : model;

            var itemChanged = msg as Msg.ItemChanged;
            if (itemChanged != null)
            {
                // handles selected child msg on parent level, and does not propagate it to child - child is removed
                if (itemChanged.ItemMsg is ToDoItem.Msg.Remove)
                    return model.With(model.Items.Without(itemChanged.ItemIndex));

                // propagate the rest of child mgs to child Update
                return model.With(model.Items.With(itemChanged.ItemIndex,
                    it => it.Update(itemChanged.ItemMsg)));
            }

            return model;
        }

        public static UI<Msg> View(this Model model)
        {
            return panel(Layout.Vertical,
                model.Items.Map((it, i) => it.View().MapMsg(Msg.ItemChanged.It(i))).ToArray().Append(
                panel(Layout.Horizontal,
                    input(model.NewItem, Msg.EditNewItem.It, props(width(100))),
                    button("Add", Msg.AddNewItem.It, props(isEnabled(model.IsNewItemValid)))
                )));
        }

        public static Model Init()
        {
            return new Model(ImList<ToDoItem.Model>.Empty
                .Prep(new ToDoItem.Model("foo"))
                .Prep(new ToDoItem.Model("bar", true)),
                string.Empty);
        }

        public static App<Msg, Model> App()
        {
            return UIApp.App(Init(), Update, View);
        }
    }
}
