using System;
using System.Windows;
using Tea.Wpf;

namespace Tea.Sample.CounterList.Wpf
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var window = new Window { Title = "Tea Sample" };

            UIApp.Run(WpfUI.Init(window), CounterList.Initial);

            new Application().Run(window);
        }
    }
}
