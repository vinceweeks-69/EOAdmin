using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.ControllerModels;
using ViewModels.DataModels;

namespace WpfApp1.ViewModels
{
    public class WorkOrderMessage : EventArgs
    {
        public WorkOrderMessage()
        {
            Inventory = new InventoryDTO();
            NotInInventory = new NotInInventoryDTO();
            Arrangement = new AddArrangementRequest();
        }

        public InventoryDTO Inventory { get; set; }

        public NotInInventoryDTO NotInInventory { get; set; }

        public AddArrangementRequest Arrangement { get; set; }

        public bool HasMessage()
        {
            bool hasMessage = false;

            if(Inventory.InventoryId != 0 || NotInInventory.NotInInventoryId != 0 || Arrangement.ArrangementInventory.Count > 0 || Arrangement.NotInInventory.Count > 0)
            {
                hasMessage = true;
            }

            return hasMessage;
        }
    }
}
