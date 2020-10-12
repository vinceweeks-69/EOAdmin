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
            Inventory = new InventoryDTO();
            NotInInventory = new NotInInventoryDTO();
            Arrangement = new AddArrangementRequest();
            CustomerContainer = new CustomerContainerDTO();
        }

        public PersonDTO Person { get; set; }

        public InventoryDTO Inventory { get; set; }

        public NotInInventoryDTO NotInInventory { get; set; }

        public AddArrangementRequest Arrangement { get; set; }

        public CustomerContainerDTO CustomerContainer { get; set; }

        public bool HasMessage()
        {
            bool hasMessage = false;

            if(Person.person_id != 0 || Inventory.InventoryId != 0 || NotInInventory.NotInInventoryId != 0 || Arrangement.ArrangementInventory.Count > 0 || Arrangement.NotInInventory.Count > 0 || CustomerContainer.CustomerContainerId > 0)
            {
                hasMessage = true;
            }

            return hasMessage;
        }
    }
}
