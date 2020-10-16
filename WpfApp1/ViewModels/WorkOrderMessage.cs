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
            Person = new PersonDTO();
            Inventory = new WorkOrderInventoryMapDTO();
            NotInInventory = new NotInInventoryDTO();
            Arrangement = new AddArrangementRequest();
            CustomerContainer = new CustomerContainerDTO();
        }

        public bool? WorkOrderPaid { get; set; }

        public PersonDTO Person { get; set; }

        public WorkOrderInventoryMapDTO Inventory { get; set; }

        public NotInInventoryDTO NotInInventory { get; set; }

        public AddArrangementRequest Arrangement { get; set; }

        public CustomerContainerDTO CustomerContainer { get; set; }

        public bool HasMessage()
        {
            bool hasMessage = false;

            if(WorkOrderPaid.HasValue || Person.person_id != 0 || Inventory.InventoryId != 0 || !String.IsNullOrEmpty(NotInInventory.NotInInventoryName) || Arrangement.ArrangementInventory.Count > 0 || Arrangement.NotInInventory.Count > 0 || CustomerContainer.CustomerContainerId > 0)
            {
                hasMessage = true;
            }

            return hasMessage;
        }
    }
}
