using System;
using System.Linq;
using System.Windows;
using AZE.Impl;
using AZE.View;
using AZE.ViewModel;
using Res = AZE.Properties.Resources;

namespace AZE
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0)
            {
                new MainWindow().Show();
            }
            else
            {
                MainViewModel vm = new MainViewModel();
                vm.CalculateBeginAndEnd(DateTime.Now, out DateTime begin, out DateTime end);

                switch (e.Args.First())
                {
                    case "begin":
                        vm.StoreTime(AzeTimeValueEnum.Begin, begin, true);
                        break;
                    case "end":
                        vm.StoreTime(AzeTimeValueEnum.End, end, true);
                        break;
                    default:
                        MessageBox.Show(string.Format(Res.ConsoleHelp, e.Args.First()), Res.WindowTitle);
                        break;
                }

                Application.Current.Shutdown();
            }
        }
    }
}