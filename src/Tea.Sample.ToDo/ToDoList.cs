using System.Text;
using ImTools;
using static Tea.UIParts;
using static Tea.Props;

namespace Tea.Sample.ToDo
{
    public class ToDoList : IComponent<ToDoList, IMsg<ToDoList.MsgType>>
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

        public enum MsgType
        {
            EditNewItem,
            AddNewItem,
            RemoveItem,
            ItemChanged
        }

        public class EditNewItem : IMsg<MsgType>
        {
            public MsgType Type => MsgType.EditNewItem;
            public string Text { get; private set; }
            public static IMsg<MsgType> It(string text) => new EditNewItem { Text = text };
        }

        public class AddNewItem : IMsg<MsgType>
        {
            public MsgType Type => MsgType.AddNewItem;
            public static readonly IMsg<MsgType> It = new AddNewItem();
        }

        public class RemoveItem : IMsg<MsgType>
        {
            public MsgType Type => MsgType.RemoveItem;
            public int ItemIndex { get; private set; }
            public static IMsg<MsgType> It(int itemIndex) => new RemoveItem { ItemIndex = itemIndex };
        }

        public ToDoList Update(IMsg<MsgType> msg)
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
            if (msg is ItemChanged<IMsg<ToDoItem.MsgType>, MsgType> itemChanged)
                return With(Items.With(itemChanged.ItemIndex, it => it.Update(itemChanged.ItemMsg)));

            return this;
        }

        public UI<IMsg<MsgType>> View()
        {
            return panel(Layout.Vertical,
                Items.Map((it, i) =>
                    panel(Layout.Horizontal,
                        it.View(i, MsgType.ItemChanged),
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
