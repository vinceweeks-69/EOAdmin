using ExcelDataReader;
using Microsoft.Win32;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using ViewModels.ControllerModels;
using ViewModels.DataModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for InventoryPage.xaml
    /// </summary>
    public partial class InventoryPage : Page
    {
        public InventoryPage()
        {
            InitializeComponent();
        }

        private void PlantsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new PlantPage() };
        }

        private void FoliageButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new FoliagePage() };
        }

        private void MaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new MaterialsPage() };
        }

        private void ContainersButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new ContainerPage() };
        }

        private void ArrangementsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new ArrangementPage() };
        }

        private void VendorsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new VendorPage() };
        }

        /// <summary>
        /// foliage.xlsx worksheet name = Plants
        /// phals.xlsx worksheet names  1 = 6" Phal's 2 = 5" Phal's 3 = 4" Phal's 4 = 3" Phal's 5 = 2" Mini
        /// containers.xlsx worksheet name = Accent
        /// orchids.xlsx worksheet names 1 = Angraecum 2 = Cattleya 3 = Dendrobium 4 = Paphiopedilum 5 = Oncidium 6 = Vandas 7 = FLOWERS
        /// Arrangement Materials.xlsx worksheet 1 name = Materials
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.MainContent.Content = new Frame() { Content = new ImportPage() };
        }
    }
}
