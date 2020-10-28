using EO.ViewModels.ControllerModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ShipmentPage.xaml
    /// </summary>
    public partial class ShipmentPage : EOStackPage
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        ObservableCollection<WorkOrderViewModel> shipmentInventoryList = new ObservableCollection<WorkOrderViewModel>();

        public ShipmentPage()
        {
            InitializeComponent();

            GetUsers();

            GetVendors();
        }

        private async void GetUsers()
        {
            GenericGetRequest request = new GenericGetRequest("GetUsers", String.Empty, 0);
            ((App)App.Current).GetRequest<GetUserResponse>(request).ContinueWith(a => UsersLoaded(a.Result));
        }

        private void UsersLoaded(GetUserResponse response)
        {
            Dispatcher.Invoke(() =>
            {
                ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
                foreach (UserDTO u in response.Users)
                {
                    list1.Add(new KeyValuePair<long, string>(u.UserId, u.UserName));
                }

                ReceiverComboBox.ItemsSource = list1;
            });
        }

        private async void GetVendors()
        {
            ((App)App.Current).PostRequest<GetPersonRequest, GetVendorResponse>("GetVendors", new GetPersonRequest()).ContinueWith(a => VendorsLoaded(a.Result));
        }

        private void VendorsLoaded(GetVendorResponse response)
        {
            Dispatcher.Invoke(() =>
            {
                ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
                foreach (VendorDTO vDTO in response.VendorList)
                {
                    list1.Add(new KeyValuePair<long, string>(vDTO.VendorId, vDTO.VendorName));
                }

                this.VendorComboBox.ItemsSource = list1;
            });
        }

        public override void LoadWorkOrderData(WorkOrderMessage msg)
        {
            //respond to arrangement filter selection

            if (msg.HasMessage())
            {
                if (msg.Inventory.InventoryId != 0)
                {
                    ProcessInventoryData(msg.Inventory);
                }

                if (!String.IsNullOrEmpty(msg.NotInInventory.NotInInventoryName))
                {
                    ProcessNotInInventoryData(msg.NotInInventory);
                }

                ReloadItemList();
            }
        }

        private void ProcessInventoryData(WorkOrderInventoryMapDTO dto)
        {
            if (!shipmentInventoryList.Where(a => a.InventoryId == dto.InventoryId).Any())
            {
                shipmentInventoryList.Add(new WorkOrderViewModel(dto));
                ReloadItemList();
            }
        }

        private void ProcessNotInInventoryData(NotInInventoryDTO dto)
        {
            if (!shipmentInventoryList.Where(a => a.NotInInventoryId == dto.NotInInventoryId).Any())
            {
                shipmentInventoryList.Add(new WorkOrderViewModel(dto));
                ReloadItemList();
            }
        }

        private void ReloadItemList()
        {
            ShipmentListView.ItemsSource = shipmentInventoryList;
        }
                
        public async void AddShipment()
        {
            AddShipmentRequest request = new AddShipmentRequest();
            ((App)App.Current).PostRequest<AddShipmentRequest, ApiResponse>("AddShipment", request).ContinueWith(a => ShipmentAdded(a.Result));
        }

        private void ShipmentAdded(ApiResponse response)
        {
            Dispatcher.Invoke(() =>
            {

            });
        }

        public void Add_Shipment()
        {
           
        }

        private void OnDeleteShipmentInventory(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            WorkOrderViewModel shipmentInventoryItem = b.CommandParameter as WorkOrderViewModel;
            if(shipmentInventoryList.Contains(shipmentInventoryItem))
            {
                shipmentInventoryList.Remove(shipmentInventoryItem);
                ReloadItemList();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ArrangementFilter filter = new ArrangementFilter(this);

            List<InventoryTypeDTO> inventoryTypes = wnd.GetInventoryTypes();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (InventoryTypeDTO inventoryType in inventoryTypes)
            {
                if (inventoryType.InventoryTypeName != "Arrangements")
                {
                    list1.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
                }
            }

            filter.InventoryTypeCombo.ItemsSource = list1;
            filter.Owner = wnd;
            filter.ShowDialog();
        }

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gView = ShipmentListView.View as GridView;

            var workingWidth = PageGrid.ActualWidth - 80;   //SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            ShipmentListView.Width = workingWidth;

            var col1 = 0.50;
            var col2 = 0.10;
            var col3 = 0.20;
            var col4 = 0.20;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;

            //var workingHeight = PageGrid.RowDefinitions.ElementAt(6).ActualHeight;

            //ShipmentListView.Height = workingHeight * 0.9;
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
