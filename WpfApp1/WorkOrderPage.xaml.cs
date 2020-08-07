using EO.ViewModels.ControllerModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    /// Interaction logic for WorkOrderPage.xaml
    /// </summary>
    public partial class WorkOrderPage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        private List<InventoryTypeDTO> inventoryTypeList;
        private List<WorkOrderInventoryMapDTO> workOrderInventoryMap = new List<WorkOrderInventoryMapDTO>();
        private List<WorkOrderInventoryItemDTO> workOrderInventoryList = new List<WorkOrderInventoryItemDTO>();
        ObservableCollection<WorkOrderInventoryItemDTO> list1 = new ObservableCollection<WorkOrderInventoryItemDTO>(); 

        public WorkOrderPage()
        {
            InitializeComponent();

            //get inventory type
            inventoryTypeList = GetInventoryTypes();

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (InventoryTypeDTO itDTO in inventoryTypeList)
            {
                list2.Add(new KeyValuePair<long, string>(itDTO.InventoryTypeId, itDTO.InventoryTypeName));
            }

            //this.InventoryTypeComboBox.ItemsSource = list2;
      
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

        private void InventoryTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //load inventory combo box based on current selection
            //get the inventory type selected and fill the inventory combo
            //List<InventoryDTO> inventoryList = new List<InventoryDTO>();

            //ComboBox cb = sender as ComboBox;
            //KeyValuePair<long, string> kvp = (KeyValuePair<long, string>)cb.SelectedItem;
            //inventoryList = GetInventory(kvp.Key);

            //ObservableCollection<KeyValuePair<long, string>> list3 = new ObservableCollection<KeyValuePair<long, string>>();

            //foreach (InventoryDTO iDTO in inventoryList)
            //{
            //    list3.Add(new KeyValuePair<long, string>(iDTO.InventoryId, iDTO.InventoryName));
            //}

            //this.InventoryComboBox.ItemsSource = list3;
        }

        private void InventoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        public void AddWorkOrder()
        {
            try
            {
                AddWorkOrderRequest addWorkOrderRequest = new AddWorkOrderRequest();

                WorkOrderDTO dto = new WorkOrderDTO()
                {
                    Seller = this.SellerTextBox.Text,
                    Buyer = this.BuyerTextBox.Text,
                    CreateDate = DateTime.Now,
                    Comments = this.CommentsTextBox.Text
                };

                foreach(WorkOrderInventoryItemDTO woii in workOrderInventoryList)
                {
                    workOrderInventoryMap.Add(new WorkOrderInventoryMapDTO()
                    {
                        InventoryId = woii.InventoryId,
                        InventoryName = woii.InventoryName,
                        Quantity = woii.Quantity
                    });
                }

                addWorkOrderRequest.WorkOrder = dto;
                addWorkOrderRequest.WorkOrderInventoryMap = workOrderInventoryMap;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                string jsonData = JsonConvert.SerializeObject(addWorkOrderRequest);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/AddWorkOrder", content).Result;
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
                        this.WorkOrderInventoryListView.ItemsSource = null;
                    }
                }
                else
                {
                    MessageBox.Show("Error adding Work Order");
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ArrangementFilter filter = new ArrangementFilter();
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            filter.Owner = wnd;

            List<InventoryTypeDTO> inventoryTypes = wnd.GetInventoryTypes();

            filter.mainWnd = wnd;
            filter.workOrderParentWnd = this;

            ObservableCollection<KeyValuePair<long, string>> list3 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (InventoryTypeDTO inventoryType in inventoryTypes)
            {
                list3.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
            }

            filter.InventoryTypeCombo.ItemsSource = list3;

            filter.ShowDialog();
        }

        public void AddInventorySelection(long inventoryId, string inventoryName)
        {
            WorkOrderInventoryItemDTO dto = new WorkOrderInventoryItemDTO(0, inventoryId, inventoryName, String.Empty, 0);

            workOrderInventoryList.Add(dto);
            list1.Add(dto);
            this.WorkOrderInventoryListView.ItemsSource = list1;

            SetWorkOrderSalesData();
        }

        public GetWorkOrderSalesDetailResponse GetWorkOrderDetail()
        {
            GetWorkOrderSalesDetailResponse response = new GetWorkOrderSalesDetailResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                string jsonData = JsonConvert.SerializeObject(new GetWorkOrderSalesDetailRequest(workOrderInventoryList,0,0));
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/GetWorkOrderDetail", content).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    response = JsonConvert.DeserializeObject<GetWorkOrderSalesDetailResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving work order sales detail");
                }
            }
            catch(Exception ex)
            {

            }

            return response;
        }

        public void AddPersonSelection(PersonDTO person)
        {
            BuyerTextBox.Text = person.CustomerName;
        }

        private void CustomerSearch_Click(object sender, RoutedEventArgs e)
        {
            PersonFilter filter = new PersonFilter();
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            filter.Owner = wnd;

            //GetPersonResponse response = wnd.GetCustomers(new GetPersonRequest());

            //List<PersonDTO> people = response.PersonAndAddress;

            filter.mainWnd = wnd;
            filter.workOrderParentWnd = this;

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            //foreach (InventoryTypeDTO inventoryType in inventoryTypes)
            //{
            //    if (inventoryType.InventoryTypeName != "Arrangements")
            //    {
            //        list1.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
            //    }
            //}

            //filter.InventoryTypeCombo.ItemsSource = list1;

            filter.ShowDialog();
        }

        private void SetWorkOrderSalesData()
        {
            GetWorkOrderSalesDetailResponse response = GetWorkOrderDetail();

            SubTotal.Text = response.SubTotal.ToString("C", CultureInfo.CurrentCulture);
            Tax.Text = response.Tax.ToString("C", CultureInfo.CurrentCulture);
            Total.Text = response.Total.ToString("C", CultureInfo.CurrentCulture);
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetWorkOrderSalesData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            WorkOrderInventoryItemDTO workOrderInventoryItemDTO = (sender as Button).CommandParameter as WorkOrderInventoryItemDTO;

            workOrderInventoryList.Remove(workOrderInventoryItemDTO);

            list1.Remove(workOrderInventoryItemDTO);

            this.WorkOrderInventoryListView.ItemsSource = list1;

            SetWorkOrderSalesData();
        }
    }
}
