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
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for WorkOrderReportPage.xaml
    /// </summary>
    public partial class WorkOrderReportPage : EOStackPage
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        List<WorkOrderInventoryMapDTO> workOrderList = new List<WorkOrderInventoryMapDTO>();
        ObservableCollection<WorkOrderInventoryMapDTO> list1 = new ObservableCollection<WorkOrderInventoryMapDTO>();
        List<InventoryDTO> inventory = new List<InventoryDTO>();

        public WorkOrderReportPage()
        {
            InitializeComponent();
            
            MainWindow mainWnd = Application.Current.MainWindow as MainWindow;

            inventory = mainWnd.GetInventoryByType(0);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //get work orders for the dates specified - if from and to dates are specified

            if (((this.FromDatePicker.SelectedDate.HasValue && this.FromDatePicker.SelectedDate.Value != DateTime.MinValue) &&
               (this.ToDatePicker.SelectedDate.HasValue && this.ToDatePicker.SelectedDate.Value != DateTime.MinValue)) &&
               (this.FromDatePicker.SelectedDate.Value < this.ToDatePicker.SelectedDate.Value))
            {
                WorkOrderListFilter filter = new WorkOrderListFilter();
                filter.FromDate = FromDatePicker.SelectedDate.Value;
                filter.ToDate = ToDatePicker.SelectedDate.Value;

                List<WorkOrderResponse> response = await  ((App)App.Current).PostRequest<WorkOrderListFilter,List<WorkOrderResponse>>("GetWorkOrders", filter);

                ObservableCollection<WorkOrderResponse>  list1 = new ObservableCollection<WorkOrderResponse>();

                foreach (WorkOrderResponse r in response)
                {
                    list1.Add(r);                  
                }

                this.WorkOrderReportListView.ItemsSource = list1;
            }
            else
            {
                MessageBox.Show("Please pick a From and To date value and ensure that the From date is earlier than the To date.");
            }
        }

        //private List<WorkOrderResponse> GetWorkOrders()
        //{
        //   List<WorkOrderResponse> workOrders = new List<WorkOrderResponse>();

        //    try
        //    {
        //        WorkOrderListFilter filter = new WorkOrderListFilter();
        //        filter.FromDate = this.FromDatePicker.SelectedDate.Value;
        //        filter.ToDate = this.ToDatePicker.SelectedDate.Value;

        //        HttpClient client = new HttpClient();
        //        client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

        //        string jsonData = JsonConvert.SerializeObject(filter);
        //        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        //        HttpResponseMessage httpResponse =
        //            client.PostAsync("api/Login/GetWorkOrders",content).Result;

        //        if (httpResponse.IsSuccessStatusCode)
        //        {
        //            Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
        //            StreamReader strReader = new StreamReader(streamData);
        //            string strData = strReader.ReadToEnd();
        //            strReader.Close();
        //            List<WorkOrderResponse> response = JsonConvert.DeserializeObject<List<WorkOrderResponse>>(strData);
        //            workOrders = response;
        //        }
        //        else
        //        {
        //            MessageBox.Show("There was an error retreiving Work Orders");
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    return workOrders;
        //}

        private void ShowWorkOrderItems_Clicked(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            ObservableCollection<WorkOrderViewModel> list1 = new ObservableCollection<WorkOrderViewModel>();

            WorkOrderResponse r = b.CommandParameter as WorkOrderResponse;

            foreach (WorkOrderInventoryMapDTO wor in r.WorkOrderList)
            {
                list1.Add(new WorkOrderViewModel(wor));
            }

            foreach (NotInInventoryDTO nii in r.NotInInventory)
            {
                list1.Add(new WorkOrderViewModel(nii));
            }

            foreach (GetArrangementResponse arrangement in r.Arrangements)
            {
                list1.Add(new WorkOrderViewModel());

                list1.Add(new WorkOrderViewModel()
                {
                    InventoryName = "Arrangement"
                });

                foreach (ArrangementInventoryItemDTO ai in arrangement.ArrangementList)
                {
                    list1.Add(new WorkOrderViewModel(ai, r.WorkOrder.WorkOrderId));
                }

                foreach (NotInInventoryDTO anii in arrangement.NotInInventory)
                {
                    list1.Add(new WorkOrderViewModel(anii));
                }

                list1.Add(new WorkOrderViewModel());
            }

            WorkOrderDetailListView.ItemsSource = list1;
        }

        private void WorkOrderReportListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //they 've selected a work order, show the detail

            try
            {

                //they've selected an individual shipment - fill the details list view
                WorkOrderInventoryMapDTO item = (sender as ListView).SelectedValue as WorkOrderInventoryMapDTO;

                //WorkOrderInventoryMapDTO r = workOrderList.Where(a => a.WorkOrder.WorkOrderId == item.WorkOrder.WorkOrderId).FirstOrDefault();

                //list2.Clear();

                //foreach (WorkOrderInventoryMapDTO m in r.InventoryList)
                //{
                //    list2.Add(m);
                //}

                //WorkOrderDetailListView.ItemsSource = list2;
            }
            catch(Exception ex)
            {

            }
        }

        private void EditWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                //the button's command parmeter is a WoorkOrderResponse object
                WorkOrderResponse response = (WorkOrderResponse)b.CommandParameter;

                if (response != null)
                {
                    //navigate to the Work Order page and load this selection
                    MainWindow wnd = Application.Current.MainWindow as MainWindow;
                    WorkOrderPage workOrderPage = new WorkOrderPage(response);
                    wnd.NavigationStack.Push(workOrderPage);
                    wnd.MainContent.Content = new Frame() { Content = workOrderPage };
                }
            }
        }

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gView1 = WorkOrderReportListView.View as GridView;
            var workingWidth = PageGrid.ActualWidth - 80;   //SystemParameters.VerticalScrollBarWidth; // = 17 not the actual displayed width 

            WorkOrderReportListView.Width = workingWidth;
            WorkOrderDetailListView.Width = workingWidth;

            var col1 = 0.33;
            var col2 = 0.33;
            var col3 = 0.33;

            gView1.Columns[0].Width = workingWidth * col1;
            gView1.Columns[1].Width = workingWidth * col2;
            gView1.Columns[2].Width = workingWidth * col3;

            GridView gView2 = WorkOrderDetailListView.View as GridView;

            col1 = 0.50;
            col2 = 0.40;
            col3 = 0.10;

            gView2.Columns[0].Width = workingWidth * col1;
            gView2.Columns[1].Width = workingWidth * col2;
            gView2.Columns[2].Width = workingWidth * col3;

            var workingHeight = PageGrid.RowDefinitions.ElementAt(4).ActualHeight;
            workingHeight += PageGrid.RowDefinitions.ElementAt(5).ActualHeight;
            workingHeight += PageGrid.RowDefinitions.ElementAt(6).ActualHeight;

            WorkOrderReportListView.Height = workingHeight * 0.5;
            WorkOrderDetailListView.Height = workingHeight * 0.3;
        }
    }
}
