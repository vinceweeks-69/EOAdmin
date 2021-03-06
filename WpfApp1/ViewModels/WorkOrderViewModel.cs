﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewModels.DataModels;

namespace WpfApp1.ViewModels
{
    public class WorkOrderViewModel : INotifyPropertyChanged
    {
        public WorkOrderViewModel()
        {
        }

        public WorkOrderViewModel(NotInInventoryDTO dto)
        {
            WorkOrderId = dto.WorkOrderId;
            //InventoryId = 387;    //temp constant
            InventoryId = 0;
            NotInInventoryId = dto.NotInInventoryId;
            UnsavedId = dto.UnsavedId;
            InventoryName = dto.NotInInventoryName;
            Quantity = dto.NotInInventoryQuantity;
            ImageId = 0;
            Size = dto.NotInInventorySize;
            GroupId = dto.ArrangementId;
        }

        public WorkOrderViewModel(WorkOrderInventoryItemDTO dto)
        {
            WorkOrderId = dto.WorkOrderId;
            InventoryId = dto.InventoryId;
            InventoryName = dto.InventoryName;
            Quantity = dto.Quantity;
            ImageId = dto.ImageId;
            Size = dto.Size;
            GroupId = dto.GroupId;
        }

        public WorkOrderViewModel(WorkOrderInventoryMapDTO dto)
        {
            WorkOrderId = dto.WorkOrderId;
            WorkOrderInventoryMapId = dto.WorkOrderInventoryMapId;
            InventoryId = dto.InventoryId;
            InventoryName = dto.InventoryName;
            Quantity = dto.Quantity;
            //ImageId = dto..ImageId;
            Size = dto.Size;
            GroupId = dto.GroupId;
        }

        public WorkOrderViewModel(ArrangementInventoryDTO dto, long workOrderId)
        {
            WorkOrderId = workOrderId;
            InventoryId = dto.InventoryId;
            InventoryName = dto.ArrangementInventoryName;
            Quantity = dto.Quantity;
            ImageId = dto.ImageId;
            Size = dto.Size;
            GroupId = dto.ArrangementId;
        }

        public WorkOrderViewModel(ArrangementInventoryItemDTO dto, long workOrderId)
        {
            WorkOrderId = workOrderId;
            InventoryId = dto.InventoryId;
            InventoryName = dto.InventoryName;
            Quantity = dto.Quantity;
            ImageId = dto.ImageId;
            //Size = dto..Size;
            GroupId = dto.ArrangementId;
        }

        public long WorkOrderId { get; set; }

        public long WorkOrderInventoryMapId { get; set; }

        public long InventoryId { get; set; }

        public long NotInInventoryId { get; set; }

        public long UnsavedId { get; set; }

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

        private Visibility visibility { get; set; }

        public Visibility ItemVisibility
        {
            set
            {
                visibility = value;
                OnPropertyChanged(nameof(ItemVisibility))
;            }
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

        private bool shouldEnable;
        public bool ShouldEnable
        {
            set
            {
                shouldEnable = value;
                OnPropertyChanged(nameof(ShouldEnable));
            }
            get
            {
                return shouldEnable;
            }
        }

        public Color BackgroundColor()
        {
            return GroupId == 0 ? Color.White : Color.LightGray;
        }

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
