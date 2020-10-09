﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ViewModels.DataModels;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for InventoryFilter.xaml
    /// </summary>
    public partial class InventoryFilter : Page
    {
        public InventoryFilter()
        {
            InitializeComponent();

            LoadProductTypes();
        }

        //load the type combo on Init
        void LoadProductTypes()
        {
            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();

            list1.Add(new KeyValuePair<long, string>(1, "Orchids"));
            list1.Add(new KeyValuePair<long, string>(2, "Containers"));

            if (!((App)App.Current).PageIsOnStack(typeof(ArrangementPage)))
            {
                list1.Add(new KeyValuePair<long, string>(3, "Arrangements"));
            }

            list1.Add(new KeyValuePair<long, string>(4, "Foliage"));
            list1.Add(new KeyValuePair<long, string>(1, "Materials"));

            ProductType.ItemsSource = list1;
        }

        //load the other combos as their "bosses" are loaded

        private void InventorySelected_Click(object sender, RoutedEventArgs e)
        {
            //send selection back to caller - this filter is used when creating a work order or an arrangement
            //either way, in either of these two modes, use the Navigation Stack

            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            WorkOrderMessage msg = new WorkOrderMessage();

            msg.Inventory = (InventoryDTO)((ListView)sender).SelectedItem;

            ((App)App.Current).WorkOrderMessage = msg;

            wnd.OnBackClick(this);
        }

        private void ProductType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox)
            {
                KeyValuePair<long, string> kvp = (KeyValuePair<long,string>) ((ComboBox)sender).SelectedItem;

                MainWindow wnd = Application.Current.MainWindow as MainWindow;

                List<InventoryDTO> inventoryList = wnd.GetInventoryByType(kvp.Key);

                ObservableCollection<InventoryDTO> list1 = new ObservableCollection<InventoryDTO>();

                foreach(InventoryDTO i in inventoryList)
                {
                    list1.Add(i);
                }

                InventoryFilterListView.ItemsSource = list1;
            }
        }
    }
}
