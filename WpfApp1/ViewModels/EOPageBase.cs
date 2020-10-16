using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfApp1.ViewModels;

namespace WpfApp1
{
    public class EOBasePage : Page
    {
        public void SetComboBoxSelection(ComboBox cb, string value)
        {
            foreach (KeyValuePair<long, string> kvp in cb.ItemsSource as ObservableCollection<KeyValuePair<long, string>>)
            {
                if (kvp.Value == value)
                {
                    cb.SelectedItem = kvp;
                }
            }
        }

        public void SetComboBoxSelection(ComboBox cb, long key)
        {
            foreach (KeyValuePair<long, string> kvp in cb.ItemsSource as ObservableCollection<KeyValuePair<long, string>>)
            {
                if (kvp.Key == key)
                {
                    cb.SelectedItem = kvp;
                }
            }
        }
    }

    public interface IEOStackPage
    {
        void LoadWorkOrderData(WorkOrderMessage msg);
    }

    public class EOStackPage : EOBasePage, IEOStackPage
    {
        public virtual void LoadWorkOrderData(WorkOrderMessage msg)
        {

        }
    }
}
