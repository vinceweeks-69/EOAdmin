using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Interaction logic for CustomerContainerPage.xaml
    /// </summary>
    public  partial class CustomerContainerPage : EOStackPage
    {
        EOStackPage basePage;

        PersonDTO Customer { get; set; }

        CustomerContainerDTO CustomerContainer { get; set; }

        public CustomerContainerPage(PersonDTO customer)
        {
            InitializeComponent();

            Customer = customer;

            FirstName.Text = Customer.first_name;
            LastName.Text = Customer.last_name;
            Phone.Text = Customer.phone_primary;

            LoadCustomerContainers();
        }

        public CustomerContainerPage(EOStackPage page, PersonDTO customer) : this(customer)
        {
            basePage = page;
        }

        private async void LoadCustomerContainers()
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
            if (response.CustomerContainers.Count > 0)
            {
                Dispatcher.Invoke(() =>
                {
                    ObservableCollection<CustomerContainerDTO> containers = new ObservableCollection<CustomerContainerDTO>();

                    foreach(CustomerContainerDTO cc in response.CustomerContainers)
                    {
                        containers.Add(cc);
                    }

                    CustomerContainerListView.ItemsSource = containers;
                });
            }
        }

        private void Camera_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            if(basePage != null)
            {
                WorkOrderMessage msg = new WorkOrderMessage();
                msg.CustomerContainer = CustomerContainer;
                basePage.LoadWorkOrderData(msg);
                wnd.OnBackClick(this);
            }
        }

        private void ContainerImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ContainerDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CustomerContainerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = sender as ListView;

            if(lv != null)
            {
                CustomerContainerDTO sel = (CustomerContainerDTO)lv.SelectedItem;

                if(sel != null)
                {
                    CustomerContainer = sel;
                    Label.Text = sel.Label;
                }
            }
        }

        public override void LoadWorkOrderData(WorkOrderMessage msg)
        {

        }
    }
}
