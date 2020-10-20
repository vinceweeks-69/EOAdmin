using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewModels.DataModels;

namespace WpfApp1.ViewModels
{
    public class WorkOrderInventoryItemDTO_Wpf : INotifyPropertyChanged
    {
        public WorkOrderInventoryItemDTO_Wpf()
        {

        }

        public WorkOrderInventoryItemDTO_Wpf(long workOrderId, long inventoryId, long inventoryTypeId,  string inventoryName, string size, long imageId, long groupID = 0, int quantity = 1)
        {
            WorkOrderId = workOrderId;
            InventoryId = inventoryId;
            InventoryTypeId = InventoryTypeId;
            InventoryName = inventoryName;
            ImageId = imageId;
            Size = size;
            Quantity = quantity;
            GroupId = groupID;
        }

        public WorkOrderInventoryItemDTO_Wpf(ArrangementInventoryDTO dto)
        {
            InventoryId = dto.InventoryId;
            InventoryTypeId = dto.InventoryTypeId;
            InventoryName = dto.ArrangementInventoryName;
            Quantity = dto.Quantity;
            ImageId = dto.ImageId;
            Size = dto.Size;
        }

        public long WorkOrderId { get; set; }

        public long InventoryId { get; set; }

        public long InventoryTypeId { get; set; }

        public string InventoryName { get; set; }

        int quantity;
        public int Quantity
        {
            get
            {
                return quantity;
            }
            set
            {
                quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public long ImageId { get; set; }

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
