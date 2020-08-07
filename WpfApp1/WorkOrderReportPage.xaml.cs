using Newtonsoft.Json;
using SharedData;
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
    /// Interaction logic for WorkOrderReportPage.xaml
    /// </summary>
    public partial class WorkOrderReportPage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        List<WorkOrderInventoryDTO> workOrderList = new List<WorkOrderInventoryDTO>();
        ObservableCollection<WorkOrderInventoryDTO> list1 = new ObservableCollection<WorkOrderInventoryDTO>();
        ObservableCollection<WorkOrderInventoryMapDTO> list2 = new ObservableCollection<WorkOrderInventoryMapDTO>();
        List<InventoryDTO> inventory = new List<InventoryDTO>(); 

        public WorkOrderReportPage()
        {
            InitializeComponent();

            MainWindow mainWnd = Application.Current.MainWindow as MainWindow;

            inventory = mainWnd.GetInventoryByType(0);
        }
               

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //get work orders for the dates specified - if from and to dates are specified

            if (((this.FromDatePicker.SelectedDate != null && this.FromDatePicker.SelectedDate != DateTime.MinValue) &&
               (this.ToDatePicker.SelectedDate != null && this.ToDatePicker.SelectedDate != DateTime.MinValue)) &&
               (this.FromDatePicker.SelectedDate < this.ToDatePicker.SelectedDate))
            {
                workOrderList = GetWorkOrders();
                list1 = new ObservableCollection<WorkOrderInventoryDTO>(workOrderList);
                foreach(WorkOrderInventoryDTO wor in list1)
                {
                    foreach(WorkOrderInventoryMapDTO x in wor.InventoryList)
                    {
                        x.InventoryName = inventory.Where(a => a.InventoryId == x.InventoryId).Select(b => b.InventoryName).First();
                    }
                }

                this.WorkOrderReportListView.ItemsSource = list1;
            }
            else
            {
                MessageBox.Show("Please pick a From and To date value and ensure that the From date is earlier than the To date.");
            }
        }

        private List<WorkOrderInventoryDTO> GetWorkOrders()
        {
           List<WorkOrderInventoryDTO> workOrders = new List<WorkOrderInventoryDTO>();

            try
            {
                WorkOrderListFilter filter = new WorkOrderListFilter();
                filter.FromDate = this.FromDatePicker.SelectedDate.Value;
                filter.ToDate = this.ToDatePicker.SelectedDate.Value;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                string jsonData = JsonConvert.SerializeObject(filter);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse =
                    client.PostAsync("api/Login/GetWorkOrders",content).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    List<WorkOrderResponse> response = JsonConvert.DeserializeObject<List<WorkOrderResponse>>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving Work Orders");
                }
            }
            catch (Exception ex)
            {

            }

            return workOrders;
        }

        private void WorkOrderReportListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //they 've selected a work order, show the detail

            try
            {

                //they've slected an individual shipment - fill the details list view
                WorkOrderInventoryDTO item = (sender as ListView).SelectedValue as WorkOrderInventoryDTO;

                WorkOrderInventoryDTO r = workOrderList.Where(a => a.WorkOrder.WorkOrderId == item.WorkOrder.WorkOrderId).FirstOrDefault();

                list2.Clear();

                foreach (WorkOrderInventoryMapDTO m in r.InventoryList)
                {
                    list2.Add(m);
                }

                WorkOrderDetailListView.ItemsSource = list2;
            }
            catch(Exception ex)
            {

            }
        }

        private void ShowWorkOrderItems_Clicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
