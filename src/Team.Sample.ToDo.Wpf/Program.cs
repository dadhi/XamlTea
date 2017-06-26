using System;
using System.Windows;
using Tea;
using Tea.Sample.ToDo;

namespace Team.Sample.ToDo.Wpf
{
    using Tea.Wpf;
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var window = new Window { Title = "Tea Sample" };
            var ui = Wpf.CreateUI(window);

            //UIApp.Run(ui, ToDoList.Init());
            UIApp.Run(ui, ToDoCards.Init());

            new Application().Run(window);
        }
    }
}
