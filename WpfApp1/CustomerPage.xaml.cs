using Microsoft.Win32;
using Newtonsoft.Json;
using OfficeOpenXml;
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
    /// Interaction logic for CustomerPage.xaml
    /// </summary>
    public partial class CustomerPage : EOStackPage
    {
        Dictionary<int, string> columnIndicesAndNames = new Dictionary<int, string>();

        Dictionary<int, List<ExcelRowData>> rowData = new Dictionary<int, List<ExcelRowData>>();

        List<PersonDTO> customers = new List<PersonDTO>();

        ObservableCollection<PersonDTO> list1 = new ObservableCollection<PersonDTO>();

        EOStackPage basePage;

        public CustomerPage()
        {
            InitializeComponent();

            MainWindow mainWnd = Application.Current.MainWindow as MainWindow;

           GetPersonResponse response  = mainWnd.GetCustomers(new GetPersonRequest());

            foreach(PersonAndAddressDTO pr in response.PersonAndAddress)
            {
                list1.Add(new PersonDTO()
                {
                    first_name = pr.Person.first_name,
                    last_name = pr.Person.last_name,
                    email = pr.Person.email,
                    phone_primary = pr.Person.phone_primary,
                    street_address = pr.Address != null ? pr.Address.street_address : String.Empty,
                    unit_apt_suite = pr.Address != null ? pr.Address.unit_apt_suite : String.Empty,
                    city = pr.Address != null ? pr.Address.city : String.Empty,
                    state = pr.Address != null ? pr.Address.state : String.Empty,
                    zipcode = pr.Address != null ? pr.Address.zipcode : String.Empty
                });
            }

            CustomerListView.ItemsSource = list1;
        }

        public CustomerPage(EOStackPage page) : this()
        {
            basePage = page;
        }

        public void LoadWorkOrderData(WorkOrderMessage msg)
        {
            int debug = 1;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            ofDialog.ShowDialog();

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            try
            {
                using (ExcelPackage excelPackage = new ExcelPackage(File.Open(ofDialog.FileName, FileMode.Open)))
                {
                    //Get a WorkSheet by index. Note that EPPlus indexes are base 1, not base 0!
                    foreach (ExcelWorksheet ohSHEET in excelPackage.Workbook.Worksheets)
                    {
                        rowData.Clear();
                        columnIndicesAndNames.Clear();

                        object[,] obj = ohSHEET.Cells.Value as object[,];

                        for (int a = 0; a < ohSHEET.Dimension.Rows; a++)
                        {
                            if (a > 0)
                            {
                                rowData.Add(a, new List<ExcelRowData>());
                            }

                            for (int b = 0; b < ohSHEET.Dimension.Columns; b++)
                            {
                                if (obj[a, b] != null)
                                {
                                    //get the header names and column indices
                                    if (a == 0)
                                    {
                                        columnIndicesAndNames.Add(b, obj[a, b] as string);
                                    }
                                    else
                                    {
                                        rowData[a].Add(new ExcelRowData(a, b, obj[a, b]));
                                    }
                                }
                            }
                        }

                        //check it all first, create a report (later)
                        foreach (KeyValuePair<int, List<ExcelRowData>> kvp in rowData)
                        {
                            ImportPerson(kvp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //report;
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

        public void AddPersonSelection(PersonDTO person)
        {
            list1.Clear();

            list1.Add(person);

            CustomerListView.ItemsSource = list1;
        }

        private void ImportPerson(KeyValuePair<int, List<ExcelRowData>> kvp)
        {
            //check for presence of person - add if not present
            long personId = 0;

            PersonDTO person = new PersonDTO();

            int column = columnIndicesAndNames.Where(a => a.Value == "First Name").Select(b => b.Key).First();

            person.first_name = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "Last Name").Select(b => b.Key).First();

            person.last_name = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "Primary Phone").Select(b => b.Key).First();

            person.phone_primary = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "Alt Phone").Select(b => b.Key).First();

            person.phone_alt = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            string jsonData = JsonConvert.SerializeObject(person);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            string result = Post("DoesPersonExist", content);

            Int64.TryParse(result, out personId);

            if (personId != 0)
                return;

            AddressDTO address = new AddressDTO();

            column = columnIndicesAndNames.Where(a => a.Value == "Address1").Select(b => b.Key).First();

            address.street_address = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "Address2").Select(b => b.Key).First();

            address.unit_apt_suite = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "City").Select(b => b.Key).First();

            address.city = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "Zip").Select(b => b.Key).First();

            address.zipcode = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "Community").Select(b => b.Key).First();

            var community = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "Business").Select(b => b.Key).First();

            var business = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            if (Convert.ToBoolean(business))
            {
                address.address_type_id = 4;
            }
            else if (!String.IsNullOrEmpty(community))
            {
                address.address_type_id = 3;
            }
            else
            {
                if(!String.IsNullOrEmpty(address.unit_apt_suite))
                {
                    address.address_type_id = 2;
                }
                else
                {
                    address.address_type_id = 1;
                }
            }

            ImportPersonRequest request = new ImportPersonRequest(person,address);

            jsonData = JsonConvert.SerializeObject(request);
            content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Post("ImportPerson", content);
        }

       

        public string Get(string endpoint, string parameters)
        {
            string strData = String.Empty;

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                           new MediaTypeWithQualityHeaderValue("application/json"));

                //string jsonData = JsonConvert.SerializeObject(addArrangementRequest);
                //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.GetAsync("api/Login/" + endpoint + "?" + parameters).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    strData = httpResponse.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    MessageBox.Show("Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return strData;
        }

        /// <summary>
        /// Don't foget to serialize the content parameter
        /// string jsonData = JsonConvert.SerializeObject(addArrangementRequest);
        /// var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="content"></param>
        public string Post(string endpoint, StringContent content)
        {
            string strData = String.Empty;

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                           new MediaTypeWithQualityHeaderValue("application/json"));

                //string jsonData = JsonConvert.SerializeObject(addArrangementRequest);
                //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/" + endpoint, content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    strData = httpResponse.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    MessageBox.Show("Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return strData;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PersonFilter filter = new PersonFilter(this);

            filter.ShowDialog();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            if (wnd.PageIsOnStack(typeof(CustomerPage)))
            {
                if (basePage != null)
                {
                    basePage.LoadWorkOrderData(new WorkOrderMessage());
                }

                wnd.OnBackClick(this);
            }
        }

        private void DeleteCustomer(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            PersonDTO p = b.CommandParameter as PersonDTO;

            //HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
            ////client.DefaultRequestHeaders.Add("appkey", "myapp_key");
            //client.DefaultRequestHeaders.Accept.Add(
            //   new MediaTypeWithQualityHeaderValue("application/json"));

            //string jsonData = JsonConvert.SerializeObject(getPlantResponse.Plant.PlantId);
            //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            //HttpResponseMessage httpResponse = client.PostAsync("api/Login/DeletePlant", content).Result;
            //if (httpResponse.IsSuccessStatusCode)
            //{
            //    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
            //    StreamReader strReader = new StreamReader(streamData);
            //    string strData = strReader.ReadToEnd();
            //    strReader.Close();

            //    customers.Remove(p);

            //    list1.Remove(p);
            //    this.CustomerListView.ItemsSource = null;
            //    this.CustomerListView.ItemsSource = list1;

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
            //    MessageBox.Show("Error deleting customer");
            //}
        }

        private void CustomerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PersonDTO item = (sender as ListView).SelectedValue as PersonDTO;

            if (item != null)
            {
                FirstNameTextBox.Text = item.first_name;
                LastNameTextBox.Text = item.last_name;
                EmailTextBox.Text = item.email;
                PhoneTextBox.Text = item.phone_primary;
                Address1TextBox.Text = item.street_address;
                Address2TextBox.Text = item.unit_apt_suite;
                CityTextBox.Text = item.city;
                StateTextBox.Text = item.state;
                ZipCodeTextBox.Text = item.zipcode;
            }
        }
    }
}
