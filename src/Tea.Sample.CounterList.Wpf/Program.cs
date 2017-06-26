using System;
using System.Windows;

namespace Tea.Sample.CounterList.Wpf
{
    using static Tea.Wpf.Wpf;

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var window = new Window { Title = "Tea Sample" };
            var ui = CreateUI(window);
            UIApp.Run(ui, CounterList.Initial);
            new Application().Run(window);
        }
    }
}
