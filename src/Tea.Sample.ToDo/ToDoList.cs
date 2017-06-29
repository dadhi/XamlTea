using System.Text;
using ImTools;
using static Tea.UIParts;
using static Tea.Props;

namespace Tea.Sample.ToDo
{
    public class ToDoList : IComponent<ToDoList>
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

        public class EditNewItem : IMsg<ToDoList>
        {
            public string Text { get; private set; }
            public static IMsg<ToDoList> It(string text) => new EditNewItem { Text = text };
        }

        public class AddNewItem : IMsg<ToDoList>
        {
            public static readonly IMsg<ToDoList> It = new AddNewItem();
        }

        public class RemoveItem : IMsg<ToDoList>
        {
            public int ItemIndex { get; private set; }
            public static IMsg<ToDoList> It(int itemIndex) => new RemoveItem { ItemIndex = itemIndex };
        }

        public ToDoList Update(IMsg<ToDoList> msg)
        {
            if (msg is EditNewItem editNewItem)
                return new ToDoList(Items, editNewItem.Text);

            if (msg is AddNewItem)
                return IsNewItemValid
                    ? new ToDoList(Items.Prep(new ToDoItem(NewItem)), string.Empty)
                    : this;

            if (msg is RemoveItem removeItem)
                return With(Items.Without(removeItem.ItemIndex));

            // propagate the rest of child mgs to child Update
            if (msg is ItemChanged<ToDoItem, ToDoList> itemChanged)
                return With(Items.With(itemChanged.Index, it => it.Update(itemChanged.Msg)));

            return this;
        }

        public UI<IMsg<ToDoList>> View()
        {
            return panel(Layout.Vertical,
                Items.Map((it, i) =>
                    panel(Layout.Horizontal,
                        it.View<ToDoItem, ToDoList>(i),
                        button("remove", RemoveItem.It(i))
                    )
                ).ToArray()
                .Append(
                    panel(Layout.Horizontal,
                        input(NewItem, EditNewItem.It, props(width(100))),
                        button("Add", AddNewItem.It, props(isEnabled(IsNewItemValid)))
                    )));
        }
    }
}
