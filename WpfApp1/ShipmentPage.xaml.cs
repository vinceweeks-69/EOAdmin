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

        private List<VendorDTO> vendorList;

        private List<ShipmentInventoryMapDTO> shipmentInventoryMap = new List<ShipmentInventoryMapDTO>();

        private List<ShipmentInventoryItemDTO> shipmentInventoryList = new List<ShipmentInventoryItemDTO>();

        private List<InventoryTypeDTO> inventoryTypeList;

        ObservableCollection<ShipmentInventoryItemDTO> list1 = new ObservableCollection<ShipmentInventoryItemDTO>();

        public ShipmentPage()
        {
            InitializeComponent();
            //this.ShipmentDate.Content = DateTime.Now.ToShortDateString();

            //load combo boxes

            //get vendors
            vendorList = GetVendors();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (VendorDTO vDTO in vendorList)
            {
                list1.Add(new KeyValuePair<long, string>(vDTO.VendorId, vDTO.VendorName));
            }

            this.VendorComboBox.ItemsSource = list1;

            //get inventory type
            inventoryTypeList = GetInventoryTypes();

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (InventoryTypeDTO itDTO in inventoryTypeList)
            {
                list2.Add(new KeyValuePair<long, string>(itDTO.InventoryTypeId, itDTO.InventoryTypeName));
            }

            //this.InventoryTypeComboBox.ItemsSource = list2;

            //get inventory - based on inventory type
        }


        public override void LoadWorkOrderData(WorkOrderMessage msg)
        {

        }

        public List<VendorDTO> GetVendors()
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            return wnd.GetVendors();
        }

        public List<InventoryTypeDTO> GetInventoryTypes()
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            return wnd.GetInventoryTypes();
        }

        public List<InventoryDTO> GetInventory(long inventoryType)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            return wnd.GetInventoryByType(inventoryType);
        }

        public void AddShipment()
        {
            try
            {
                AddShipmentRequest addShipmentRequest = new AddShipmentRequest();

                ShipmentDTO dto = new ShipmentDTO()
                {
                    VendorId = ((KeyValuePair<long, string>)this.VendorComboBox.SelectedValue).Key,
                    ShipmentDate = this.ShipmentDate.SelectedDate.HasValue ? this.ShipmentDate.SelectedDate.Value : DateTime.Now 
                };

                
                foreach(ShipmentInventoryItemDTO itemDTO in shipmentInventoryList)
                {
                    shipmentInventoryMap.Add(new ShipmentInventoryMapDTO()
                    {
                        InventoryId = itemDTO.InventoryId,
                        InventoryName = itemDTO.InventoryName,
                        Quantity = itemDTO.Quantity
                    });
                }

                addShipmentRequest.ShipmentDTO = dto;
                addShipmentRequest.ShipmentInventoryMap = shipmentInventoryMap;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                string jsonData = JsonConvert.SerializeObject(addShipmentRequest);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/AddShipment", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(strData);

                    if (apiResponse.Messages.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (KeyValuePair<string, List<string>> messages in apiResponse.Messages)
                        {
                            foreach (string msg in messages.Value)
                            {
                                sb.AppendLine(msg);
                            }
                        }

                        MessageBox.Show(sb.ToString());
                    }
                    else
                    {
                        this.ShipmentListView.ItemsSource = null;
                    }
                }
                else
                {
                    MessageBox.Show("Error adding Shipment");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void OnDeleteShipmentInventory(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            ShipmentInventoryItemDTO shipmentInventoryItem = b.CommandParameter as ShipmentInventoryItemDTO;
            list1.Remove(shipmentInventoryItem);
            shipmentInventoryList.Remove(shipmentInventoryItem);
            this.ShipmentListView.ItemsSource = list1;
        }

        private void InventoryTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //get the inventory type selected and fill the inventory combo
            //List<InventoryDTO> inventoryList = new List<InventoryDTO>();
                        
            //ComboBox cb = sender as ComboBox;
            //KeyValuePair<long, string> kvp = (KeyValuePair<long, string>)cb.SelectedItem;
            //inventoryList = GetInventory(kvp.Key);

            //ObservableCollection<KeyValuePair<long,string>> list3 = new ObservableCollection<KeyValuePair<long,string>>();

            //foreach(InventoryDTO iDTO in inventoryList)
            //{
            //    list3.Add(new KeyValuePair<long,string>(iDTO.InventoryId,iDTO.InventoryName));
            //}

            //this.InventoryComboBox.ItemsSource = list3;
        }

        private void AddInventoryToShipmentBtn_Click(object sender, RoutedEventArgs e)
        {
            //string qtyText = this.QuantityTextBox.Text;
            //int qty = 0;
            //Int32.TryParse(qtyText, out qty);

            //long inventoryId = ((KeyValuePair<long, string>)this.InventoryComboBox.SelectedItem).Key;
            //string inventoryName = ((KeyValuePair<long, string>)this.InventoryComboBox.SelectedItem).Value;

            //if(qty == 0 || inventoryId == null || inventoryId == 0 || String.IsNullOrEmpty(inventoryName))
            //{
            //    MessageBox.Show("Nothing to add");
            //    return;
            //}

            try
            {
                //ShipmentInventoryMapDTO dto = new ShipmentInventoryMapDTO()
                //{
                //    InventoryId = ((KeyValuePair<long, string>)this.InventoryComboBox.SelectedItem).Key,
                //    InventoryName = ((KeyValuePair<long, string>)this.InventoryComboBox.SelectedItem).Value,
                //    Quantity = qty
                //};

                //shipmentInventoryMap.Add(dto);

                //ObservableCollection<ShipmentInventoryMapDTO> list4 = new ObservableCollection<ShipmentInventoryMapDTO>();

                //foreach (ShipmentInventoryMapDTO simDTO in shipmentInventoryMap)
                //{
                //    list4.Add(simDTO);
                //}

                //this.ShipmentListView.ItemsSource = null;
                //this.ShipmentListView.ItemsSource = list4;
            }
            catch(Exception ex)
            {

            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ArrangementFilter filter = new ArrangementFilter(this);
            //MainWindow wnd = Application.Current.MainWindow as MainWindow;
            //filter.Owner = wnd;

            List<InventoryTypeDTO> inventoryTypes = wnd.GetInventoryTypes();

            //filter.mainWnd = wnd;
            //filter.shipmentParentWnd = this;

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

        public void AddInventorySelection(long inventoryId, string inventoryName)
        {
            ShipmentInventoryItemDTO dto = new ShipmentInventoryItemDTO(0, inventoryId, inventoryName, String.Empty, 0);
            shipmentInventoryList.Add(dto);

            list1.Add(dto);

            this.ShipmentListView.ItemsSource = list1;
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

            var workingHeight = PageGrid.RowDefinitions.ElementAt(4).ActualHeight;

            ShipmentListView.Height = workingHeight * 0.9;
        }
    }
}
