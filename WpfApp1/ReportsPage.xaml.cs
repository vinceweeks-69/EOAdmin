using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new WorkOrderReportPage() };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new ShipmentReportPage() };
        }
    }
}
