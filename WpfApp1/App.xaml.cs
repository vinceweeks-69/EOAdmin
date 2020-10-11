using EO.ViewModels.ControllerModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ViewModels.ControllerModels;
using WpfApp1.ViewModels;

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
            LAN_Address = "http://10.0.0.4:9000/";   //Me Fl Royalwood

            //LAN_Address = "http://10.1.10.36:9000";  //Roseanne from jdambar and Orchid Home

            //LAN_Address = "elegantsystem.ddns.net";  // the no ip server

            GetNetworkConfig();

            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
        }

        private void GetNetworkConfig()
        {

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection nics = mc.GetInstances();

            List<string> enabledIPs = new List<string>();

            foreach (ManagementObject nic in nics)
            {
                if (Convert.ToBoolean(nic["ipEnabled"]) == true)
                {
                    string IpAddress = (nic["IPAddress"] as String[])[0];

                    enabledIPs.Add(IpAddress);

                    string IPSubnet = (nic["IPSubnet"] as String[])[0];

                    string DefaultGateWay = (nic["DefaultIPGateway"] as String[])[0];
                }
            }

            if (enabledIPs.Count > 0)
            {
                //get protocol - as of 10 - 2020 it's still http

                LAN_Address = "http://" + enabledIPs.First() + ":9000";
            }
        }

        void AddressChangedCallback(object sender, EventArgs e)
        {
            GetNetworkConfig();
        }

        //where the call you want make is paramName = objectPkId, objectName (string)  OR leave paramName and paramID empty for "Get All"
        public async Task<T> GetRequest<T>(GenericGetRequest getRequest) where T : new()
        {
            try
            {
                string webServiceAdx = "api/login/" + getRequest.Uri;

                if (!String.IsNullOrEmpty(getRequest.ParamName))
                {
                    webServiceAdx += "?" + getRequest.ParamName + "=";

                    if (!String.IsNullOrEmpty(getRequest.ParamValue))
                    {
                        webServiceAdx += getRequest.ParamValue;
                    }
                    else
                    {
                        webServiceAdx += getRequest.ParamId.ToString();
                    }
                }

                var client = new HttpClient();
                client.BaseAddress = new Uri(LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                using (HttpResponseMessage httpResponse = await
                    client.GetAsync(webServiceAdx))
                {
                    httpResponse.EnsureSuccessStatusCode();

                    Stream streamData = await httpResponse.Content.ReadAsStreamAsync();
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(strData);
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(getRequest.Uri, ex);
                LogError(ex2.Message, JsonConvert.SerializeObject(getRequest));
                //this returns null for reference types
                return new T();
            }
        }

        public async Task<TOut> PostRequest<TIn, TOut>(string uri, TIn content) where TOut : new()
        {
            string serializedContent = String.Empty;

            try
            {
                string webServiceAdx = "api/login/" + uri;

                var client = new HttpClient();
                client.BaseAddress = new Uri(LAN_Address);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                serializedContent = JsonConvert.SerializeObject(content);
                StringContent serialized = new StringContent(serializedContent, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await client.PostAsync(webServiceAdx, serialized))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<TOut>(responseBody);
                }
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(uri, ex);
                LogError(ex2.Message, serializedContent);
                return new TOut();
            }
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
