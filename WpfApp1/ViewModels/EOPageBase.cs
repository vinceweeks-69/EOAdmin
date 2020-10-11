using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp1.ViewModels
{
    public interface IEOBasePage
    {
        void LoadWorkOrderData(WorkOrderMessage msg);
    }

    public class EOPageBase : Page, IEOBasePage
    {
        public void LoadWorkOrderData(WorkOrderMessage msg)
        {
            int debug = 1;
        }
    }
}
