namespace Tea.Sample.ToDo
{
    using System.Text;
    using ImTools;
    using static UIElements;
    using M = IMessage<ToDoList>;

    public class ToDoList : IComponent<ToDoList>
    {
        public readonly ImZipper<ToDoItem> Items;
        public readonly string NewItem;
        public readonly bool IsNewItemValid;

        public ToDoList(ImZipper<ToDoItem> items, string newItem = "")
        {
            Items = items;
            NewItem = newItem;
            IsNewItemValid = !string.IsNullOrWhiteSpace(newItem);
        }

        public static ToDoList Init() => 
            new ToDoList(ImZipper.Zip(new ToDoItem("foo"), new ToDoItem("bar", true)));

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{NewItem='").Append(NewItem).Append("'").Append(",Items=[");
            s = Items.Fold(s, (it, i, sb) => (i == 0 ? sb : sb.Append(",(")).Append(it).Append(')'));
            s.Append("]}");
            return s.ToString();
        }

        public class EditNewItem : M
        {
            public string Text { get; private set; }
            public static M It(string text) => new EditNewItem { Text = text };
        }

        public class AddNewItem : M
        {
            public static readonly M It = new AddNewItem();
        }

        public class RemoveItem : M
        {
            public int ItemIndex { get; private set; }
            public static M It(int itemIndex) => new RemoveItem { ItemIndex = itemIndex };
        }

        public ToDoList Update(M message)
        {
            switch (message)
            {
                case EditNewItem editNewItem:
                    return new ToDoList(Items, editNewItem.Text);
                
                case AddNewItem _ when IsNewItemValid:
                    return new ToDoList(Items.Append(new ToDoItem(NewItem)));

                case RemoveItem removeItem:
                    return new ToDoList(Items.RemoveAt(removeItem.ItemIndex), NewItem);

                case ChildChanged<ToDoItem, ToDoList> itemChanged:
                    return new ToDoList(Items.UpdateAt(itemChanged.Index, x => x.Update(itemChanged.Message)), NewItem);
                
                default: 
                    return this;
            }
        }

        public UI<M> View() =>
            column(
                column(Items.Map((item, i) => 
                    row(item.View(this, i), button("remove", RemoveItem.It(i))))),
                
                row(input(NewItem, EditNewItem.It)), button("Add", AddNewItem.It));
    }
}
