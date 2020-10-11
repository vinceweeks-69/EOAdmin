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
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for CustomerContainerPage.xaml
    /// </summary>
    public partial class CustomerContainerPage : Page
    {
        IEOBasePage basePage;

        public CustomerContainerPage()
        {
            InitializeComponent();
        }

        public CustomerContainerPage(IEOBasePage page) : this()
        {
            basePage = page;
        }

        private void Camera_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            if(basePage != null)
            {
                WorkOrderMessage msg = new WorkOrderMessage();
                basePage.LoadWorkOrderData(msg);
                wnd.OnBackClick(this);
            }
        }

        private void ContainerImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ContainerDelete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
