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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ContainerPage.xaml
    /// </summary>
    public partial class ContainerPage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        string fileName;
        HttpContent fileStreamContent = null;
        List<ContainerTypeDTO> containerTypes = new List<ContainerTypeDTO>();
        List<ServiceCodeDTO> serviceCodes = new List<ServiceCodeDTO>();
        List<ContainerInventoryDTO> containers = new List<ContainerInventoryDTO>();
        ObservableCollection<ContainerInventoryDTO> list3 = new ObservableCollection<ContainerInventoryDTO>();
        public ContainerPage()
        {
            InitializeComponent();

            containerTypes = GetContainerTypes();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (ContainerTypeDTO code in containerTypes)
            {
                list1.Add(new KeyValuePair<long, string>(code.ContainerTypeId, code.ContainerTypeName));
            }

            this.ContainerTypes.ItemsSource = list1;

            serviceCodes = GetServiceCodes();

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (ServiceCodeDTO code in serviceCodes)
            {
                list2.Add(new KeyValuePair<long, string>(code.ServiceCodeId, code.ServiceCode + "($ " + code.Price.ToString() + ")"));
            }

            //this.ServiceCodes.ItemsSource = list2;

            containers = GetContainers().ContainerInventoryList;

            foreach (ContainerInventoryDTO container in containers)
            {
                list3.Add(container);
            }

            this.ContainerListView.ItemsSource = list3;
        }

        private List<ContainerTypeDTO> GetContainerTypes()
        {
            return wnd.GetContainerTypes();
        }

        private List<ServiceCodeDTO> GetServiceCodes()
        {
            return wnd.GetServiceCodes(SharedData.Enums.ServiceCodeType.Plant);
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

        public void AddContainer()
        {
            long image_id = 0;
            string jsonData = String.Empty;

            try
            {
                AddContainerRequest addContainerRequest = new AddContainerRequest();

                //save image if the user has selected one
                if (fileStreamContent != null)
                {
                    var url = "http://localhost:9000/api/Login/UploadPlantImage";
                    fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
                    fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    using (var client1 = new HttpClient())
                    {
                        using (var formData = new MultipartFormDataContent())
                        {
                            client1.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);
                            formData.Add(fileStreamContent);
                            var response = client1.PostAsync(url, formData).Result;
                            string strImageId = response.Content.ReadAsStringAsync().Result;

                            Int64.TryParse(strImageId, out image_id);
                        }
                    }
                }

                KeyValuePair<long, string> containerName = (KeyValuePair<long, string>)this.ContainerNames.SelectedValue;

                KeyValuePair<long, string> containerType = (KeyValuePair<long, string>)this.ContainerTypes.SelectedValue;

                //KeyValuePair<long, string> serviceCode = (KeyValuePair<long, string>)this.ServiceCodes.SelectedValue;

                ContainerDTO cDTO = new ContainerDTO();
                cDTO.ContainerName = containerName.Value;
                cDTO.ContainerTypeId = containerType.Key;
                cDTO.ContainerTypeName = containerType.Value;
                addContainerRequest.Container = cDTO;

                InventoryDTO iDTO = new InventoryDTO();
                iDTO.InventoryName = containerName.Value;
                iDTO.InventoryTypeId = 2; //"Containers"
                //iDTO.ServiceCodeId = serviceCode.Key;

                addContainerRequest.Container = cDTO;
                addContainerRequest.Inventory = iDTO;
                if (image_id > 0)
                {
                    addContainerRequest.ImageId = image_id;
                }

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                jsonData = JsonConvert.SerializeObject(addContainerRequest);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/AddContainer", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    GetContainerResponse apiResponse = JsonConvert.DeserializeObject<GetContainerResponse>(strData);

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
                        foreach (ContainerInventoryDTO c in apiResponse.ContainerInventoryList)
                        {
                            list3.Add(c);
                        }
                        this.ContainerListView.ItemsSource = null;
                        this.ContainerListView.ItemsSource = list3;
                    }
                }
                else
                {
                    MessageBox.Show("Error adding container");
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception("Admin - AddContainer", ex);
                ((App)App.Current).LogError(ex2.Message, jsonData);
            }
        }

        public GetContainerResponse GetContainers()
        {
            GetContainerResponse containers = new GetContainerResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetContainers").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    containers = JsonConvert.DeserializeObject<GetContainerResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving containers");
                }
            }
            catch (Exception ex)
            {

            }
            return containers;
        }

        private void ShowImage(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            ContainerInventoryDTO containerInventoryDTO= b.CommandParameter as ContainerInventoryDTO;

            GetByteArrayResponse imageResponse = wnd.GetImage(containerInventoryDTO.ImageId);
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

        private void DeleteContainer(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            GetContainerResponse getContainerResponse = b.CommandParameter as GetContainerResponse;
            string jsonData = String.Empty;

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                //client.DefaultRequestHeaders.Add("appkey", "myapp_key");
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                jsonData = JsonConvert.SerializeObject(getContainerResponse.ContainerInventoryList.First().Container.ContainerId);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/DeleteContainer", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();

                    //plantList.Remove(getPlantResponse);

                    //list3.Remove(getPlantResponse);
                    //this.PlantInventoryListView.ItemsSource = null;
                    //this.PlantInventoryListView.ItemsSource = list3;

                    ////ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(strData);

                    ////if (apiResponse.Messages.Count > 0)
                    ////{
                    ////    StringBuilder sb = new StringBuilder();
                    ////    foreach (KeyValuePair<string, List<string>> messages in apiResponse.Messages)
                    ////    {
                    ////        foreach (string msg in messages.Value)
                    ////        {
                    ////            sb.AppendLine(msg);
                    ////        }
                    ////    }

                    ////    MessageBox.Show(sb.ToString());
                    ////}
                }
                else
                {
                    MessageBox.Show("Error deleting plant");
                }
            }
            catch(Exception ex)
            {
                Exception ex2 = new Exception("DeleteContainer", ex);
                ((App)App.Current).LogError(ex2.Message, jsonData);
            }
        }

        private void ContainerTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //load container names combo box
            KeyValuePair<long, string> s = (KeyValuePair<long, string>)((ComboBox)sender).SelectedValue;

            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            List<ContainerNameDTO> containerNames = wnd.GetContainerNamesByType(s.Key);

            ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();

            foreach (ContainerNameDTO dto in containerNames)
            {
                list2.Add(new KeyValuePair<long, string>(dto.ContainerNameId, dto.ContainerName));
            }

            this.ContainerNames.ItemsSource = null;
            this.ContainerNames.ItemsSource = list2;
        }
    }
}
