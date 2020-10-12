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
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for WorkOrderPage.xaml
    /// </summary>
    public partial class WorkOrderPage : Page, IEOBasePage
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        private List<InventoryTypeDTO> inventoryTypeList;
        //private List<WorkOrderInventoryMapDTO> workOrderInventoryMap = new List<WorkOrderInventoryMapDTO>();
        //private List<WorkOrderInventoryItemDTO> workOrderInventoryList = new List<WorkOrderInventoryItemDTO>();
        ObservableCollection<WorkOrderInventoryItemDTO> list1 = new ObservableCollection<WorkOrderInventoryItemDTO>();

        List<AddArrangementRequest> arrangementList = new List<AddArrangementRequest>();
        List<WorkOrderInventoryItemDTO> workOrderInventoryList = new List<WorkOrderInventoryItemDTO>();
        List<NotInInventoryDTO> notInInventory = new List<NotInInventoryDTO>();

        public PersonDTO Customer { get; set; }

        public WorkOrderPage()
        {
            InitializeComponent();

            ObservableCollection<KeyValuePair<int, string>> list0 = new ObservableCollection<KeyValuePair<int, string>>();
            list0.Add(new KeyValuePair<int, string>(0, "Melissa"));
            list0.Add(new KeyValuePair<int, string>(1, "Thom"));
            list0.Add(new KeyValuePair<int, string>(2, "Roseanne"));
            list0.Add(new KeyValuePair<int, string>(3, "Vicky"));
            list0.Add(new KeyValuePair<int, string>(4, "Marguerita"));

            Seller.ItemsSource = list0;

            ObservableCollection<KeyValuePair<int, string>> list1 = new ObservableCollection<KeyValuePair<int, string>>();
            list1.Add(new KeyValuePair<int, string>(0, "Walk In"));
            list1.Add(new KeyValuePair<int, string>(1, "Phone"));
            list1.Add(new KeyValuePair<int, string>(2, "Email"));

            OrderType.ItemsSource = list1;

            ObservableCollection<KeyValuePair<int, string>> list2 = new ObservableCollection<KeyValuePair<int, string>>();
            list2.Add(new KeyValuePair<int, string>(1, "Pickup"));
            list2.Add(new KeyValuePair<int, string>(2, "Delivery"));
            list2.Add(new KeyValuePair<int, string>(3, "Site Service"));

            DeliveryType.ItemsSource = list2;
            DeliveryType.SelectionChanged += DeliveryType_SelectionChanged;

            ObservableCollection<KeyValuePair<int, string>> list3 = new ObservableCollection<KeyValuePair<int, string>>();
            list3.Add(new KeyValuePair<int, string>(1, "Melissa"));
            list3.Add(new KeyValuePair<int, string>(2, "Thom"));
            list3.Add(new KeyValuePair<int, string>(3, "Danny"));
            list3.Add(new KeyValuePair<int, string>(3, "Robert"));

            DeliveryPerson.ItemsSource = list3;
            DeliveryPerson.SelectionChanged += DeliveryPerson_SelectionChanged;

            DeliveryPerson.Visibility = Visibility.Hidden;
            DeliveryPersonLabel.Visibility = Visibility.Hidden;

            DeliveryDate.Visibility = Visibility.Hidden;
            DeliveryDateLabel.Visibility = Visibility.Hidden;

            ObservableCollection<KeyValuePair<int, string>> list4 = new ObservableCollection<KeyValuePair<int, string>>();
            list4.Add(new KeyValuePair<int, string>(1, "Pick one"));
            list4.Add(new KeyValuePair<int, string>(2, "Choose existing"));
            list4.Add(new KeyValuePair<int, string>(3, "Create new"));

            PickOrCreateBuyer.ItemsSource = list4;
            PickOrCreateBuyer.SelectionChanged += PickOrCreateBuyer_SelectionChanged;
        }

        private void DeliveryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if(cb != null)
            {
                if(cb.SelectedIndex > 0)
                {
                    DeliveryPerson.Visibility = Visibility.Visible;
                    DeliveryPersonLabel.Visibility = Visibility.Visible;

                    DeliveryDate.Visibility = Visibility.Visible;
                    DeliveryDateLabel.Visibility = Visibility.Visible;

                    PickupDate.Visibility = Visibility.Hidden;
                    PickupDateLabel.Visibility = Visibility.Hidden;
                }
                else
                {
                    DeliveryPerson.Visibility = Visibility.Hidden;
                    DeliveryPersonLabel.Visibility = Visibility.Hidden;

                    DeliveryDate.Visibility = Visibility.Hidden;
                    DeliveryDateLabel.Visibility = Visibility.Hidden;

                    PickupDate.Visibility = Visibility.Visible;
                    PickupDateLabel.Visibility = Visibility.Visible;
                }
            }
        }

        private void PickOrCreateBuyer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if (cb != null)
            {
                if (cb.SelectedIndex == 1)
                {
                    PersonFilter pFilter = new PersonFilter(this);

                    pFilter.ShowDialog();
                }
                else if(cb.SelectedIndex == 2)
                {
                    MainWindow wnd = Application.Current.MainWindow as MainWindow;
                    CustomerPage customerPage = new CustomerPage(this);
                    wnd.NavigationStack.Push(customerPage);
                    wnd.MainContent.Content = new Frame() { Content = customerPage };
                }
            }
        }

        private void DeliveryPerson_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            if(cb != null)
            {
                if(cb.SelectedIndex > 0)
                {
                    DeliveryPerson.Visibility = Visibility.Visible;
                    DeliveryPersonLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    DeliveryPerson.Visibility = Visibility.Hidden;
                    DeliveryPersonLabel.Visibility = Visibility.Hidden;
                }
            }
        }

        public WorkOrderPage(WorkOrderResponse workOrderResponse) : this()
        {

        }

        public void LoadWorkOrderData(WorkOrderMessage msg)
        {
            if(msg.HasMessage())
            {
                if(msg.Person.person_id != 0)
                {
                    Customer = msg.Person;
                    Buyer.Text = Customer.CustomerName;
                }
            }
        }


        public void AddWorkOrder()
        {
            try
            {
                AddWorkOrderRequest addWorkOrderRequest = new AddWorkOrderRequest();

                WorkOrderDTO dto = new WorkOrderDTO()
                {
                    Seller = this.Seller.Text,
                    Buyer = this.Buyer.Text,
                    CreateDate = DateTime.Now,
                    Comments = this.CommentsTextBox.Text
                };

                //foreach(WorkOrderInventoryItemDTO woii in workOrderInventoryList)
                //{
                //    workOrderInventoryMap.Add(new WorkOrderInventoryMapDTO()
                //    {
                //        InventoryId = woii.InventoryId,
                //        InventoryName = woii.InventoryName,
                //        Quantity = woii.Quantity
                //    });
                //}

                addWorkOrderRequest.WorkOrder = dto;
                //addWorkOrderRequest.WorkOrderInventoryMap = workOrderInventoryMap;

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
            Buyer.Text = person.CustomerName;
        }

        private void CustomerSearch_Click(object sender, RoutedEventArgs e)
        {
            PersonFilter filter = new PersonFilter(this);

            filter.ShowDialog();
        }

        private void SetWorkOrderSalesData()
        {
            GetWorkOrderSalesDetailResponse response = GetWorkOrderDetail();

            //SubTotal.Text = response.SubTotal.ToString("C", CultureInfo.CurrentCulture);
            //Tax.Text = response.Tax.ToString("C", CultureInfo.CurrentCulture);
            //Total.Text = response.Total.ToString("C", CultureInfo.CurrentCulture);
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

        private void AddProductToWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            //show the product screen - pass workOrderId
            MainWindow wnd = Window.GetWindow(this) as MainWindow;
            InventoryFilter inventoryFilter = new InventoryFilter(this);
            wnd.NavigationStack.Push(inventoryFilter);
            wnd.MainContent.Content = new Frame() { Content = inventoryFilter};
        }
    }
}
