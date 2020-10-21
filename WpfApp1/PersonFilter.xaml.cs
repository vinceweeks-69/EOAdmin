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
            request.FirstName = FirstName.Text;
            request.LastName = LastName.Text;
            request.PhonePrimary = Phone.Text;
            request.Email = Email.Text;
            request.Address = Address.Text;
            request.ZipCode = ZipCode.Text;

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

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gView = PersonFilterListView.View as GridView;

            var workingWidth = PageGrid.ActualWidth - 80;   //SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            PersonFilterListView.Width = workingWidth;

            var col1 = 0.12;
            var col2 = 0.12;
            var col3 = 0.12;
            var col4 = 0.12;
            var col5 = 0.12;
            var col6 = 0.12;
            var col7 = 0.12;
            var col8 = 0.12;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;
            gView.Columns[4].Width = workingWidth * col5;
            gView.Columns[5].Width = workingWidth * col6;
            gView.Columns[6].Width = workingWidth * col7;
            gView.Columns[7].Width = workingWidth * col8;
        }
    }
}
