using Newtonsoft.Json;
//using SharedData.ListFilters;
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
    /// Interaction logic for ShipmentReportPage.xaml
    /// </summary>
    public partial class ShipmentReportPage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

       GetShipmentResponse shipments = new GetShipmentResponse();

        List<InventoryDTO> inventory = new List<InventoryDTO>();

        ObservableCollection<ShipmentDTO> list1 = new ObservableCollection<ShipmentDTO>();

        ObservableCollection<ShipmentInventoryMapDTO> list2 = new ObservableCollection<ShipmentInventoryMapDTO>(); 

        public ShipmentReportPage()
        {
            InitializeComponent();

            MainWindow mainWnd = Application.Current.MainWindow as MainWindow;

            inventory = mainWnd.GetInventoryByType(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //show shipments for the from and to dates specified - if from and to dates are specified

            if(((this.FromDatePicker.SelectedDate != null && this.FromDatePicker.SelectedDate != DateTime.MinValue) &&
                (this.ToDatePicker.SelectedDate != null && this.ToDatePicker.SelectedDate != DateTime.MinValue)) &&
                (this.FromDatePicker.SelectedDate < this.ToDatePicker.SelectedDate))
            {
                shipments = GetShipments();

                foreach(ShipmentInventoryDTO s in shipments.ShipmentList)
                {
                    foreach (ShipmentInventoryMapDTO x in s.ShipmentInventoryMap)
                    {
                        x.InventoryName = inventory.Where(a => a.InventoryId == x.InventoryId).Select(b => b.InventoryName).First();
                    }

                    list1.Add(s.Shipment);
                }

                ShipmentReportListView.ItemsSource = list1;
            }
            else
            {
                MessageBox.Show("Please pick a From and To date value and ensure that the From date is earlier than the To date.");
            }
        }

        private GetShipmentResponse GetShipments()
        {
            GetShipmentResponse shipments = new GetShipmentResponse();

            try
            {
                ShipmentFilter filter = new ShipmentFilter();
                filter.FromDate = this.FromDatePicker.SelectedDate.Value;
                filter.ToDate = this.ToDatePicker.SelectedDate.Value;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                string jsonData = JsonConvert.SerializeObject(filter);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse =
                    client.PostAsync("api/Login/GetShipments", content).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    shipments = JsonConvert.DeserializeObject<GetShipmentResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving Work Orders");
                }
            }
            catch (Exception ex)
            {

            }

            return shipments;
        }

        private void ShipmentReportListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //they've slected an individual shipment - fill the details list view
            try
            {

                ShipmentDTO item = (sender as ListView).SelectedValue as ShipmentDTO;

                ShipmentInventoryDTO s = shipments.ShipmentList.Where(a => a.Shipment.ShipmentId == item.ShipmentId).FirstOrDefault();

                list2.Clear();

                foreach (ShipmentInventoryMapDTO m in s.ShipmentInventoryMap)
                {
                    list2.Add(m);
                }

                ShipmentDetailListView.ItemsSource = list2;
            }
            catch(Exception ex)
            {

            }
        }

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gView1 = ShipmentReportListView.View as GridView;
            var workingWidth = PageGrid.ActualWidth - 80;   //SystemParameters.VerticalScrollBarWidth; // = 17 not the actual displayed width 

            ShipmentReportListView.Width = workingWidth;
            ShipmentDetailListView.Width = workingWidth;

            var col1 = 0.50;
            var col2 = 0.50;

            gView1.Columns[0].Width = workingWidth * col1;
            gView1.Columns[1].Width = workingWidth * col2;

            GridView gView2 = ShipmentDetailListView.View as GridView;

            col1 = 0.50;
            col2 = 0.50;

            gView2.Columns[0].Width = workingWidth * col1;
            gView2.Columns[1].Width = workingWidth * col2;

            var workingHeight = PageGrid.RowDefinitions.ElementAt(5).ActualHeight;
            workingHeight += PageGrid.RowDefinitions.ElementAt(6).ActualHeight;
            workingHeight += PageGrid.RowDefinitions.ElementAt(7).ActualHeight;

            ShipmentReportListView.Height = workingHeight * 0.5;
            ShipmentDetailListView.Height = workingHeight * 0.5;
        }
    }
}
