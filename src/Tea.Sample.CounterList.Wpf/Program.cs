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

            UIApplication.Run(WpfUI.Create(window), CounterList.Init());

            new Application().Run(window);
        }
    }
}
