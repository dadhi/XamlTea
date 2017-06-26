using System;
using System.Text;
using ImTools;
using static Tea.UIParts;
using static Tea.Props;

namespace Tea.Sample.ToDo
{
    public static class ToDoList
    {
        public class Model : IComponent<Model, Msg>
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

            public Model Update(Msg msg)
            {
                if (msg is Msg.EditNewItem editNewItem)
                    return new Model(Items, editNewItem.Text);

                if (msg is Msg.AddNewItem)
                    return IsNewItemValid
                        ? new Model(Items.Prep(new ToDoItem.Model(NewItem)), string.Empty)
                        : this;

                if (msg is Msg.RemoveItem removeItem)
                    return With(Items.Without(removeItem.ItemIndex));

                // propagate the rest of child mgs to child Update
                if (msg is Msg.ItemChanged itemChanged)
                    return With(Items.With(itemChanged.ItemIndex, it => it.Update(itemChanged.ItemMsg)));

                return this;
            }

            public UI<Msg> View()
            {
                return panel(Layout.Vertical,
                    Items.Map((it, i) =>
                        panel(Layout.Horizontal,
                            it.View().MapMsg(Msg.ItemChanged.It(i)),
                            button("remove", Msg.RemoveItem.It(i))
                        )
                    ).ToArray()
                    .Append(
                        panel(Layout.Horizontal,
                            input(NewItem, Msg.EditNewItem.It, props(width(100))),
                            button("Add", Msg.AddNewItem.It, props(isEnabled(IsNewItemValid)))
                        )));
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

            public class RemoveItem : Msg
            {
                public int ItemIndex { get; private set; }
                public static Msg It(int itemIndex) => new RemoveItem { ItemIndex = itemIndex };
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

        public static Model Init()
        {
            return new Model(ImList<ToDoItem.Model>.Empty
                .Prep(new ToDoItem.Model("foo"))
                .Prep(new ToDoItem.Model("bar", true)),
                string.Empty);
        }
    }
}
