using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ViewModels.ControllerModels;
using ViewModels.DataModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ArrangementFilter.xaml
    /// </summary>
    public partial class ArrangementFilter : Window
    {
        List<InventoryTypeDTO> inventoryTypes = new List<InventoryTypeDTO>();

        public List<PlantInventoryDTO> plants = new List<PlantInventoryDTO>();
        List<PlantNameDTO> plantNames = new List<PlantNameDTO>();
        List<PlantTypeDTO> plantTypes = new List<PlantTypeDTO>();

        public List<FoliageInventoryDTO> foliage = new List<FoliageInventoryDTO>();
        List<FoliageNameDTO> foliageNames = new List<FoliageNameDTO>();
        List<FoliageTypeDTO> foliageTypes = new List<FoliageTypeDTO>();

        public List<MaterialInventoryDTO> materials = new List<MaterialInventoryDTO>();
        List<MaterialNameDTO> materialNames = new List<MaterialNameDTO>();
        List<MaterialTypeDTO> materialTypes = new List<MaterialTypeDTO>();

        List<ContainerNameDTO> containerNames = new List<ContainerNameDTO>();
        List<ContainerTypeDTO> containerTypes = new List<ContainerTypeDTO>();
        public List<ContainerInventoryDTO> containers = new List<ContainerInventoryDTO>();

        public MainWindow mainWnd { get; set; }
        public ArrangementPage arrangementParentWnd { get; set; }
        public WorkOrderPage workOrderParentWnd { get; set; }
        public ShipmentPage  shipmentParentWnd { get; set; }
        public ArrangementFilter()
        {
            InitializeComponent();
        }

        private void InventoryTypeCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //get types for the current inventory type selection
            ComboBox cb = sender as ComboBox;
            KeyValuePair<long, string> kvp = (KeyValuePair<long, string>)cb.SelectedItem;

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();

            switch (kvp.Value)
            {
                case "Orchids":
                    if(plantTypes.Count == 0)
                    {
                        plantTypes = mainWnd.GetPlantTypes();
                    }

                    foreach (PlantTypeDTO plantType in plantTypes)
                    {
                        list1.Add(new KeyValuePair<long, string>(plantType.PlantTypeId, plantType.PlantTypeName));
                    }

                    break;

                case "Foliage":
                    if (foliageTypes.Count == 0)
                    {
                        foliageTypes = mainWnd.GetFoliageTypes();
                    }

                    foreach (FoliageTypeDTO foliageType in foliageTypes)
                    {
                        list1.Add(new KeyValuePair<long, string>(foliageType.FoliageTypeId, foliageType.FoliageTypeName));
                    }

                    break;

                case "Materials":
                    if (materialTypes.Count == 0)
                    {
                        materialTypes = mainWnd.GetMaterialTypes();
                    }

                    foreach (MaterialTypeDTO materialType in materialTypes)
                    {
                        list1.Add(new KeyValuePair<long, string>(materialType.MaterialTypeId, materialType.MaterialTypeName));
                    }

                    break;


                case "Containers":
                    if(containerTypes.Count == 0)
                    {
                        containerTypes = mainWnd.GetContainerTypes();
                    }

                    foreach (ContainerTypeDTO container in containerTypes)
                    {
                        list1.Add(new KeyValuePair<long, string>(container.ContainerTypeId, container.ContainerTypeName));
                    }
                    break;
            }

            TypeCombo.ItemsSource = list1;

            List<string> sizes = mainWnd.GetSizeByInventoryType(kvp.Key);

            ObservableCollection<string> list2 = new ObservableCollection<string>();

            foreach (string s in sizes)
            {
                list2.Add(s);
            }

            SizeCombo.ItemsSource = list2;
        }

        private void TypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string inventoryType =( (KeyValuePair<long,string>)InventoryTypeCombo.SelectedValue).Value;

            //get names for the current inventory type selection
            ComboBox cb = sender as ComboBox;
            KeyValuePair<long, string> kvp = (KeyValuePair<long, string>)cb.SelectedItem;

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();

            switch(inventoryType)
            {
                case "Orchids":
                    plantNames = mainWnd.GetPlantNamesByType(kvp.Key);

                    foreach (PlantNameDTO plantName in plantNames)
                    {
                        list1.Add(new KeyValuePair<long, string>(plantName.PlantNameId, plantName.PlantName));
                    }

                    NameCombo.ItemsSource = list1;

                    break;

                case "Foliage":
                    foliageNames = mainWnd.GetFoliageNamesByType(kvp.Key);

                    foreach (FoliageNameDTO foliageName in foliageNames)
                    {
                        list1.Add(new KeyValuePair<long, string>(foliageName.FoliageNameId, foliageName.FoliageName));
                    }

                    NameCombo.ItemsSource = list1;

                    break;

                case "Materials":
                    materialNames = mainWnd.GetMaterialNamesByType(kvp.Key);

                    foreach (MaterialNameDTO materialName in materialNames)
                    {
                        list1.Add(new KeyValuePair<long, string>(materialName.MaterialNameId, materialName.MaterialName));
                    }

                    NameCombo.ItemsSource = list1;

                    break;

                case "Containers":
                    containerNames = mainWnd.GetContainerNamesByType(kvp.Key);

                    foreach (ContainerNameDTO containerName in containerNames)
                    {
                        list1.Add(new KeyValuePair<long, string>(containerName.ContainerNameId, containerName.ContainerName));
                    }

                    NameCombo.ItemsSource = list1;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string inventoryType = ((KeyValuePair<long, string>)InventoryTypeCombo.SelectedValue).Value;

            long typeId = ((KeyValuePair<long, string>)TypeCombo.SelectedValue).Key;

            string name = String.Empty;
            if (NameCombo.SelectedValue != null)
            {
                name = ((KeyValuePair<long, string>)NameCombo.SelectedValue).Value;
            }    
            ObservableCollection<ArrangementInventoryFilteredItem> list1 = new ObservableCollection<ArrangementInventoryFilteredItem>();

            switch (inventoryType)
            {
                case "Orchids":
                    plants = mainWnd.GetPlantsByType(typeId).PlantInventoryList;

                    if(!String.IsNullOrEmpty(name))
                    {
                        plants = plants.Where(a => a.Plant.PlantName.Contains(name)).ToList();
                    }

                    foreach(PlantInventoryDTO p in plants)
                    {
                        list1.Add(new ArrangementInventoryFilteredItem()
                        {
                            Id = p.Inventory.InventoryId,
                            Type = p.Inventory.InventoryName,
                            Name = p.Plant.PlantName,
                            Size = p.Plant.PlantSize,
                            ServiceCode = p.Inventory.ServiceCodeName
                        });
                    }
                    break;

                case "Foliage":
                    foliage = mainWnd.GetFoliageByType(typeId).FoliageInventoryList;

                    if (!String.IsNullOrEmpty(name))
                    {
                        foliage = foliage.Where(a => a.Foliage.FoliageName.Contains(name)).ToList();
                    }

                    foreach (FoliageInventoryDTO f in foliage)
                    {
                        list1.Add(new ArrangementInventoryFilteredItem()
                        {
                            Id = f.Inventory.InventoryId,
                            Type = f.Inventory.InventoryName,
                            Name = f.Foliage.FoliageName,
                            Size = f.Foliage.FoliageSize,
                            ServiceCode = f.Inventory.ServiceCodeName
                        });
                    }
                    break;

                case "Materials":
                    materials = mainWnd.GetMaterialsByType(typeId).MaterialInventoryList;

                    if (!String.IsNullOrEmpty(name))
                    {
                        materials = materials.Where(a => a.Material.MaterialName.Contains(name)).ToList();
                    }

                    foreach (MaterialInventoryDTO m in materials)
                    {
                        list1.Add(new ArrangementInventoryFilteredItem()
                        {
                            Id = m.Inventory.InventoryId,
                            Type = m.Inventory.InventoryName,
                            Name =m.Material.MaterialName,
                            Size = m.Material.MaterialSize,
                            ServiceCode = m.Inventory.ServiceCodeName
                        });
                    }
                    break;

                case "Containers":
                    containers = mainWnd.GetContainersByType(typeId).ContainerInventoryList;

                    if (!String.IsNullOrEmpty(name))
                    {
                        containers = containers.Where(a => a.Container.ContainerName.Contains(name)).ToList();
                    }

                    foreach (ContainerInventoryDTO c in containers)
                    {
                        list1.Add(new ArrangementInventoryFilteredItem()
                        {
                            Id = c.Inventory.InventoryId,
                            Type = c.Container.ContainerTypeName,
                            Name = c.Inventory.InventoryName,
                            Size = c.Container.ContainerSize,
                            ServiceCode = c.Inventory.ServiceCodeName
                        });
                    }
                    break;
            }

            ArrangementInventoryList.ItemsSource = list1;
        }

        private void ArrangementInventoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ArrangementInventoryFilteredItem item = (sender as ListView).SelectedValue as ArrangementInventoryFilteredItem;

            if (item != null)
            {
                if (arrangementParentWnd != null)
                {
                    arrangementParentWnd.AddInventorySelection(item.Id, item.Name);
                    arrangementParentWnd = null;
                }
                else if(workOrderParentWnd != null)
                {
                    workOrderParentWnd.AddInventorySelection(item.Id, item.Name);
                    workOrderParentWnd = null;
                }
                else if(shipmentParentWnd != null)
                {
                    shipmentParentWnd.AddInventorySelection(item.Id, item.Name);
                    shipmentParentWnd = null;
                }
            }
            this.Close();
        }
    }
}
