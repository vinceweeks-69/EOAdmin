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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for PersonFilter.xaml
    /// </summary>
    public partial class PersonFilter : Window
    {
        public MainWindow mainWnd { get; set; }
        public CustomerPage customerParentWnd { get; set; }
        public VendorPage vendorParentWnd { get; set; }
        public WorkOrderPage workOrderParentWnd { get; set; }

        public PersonFilter()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //run filter against person table

            GetPersonRequest request = new GetPersonRequest();
            request.FirstName = Name.Text;
            //request.LastName = LastNameTextBox.Text;
            request.PhonePrimary = Phone.Text;
            request.Email = Email.Text;

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

            if (item != null)
            {
                if (customerParentWnd != null)
                {
                    customerParentWnd.AddPersonSelection(item);
                    customerParentWnd = null;
                }
                else if (vendorParentWnd != null)
                {
                    vendorParentWnd.AddPersonSelection(item);
                    vendorParentWnd = null;
                }
                else if (workOrderParentWnd != null)
                {
                    workOrderParentWnd.AddPersonSelection(item);
                    workOrderParentWnd = null;
                }
            }

            this.Close();
        }
    }
}
