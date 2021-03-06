﻿using System;
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
using ViewModels.ControllerModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : EOBasePage
    {
        public DashboardPage()
        {
            InitializeComponent();
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = new InventoryPage() };
        }

        private void ArrangementsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = new ArrangementPage(new AddArrangementRequest()) };
        }

        private void WorkOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.NavigationStack.Push(this);
            WorkOrderPage workOrderPage = new WorkOrderPage();
            wnd.NavigationStack.Push(workOrderPage);
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = workOrderPage };
        }

        private void VendorsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = new VendorPage() };
        }

        private void ShipmentsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = new ShipmentPage() };
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.NavigationStack.Push(this);
            ReportsPage reportsPage = new ReportsPage();
            wnd.NavigationStack.Push(reportsPage);
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = reportsPage };
        }

        private void CustomersButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = new CustomerPage() };
        }

        private void BugsButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            wnd.ButtonContent.Content = new Frame() { Content = new ButtonPage(), Visibility = Visibility.Visible };
            wnd.MainContent.Content = new Frame() { Content = new BugsPage() };
        }
    }    
}
