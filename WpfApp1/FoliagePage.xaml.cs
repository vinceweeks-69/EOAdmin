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
    /// Interaction logic for FoliagePage.xaml
    /// </summary>
    public partial class FoliagePage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;
        string fileName;
        HttpContent fileStreamContent = null;
        List<FoliageTypeDTO> foliageTypes = new List<FoliageTypeDTO>();
        List<FoliageNameDTO> foliageNames = new List<FoliageNameDTO>();
        List<ServiceCodeDTO> serviceCodes = new List<ServiceCodeDTO>();
        List<FoliageInventoryDTO> foliageList = new List<FoliageInventoryDTO>();

        ObservableCollection<FoliageInventoryDTO> list3 = new ObservableCollection<FoliageInventoryDTO>();

        public FoliagePage()
        {
            InitializeComponent();

            foliageTypes = wnd.GetFoliageTypes();

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            foreach (FoliageTypeDTO code in foliageTypes)
            {
                list1.Add(new KeyValuePair<long, string>(code.FoliageTypeId, code.FoliageTypeName));
            }

            this.FoliageTypes.ItemsSource = list1;

            //serviceCodes = GetServiceCodes();

            //ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();
            //foreach (ServiceCodeDTO code in serviceCodes)
            //{
            //    list2.Add(new KeyValuePair<long, string>(code.ServiceCodeId, code.ServiceCode + "($ " + code.Price.ToString() + ")"));
            //}

            foliageList = GetFoliage().FoliageInventoryList;

            foreach (FoliageInventoryDTO foliage in foliageList)
            {
                list3.Add(foliage);
            }

            this.FoliageInventoryListView.ItemsSource = list3;
        }

        private List<FoliageTypeDTO> GetFoliageTypes()
        { 
            return wnd.GetFoliageTypes();
        }

        private List<FoliageNameDTO> GetFoliageNames()
        {
            return wnd.GetFoliageNames();
        }

        private List<ServiceCodeDTO> GetServiceCodes()
        {
            return wnd.GetServiceCodes(SharedData.Enums.ServiceCodeType.Plant);
        }

        private GetFoliageResponse GetFoliageByType(long foliageTypeId)
        {
            return wnd.GetFoliageByType(foliageTypeId);
        }

        private GetFoliageResponse GetFoliage()
        {
            GetFoliageResponse foliage = new GetFoliageResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetFoliage").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    foliage = JsonConvert.DeserializeObject<GetFoliageResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving foliage");
                }
            }
            catch (Exception ex)
            {

            }

            return foliage;
        }
        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            ////add a check to see if this plant already has an image and warn if
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //if (openFileDialog.ShowDialog() == true)
            //{
            //    fileStreamContent = new StreamContent(File.OpenRead(openFileDialog.FileName));
            //}
        }

        private void ShowImage(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            FoliageInventoryDTO foliageInventoryDTO = b.CommandParameter as FoliageInventoryDTO;

            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            GetByteArrayResponse imageResponse = wnd.GetImage(foliageInventoryDTO.ImageId);
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

            imageWindow.Owner = wnd;
            imageWindow.ShowDialog();
        }

        private void DeleteFoliage(object sender, RoutedEventArgs e)
        {
            //Button b = sender as Button;
            //PlantInventoryDTO plantInventoryDTO = b.CommandParameter as PlantInventoryDTO;

            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
            ////client.DefaultRequestHeaders.Add("appkey", "myapp_key");
            //client.DefaultRequestHeaders.Accept.Add(
            //   new MediaTypeWithQualityHeaderValue("application/json"));

            //client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

            //string jsonData = JsonConvert.SerializeObject(plantInventoryDTO.Plant.PlantId);
            //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            //HttpResponseMessage httpResponse = client.PostAsync("api/Login/DeletePlant", content).Result;
            //if (httpResponse.IsSuccessStatusCode)
            //{
            //    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
            //    StreamReader strReader = new StreamReader(streamData);
            //    string strData = strReader.ReadToEnd();
            //    strReader.Close();

            //    plantList.Remove(plantInventoryDTO);

            //    list3.Remove(plantInventoryDTO);
            //    this.PlantInventoryListView.ItemsSource = null;
            //    this.PlantInventoryListView.ItemsSource = list3;

            //    //ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(strData);

            //    //if (apiResponse.Messages.Count > 0)
            //    //{
            //    //    StringBuilder sb = new StringBuilder();
            //    //    foreach (KeyValuePair<string, List<string>> messages in apiResponse.Messages)
            //    //    {
            //    //        foreach (string msg in messages.Value)
            //    //        {
            //    //            sb.AppendLine(msg);
            //    //        }
            //    //    }

            //    //    MessageBox.Show(sb.ToString());
            //    //}
            //}
            //else
            //{
            //    MessageBox.Show("Error deleting plant");
            //}
        }

        public void AddPlant()
        {
            long image_id = 0;

            //try
            //{
            //    List<string> errorMsgs = new List<string>();

            //    KeyValuePair<long, string> p = (KeyValuePair<long, string>)this.PlantNames.SelectedValue;

            //    long plantNameId = p.Key;
            //    String plantName = p.Value;
            //    long selectedPlantTypeIndex = ((KeyValuePair<long, string>)this.PlantTypes.SelectedValue).Key;
            //    //long selectedServiceCode = ((KeyValuePair<long, string>)this.ServiceCodes.SelectedValue).Key;

            //    if (String.IsNullOrEmpty(plantName))
            //    {
            //        errorMsgs.Add("You must enter a plant name.");
            //    }

            //    if (selectedPlantTypeIndex == 0)
            //    {
            //        errorMsgs.Add("You must select a plant type.");
            //    }

            //    //if(selectedServiceCode == 0)
            //    //{
            //    //    errorMsgs.Add("You must select a service code.");
            //    //}

            //    if (fileStreamContent == null)
            //    {
            //        MessageBoxResult msgBoxResult = MessageBox.Show("Do you want to add an image?");

            //        if (msgBoxResult == MessageBoxResult.Yes)
            //        {
            //            return;
            //        }
            //    }

            //    if (errorMsgs.Count > 0)
            //    {
            //        StringBuilder sb = new StringBuilder();
            //        foreach (string msg in errorMsgs)
            //        {
            //            sb.AppendLine(msg);
            //        }

            //        MessageBox.Show(sb.ToString());
            //        return;
            //    }

            //    AddPlantRequest addPlantRequest = new AddPlantRequest();

            //    //save image if the user has selected one
            //    if (fileStreamContent != null)
            //    {
            //        var url = "http://localhost:9000/api/Login/UploadPlantImage";
            //        fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
            //        fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            //        using (var client1 = new HttpClient())
            //        {
            //            using (var formData = new MultipartFormDataContent())
            //            {
            //                client1.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

            //                formData.Add(fileStreamContent);
            //                var response = client1.PostAsync(url, formData).Result;
            //                string strImageId = response.Content.ReadAsStringAsync().Result;

            //                Int64.TryParse(strImageId, out image_id);
            //            }
            //        }
            //    }

            //    PlantDTO pDTO = new PlantDTO();
            //    pDTO.PlantName = plantName;
            //    pDTO.PlantTypeId = selectedPlantTypeIndex;
            //    addPlantRequest.Plant = pDTO;

            //    InventoryDTO iDTO = new InventoryDTO();
            //    iDTO.InventoryName = plantName;
            //    iDTO.InventoryTypeId = 1; //"Plants"
            //    //iDTO.ServiceCodeId = selectedServiceCode;

            //    addPlantRequest.Plant = pDTO;
            //    addPlantRequest.Inventory = iDTO;
            //    if (image_id > 0)
            //    {
            //        addPlantRequest.ImageId = image_id;
            //    }

            //    HttpClient client = new HttpClient();
            //    client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
            //    //client.DefaultRequestHeaders.Add("appkey", "myapp_key");
            //    client.DefaultRequestHeaders.Accept.Add(
            //       new MediaTypeWithQualityHeaderValue("application/json"));

            //    client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

            //    string jsonData = JsonConvert.SerializeObject(addPlantRequest);
            //    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            //    HttpResponseMessage httpResponse = client.PostAsync("api/Login/AddPlant", content).Result;
            //    if (httpResponse.IsSuccessStatusCode)
            //    {
            //        Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
            //        StreamReader strReader = new StreamReader(streamData);
            //        string strData = strReader.ReadToEnd();
            //        strReader.Close();
            //        GetPlantResponse apiResponse = JsonConvert.DeserializeObject<GetPlantResponse>(strData);

            //        if (apiResponse.Messages.Count > 0)
            //        {
            //            StringBuilder sb = new StringBuilder();
            //            foreach (KeyValuePair<string, List<string>> messages in apiResponse.Messages)
            //            {
            //                foreach (string msg in messages.Value)
            //                {
            //                    sb.AppendLine(msg);
            //                }
            //            }

            //            MessageBox.Show(sb.ToString());
            //        }
            //        else
            //        {
            //            list3.Add(apiResponse.PlantInventoryList.First());
            //            this.PlantInventoryListView.ItemsSource = null;
            //            this.PlantInventoryListView.ItemsSource = list3;
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Error adding plant");
            //    }

            //    fileStreamContent = null;
            //}
            //catch (Exception ex)
            //{

            //}
        }

        private void FoliageTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ////get the related plant names for this type and populate the the PlantNames combo box
            //KeyValuePair<long, string> s = (KeyValuePair<long, string>)((ComboBox)sender).SelectedValue;

            //MainWindow wnd = Application.Current.MainWindow as MainWindow;
            //List<PlantNameDTO> plantNames = wnd.GetPlantNamesByType(s.Key);

            //ObservableCollection<KeyValuePair<long, string>> list2 = new ObservableCollection<KeyValuePair<long, string>>();

            //foreach (PlantNameDTO dto in plantNames)
            //{
            //    list2.Add(new KeyValuePair<long, string>(dto.PlantNameId, dto.PlantName));
            //}

            //this.PlantNames.ItemsSource = null;
            //this.PlantNames.ItemsSource = list2;

            //plantList = GetPlantsByType(s.Key).PlantInventoryList;

            //list3.Clear();

            //foreach (PlantInventoryDTO plant in plantList)
            //{
            //    list3.Add(plant);
            //}

            //this.PlantInventoryListView.ItemsSource = list3;
        }

        private void FoliageNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //KeyValuePair<long, string> name = (KeyValuePair<long, string>)((ComboBox)sender).SelectedValue;

            //List<PlantInventoryDTO> plantsWithSimilarName = plantList.Where(a => a.Plant.PlantName.Contains(name.Value)).ToList();


            //list3.Clear();

            //foreach (PlantInventoryDTO plant in plantsWithSimilarName)
            //{
            //    list3.Add(plant);
            //}

            //this.FoliageInventoryListView.ItemsSource = list3;
        }
    }
}
