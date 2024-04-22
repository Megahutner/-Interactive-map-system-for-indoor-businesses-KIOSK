using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
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
using UWEKiosk.Entities;
using Newtonsoft.Json;
using System.Security.Policy;
using System.IO;
using System.Configuration;
using Microsoft.SqlServer.Server;
using System.Net;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Imaging;

namespace UWEKiosk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GetZoneDetails();
        }

        private void GetZoneDetails()
        {
            string zoneId = ConfigurationManager.AppSettings["zoneId"].ToString();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7107/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("api/zone/get-zone-info?zoneId=" + zoneId).Result;
            if (response.IsSuccessStatusCode)
            {
                File.WriteAllText($"{Directory.GetCurrentDirectory()}/DataInformation/json.txt", response.Content.ReadAsStringAsync().Result);
                var content = JsonConvert.DeserializeObject<DataResponse>(response.Content.ReadAsStringAsync().Result);
                if(content.data.ImgUrl != "")
                {
                    WebClient Webclient = new WebClient();
                    Stream stream = Webclient.OpenRead(new Uri("https://localhost:7107/Uploads/" + content.data.ImgUrl));
                    Bitmap bitmap; bitmap = new Bitmap(stream);

                    if (bitmap != null)
                    {
                        bitmap.Save($"{Directory.GetCurrentDirectory()}/DataInformation/zoneImage.png", ImageFormat.Png);
                    }

                    stream.Flush();
                    stream.Close();
                    client.Dispose();
                }        
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            }
        }
    }
}
