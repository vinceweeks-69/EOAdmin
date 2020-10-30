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

        List<UserDTO> Users { get; set; }

        List<AddArrangementRequest> arrangementList = new List<AddArrangementRequest>();

        ObservableCollection<WorkOrderInventoryMapDTO> list1 = new ObservableCollection<WorkOrderInventoryMapDTO>();

        WorkOrderResponse currentWorkOrder = new WorkOrderResponse();

        public long CurrentWorkOrderId { get { return currentWorkOrder.WorkOrder.WorkOrderId; } }

        public PersonDTO Customer { get; set; }

        public WorkOrderPage()
        {
            InitializeComponent();

            GetUsers();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            list1.Add(new KeyValuePair<long, string>(0, "Walk In"));
            list1.Add(new KeyValuePair<long, string>(1, "Phone"));
            list1.Add(new KeyValuePair<long, string>(2, "Email"));

            OrderType.ItemsSource = list1;
            OrderType.SelectedIndex = 0;

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            list2.Add(new KeyValuePair<long, string>(1, "Pickup"));
            list2.Add(new KeyValuePair<long, string>(2, "Delivery"));
            list2.Add(new KeyValuePair<long, string>(3, "Site Service"));

            DeliveryType.ItemsSource = list2;
            DeliveryType.SelectionChanged += DeliveryType_SelectionChanged;
            DeliveryType.SelectedIndex = 0;

            WorkOrderDate.SelectedDate = DateTime.Today;
            PickupDate.SelectedDate = DateTime.Today;

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

            WorkOrderInventoryListView.ItemsSource = new ObservableCollection<WorkOrderViewModel>();
        }

        public WorkOrderPage(WorkOrderResponse workOrderResponse) : this()
        {
            currentWorkOrder = workOrderResponse;
            arrangementList = workOrderResponse.ArrangementRequestList();

            if (currentWorkOrder.WorkOrder.WorkOrderId <= 0)
            {
                PayButton.IsEnabled = false;
            }
            else
            {
                PayButton.IsEnabled = true;
                SaveButton.IsEnabled = true;

                if (currentWorkOrder.WorkOrder.Paid)
                {
                    PayButton.IsEnabled = false;
                    SaveButton.IsEnabled = false;
                }
            }

            LoadCustomer();    
        }

        private async void GetUsers()
        {
            GenericGetRequest request = new GenericGetRequest("GetUsers", String.Empty, 0);
            ((App)App.Current).GetRequest<GetUserResponse>(request).ContinueWith(a => UsersLoaded(a.Result));
        }

        private void UsersLoaded(GetUserResponse response)
        {
            Users = response.Users;

            List<UserDTO> sellers = Users.Where(a => a.RoleId < 3).ToList();

            List<UserDTO> deliverers = Users.Where(a => a.RoleId == 1 || a.RoleId == 3).ToList();

            Dispatcher.Invoke(() =>
            {
                ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
                foreach (UserDTO u in sellers)
                {
                    list1.Add(new KeyValuePair<long, string>(u.UserId, u.UserName));
                }

                Seller.ItemsSource = list1;

                ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
                foreach (UserDTO u in deliverers)
                {
                    list2.Add(new KeyValuePair<long, string>(u.UserId, u.UserName));
                }

                DeliveryPerson.ItemsSource = list2;

                DeliveryPerson.SelectionChanged += DeliveryPerson_SelectionChanged;
            });
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
                MainWindow wnd = Application.Current.MainWindow as MainWindow;

                if (cb.SelectedIndex == 1)
                {
                    PersonFilter pFilter = new PersonFilter(this);
                    pFilter.Owner = wnd;
                    pFilter.ShowDialog();
                }
                else if(cb.SelectedIndex == 2)
                {
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
        public override void LoadWorkOrderData(WorkOrderMessage msg)
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

                if(msg.WorkOrderPaid.HasValue)
                {
                    PayButton.IsEnabled = false;
                    SaveButton.IsEnabled = false;
                    CameraButton.IsEnabled = false;
                    currentWorkOrder.WorkOrder.Paid = true;
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

            //names must be unique
            if(!currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryName == notIn.NotInInventoryName).Any())
            {
                currentWorkOrder.NotInInventory.Add(notIn);
            }
            else
            {
                MessageBox.Show(Application.Current.MainWindow,"Duplicate name for not in inventory item.", "Error", MessageBoxButton.OK);
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

            bool shouldEnable = currentWorkOrder.WorkOrder.Paid ? false : true;

            foreach (WorkOrderInventoryMapDTO dto in currentWorkOrder.WorkOrderList)
            {
                WorkOrderViewModel vm = new WorkOrderViewModel(dto);

                if(((ObservableCollection<WorkOrderViewModel>)WorkOrderInventoryListView.ItemsSource).Where(a => a.InventoryId == dto.InventoryId).Any())
                {
                    WorkOrderViewModel wovm = ((ObservableCollection<WorkOrderViewModel>)WorkOrderInventoryListView.ItemsSource).Where(a => a.InventoryId == dto.InventoryId).First();
                    vm.Quantity = wovm.Quantity;
                }
                vm.ShouldEnable = shouldEnable;
                list1.Add(vm);
            }

            foreach(NotInInventoryDTO dto in currentWorkOrder.NotInInventory)
            {
                WorkOrderViewModel vm = new WorkOrderViewModel(dto);

                if (((ObservableCollection<WorkOrderViewModel>)WorkOrderInventoryListView.ItemsSource).Where(a => a.NotInInventoryId == dto.NotInInventoryId).Any())
                {
                    WorkOrderViewModel wovm = ((ObservableCollection<WorkOrderViewModel>)WorkOrderInventoryListView.ItemsSource).Where(a => a.NotInInventoryId == dto.NotInInventoryId).First();
                    vm.Quantity = wovm.Quantity;
                }

                vm.ShouldEnable = shouldEnable;
                list1.Add(vm);
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
                    vm.ShouldEnable = shouldEnable;
                    list1.Add(vm);
                }

                foreach(NotInInventoryDTO notIn in aar.NotInInventory)
                {
                    vm = new WorkOrderViewModel(notIn);
                    vm.ShouldEnable = shouldEnable;
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

        public async Task<bool> AddWorkOrder(bool showMsgBox = true)
        {
            bool result = false;

            AddWorkOrderRequest addWorkOrderRequest = new AddWorkOrderRequest();

            try
            {
                if (!currentWorkOrder.WorkOrder.IsCancelled && !currentWorkOrder.WorkOrder.Paid)
                {
                    currentWorkOrder.WorkOrder.Buyer = Buyer.Text;
                    currentWorkOrder.WorkOrder.Seller = Seller.Text;
                    currentWorkOrder.WorkOrder.CreateDate = DateTime.Now;
                    currentWorkOrder.WorkOrder.Comments = CommentsTextBox.Text;

                    if (Customer != null && Customer.person_id != 0)
                    {
                        currentWorkOrder.WorkOrder.CustomerId = Customer.person_id;
                    }

                    currentWorkOrder.WorkOrder.OrderType = Convert.ToInt32(OrderType.SelectedItem != null ? ((KeyValuePair<long, string>)OrderType.SelectedItem).Key : 1);
                    currentWorkOrder.WorkOrder.DeliveryType = Convert.ToInt32(DeliveryType.SelectedItem != null ? ((KeyValuePair<long, string>)DeliveryType.SelectedItem).Key : 1);
                    if (currentWorkOrder.WorkOrder.DeliveryType > 1)
                    {
                        currentWorkOrder.WorkOrder.IsDelivery = true;
                    }

                    currentWorkOrder.WorkOrder.DeliveredBy = DeliveryPerson.SelectedItem != null ? ((KeyValuePair<long, string>)DeliveryPerson.SelectedItem).Value : String.Empty;
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

                    if (response.Id == 0 || !response.Success)
                    {
                        MessageBox.Show(wnd,"Error adding Work Order","Error",MessageBoxButton.OK);
                    }
                    else
                    {
                        if (showMsgBox)
                        {
                            MessageBox.Show(wnd, "Work Order saved", "Success", MessageBoxButton.OK);
                        }

                        currentWorkOrder.WorkOrder.WorkOrderId = response.Id;
                        PayButton.IsEnabled = true;
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ((App)App.Current).LogError(ex.Message, JsonConvert.SerializeObject(addWorkOrderRequest));
            }

            return result;
        }

        public async Task<GetWorkOrderSalesDetailResponse> GetWorkOrderDetail()
        {
             
            WorkOrderResponse request = new WorkOrderResponse();
            request.WorkOrderList = currentWorkOrder.WorkOrderList;
            request.NotInInventory = currentWorkOrder.NotInInventory;
            List<GetArrangementResponse> arrangementResponseList = new List<GetArrangementResponse>();
            foreach(AddArrangementRequest aar in arrangementList)
            {
                GetArrangementResponse getArrangement = new GetArrangementResponse();
                getArrangement.ArrangementList = aar.ArrangementInventory;
                getArrangement.NotInInventory = aar.NotInInventory;
                arrangementResponseList.Add(getArrangement);
            }

            request.Arrangements = arrangementResponseList;

            GetWorkOrderSalesDetailResponse response = await ((App)App.Current).PostRequest<WorkOrderResponse, GetWorkOrderSalesDetailResponse>("GetWorkOrderDetail", request);

            return response;
        }

        public void AddPersonSelection(PersonDTO person)
        {
            Buyer.Text = person.CustomerName;
        }

        private void CustomerSearch_Click(object sender, RoutedEventArgs e)
        {
            PersonFilter filter = new PersonFilter(this);
            filter.Owner = Application.Current.MainWindow as MainWindow;
            filter.ShowDialog();
        }

        private void SetWorkOrderSalesData()
        {
            
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach(WorkOrderViewModel vm in WorkOrderInventoryListView.ItemsSource as ObservableCollection<WorkOrderViewModel>)
            {
                if(vm.InventoryId != 0 && currentWorkOrder.WorkOrderList.Where(a => a.InventoryId == vm.InventoryId).Any())
                {
                    WorkOrderInventoryMapDTO woii = currentWorkOrder.WorkOrderList.Where(a => a.InventoryId == vm.InventoryId).First();
                    woii.Quantity = vm.Quantity;
                }
   
                if(vm.NotInInventoryId != 0 && currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryId == vm.NotInInventoryId).Any())
                {
                    NotInInventoryDTO notIn = currentWorkOrder.NotInInventory.Where(a => a.NotInInventoryId == vm.NotInInventoryId).First();
                    notIn.NotInInventoryQuantity = vm.Quantity;
                }
            }
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

            //couldn't get at the "Add" button that is in the list view header to disable it 
            //if the work order has been paid
            if(currentWorkOrder.WorkOrder.Paid)
            {
                Button b = sender as Button;
                if(b != null)
                {
                    b.IsEnabled = false;
                }
                return;
            }

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
            filter.Owner = wnd;
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

            var col1 = 0.40;
            var col2 = 0.15;
            var col3 = 0.15;
            var col4 = 0.15;
            var col5 = 0.15;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;
            gView.Columns[4].Width = workingWidth * col5;
        }

        private async void PayButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareForPayment();   
        }

        private async void PrepareForPayment()
        {
            AddWorkOrder(false).ContinueWith(a => MakePayment(a.Result));
        }

        private void MakePayment(bool saveSuccessful)
        {
            Dispatcher.Invoke(() =>
            {
                MainWindow wnd = Application.Current.MainWindow as MainWindow;

                if (saveSuccessful) // else AddWorkOrder has put a up a msg box
                {
                    WorkOrderResponse wor = new WorkOrderResponse();
                    wor.WorkOrder = currentWorkOrder.WorkOrder;
                    wor.WorkOrderList = currentWorkOrder.WorkOrderList;
                    wor.NotInInventory = currentWorkOrder.NotInInventory;
                    List<GetArrangementResponse> arrangementResponseList = new List<GetArrangementResponse>();
                    foreach (AddArrangementRequest aar in arrangementList)
                    {
                        GetArrangementResponse getArrangement = new GetArrangementResponse();
                        getArrangement.ArrangementList = aar.ArrangementInventory;
                        getArrangement.NotInInventory = aar.NotInInventory;
                        arrangementResponseList.Add(getArrangement);
                    }

                    wor.Arrangements = arrangementResponseList;

                    PaymentPage paymentPage = new PaymentPage(wor, Customer);
                    wnd.NavigationStack.Push(paymentPage);
                    wnd.MainContent.Content = new Frame() { Content = paymentPage };
                }
            });
        }
    }
}
