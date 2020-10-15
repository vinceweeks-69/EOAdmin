using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1.ViewModels
{
    public class WorkOrderInventoryMapDTO_Wpf : INotifyPropertyChanged
    {
        public long WorkOrderInventoryMapId { get; set; }

        public long WorkOrderId { get; set; }

        public long InventoryId { get; set; }

        public string InventoryName { get; set; }

        public int Quantity { get; set; }

        public string Size { get; set; }

        public long? GroupId { get; set; }

        public string NotInInventoryName { get; set; }

        public string NotInInventorySize { get; set; }

        public decimal NotInInventoryPrice { get; set; }

        private Visibility visibility { get; set; }

        public Visibility ItemVisibility
        {
            set
            {
                visibility = value;
                OnPropertyChanged(nameof(ItemVisibility))
;
            }
            get
            {
                return Quantity == 0 ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private bool shouldShow;
        public bool ShouldShow
        {
            set
            {
                shouldShow = value;
                OnPropertyChanged(nameof(ShouldShow));
            }
            get
            {
                return Quantity == 0 ? false : true;
            }
        }

        //public Color BackgroundColor()
        //{
        //    return GroupId == 0 ? Color.White : Color.LightGray;
        //}

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
