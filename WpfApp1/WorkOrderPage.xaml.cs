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
    public partial class WorkOrderPage : EOStackPage
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        List<AddArrangementRequest> arrangementList = new List<AddArrangementRequest>();

        ObservableCollection<WorkOrderInventoryMapDTO> list1 = new ObservableCollection<WorkOrderInventoryMapDTO>();

        WorkOrderResponse currentWorkOrder = new WorkOrderResponse();

        public long CurrentWorkOrderId { get { return currentWorkOrder.WorkOrder.WorkOrderId; } }

        public PersonDTO Customer { get; set; }

        public WorkOrderPage()
        {
            InitializeComponent();

            ObservableCollection<KeyValuePair<long, string>> list0 = new ObservableCollection<KeyValuePair<long, string>>();
            list0.Add(new KeyValuePair<long, string>(0, "Melissa"));
            list0.Add(new KeyValuePair<long, string>(1, "Thom"));
            list0.Add(new KeyValuePair<long, string>(2, "Roseanne"));
            list0.Add(new KeyValuePair<long, string>(3, "Vicky"));
            list0.Add(new KeyValuePair<long, string>(4, "Marguerita"));

            Seller.ItemsSource = list0;

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            list1.Add(new KeyValuePair<long, string>(0, "Walk In"));
            list1.Add(new KeyValuePair<long, string>(1, "Phone"));
            list1.Add(new KeyValuePair<long, string>(2, "Email"));

            OrderType.ItemsSource = list1;

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            list2.Add(new KeyValuePair<long, string>(1, "Pickup"));
            list2.Add(new KeyValuePair<long, string>(2, "Delivery"));
            list2.Add(new KeyValuePair<long, string>(3, "Site Service"));

            DeliveryType.ItemsSource = list2;
            DeliveryType.SelectionChanged += DeliveryType_SelectionChanged;

            ObservableCollection<KeyValuePair<long, string>> list3 = new ObservableCollection<KeyValuePair<long, string>>();
            list3.Add(new KeyValuePair<long, string>(1, "Melissa"));
            list3.Add(new KeyValuePair<long, string>(2, "Thom"));
            list3.Add(new KeyValuePair<long, string>(3, "Danny"));
            list3.Add(new KeyValuePair<long, string>(3, "Robert"));

            DeliveryPerson.ItemsSource = list3;
            DeliveryPerson.SelectionChanged += DeliveryPerson_SelectionChanged;

            DeliveryPerson.Visibility = Visibility.Hidden;
            DeliveryPersonLabel.Visibility = Visibility.Hidden;

            DeliveryDate.Visibility = Visibility.Hidden;
            DeliveryDateLabel.Visibility = Visibility.Hidden;

            ObservableCollection<KeyValuePair<long, string>> list4 = new ObservableCollection<KeyValuePair<long, string>>();
            list4.Add(new KeyValuePair<long, string>(1, "Pick one"));
            list4.Add(new KeyValuePair<long, string>(2, "Choose existing"));
            list4.Add(new KeyValuePair<long, string>(3, "Create new"));

            PickOrCreateBuyer.ItemsSource = list4;
            PickOrCreateBuyer.SelectionChanged += PickOrCreateBuyer_SelectionChanged;

            if(currentWorkOrder.WorkOrder.WorkOrderId <= 0)
            {
                PayButton.IsEnabled = false;  
            }
        }

        public WorkOrderPage(WorkOrderResponse workOrderResponse) : this()
        {
            currentWorkOrder = workOrderResponse;
            arrangementList = workOrderResponse.ArrangementRequestList();

            LoadCustomer();    
        }

        async void LoadCustomer()
        {
            if (currentWorkOrder.WorkOrder.CustomerId != 0)
            {
                GetPersonRequest personRequest = new GetPersonRequest();
                personRequest.PersonId = currentWorkOrder.WorkOrder.CustomerId;
                ((App)App.Current).PostRequest<GetPersonRequest, GetPersonResponse>("GetPerson", personRequest).ContinueWith(a => CustomerLoaded(a.Result));
            }
        }

        void CustomerLoaded(GetPersonResponse personResponse)
        {
            Dispatcher.Invoke(() =>
           {
               if (personResponse.PersonAndAddress.Count == 1)
               {
                   Customer = personResponse.PersonAndAddress.First().Person;
               }

               LoadPageData();

               ReloadItemList();
           });
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
        public void LoadWorkOrderData(WorkOrderMessage msg)
        {
            if(msg.HasMessage())
            {
                if(msg.Person.person_id != 0)
                {
                    Customer = msg.Person;
                    Buyer.Text = Customer.CustomerName;
                }

                if(msg.Inventory.InventoryId != 0)
                {
                    ProcessInventoryData(msg.Inventory);  
                }

                if(!String.IsNullOrEmpty(msg.NotInInventory.NotInInventoryName))
                {
                    ProcessNotInInventoryData(msg.NotInInventory);    
                }

                if((msg.Arrangement.Arrangement.ArrangementId != 0 || msg.Arrangement.Arrangement.UnsavedId != 0) && 
                    (msg.Arrangement.ArrangementInventory.Count > 0 || msg.Arrangement.NotInInventory.Count > 0))
                {
                    ProcessArrangementData(msg);  
                }

                ReloadItemList();
            }
        }

        void ProcessInventoryData(WorkOrderInventoryMapDTO dto)
        {
            if(currentWorkOrder.WorkOrderList.Where(a => a.InventoryId == dto.InventoryId).Any())
            {
                WorkOrderInventoryMapDTO i = currentWorkOrder.WorkOrderList.Where(a => a.InventoryId == dto.InventoryId).First();
                currentWorkOrder.WorkOrderList.Remove(i);
            }
                        
            currentWorkOrder.WorkOrderList.Add(dto);
        }

        void ProcessNotInInventoryData(NotInInventoryDTO notIn)
        {
            if(notIn.NotInInventoryId != 0)
            {
                if(currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryId == notIn.NotInInventoryId).Any())
                {
                    NotInInventoryDTO dto = currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryId == notIn.NotInInventoryId).First();
                    currentWorkOrder.NotInInventory.Remove(dto);
                }
            }
            else if (notIn.UnsavedId != 0)
            {
                if (currentWorkOrder.NotInInventory.Where(a => a.UnsavedId == notIn.UnsavedId).Any())
                {
                    NotInInventoryDTO dto = currentWorkOrder.NotInInventory.Where(a => a.UnsavedId == notIn.UnsavedId).First();
                    currentWorkOrder.NotInInventory.Remove(dto);
                }
            }

            //names must be unique
            if(!currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryName == notIn.NotInInventoryName).Any())
            {
                currentWorkOrder.NotInInventory.Add(notIn);
            }
            else
            {
                MessageBox.Show("Duplicate name for not in inventory item.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
        }

        void ProcessArrangementData(WorkOrderMessage msg)
        {
            if (msg.Arrangement.Arrangement.ArrangementId != 0)
            {
                if (currentWorkOrder.Arrangements.Where(a => a.Arrangement.ArrangementId == msg.Arrangement.Arrangement.ArrangementId).Any())
                {
                    AddArrangementRequest aar = arrangementList.Where(a => a.Arrangement.ArrangementId == msg.Arrangement.Arrangement.ArrangementId).First();

                    arrangementList.Remove(aar);
                }

                arrangementList.Add(msg.Arrangement);
            }
            else if (msg.Arrangement.Arrangement.UnsavedId != 0)
            {
                if (arrangementList.Where(a => a.Arrangement.UnsavedId == msg.Arrangement.Arrangement.UnsavedId).Any())
                {
                    AddArrangementRequest aar = arrangementList.Where(a => a.Arrangement.UnsavedId == msg.Arrangement.Arrangement.UnsavedId).First();
                    arrangementList.Remove(aar);
                }

                arrangementList.Add(msg.Arrangement);
            }
        }

        void LoadPageData()
        {
            
            //set page form values from currentArrangement
            SetComboBoxSelection(Seller, currentWorkOrder.WorkOrder.Seller);
            Buyer.Text = currentWorkOrder.WorkOrder.Buyer;
            CommentsTextBox.Text = currentWorkOrder.WorkOrder.Comments;
            SetComboBoxSelection(OrderType, currentWorkOrder.WorkOrder.OrderType);
            SetComboBoxSelection(DeliveryType, currentWorkOrder.WorkOrder.DeliveryType);
            SetComboBoxSelection(DeliveryPerson, currentWorkOrder.WorkOrder.DeliveredBy);
            WorkOrderDate.SelectedDate = currentWorkOrder.WorkOrder.CreateDate.Date;
            DeliveryDate.SelectedDate = currentWorkOrder.WorkOrder.DeliveryDate.Date;
            PickupDate.SelectedDate = currentWorkOrder.WorkOrder.DeliveryDate.Date;

        }
        void ReloadItemList()
        {
            ObservableCollection<WorkOrderViewModel> list1 = new ObservableCollection<WorkOrderViewModel>();

            foreach(WorkOrderInventoryMapDTO dto in currentWorkOrder.WorkOrderList)
            {
                list1.Add(new WorkOrderViewModel(dto));
            }

            foreach(NotInInventoryDTO dto in currentWorkOrder.NotInInventory)
            {
                list1.Add(new WorkOrderViewModel(dto));
            }

            foreach(AddArrangementRequest aar in arrangementList)
            {
                //blank row
                WorkOrderViewModel vm = new WorkOrderViewModel();

                list1.Add(vm);

                //header row

                vm = new WorkOrderViewModel();
                vm.InventoryName = "Arrangement";
                list1.Add(vm);

                //items
                foreach(ArrangementInventoryItemDTO ai in aar.ArrangementInventory)
                {
                    vm = new WorkOrderViewModel(ai,currentWorkOrder.WorkOrder.WorkOrderId);
                    list1.Add(vm);
                }

                foreach(NotInInventoryDTO notIn in aar.NotInInventory)
                {
                    vm = new WorkOrderViewModel(notIn);
                    list1.Add(vm);
                }

                //blank row
                vm = new WorkOrderViewModel();
                list1.Add(vm);
            }

            WorkOrderInventoryListView.ItemsSource = list1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            AddWorkOrder();
        }

        public async void AddWorkOrder()
        {
            AddWorkOrderRequest addWorkOrderRequest = new AddWorkOrderRequest();

            try
            {
                currentWorkOrder.WorkOrder.Buyer = Buyer.Text;
                currentWorkOrder.WorkOrder.Seller = Seller.Text;
                currentWorkOrder.WorkOrder.CreateDate = DateTime.Now;
                currentWorkOrder.WorkOrder.Comments = CommentsTextBox.Text;
                
                if(Customer.person_id != 0)
                {
                    currentWorkOrder.WorkOrder.CustomerId = Customer.person_id;
                }

                currentWorkOrder.WorkOrder.OrderType = OrderType.SelectedItem != null ? ((KeyValuePair<int, string>)OrderType.SelectedItem).Key : 1;
                currentWorkOrder.WorkOrder.DeliveryType = DeliveryType.SelectedItem != null ? ((KeyValuePair<int, string>)DeliveryType.SelectedItem).Key : 1;
                if(currentWorkOrder.WorkOrder.DeliveryType > 1)
                {
                    currentWorkOrder.WorkOrder.IsDelivery = true;
                }

                currentWorkOrder.WorkOrder.DeliveredBy = DeliveryPerson.SelectedItem != null ? ((KeyValuePair<int, string>)DeliveryPerson.SelectedItem).Value : String.Empty;
                currentWorkOrder.WorkOrder.DeliveryDate = DeliveryDate.SelectedDate.HasValue ? DeliveryDate.SelectedDate.Value : DateTime.MinValue;
                if (!currentWorkOrder.WorkOrder.IsDelivery)
                {
                    currentWorkOrder.WorkOrder.DeliveryDate = PickupDate.SelectedDate.HasValue ? PickupDate.SelectedDate.Value : DateTime.MinValue;
                }

                addWorkOrderRequest.WorkOrder = currentWorkOrder.WorkOrder;
                addWorkOrderRequest.WorkOrderInventoryMap = currentWorkOrder.WorkOrderList;
                addWorkOrderRequest.NotInInventory = currentWorkOrder.NotInInventory;
                addWorkOrderRequest.Arrangements = arrangementList;

                ApiResponse response = await ((App)App.Current).PostRequest<AddWorkOrderRequest, ApiResponse>("AddWorkOrder", addWorkOrderRequest);

                if(!response.Success)
                {
                    MessageBox.Show("Error adding Work Order");
                }
                else
                {
                    //clear form?
                    MessageBox.Show("Work Order saved");
                }
            }
            catch (Exception ex)
            {
                ((App)App.Current).LogError(ex.Message, JsonConvert.SerializeObject(addWorkOrderRequest));
            }
        }

        public GetWorkOrderSalesDetailResponse GetWorkOrderDetail()
        {
            GetWorkOrderSalesDetailResponse response = new GetWorkOrderSalesDetailResponse();

            //try
            //{
            //    HttpClient client = new HttpClient();
            //    client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

            //    string jsonData = JsonConvert.SerializeObject(new GetWorkOrderSalesDetailRequest(workOrderInventoryList,0,0));
            //    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            //    HttpResponseMessage httpResponse = client.PostAsync("api/Login/GetWorkOrderDetail", content).Result;

            //    if (httpResponse.IsSuccessStatusCode)
            //    {
            //        Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
            //        StreamReader strReader = new StreamReader(streamData);
            //        string strData = strReader.ReadToEnd();
            //        strReader.Close();
            //        response = JsonConvert.DeserializeObject<GetWorkOrderSalesDetailResponse>(strData);
            //    }
            //    else
            //    {
            //        MessageBox.Show("There was an error retreiving work order sales detail");
            //    }
            //}
            //catch(Exception ex)
            //{

            //}

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
            //GetWorkOrderSalesDetailResponse response = GetWorkOrderDetail();

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
            WorkOrderViewModel dto = (sender as Button).CommandParameter as WorkOrderViewModel;

            if (dto != null)
            {
                if (dto.InventoryId != 0)
                {
                    if (dto.GroupId.HasValue && dto.GroupId.Value != 0)
                    {
                        if (arrangementList.Where(a => a.Arrangement.ArrangementId == dto.GroupId.Value).Any())
                        {
                            AddArrangementRequest aar = arrangementList.Where(b => b.Arrangement.ArrangementId == dto.GroupId.Value).First();

                            if (aar.ArrangementInventory.Where(c => c.InventoryId == dto.InventoryId).Any())
                            {
                                ArrangementInventoryItemDTO item = aar.ArrangementInventory.Where(c => c.InventoryId == dto.InventoryId).First();

                                aar.ArrangementInventory.Remove(item);
                            }
                        }
                    }
                    else
                    {
                        if (currentWorkOrder.WorkOrderList.Where(a => a.InventoryId == dto.InventoryId).Any())
                        {
                            WorkOrderInventoryMapDTO map = currentWorkOrder.WorkOrderList.Where(b => b.InventoryId == dto.InventoryId).First();
                            currentWorkOrder.WorkOrderList.Remove(map);
                        }
                    }
                }
                else if (dto.NotInInventoryId != 0)
                {
                    if (currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryId == dto.NotInInventoryId).Any())
                    {
                        NotInInventoryDTO notIn = currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryId == dto.NotInInventoryId).First();

                        currentWorkOrder.NotInInventory.Remove(notIn);
                    }
                }

                ReloadItemList();

                SetWorkOrderSalesData();
            }
        }

        private void AddProductToWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            //show the product screen - pass workOrderId
            //MainWindow wnd = Window.GetWindow(this) as MainWindow;
            //InventoryFilter inventoryFilter = new InventoryFilter(this);
            //wnd.NavigationStack.Push(inventoryFilter);
            //wnd.MainContent.Content = new Frame() { Content = inventoryFilter};

            ArrangementFilter filter = new ArrangementFilter(this);
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            List<InventoryTypeDTO> inventoryTypes = wnd.GetInventoryTypes();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (InventoryTypeDTO inventoryType in inventoryTypes)
            {
                //if (inventoryType.InventoryTypeName != "Arrangements")
                {
                    list1.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
                }
            }

            filter.InventoryTypeCombo.ItemsSource = list1;

            filter.ShowDialog();
        }

        private void EditWorkOrderItem_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            if(b != null)
            {
                WorkOrderViewModel vm = (WorkOrderViewModel)b.CommandParameter;

                if(vm != null)
                {
                    if (arrangementList.Where(a => a.Arrangement.ArrangementId == vm.GroupId).Any())
                    {
                        AddArrangementRequest aar = arrangementList.Where(a => a.Arrangement.ArrangementId == vm.GroupId).First();

                        MainWindow wnd = Application.Current.MainWindow as MainWindow;
                        ArrangementPage arrangementPage = new ArrangementPage(aar);
                        wnd.NavigationStack.Push(arrangementPage);
                        wnd.MainContent.Content = new Frame() { Content = arrangementPage };
                    }
                }
            }
        }

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gView = WorkOrderInventoryListView.View as GridView;

            var workingWidth = PageGrid.ActualWidth - 80;   //SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            WorkOrderInventoryListView.Width = workingWidth;

            var col1 = 0.50;
            var col2 = 0.20;
            var col3 = 0.15;
            var col4 = 0.15;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;
        }
    }
}
