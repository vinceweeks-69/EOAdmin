using EO.ViewModels.ControllerModels;
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
    /// Interaction logic for VendorPage.xaml
    /// </summary>
    public partial class VendorPage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;
        ObservableCollection<VendorDTO> list3 = new ObservableCollection<VendorDTO>();

        public VendorPage()
        {
            InitializeComponent();

            GetVendors();
        }

        private async void GetVendors()
        {
            ((App)App.Current).PostRequest<GetPersonRequest, GetVendorResponse>("GetVendors", new GetPersonRequest()).ContinueWith(a => VendorsLoaded(a.Result));
        }

        private void VendorsLoaded(GetVendorResponse response)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (VendorDTO vDTO in response.VendorList)
                {
                    list3.Add(vDTO);
                }

                this.VendorListView.ItemsSource = list3;
            });
        }

        private bool ValidateVendorData()
        {
            bool vendorDataValid = true;
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(this.VendorName.Text))
            {
                sb.AppendLine("The least you can do is enter a name...");
            }

            if(sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
                vendorDataValid = false;
            }

            return vendorDataValid;
        }

        private AddVendorRequest GetVendorData()
        {
            AddVendorRequest addVendorRequest = new AddVendorRequest();

            addVendorRequest.Vendor.VendorName = this.VendorName.Text;
            addVendorRequest.Vendor.VendorEmail = this.VendorEmail.Text;
            addVendorRequest.Vendor.VendorPhone = this.VendorPhone.Text;
            addVendorRequest.Vendor.StreetAddress = this.Address1.Text;
            addVendorRequest.Vendor.UnitAptSuite = this.Address2.Text;
            addVendorRequest.Vendor.City = this.City.Text;
            addVendorRequest.Vendor.State = this.State.Text;
            addVendorRequest.Vendor.ZipCode = this.Zip.Text;

            return addVendorRequest;
        }

        public async void AddVendor()
        {
            if (ValidateVendorData())
            {
                AddVendorRequest request = GetVendorData();
                ((App)App.Current).PostRequest<AddVendorRequest, ApiResponse>("AddVendor", request).ContinueWith(a => VendorAdded(a.Result));
            }
        }

        private void VendorAdded(ApiResponse response)
        {
            Dispatcher.Invoke(() =>
            {
                if(response.Success && response.Id > 0)
                {
                    ResetUI();

                    MessageBox.Show("Vendor added");
                }
                else
                {
                    MessageBox.Show("Error adding Vendor");
                }
            });
        }

        private void ResetUI()
        {
             this.VendorName.Text = String.Empty;
             this.VendorEmail.Text = String.Empty;
             this.VendorPhone.Text = String.Empty;
             this.Address1.Text = String.Empty;
             this.Address2.Text = String.Empty;
             this.City.Text = String.Empty;
             this.State.Text = String.Empty;
             this.Zip.Text = String.Empty;
        }

        public void AddPersonSelection(PersonDTO person)
        {
            list3.Clear();

            VendorDTO v = new VendorDTO();

            v.City = person.city;
            v.State = person.state;
            v.StreetAddress = person.street_address;
            v.UnitAptSuite = person.unit_apt_suite;
            v.VendorEmail = person.email;
            v.VendorName = person.CustomerName;
            v.VendorPhone = person.phone_primary;
            v.ZipCode = person.zipcode;

            list3.Add(v);

            VendorListView.ItemsSource = list3;
        }

        private void ClearEditFields()
        {
            this.VendorEmail.Text = "";
            this.VendorName.Text = "";
            this.VendorPhone.Text = "";
            this.Address1.Text = "";
            this.Address2.Text = "";
            this.City.Text = "";
            this.State.Text = "";
            this.Zip.Text = "";
        }
        private void SaveVendorButton_Click(object sender, RoutedEventArgs e)
        {
            AddVendor();
        }

        private void VendorListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VendorDTO item = (sender as ListView).SelectedValue as VendorDTO;

            //populate the edit fields
            this.VendorEmail.Text = item.VendorEmail;
            this.VendorName.Text = item.VendorName;
            this.VendorPhone.Text = item.VendorPhone;
            this.Address1.Text = item.StreetAddress;
            this.Address2.Text = item.UnitAptSuite;
            this.City.Text = item.City;
            this.State.Text = item.State;
            this.Zip.Text = item.ZipCode;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //search
            PersonFilter filter = new PersonFilter();
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            filter.Owner = wnd;

            // inventoryTypes = wnd.GetInventoryTypes();

            filter.mainWnd = wnd;
            filter.vendorParentWnd = this;

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            //foreach (InventoryTypeDTO inventoryType in inventoryTypes)
            //{
            //    if (inventoryType.InventoryTypeName != "Arrangements")
            //    {
            //        list1.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
            //    }
            //}

            //filter.InventoryTypeCombo.ItemsSource = list1;
            filter.ShowDialog();
        }

        private void PageGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gView = VendorListView.View as GridView;

            var workingWidth = PageGrid.ActualWidth - 32;             // SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar
            VendorListView.Width = workingWidth;

            var col1 = 0.20;
            var col2 = 0.10;
            var col3 = 0.10;
            var col4 = 0.10;
            var col5 = 0.10;
            var col6 = 0.10;
            var col7 = 0.10;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;
            gView.Columns[4].Width = workingWidth * col5;
            gView.Columns[5].Width = workingWidth * col6;
            gView.Columns[6].Width = workingWidth * col7;

            var workingHeight = PageGrid.RowDefinitions.ElementAt(8).ActualHeight;

            VendorListView.Height = workingHeight * 0.9;
        }
    }
}
