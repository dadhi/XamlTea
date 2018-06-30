namespace Tea.Sample.ToDo
{
    using System.Text;
    using ImTools;
    using static UIElements;
    using static ImToolsExt;
    using M = IMessage<ToDoList>;

    public class ToDoList : IComponent<ToDoList>
    {
        public readonly ImList<ToDoItem> Items;
        public readonly string NewItem;
        public readonly bool IsNewItemValid;

        public ToDoList(ImList<ToDoItem> items, string newItem = "")
        {
            Items = items;
            NewItem = newItem;
            IsNewItemValid = !string.IsNullOrWhiteSpace(newItem);
        }

        public static ToDoList Init() => 
            new ToDoList(list(new ToDoItem("foo"), new ToDoItem("bar", true)));

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{NewItem='").Append(NewItem).Append("'");
            s.Append(",Items=[");
            Items.Fold(s, (it, i, _) => (i == 0 ? _ : _.Append(",(")).Append(it.ToString()).Append(')'));
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
            if (message is EditNewItem editNewItem)
                return new ToDoList(Items, editNewItem.Text);

            if (message is AddNewItem)
                return IsNewItemValid
                    ? new ToDoList(new ToDoItem(NewItem).Cons(Items))
                    : this;

            if (message is RemoveItem removeItem)
                return new ToDoList(Items.RemoveAt(removeItem.ItemIndex), NewItem);

            // propagate the rest of child mgs to child Update
            if (message is ChildChanged<ToDoItem, ToDoList> itemChanged)
                return new ToDoList(Items.UpdateAt(itemChanged.Index, x => x.Update(itemChanged.Message)), NewItem);

            return this;
        }

        public UI<M> View() =>
            column(
                column(Items.Map((it, i) => row(it.In(this, i), button("remove", RemoveItem.It(i))))),
                row(input(NewItem, EditNewItem.It)), button("Add", AddNewItem.It));
    }
}
