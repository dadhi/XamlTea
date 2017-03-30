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
        }

        public static Model Update(Msg msg, Model model)
        {
            var addNewItem = msg as Msg.AddNewItem;
            if (addNewItem != null)
            {
                return new Model(model.Items, addNewItem.Text);
            }
            return model;
        }

        public static UI<Msg> View(Model model)
        {
            return div(Layout.Vertical, 
                model.Items.Map(it => it.View().MapMsg(msg => default(Msg))).ToArray().Append(
                input(model.NewItem, Msg.AddNewItem.It),
                text<Msg>($"{model.NewItem}")
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

        public enum Msg {}

        public static UI<Msg> View(this Model model)
        {
            return text<Msg>($"{model.Text} : {model.IsDone}");
        }
    }
}
