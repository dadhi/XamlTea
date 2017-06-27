using System;
using System.Text;
using ImTools;
using static Tea.UIParts;
using static Tea.Props;

namespace Tea.Sample.ToDo
{
    public class ToDoList : IComponent<ToDoList, ToDoList.Msg>
    {
        public readonly ImList<ToDoItem> Items;
        public readonly string NewItem;
        public readonly bool IsNewItemValid;

        public ToDoList(ImList<ToDoItem> items, string newItem)
        {
            Items = items;
            NewItem = newItem;
            IsNewItemValid = !string.IsNullOrWhiteSpace(newItem);
        }

        public static ToDoList Init()
        {
            return new ToDoList(ImList<ToDoItem>.Empty
                    .Prep(new ToDoItem("foo"))
                    .Prep(new ToDoItem("bar", true)),
                string.Empty);
        }

        public ToDoList With(ImList<ToDoItem> items)
        {
            return new ToDoList(items, NewItem);
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

        public ToDoList Update(Msg msg)
        {
            if (msg is Msg.EditNewItem editNewItem)
                return new ToDoList(Items, editNewItem.Text);

            if (msg is Msg.AddNewItem)
                return IsNewItemValid
                    ? new ToDoList(Items.Prep(new ToDoItem(NewItem)), string.Empty)
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
    }
}
