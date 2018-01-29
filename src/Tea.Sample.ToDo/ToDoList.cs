using System.Text;
using ImTools;
using static Tea.UIParts;
using static Tea.ImToolsExt;

namespace Tea.Sample.ToDo
{
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
                    ? new ToDoList(new ToDoItem(NewItem).Cons(Items))
                    : this;

            if (msg is RemoveItem removeItem)
                return new ToDoList(Items.RemoveAt(removeItem.ItemIndex), NewItem);

            // propagate the rest of child mgs to child Update
            if (msg is ItemChanged<ToDoItem, ToDoList> itemChanged)
                return new ToDoList(Items.UpdateAt(itemChanged.Index, x => x.Update(itemChanged.Msg)), NewItem);

            return this;
        }

        public UI<IMsg<ToDoList>> View() => 
            column(
                column(Items.Map((item, i) =>
                    row(item.ViewIn(this, i), button("remove", RemoveItem.It(i))))),
                row(input(NewItem, EditNewItem.It)), button("Add", AddNewItem.It));
    }
}
