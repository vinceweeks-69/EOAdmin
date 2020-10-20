using Microsoft.Win32;
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
    /// Interaction logic for ArrangementPage.xaml
    /// </summary>
    public partial class ArrangementPage : EOStackPage
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        string fileName;
        HttpContent fileStreamContent = null;
        List<PlantNameDTO> plantNames = new List<PlantNameDTO>();
        List<PlantTypeDTO> plantTypes = new List<PlantTypeDTO>();
        List<ContainerNameDTO> containerNames = new List<ContainerNameDTO>();
        List<ContainerTypeDTO> containerTypes = new List<ContainerTypeDTO>();
        List<ServiceCodeDTO> serviceCodes = new List<ServiceCodeDTO>();
        List<InventoryTypeDTO> inventoryTypes = new List<InventoryTypeDTO>();
        List<ArrangementInventoryDTO> arrangementList = new List<ArrangementInventoryDTO>();
        List<ArrangementInventoryItemDTO> arrangementInventoryList = new List<ArrangementInventoryItemDTO>();

        List<NotInInventoryDTO> notInInventory = new List<NotInInventoryDTO>();

        List<CustomerContainerDTO> customerContainers = new List<CustomerContainerDTO>();

        CustomerContainerDTO CustomerContainer;

        AddArrangementRequest currentArrangement = new AddArrangementRequest();

        ObservableCollection<KeyValuePair<long, string>> containers = new ObservableCollection<KeyValuePair<long, string>>();

        PersonDTO Customer { get; set; }

        public  ArrangementPage(AddArrangementRequest arrangementRequest)
        {
            InitializeComponent();

            currentArrangement = arrangementRequest;

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            list1.Add(new KeyValuePair<long, string>(1, "Vicky"));
            list1.Add(new KeyValuePair<long, string>(2, "Marguerita"));

            Designer.ItemsSource = list1;

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            list2.Add(new KeyValuePair<long, string>(0, "180"));
            list2.Add(new KeyValuePair<long, string>(1, "360"));

            Style.ItemsSource = list2;

            containers.Add(new KeyValuePair<long, string>(1, "New container"));

            Container.ItemsSource = containers;

            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            if(wnd.PageIsOnStack(typeof(WorkOrderPage)))
            {
                WorkOrderPage wo = (WorkOrderPage)wnd.GetPageFromStack(typeof(WorkOrderPage));

                if(wo != null)
                {
                    currentArrangement.Arrangement.WorkOrderId = wo.CurrentWorkOrderId;

                    Customer = wo.Customer;

                    if (Customer != null && Customer.person_id != 0)
                    {
                        LoadCustomerContainers(Customer.person_id);
                    }
                }
            }

            ObservableCollection<WorkOrderViewModel> list3 = new ObservableCollection<WorkOrderViewModel>();

            arrangementInventoryList = currentArrangement.ArrangementInventory;
            notInInventory = currentArrangement.NotInInventory;

            ArrangementInventoryListView.ItemsSource = new ObservableCollection<WorkOrderViewModel>();

            ReloadListData();
        }

        private async void LoadCustomerContainers(long customerId)
        {
            CustomerContainerRequest request = new CustomerContainerRequest();
            request.CustomerContainer.CustomerId = Customer.person_id;
            ((App)App.Current).PostRequest<CustomerContainerRequest, CustomerContainerResponse>("GetCustomerContainers", request).ContinueWith(a =>
            {
                CustomerContainersLoaded(a.Result);
            });
        }

        private void CustomerContainersLoaded(CustomerContainerResponse response)
        {
            Dispatcher.Invoke(() =>
            {
                if (response.CustomerContainers.Count > 0)
                {
                    EnableCustomerContainerSecondaryControls(true);

                    containers.Add(new KeyValuePair<long, string>(2, "Customer container at EO"));
                    containers.Add(new KeyValuePair<long, string>(3, "Customer container at customer location"));

                    if (currentArrangement.Arrangement.CustomerContainerId.HasValue)
                    {
                        customerContainers = response.CustomerContainers;
                        SetComboBoxSelection(Container, currentArrangement.Arrangement.Container);

                        if(customerContainers.Where(a => a.CustomerContainerId == currentArrangement.Arrangement.CustomerContainerId).Any())
                        {
                            CustomerContainer = customerContainers.Where(a => a.CustomerContainerId == currentArrangement.Arrangement.CustomerContainerId).First();
                            CustomerContainerLabelEntry.Text = CustomerContainer.Label;
                        }
                    }
                }
                else
                {
                    EnableCustomerContainerSecondaryControls(false);
                }

                if(currentArrangement.Arrangement.ArrangementId > 0)
                {
                    LoadPageData();

                    ReloadListData();
                }

                //set the handler last - selection change triggers a page load
                Container.SelectionChanged += Container_SelectionChanged;
            });
        }

        private void LoadPageData()
        {
            ArrangementName.Text = currentArrangement.Arrangement.ArrangementName;

            SetComboBoxSelection(Designer, currentArrangement.Arrangement.DesignerName);

            SetComboBoxSelection(Style, currentArrangement.Arrangement._180or360);

            Location.Text = currentArrangement.Arrangement.LocationName;

            CustomerContainerLabelEntry.Text = CustomerContainer != null && CustomerContainer.CustomerContainerId != 0 ? CustomerContainer.Label : String.Empty;

            GiftCheckBox.IsChecked = currentArrangement.Arrangement.IsGift == 0 ? false : true;

            GiftCheckBox_Clicked(GiftCheckBox, new RoutedEventArgs());

            GiftMessage.Text = currentArrangement.Arrangement.GiftMessage;
        }

        //Consolidate DTOs
        private void ReloadListData()
        {
            ObservableCollection<WorkOrderViewModel> list4 = new ObservableCollection<WorkOrderViewModel>();

            foreach (ArrangementInventoryItemDTO aiid in currentArrangement.ArrangementInventory)
            {
                if (((ObservableCollection<WorkOrderViewModel>)ArrangementInventoryListView.ItemsSource).Where(a => a.InventoryId == aiid.InventoryId).Any())
                {
                    WorkOrderViewModel wovm = ((ObservableCollection<WorkOrderViewModel>)ArrangementInventoryListView.ItemsSource).Where(a => a.InventoryId == aiid.InventoryId).First();
                    aiid.Quantity = wovm.Quantity;
                }

                list4.Add(new WorkOrderViewModel(aiid, 0));
            }

            foreach (NotInInventoryDTO notIn in currentArrangement.NotInInventory)
            {
                if (((ObservableCollection<WorkOrderViewModel>)ArrangementInventoryListView.ItemsSource).Where(a => a.NotInInventoryId == notIn.NotInInventoryId).Any())
                {
                    WorkOrderViewModel wovm = ((ObservableCollection<WorkOrderViewModel>)ArrangementInventoryListView.ItemsSource).Where(a => a.NotInInventoryId == notIn.NotInInventoryId).First();
                    notIn.NotInInventoryQuantity = wovm.Quantity;
                }

                list4.Add(new WorkOrderViewModel(notIn));
            }

            ArrangementInventoryListView.ItemsSource = list4;
        }

        private void EnableCustomerContainerSecondaryControls(bool shouldShow)
        {
            if (shouldShow)
            {
                CustomerContainerLabel.Visibility = Visibility.Visible;
                CustomerContainerLabelEntry.Visibility = Visibility.Visible;
            }
            else
            {
                CustomerContainerLabel.Visibility = Visibility.Hidden;
                CustomerContainerLabelEntry.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Add data from a subordinate page to this arrangement
        /// </summary>
        /// <param name="msg"></param>
        public override void LoadWorkOrderData(WorkOrderMessage msg)
        {
            if(msg.HasMessage())
            {
                if(msg.CustomerContainer.CustomerContainerId != 0)
                {
                    CustomerContainer = msg.CustomerContainer;
                    CustomerContainerLabelEntry.Text = CustomerContainer.Label;
                }

                if(msg.Inventory.InventoryId != 0)
                {
                    if(currentArrangement.ArrangementInventory.Where(a => a.InventoryId == msg.Inventory.InventoryId).Any())
                    {
                        ArrangementInventoryItemDTO dto = currentArrangement.ArrangementInventory.Where(a => a.InventoryId == msg.Inventory.InventoryId).First();
                        currentArrangement.ArrangementInventory.Remove(dto);
                    }

                    currentArrangement.ArrangementInventory.Add(new ArrangementInventoryItemDTO()
                    {
                        ArrangementId = currentArrangement.Arrangement.ArrangementId,
                        InventoryId = msg.Inventory.InventoryId,
                        InventoryName = msg.Inventory.InventoryName + " " + msg.Inventory.Size,
                        Quantity = 1
                    }); ;
                }

                if(!String.IsNullOrEmpty(msg.NotInInventory.NotInInventoryName))
                {
                    if(msg.NotInInventory.NotInInventoryId != 0)
                    {
                        if(currentArrangement.NotInInventory.Where(a => a.NotInInventoryId == msg.NotInInventory.NotInInventoryId).Any())
                        {
                            NotInInventoryDTO dto = currentArrangement.NotInInventory.Where(a => a.NotInInventoryId == msg.NotInInventory.NotInInventoryId).First();
                            currentArrangement.NotInInventory.Remove(dto);
                        }
                    }
                    else if(msg.NotInInventory.UnsavedId != 0)
                    {
                        if (currentArrangement.NotInInventory.Where(a => a.UnsavedId == msg.NotInInventory.UnsavedId).Any())
                        {
                            NotInInventoryDTO dto = currentArrangement.NotInInventory.Where(a => a.UnsavedId == msg.NotInInventory.UnsavedId).First();
                            currentArrangement.NotInInventory.Remove(dto);
                        }
                    }

                    //validate - no dupes
                    msg.NotInInventory.ArrangementId = currentArrangement.Arrangement.ArrangementId;
                    currentArrangement.NotInInventory.Add(msg.NotInInventory);
                }

                ReloadListData();
            }
        }
        private List<KeyValuePair<long,string>> GetInventoryList()
        {
            List<KeyValuePair<long, string>> inventoryList = new List<KeyValuePair<long, string>>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetInventoryList").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    GetKvpLongStringResponse response = JsonConvert.DeserializeObject<GetKvpLongStringResponse>(strData);
                    inventoryList = response.KvpList;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving arrangements");
                }
            }
            catch (Exception ex)
            {

            }
            return inventoryList;
        }

        private List<ServiceCodeDTO> GetServiceCodes()
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            return wnd.GetServiceCodes(SharedData.Enums.ServiceCodeType.Plant);
        }

        //see the mobile app - there is a new dto called SimpleArrangmentResponse - validate that the user has enetered a name to search with
        //private List<ArrangementInventoryItemDTO> GetArrangements()
        //{
        //    List<ArrangementInventoryItemDTO> arrangementList = new List<ArrangementInventoryItemDTO>();

        //    try
        //    {
        //        HttpClient client = new HttpClient();
        //        client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

        //        HttpResponseMessage httpResponse =
        //            client.GetAsync("api/Login/GetArrangements").Result;
        //        if (httpResponse.IsSuccessStatusCode)
        //        {
        //            Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
        //            StreamReader strReader = new StreamReader(streamData);
        //            string strData = strReader.ReadToEnd();
        //            strReader.Close();
        //            GetArrangementResponse response = JsonConvert.DeserializeObject<GetArrangementResponse>(strData);

        //            arrangementList = response.ArrangementList;
        //        }
        //        else
        //        {
        //            MessageBox.Show("There was an error retreiving arrangements");
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return arrangementList;
        //}

        private void ShowImage(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            ArrangementInventoryDTO getArrangementResponse = b.CommandParameter as ArrangementInventoryDTO;

            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            GetByteArrayResponse imageResponse = wnd.GetImage(getArrangementResponse.ImageId);
            ImageWindow imageWindow = new ImageWindow();
            imageWindow.Top = Application.Current.MainWindow.Top + 200;
            imageWindow.Left = Application.Current.MainWindow.Left + 200;

            if (imageResponse.ImageData != null && imageResponse.ImageData.Length > 0)
            {
                BitmapImage image = new BitmapImage();
                using (var mem = new System.IO.MemoryStream(imageResponse.ImageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();

                imageWindow.ImageBox.Source = image;
            }

            imageWindow.ShowDialog();
        }

        private void OnShowArrangementInventoryImage(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            //add a check to see if this plant already has an image and warn if
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                fileStreamContent = new StreamContent(File.OpenRead(openFileDialog.FileName));
            }
        }

        private void DeleteArrangement(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            //ProductCategory productCategory = b.CommandParameter as ProductCategory;
            //MessageBox.Show(productCategory.Id);
        }
        
        public void OnSave(object sender, EventArgs e)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            if(wnd.PageIsOnStack(typeof(WorkOrderPage)))
            {
                //save this arangement to the work order in progress
                IEOStackPage workOrderPage = wnd.GetEOStackPage(typeof(WorkOrderPage));

                if(workOrderPage != null)
                {
                    WorkOrderMessage msg = new WorkOrderMessage();
                    //load the currennt object with data from the form
                    GetArrangementData();
                    msg.Arrangement = currentArrangement;
                    workOrderPage.LoadWorkOrderData(new WorkOrderMessage());
                }

                wnd.OnBackClick(this);
            }
            else
            {
                //AddArrangement();
            }
        }

        void GetArrangementData()
        {
            currentArrangement.Arrangement.ArrangementName = ArrangementName.Text;
            if (CustomerContainer != null)
            {
                currentArrangement.Arrangement.CustomerContainerId = CustomerContainer.CustomerContainerId;
            }

            if (Designer.SelectedItem != null)
            {
                currentArrangement.Arrangement.DesignerName = ((KeyValuePair<long, string>)Designer.SelectedItem).Value;
            }

            if (GiftCheckBox.IsChecked.HasValue)
            {
                currentArrangement.Arrangement.IsGift = GiftCheckBox.IsChecked.Value == true  ? 1 : 0;
            }

            currentArrangement.Arrangement.GiftMessage = GiftMessage.Text;
            currentArrangement.Arrangement.LocationName = Location.Text;

            if (Style.SelectedItem != null)
            {
                currentArrangement.Arrangement._180or360 = ((KeyValuePair<long, string>)Style.SelectedItem).Value == "180" ? 0 : 1;
            }

            //currentArrangment has current inventory and non inventory lists
            ReloadListData();

            currentArrangement.ArrangementInventory = arrangementInventoryList;

            currentArrangement.NotInInventory = notInInventory;
        }

        public void AddArrangement()
        {
            long image_id = 0;
            string jsonData = String.Empty;

            try
            {
                AddArrangementRequest addArrangementRequest = new AddArrangementRequest();

                //save image if the user has selected one
                if (fileStreamContent != null)
                {
                    var url = "http://localhost:9000/api/Login/UploadPlantImage";
                    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    using (var client1 = new HttpClient())
                    {
                        client1.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                        using (var formData = new MultipartFormDataContent())
                        {
                            formData.Add(fileStreamContent);
                            var response = client1.PostAsync(url, formData).Result;
                            string strImageId = response.Content.ReadAsStringAsync().Result;

                            Int64.TryParse(strImageId, out image_id);
                        }
                    }
                }

                String arrangementName = this.ArrangementName.Text;

                //KeyValuePair<long, string> serviceCode = (KeyValuePair<long, string>)this.ServiceCodes.SelectedValue;

                ArrangementDTO aDTO = new ArrangementDTO();
                aDTO.ArrangementName = arrangementName;
                addArrangementRequest.Arrangement = aDTO;

                InventoryDTO iDTO = new InventoryDTO();
                iDTO.InventoryName = arrangementName;
                iDTO.InventoryTypeId = 3; //"Arrangements"
                //iDTO.ServiceCodeId = serviceCode.Key;

                //foreach (ArrangementInventoryItemDTO ai in arrangementInventoryList)
                //{
                //    addArrangementRequest.ArrangementInventory.Add(new ArrangementInventoryItemDTO()
                //    {
                //        ArrangementInventoryName = ai.InventoryName,
                //        InventoryId = ai.InventoryId,
                //        Quantity = ai.Quantity,
                //    });
                //}

                ///ADD FROM currentArrangement

                addArrangementRequest.Arrangement = aDTO;
                addArrangementRequest.Inventory = iDTO;
                if (image_id > 0)
                {
                    addArrangementRequest.ImageId = image_id;
                }

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                jsonData = JsonConvert.SerializeObject(addArrangementRequest);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/AddArrangement", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    //Reload();
                }
                else
                {
                    MessageBox.Show("Error adding arrangement");
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception("AddArrangement", ex);
                ((App)App.Current).LogError(ex2.Message, jsonData);
            }
        }

        //add the selected inventory item to the list of inventory items for the current arrangement
        private void InventoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //KeyValuePair<long, string> kvp = ((KeyValuePair<long, string>)InventoryCombo.SelectedValue);
            //long inventoryId = kvp.Key;
            //string inventoryName = kvp.Value;
            //arrangementInventoryList.Add(new ArrangementInventoryItemDTO(0, inventoryId, inventoryName, 0));
            //this.InventoryListView.ItemsSource = null;
            //this.InventoryListView.ItemsSource = arrangementInventoryList;
        }

        private void OnDeleteArrangementInventory(object sender, RoutedEventArgs e)
        {
            //Button b = sender as Button;
            //ArrangementInventoryItemDTO arrangementInventoryItem = b.CommandParameter as ArrangementInventoryItemDTO;
            //arrangementInventoryList.Remove(arrangementInventoryItem);
            //this.InventoryListView.ItemsSource = null;
            //this.InventoryListView.ItemsSource = arrangementInventoryList;
        }

        public void AddInventorySelection(long inventoryId, string inventoryName,long inventoryTypeId)
        {
            arrangementInventoryList.Add(new ArrangementInventoryItemDTO(0, inventoryId, inventoryName, inventoryTypeId, 0));
            //this.InventoryListView.ItemsSource = null;
            //this.InventoryListView.ItemsSource = arrangementInventoryList;
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            ArrangementFilter filter = new ArrangementFilter(this);
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            inventoryTypes = wnd.GetInventoryTypes();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (InventoryTypeDTO inventoryType in inventoryTypes)
            {
                if (inventoryType.InventoryTypeName != "Arrangements")
                {
                    list1.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
                }
            }

            filter.InventoryTypeCombo.ItemsSource = list1;

            filter.ShowDialog();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GiftCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if(cb != null)
            {
                if(cb.IsChecked.HasValue)
                {
                    if (cb.IsChecked.Value)
                    {
                        GiftMessageBorder.Visibility = Visibility.Visible;
                        GiftMessageLabel.Visibility = Visibility.Visible;
                        GiftMessage.Visibility = Visibility.Visible;
                        GiftMessage.IsEnabled = true;
                    }
                    else
                    {
                        GiftMessageBorder.Visibility = Visibility.Hidden;
                        GiftMessageLabel.Visibility = Visibility.Hidden;
                        GiftMessage.Visibility = Visibility.Hidden;
                        GiftMessage.IsEnabled = false;
                    }
                }
                else
                {
                    GiftMessageBorder.Visibility = Visibility.Hidden;
                    GiftMessageLabel.Visibility = Visibility.Hidden;
                    GiftMessage.Visibility = Visibility.Hidden;
                    GiftMessage.IsEnabled = false;
                }
            }
        }

        private void ArrangementSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearArrangement_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveArrangement_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            if (wnd.PageIsOnStack(typeof(WorkOrderPage)))
            {
                if (wnd.PageIsOnStack(typeof(InventoryFilter)))
                {
                    wnd.NavigationStack.Pop();
                }

                //save this arangement to the work order in progress
                IEOStackPage workOrderPage = wnd.GetEOStackPage(typeof(WorkOrderPage));

                if (workOrderPage != null)
                {
                    GetArrangementData();

                    WorkOrderMessage msg = new WorkOrderMessage();
                    msg.Arrangement = currentArrangement;
                    workOrderPage.LoadWorkOrderData(msg);
                }

                wnd.OnBackClick(this);
            }
            else
            {
                //AddArrangement();
            }
        }

        private void AddItemNotInInventory_Click(object sender, RoutedEventArgs e)
        {
            //if(NotInInventoryName.Text != String.Empty && NotInInventoryQuantity.Text != String.Empty && NotInInventorySize.Text != String.Empty && NotInInventoryPrice.Text != String.Empty)
            //{
            //    if(!notInInventory.Where(a => a.NotInInventoryName != NotInInventoryName.Text && a.NotInInventoryQuantity != Convert.ToInt32(NotInInventoryQuantity.Text) &&
            //        a.NotInInventorySize != NotInInventorySize.Text && a.NotInInventoryPrice != Convert.ToDecimal(NotInInventoryPrice.Text)).Any())
            //    {
            //        NotInInventoryDTO notIn = new NotInInventoryDTO();
            //        notIn.NotInInventoryName = NotInInventoryName.Text;
            //        notIn.NotInInventoryQuantity = Convert.ToInt32(NotInInventoryQuantity.Text);
            //        notIn.NotInInventorySize = NotInInventorySize.Text;
            //        notIn.NotInInventoryPrice = Convert.ToDecimal(NotInInventoryPrice.Text);
            //        notInInventory.Add(notIn);

            //        ReloadListData();
            //    }
            //}
        }

        private void Container_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if container type is "Customer container at EO" pick from  customer containers on site

            //if container type is "Customer container at customer site" means use liner

            //if container type is "New container" pick container from inventory

            ComboBox p = sender as ComboBox;

            if (p != null)
            {
                if (p.SelectedIndex == 0)
                {
                    EnableCustomerContainerSecondaryControls(false);
                }
                else
                {
                    EnableCustomerContainerSecondaryControls(true);

                    MainWindow wnd = Application.Current.MainWindow as MainWindow;
                    CustomerContainerPage customerContainerPage = new CustomerContainerPage(this, this.Customer);
                    wnd.NavigationStack.Push(customerContainerPage);
                    wnd.MainContent.Content = new Frame() { Content = customerContainerPage };
                }
            }
        }

        public void SetComboBoxSelection(ComboBox cb, string value)
        {
            foreach (KeyValuePair<long, string> kvp in cb.ItemsSource as ObservableCollection<KeyValuePair<long, string>>)
            {
                if (kvp.Value == value)
                {
                    cb.SelectedItem = kvp;
                }
            }
        }

        public void SetComboBoxSelection(ComboBox cb, long key)
        {
            foreach (KeyValuePair<long, string> kvp in cb.ItemsSource as ObservableCollection<KeyValuePair<long, string>>)
            {
                if (kvp.Key == key)
                {
                    cb.SelectedItem = kvp;
                }
            }
        }

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gView = ArrangementInventoryListView.View as GridView;

            var workingWidth = PageGrid.ActualWidth - 32;             // SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            ArrangementInventoryListView.Width = workingWidth;

            var col1 = 0.50;
            var col2 = 0.10;
            var col3 = 0.20;
            var col4 = 0.20;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;

            var workingHeight = PageGrid.RowDefinitions.ElementAt(12).ActualHeight;

            ArrangementInventoryListView.Height = workingHeight * 0.9;
        }
    }
}
