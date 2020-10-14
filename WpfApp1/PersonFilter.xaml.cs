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
using System.Windows.Shapes;
using ViewModels.ControllerModels;
using ViewModels.DataModels;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for PersonFilter.xaml
    /// </summary>
    public partial class PersonFilter : Window
    {
        EOStackPage basePage;
        public MainWindow mainWnd { get; set; }
        public CustomerPage customerParentWnd { get; set; }
        public VendorPage vendorParentWnd { get; set; }
        public WorkOrderPage workOrderParentWnd { get; set; }

        public PersonFilter()
        {
            InitializeComponent();
        }

        public PersonFilter(EOStackPage page) : this()
        {
            basePage = page;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //run filter against person table

            GetPersonRequest request = new GetPersonRequest();
            request.FirstName = Name.Text;
            //request.LastName = LastNameTextBox.Text;
            request.PhonePrimary = Phone.Text;
            request.Email = Email.Text;

            MainWindow mainWnd = Application.Current.MainWindow as MainWindow;
            GetPersonResponse response = mainWnd.GetCustomers(request);
            ObservableCollection<PersonDTO> list1 = new ObservableCollection<PersonDTO>();
            foreach (PersonAndAddressDTO p in response.PersonAndAddress)
            {
                list1.Add(p.Person);
            }

            PersonFilterListView.ItemsSource = list1;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //they selected a person - close the dialog and populate the edit fields on the parent form

            PersonDTO item = (sender as ListView).SelectedValue as PersonDTO;

            if (basePage != null)
            {
                WorkOrderMessage msg = new WorkOrderMessage();
                msg.Person = item;

                basePage.LoadWorkOrderData(msg);
            }

            this.Close();
        }
    }
}
