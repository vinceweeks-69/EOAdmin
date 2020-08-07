using Microsoft.Win32;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using ViewModels.ControllerModels;
using ViewModels.DataModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;

        Dictionary<int, string> columnIndicesAndNames = new Dictionary<int, string>();

        Dictionary<int, List<ExcelRowData>> rowData = new Dictionary<int, List<ExcelRowData>>();

        public ImportPage()
        {
            InitializeComponent();
        }
        public void Import(OpenFileDialog ofDialog)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            string fileName = ofDialog.SafeFileName;

            using (ExcelPackage excelPackage = new ExcelPackage(File.Open(ofDialog.FileName, FileMode.Open)))
            {
                try
                {
                    //Get a WorkSheet by index. Note that EPPlus indexes are base 1, not base 0!
                    foreach (ExcelWorksheet ohSHEET in excelPackage.Workbook.Worksheets)
                    {
                        rowData.Clear();
                        columnIndicesAndNames.Clear();

                        object[,] obj = ohSHEET.Cells.Value as object[,];

                        for (int a = 0; a < ohSHEET.Dimension.Rows; a++)
                        {
                            if (a > 0)
                            {
                                rowData.Add(a, new List<ExcelRowData>());
                            }

                            for (int b = 0; b < ohSHEET.Dimension.Columns; b++)
                            {
                                if (obj[a, b] != null)
                                {
                                    //get the header names and column indices
                                    if (a == 0)
                                    {
                                        columnIndicesAndNames.Add(b, obj[a, b] as string);
                                    }
                                    else
                                    {
                                        rowData[a].Add(new ExcelRowData(a, b, obj[a, b]));
                                    }
                                }
                            }
                        }

                        if (ImportFileIsValid())
                        {
                            //check it all first, create a report (later)
                            foreach (KeyValuePair<int, List<ExcelRowData>> kvp in rowData)
                            {
                                ExcelPicture img = null;
                                try
                                {
                                    img = (ExcelPicture)ohSHEET.Drawings.Where(a => a.From.Row == kvp.Key).FirstOrDefault();
                                }
                                catch (Exception ex)
                                {
                                    //record the fact that there was something there, but not something we could use
                                    int debug = 1;
                                }

                                switch (fileName)
                                {
                                    case "Phals.xlsx":
                                        if (kvp.Value.Count > 0 && ohSHEET.Name.ToUpper() != "CODES")
                                        {
                                            ImportPlant(kvp, img);
                                        }
                                        break;

                                    case "Foliage.xlsx":
                                        ImportFoliage(kvp, img);
                                        break;

                                    case "Materials.xlsx":
                                        ImportMaterial(kvp, img);
                                        break;

                                    case "Containers.xlsx":
                                        ImportContainer(kvp, img);
                                        break;

                                }
                            }

                            MessageBox.Show(ofDialog.FileName + " was imported successfully." , "Success", MessageBoxButton.OK);
                        }
                        else
                        {
                            MessageBox.Show("Import file not configured correctly", "Import Error", MessageBoxButton.OK);
                        }
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("There was an error importing " + ofDialog.FileName, "Import Error", MessageBoxButton.OK);
                }
            }

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }

        private void PlantsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            ofDialog.ShowDialog();

            if (ofDialog.FileName.EndsWith("Phals.xlsx"))
            {
                Import(ofDialog);
            }
            else
            {
                MessageBox.Show("This is not Phals.xlsx - What do you think you're doing?");
            }
        }

        private void FoliageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            ofDialog.ShowDialog();

            if (ofDialog.FileName.EndsWith("Foliage.xlsx"))
            {
                Import(ofDialog);
            }
            else
            {
                MessageBox.Show("This is not Foliage.xlsx - What do you think you're doing?");
            }
        }

        private void MaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            ofDialog.ShowDialog();

            if (ofDialog.FileName.EndsWith("Materials.xlsx"))
            {
                Import(ofDialog);
            }
            else
            {
                MessageBox.Show("This is not Materials.xlsx - What do you think you're doing?");
            }
        }

        private void ContainersButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            ofDialog.ShowDialog();

            if (ofDialog.FileName.EndsWith("Containers.xlsx"))
            {
                Import(ofDialog);
            }
            else
            {
                MessageBox.Show("This is not Containers.xlsx - What do you think you're doing?");
            }
        }

        private void ArrangementsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Under construction - come back later");
        }

        private void VendorButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Under construction - come back later");
        }

        private void CustomerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Under construction - come back later");
        }

        /// <summary>
        /// Import an image by itself
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            ofDialog.ShowDialog();

            if (ofDialog.FileName.EndsWith(".jpg") || ofDialog.FileName.EndsWith(".png") || ofDialog.FileName.EndsWith(".bmp"))
            {
                ImportImage(ofDialog.FileName);
            }
            else
            {
                MessageBox.Show(".png, .jpg or .bmp for image import, please");
            }
        }

        private long ImportImage(string fileName)
        {
            long newImageId = 0;
            try
            {
                byte[] ImageData;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                ImageData = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();

                AddImageRequest request = new AddImageRequest();
                request.imgBytes = ImageData;
                string jsonData = JsonConvert.SerializeObject(request);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                string result = Post("ImportImage", content);

                GetLongIdResponse r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
                newImageId = r.returnedId;
            }
            catch(Exception ex)
            {

            }

            return newImageId;
        }

        private void ImportPlant(KeyValuePair<int, List<ExcelRowData>> kvp, ExcelPicture img)
        {
            //check for presence of plantName - add if not present
            long plantNameId = 0;

            int column = columnIndicesAndNames.Where(a => a.Value == "NAME").Select(b => b.Key).First();

            string plantName = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            string result = Get("DoesPlantNameExist", "plantName=" + plantName);

            GetLongIdResponse r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            plantNameId = r.returnedId;

            //check for presence of plantType - add if not present

            long plantTypeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value == "TYPE").Select(b => b.Key).First();

            string plantType = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("DoesPlantTypeExist", "plantType=" + plantType);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            plantTypeId = r.returnedId;

            column = columnIndicesAndNames.Where(a => a.Value.ToUpper() == "SIZE").Select(b => b.Key).First();

            string plantSize = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            ////using cost, retail, plantName and plant Type, check for presence of service code - add if not present

            //ServiceCodeDTO serviceCode = GetServiceCodeDTO(kvp);

            //string jsonData = JsonConvert.SerializeObject(serviceCode);
            //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //result = Post("DoesServiceCodeExist", content);

            long serviceCodeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value == "CODE").Select(b => b.Key).First();

            string serviceCode = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("ServiceCodeIsNotUnique", "serviceCode=" + serviceCode);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            serviceCodeId = r.returnedId;

            //check for presence of plant - add if not present

            long plantId = 0;

            PlantDTO plantDTO = GetPlantDTO(kvp);

            string jsonData = JsonConvert.SerializeObject(plantDTO);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            result = Post("DoesPlantExist", content);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            plantId = r.returnedId;

            //check for presence of inventory - add if not present

            ImportPlantRequest request = new ImportPlantRequest();

            request.PlantSize = plantSize;
            request.AddPlantRequest.Plant.PlantSize = plantSize;

            //add image if present
            if (img != null)
            {
                ImageConverter imgCon = new ImageConverter();
                request.imageBytes = (byte[])imgCon.ConvertTo(img.Image, typeof(byte[]));
            }

            //create DTO - send to service - backend will do the hookup

            if (plantNameId == 0)
            {
                request.PlantName = plantName;
            }
            else
            {
                request.AddPlantRequest.Plant.PlantNameId = plantNameId;
            }

            if (plantTypeId == 0)
            {
                request.PlantType = plantType;
            }
            else
            {
                request.AddPlantRequest.Plant.PlantTypeId = plantTypeId;
            }


            request.ServiceCode = GetServiceCodeDTO(kvp);

            if (serviceCodeId > 0)
            {
                ServiceCodeDTO original = GetServiceCodeById(serviceCodeId);
                UpdateServiceCode(original, request.ServiceCode);
                request.ServiceCode = original;
                request.ServiceCode.ServiceCodeId = serviceCodeId;
                request.AddPlantRequest.Inventory.ServiceCodeId = serviceCodeId;
            }

            jsonData = JsonConvert.SerializeObject(request);
            content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            if(request.PlantSize == "5")
            {
                int debug = 1;
            }

            Post("ImportPlant", content);
        }

        private void UpdateServiceCode(ServiceCodeDTO original, ServiceCodeDTO modified)
        {
            original.Cost = modified.Cost;
            original.Price = modified.Price;
            original.Taxable = modified.Taxable;
        }

        private ServiceCodeDTO GetServiceCodeById(long serviceCodeId)
        {
            MainWindow wnd = Application.Current.MainWindow as MainWindow;

            return wnd.GetServiceCodeById(serviceCodeId);
        }

        private ServiceCodeDTO GetServiceCodeDTO(KeyValuePair<int, List<ExcelRowData>> kvp)
        {
            ServiceCodeDTO dto = new ServiceCodeDTO();

            long serviceCodeId = 0;

            int column = columnIndicesAndNames.Where(a => a.Value.ToUpper() == "CODE").Select(b => b.Key).First();

            dto.ServiceCode = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            column = columnIndicesAndNames.Where(a => a.Value == "COST").Select(b => b.Key).First();

            dto.Cost = Convert.ToDecimal(rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault());

            column = columnIndicesAndNames.Where(a => a.Value == "RETAIL").Select(b => b.Key).First();

            dto.Price = Convert.ToDecimal(rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault());

            column = columnIndicesAndNames.Where(a => a.Value == "TAXABLE").Select(b => b.Key).First();

            string taxable = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            dto.Taxable = taxable.Equals("YES") ? true : false;

            return dto;
        }

        private PlantDTO GetPlantDTO(KeyValuePair<int, List<ExcelRowData>> kvp)
        {
            PlantDTO dto = new PlantDTO();

            return dto;
        }

        private FoliageDTO GetFoliageDTO(KeyValuePair<int, List<ExcelRowData>> kvp)
        {
            FoliageDTO dto = new FoliageDTO();

            return dto;
        }

        private MaterialDTO GetMaterialDTO(KeyValuePair<int, List<ExcelRowData>> kvp)
        {
            MaterialDTO dto = new MaterialDTO();

            return dto;
        }

        private ContainerDTO GetContainerDTO(KeyValuePair<int, List<ExcelRowData>> kvp)
        {
            ContainerDTO dto = new ContainerDTO();

            return dto;
        }

        private void ImportFoliage(KeyValuePair<int, List<ExcelRowData>> kvp, ExcelPicture img)
        {
            //check for presence of plantName - add if not present
            long foliageNameId = 0;
                        
            int column = columnIndicesAndNames.Where(a => a.Value == "NAME").Select(b => b.Key).First();

            string foliageName = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            string result = Get("DoesFoliageNameExist", "foliageName=" + foliageName);

            GetLongIdResponse r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            foliageNameId = r.returnedId;

            column = columnIndicesAndNames.Where(a => a.Value == "SIZE").Select(b => b.Key).First();

            string foliageSize = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;


            //check for presence of plantType - add if not present

            long foliageTypeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value == "TYPE").Select(b => b.Key).First();

            string foliageType = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("DoesFoliageTypeExist", "foliageType=" + foliageType);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            foliageTypeId = r.returnedId;

            ////using cost, retail, plantName and plant Type, check for presence of service code - add if not present

            //ServiceCodeDTO serviceCode = GetServiceCodeDTO(kvp);

            //string jsonData = JsonConvert.SerializeObject(serviceCode);
            //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //result = Post("DoesServiceCodeExist", content);

            long serviceCodeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value.ToUpper() == "CODE").Select(b => b.Key).First();

            string serviceCode = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("ServiceCodeIsNotUnique", "serviceCode=" + serviceCode);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);

            serviceCodeId = r.returnedId;

            //check for presence of plant - add if not present

            long foliageId = 0;

            FoliageDTO foliageDTO = GetFoliageDTO(kvp);

            string jsonData = JsonConvert.SerializeObject(foliageDTO);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            result = Post("DoesFoliageExist", content);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);

            foliageId = r.returnedId;

            //check for presence of inventory - add if not present

            ImportFoliageRequest request = new ImportFoliageRequest();

            request.AddFoliageRequest.Foliage.FoliageSize = foliageSize;

            //add image if present
            if (img != null)
            {
                ImageConverter imgCon = new ImageConverter();
                request.imageBytes = (byte[])imgCon.ConvertTo(img.Image, typeof(byte[]));
            }

            //create DTO - send to service - backend will do the hookup

            if (foliageNameId == 0)
            {
                request.FoliageName = foliageName;
            }
            else
            {
                request.AddFoliageRequest.Foliage.FoliageNameId = foliageNameId;
            }

            if (foliageTypeId == 0)
            {
                request.FoliageType = foliageType;
            }
            else
            {
                request.AddFoliageRequest.Foliage.FoliageTypeId = foliageTypeId;
            }


            request.ServiceCode = GetServiceCodeDTO(kvp);

            if (serviceCodeId > 0)
            {
                ServiceCodeDTO original = GetServiceCodeById(serviceCodeId);
                UpdateServiceCode(original, request.ServiceCode);
                request.ServiceCode = original;
                request.ServiceCode.ServiceCodeId = serviceCodeId;
                request.AddFoliageRequest.Inventory.ServiceCodeId = serviceCodeId;
            }

            jsonData = JsonConvert.SerializeObject(request);
            content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Post("ImportFoliage", content);
        }

        private void ImportContainer(KeyValuePair<int, List<ExcelRowData>> kvp, ExcelPicture img)
        {
            //check for presence of plantName - add if not present
            long containerNameId = 0;

            int column = columnIndicesAndNames.Where(a => a.Value == "CONTAINER NAME").Select(b => b.Key).First();

            string containerName = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            string result = Get("DoesContainerNameExist", "containerName=" + containerName);

            GetLongIdResponse r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            containerNameId = r.returnedId;

            //check for presence of plantType - add if not present

            column = columnIndicesAndNames.Where(a => a.Value.ToUpper() == "SIZE").Select(b => b.Key).First();

            string containerSize = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            long containerTypeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value == "TYPE").Select(b => b.Key).First();

            string containerType = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("DoesContainerTypeExist", "containerType=" + containerType);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            containerTypeId = r.returnedId;

            ////using cost, retail, plantName and plant Type, check for presence of service code - add if not present

            //ServiceCodeDTO serviceCode = GetServiceCodeDTO(kvp);

            //string jsonData = JsonConvert.SerializeObject(serviceCode);
            //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //result = Post("DoesServiceCodeExist", content);

            long serviceCodeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value == "CODE").Select(b => b.Key).First();

            string serviceCode = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("ServiceCodeIsNotUnique", "serviceCode=" + serviceCode);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            serviceCodeId = r.returnedId;

            //check for presence of plant - add if not present

            long containerId = 0;

            ContainerDTO containerDTO = GetContainerDTO(kvp);

            string jsonData = JsonConvert.SerializeObject(containerDTO);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            result = Post("DoesContainerExist", content);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            containerId = r.returnedId;

            //check for presence of inventory - add if not present

            ImportContainerRequest request = new ImportContainerRequest();

            //add image if present
            if (img != null)
            {
                ImageConverter imgCon = new ImageConverter();
                request.imageBytes = (byte[])imgCon.ConvertTo(img.Image, typeof(byte[]));
            }

            //create DTO - send to service - backend will do the hookup
            request.ContainerSize = containerSize;

            if (containerNameId == 0)
            {
                request.ContainerName = containerName;
            }
            else
            {
                request.AddContainerRequest.Container.ContainerNameId = containerNameId;
            }

            if (containerTypeId == 0)
            {
                request.ContainerType = containerType;
            }
            else
            {
                request.AddContainerRequest.Container.ContainerTypeId = containerTypeId;
            }


            request.ServiceCode = GetServiceCodeDTO(kvp);

            if (serviceCodeId > 0)
            {
                ServiceCodeDTO original = GetServiceCodeById(serviceCodeId);
                UpdateServiceCode(original, request.ServiceCode);
                request.ServiceCode = original;
                request.ServiceCode.ServiceCodeId = serviceCodeId;
                request.AddContainerRequest.Inventory.ServiceCodeId = serviceCodeId;
            }

            jsonData = JsonConvert.SerializeObject(request);
            content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Post("ImportContainer", content);
        }

        private void ImportMaterial(KeyValuePair<int, List<ExcelRowData>> kvp, ExcelPicture img)
        {
            //check for presence of plantName - add if not present
            long materialNameId = 0;

            int column = columnIndicesAndNames.Where(a => a.Value == "NAME").Select(b => b.Key).First();

            string materialName = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            string result = Get("DoesMaterialNameExist", "materialName=" + materialName);

            GetLongIdResponse r  = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            materialNameId = r.returnedId;

            column = columnIndicesAndNames.Where(a => a.Value == "SIZE").Select(b => b.Key).First();

            string materialSize = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            //check for presence of plantType - add if not present

            long materialTypeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value == "TYPE").Select(b => b.Key).First();

            string materialType = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("DoesMaterialTypeExist", "materialType=" + materialType);

            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);
            materialTypeId = r.returnedId;

            ////using cost, retail, plantName and plant Type, check for presence of service code - add if not present

            //ServiceCodeDTO serviceCode = GetServiceCodeDTO(kvp);

            //string jsonData = JsonConvert.SerializeObject(serviceCode);
            //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //result = Post("DoesServiceCodeExist", content);

            long serviceCodeId = 0;

            column = columnIndicesAndNames.Where(a => a.Value == "CODE").Select(b => b.Key).First();

            string serviceCode = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;

            result = Get("ServiceCodeIsNotUnique", "serviceCode=" + serviceCode);
            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);

            serviceCodeId = r.returnedId;

            //check for presence of plant - add if not present

            long materialId = 0;

            MaterialDTO materialDTO = GetMaterialDTO(kvp);

            string jsonData = JsonConvert.SerializeObject(materialDTO);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            result = Post("DoesMaterialExist", content);
            r = JsonConvert.DeserializeObject<GetLongIdResponse>(result);

            materialId = r.returnedId;

            column = columnIndicesAndNames.Where(a => a.Value == "SKU").Select(b => b.Key).FirstOrDefault();

            string sku = String.Empty;
            if(column == 9)
            {
                sku = rowData[kvp.Key].Where(a => a.column == column).Select(b => b.data).FirstOrDefault() as string;
            }

            //check for presence of inventory - add if not present

            ImportMaterialRequest request = new ImportMaterialRequest();

            //add image if present
            if (img != null)
            {
                ImageConverter imgCon = new ImageConverter();
                request.imageBytes = (byte[])imgCon.ConvertTo(img.Image, typeof(byte[]));
            }

            //create DTO - send to service - backend will do the hookup

            request.MaterialSize = materialSize;
            request.AddMaterialRequest.Material.MaterialSize = materialSize;

            if (materialNameId == 0)
            {
                request.MaterialName = materialName;
            }
            else
            {
                request.AddMaterialRequest.Material.MaterialNameId = materialNameId;
            }

            if (materialTypeId == 0)
            {
                request.MaterialType = materialType;
            }
            else
            {
                request.AddMaterialRequest.Material.MaterialTypeId = materialTypeId;
            }


            request.ServiceCode = GetServiceCodeDTO(kvp);

            if (serviceCodeId > 0)
            {
                ServiceCodeDTO original = GetServiceCodeById(serviceCodeId);
                UpdateServiceCode(original, request.ServiceCode);
                request.ServiceCode = original;
                request.ServiceCode.ServiceCodeId = serviceCodeId;
                request.AddMaterialRequest.Inventory.ServiceCodeId = serviceCodeId;
            }

            if (!String.IsNullOrEmpty(sku))
            {
               //Currently, no db support
               //request.ServiceCode.SKU = sku;
            }

            jsonData = JsonConvert.SerializeObject(request);
            content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Post("ImportMaterial", content);
        }

        private bool ImportFileIsValid()
        {
            bool importFileIsValid = true;

            if (columnIndicesAndNames.Count < 9)
            {
                importFileIsValid = false;
            }

            foreach (KeyValuePair<int, string> kvp in columnIndicesAndNames)
            {
                switch (kvp.Key)
                {
                    case 0:
                        if (kvp.Value.ToUpper() != "TYPE")
                            importFileIsValid = false;
                        break;

                    case 1:
                        if (kvp.Value.ToUpper() != "NAME")
                            importFileIsValid = false;
                        break;

                    case 2:
                        if (kvp.Value.ToUpper() != "SIZE")
                            importFileIsValid = false;
                        break;

                    case 3:
                        if (kvp.Value.ToUpper() != "RETAIL")
                            importFileIsValid = false;
                        break;

                    case 4:
                        if (kvp.Value.ToUpper() != "COST")
                            importFileIsValid = false;
                        break;

                    case 5:
                        if (kvp.Value.ToUpper() != "CODE")
                            importFileIsValid = false;
                        break;

                    case 6:
                        if (kvp.Value.ToUpper() != "PHOTO")
                            importFileIsValid = false;
                        break;

                    case 7:
                        if (kvp.Value.ToUpper() != "COLORS")
                            importFileIsValid = false;
                        break;

                    case 8:
                        if (kvp.Value.ToUpper() != "TAXABLE")
                            importFileIsValid = false;
                        break;
                }
            }

            return importFileIsValid;
        }

        public string Get(string endpoint, string parameters)
        {
            string strData = String.Empty;

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                           new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                //string jsonData = JsonConvert.SerializeObject(addArrangementRequest);
                //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.GetAsync("api/Login/" + endpoint + "?" + parameters).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    strData = httpResponse.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    MessageBox.Show("Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return strData;
        }

        /// <summary>
        /// Don't forget to serialize the content parameter
        /// string jsonData = JsonConvert.SerializeObject(addArrangementRequest);
        /// var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="content"></param>
        public string Post(string endpoint, StringContent content)
        {
            string strData = String.Empty;

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(((App)App.Current).LAN_Address);

                client.DefaultRequestHeaders.Accept.Add(
                           new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                //string jsonData = JsonConvert.SerializeObject(addArrangementRequest);
                //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/" + endpoint, content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    strData = httpResponse.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    MessageBox.Show("Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return strData;
        }
    }

    public class ExcelRowData
    {
        public ExcelRowData()
        {

        }

        public ExcelRowData(int row, int col, object data)
        {
            this.row = row;
            this.column = col;
            this.data = data;
        }
        public int row { get; set; }

        public int column { get; set; }

        public object data { get; set; }
    }
}
