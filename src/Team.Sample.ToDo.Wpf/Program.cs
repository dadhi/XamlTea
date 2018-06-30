using System;
using System.Windows;
using Tea;
using Tea.Sample;
using Tea.Sample.ToDo;

namespace Team.Sample.ToDo.Wpf
{
    using Tea.Wpf;
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var window = new Window { Title = "Sample App" };
            var ui = WpfUI.Create(window);

            //UIApplication.Run(ui, new ChangedEventSample());

            //UIApplication.Run(ui, ToDoList.Init());
            //UIApplication.Run(ui, ToDoApp.Init());
            UIApplication.Run(ui, ToDoAppWithHistory.Init());

            var app = new Application();
            app.Run(window);
        }
    }
}
