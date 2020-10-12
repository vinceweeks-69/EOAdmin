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
    public partial class ArrangementPage : IEOBasePage
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

        List<CustomerContainerDTO> customerContainers = new List<CustomerContainerDTO>();

        CustomerContainerDTO CustomerContainer;

        AddArrangementRequest currentArrangement = new AddArrangementRequest();

        ObservableCollection<KeyValuePair<long, string>> containers = new ObservableCollection<KeyValuePair<long, string>>();

        PersonDTO Customer { get; set; }

        public  ArrangementPage()
        {
            InitializeComponent();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            list1.Add(new KeyValuePair<long, string>(1, "Vicky"));
            list1.Add(new KeyValuePair<long, string>(2, "Marguerita"));

            Designer.ItemsSource = list1;

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            list2.Add(new KeyValuePair<long, string>(1, "180"));
            list2.Add(new KeyValuePair<long, string>(2, "360"));

            Style.ItemsSource = list2;

            containers.Add(new KeyValuePair<long, string>(1, "New container"));

            Container.ItemsSource = containers;

            EnableCustomerContainerSecondaryControls(false);

            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            WorkOrderPage workOrder = (WorkOrderPage)wnd.GetPageFromStack(typeof(WorkOrderPage));

            if(workOrder != null)
            {
                Customer = workOrder.Customer;

                if(Customer != null && Customer.person_id != 0)
                {
                    LoadCustomerContainers(Customer.person_id);
                }
            }
        }

        private async void LoadCustomerContainers(long customer_id)
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
            if(response.CustomerContainers.Count > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    containers.Add(new KeyValuePair<long, string>(2, "Customer container at EO"));
                    containers.Add(new KeyValuePair<long, string>(3, "Customer container at customer location"));
                });
            }
        }

        public ArrangementPage(AddArrangementRequest arrangement) : this()
        {
            InitializeComponent();

            currentArrangement = arrangement;
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
        /// Add an individual inventory item to this arrangement
        /// </summary>
        /// <param name="msg"></param>
        public void LoadWorkOrderData(WorkOrderMessage msg)
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
                    arrangementInventoryList.Add(new ArrangementInventoryItemDTO()
                    {
                        InventoryId = msg.Inventory.InventoryId,
                        InventoryName = msg.Inventory.InventoryTypeName + " " + msg.Inventory.InventoryName +  " " + msg.Inventory.Size,
                        Quantity = 1
                    });

                    ObservableCollection<WorkOrderViewModel> list4 = new ObservableCollection<WorkOrderViewModel>();

                    foreach(ArrangementInventoryItemDTO aiid in arrangementInventoryList)
                    {
                        list4.Add(new WorkOrderViewModel(aiid,0));
                    }

                    ArrangementInventoryListView.ItemsSource = list4;
                }
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
        private List<ArrangementInventoryDTO> GetArrangements()
        {
            List<ArrangementInventoryDTO> arrangementList = new List<ArrangementInventoryDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetArrangements").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    GetArrangementResponse response = JsonConvert.DeserializeObject<GetArrangementResponse>(strData);

                    arrangementList = response.ArrangementList;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving arrangements");
                }
            }
            catch (Exception ex)
            {

            }

            return arrangementList;
        }

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
                IEOBasePage workOrderPage = wnd.GetEOBasePage(typeof(WorkOrderPage));

                if(workOrderPage != null)
                {
                    WorkOrderMessage msg = new WorkOrderMessage();
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

                foreach (ArrangementInventoryItemDTO ai in arrangementInventoryList)
                {
                    addArrangementRequest.ArrangementInventory.Add(new ArrangementInventoryDTO()
                    {
                        ArrangementInventoryName = ai.InventoryName,
                        InventoryId = ai.InventoryId,
                        Quantity = ai.Quantity,
                    });
                }

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

        //private void SearchButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ArrangementFilter filter = new ArrangementFilter(this);
        //    MainWindow wnd = Application.Current.MainWindow as MainWindow;
        //    //filter.Owner = wnd;
            
        //    inventoryTypes = wnd.GetInventoryTypes();

        //    //filter.mainWnd = wnd;
        //    //filter.arrangementParentWnd = this;

        //    ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
        //    foreach (InventoryTypeDTO inventoryType in inventoryTypes)
        //    {
        //        if (inventoryType.InventoryTypeName != "Arrangements")
        //        {
        //            list1.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
        //        }
        //    }

        //    filter.InventoryTypeCombo.ItemsSource = list1;

        //    filter.ShowDialog();
        //}

        public void AddInventorySelection(long inventoryId, string inventoryName)
        {
            arrangementInventoryList.Add(new ArrangementInventoryItemDTO(0, inventoryId, inventoryName, 0));
            //this.InventoryListView.ItemsSource = null;
            //this.InventoryListView.ItemsSource = arrangementInventoryList;
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            ArrangementFilter filter = new ArrangementFilter(this);
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            //filter.Owner = wnd;

            inventoryTypes = wnd.GetInventoryTypes();

            //filter.mainWnd = wnd;
            //filter.arrangementParentWnd = this;

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

        private void GiftCheckBox_Checked(object sender, RoutedEventArgs e)
        {

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
                IEOBasePage workOrderPage = wnd.GetEOBasePage(typeof(WorkOrderPage));

                if (workOrderPage != null)
                {
                    WorkOrderMessage msg = new WorkOrderMessage();
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

        private void AddItemNotInInventory_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
