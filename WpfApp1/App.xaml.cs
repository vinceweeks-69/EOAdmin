using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ViewModels.ControllerModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string User { get; set; }

        public string Pwd { get; set; }

        public string LAN_Address { get; set; }

        public string Dootster { get { return "Orchids@5185"; } }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //set LAN Address
            LAN_Address = "http://10.0.0.5:9000/";   //Me Fl Royalwood

            //LAN_Address = "elegantsystem.ddns.net";  // the no ip server
        }

        public void LogError(string message, string payload)
        {
            try
            {
                ErrorLogRequest request = new ErrorLogRequest();
                request.ErrorLog.Message = message;
                request.ErrorLog.Payload = payload;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                string jsonData = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/LogError", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {

                }
            }
            catch (Exception ex)
            {
                //ironic
            }
        }
    }
}
