using Newtonsoft.Json;
using SharedData;
using System;
using System.Collections.Generic;
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
using ViewModels.DataModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for BugsPage.xaml
    /// </summary>
    public partial class BugsPage : Page
    {
        public BugsPage()
        {
            InitializeComponent();
        }

        public void Send_Click(object sender, RoutedEventArgs e)
        {
            if(bugs.Text.Length > 0)
            {
                Window wnd = Application.Current.MainWindow;

                EOMailMessage msg = new EOMailMessage();
                try
                {
                    msg = new EOMailMessage("service@elegantorchids.com", "vinceweeks@yahoo.com", "Admin Bug or Feature Request", bugs.Text, ((App)App.Current).Dootster);
                    if (Email.SendEmail(msg))
                    {
                        bugs.Text = String.Empty;
                        MessageBox.Show(wnd, "Your message has been set to the development team", "Success", MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBox.Show(wnd,"There was an error sending your message - call Vince!","Error", MessageBoxButton.OK);
                    }
                
                }
                catch(Exception ex)
                {
                    Exception ex2 = new Exception("EO Admin Bugs Send Email", ex);
                    ((App)App.Current).LogError(ex2.Message, JsonConvert.SerializeObject(msg));
                }
            }
        }
    }
}
