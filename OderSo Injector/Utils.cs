using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace OderSo_Injector
{
    public class Utils
    {
        public static string GetLatestReleaseName()
        {
            string downloadBase = "https://github.com/MasonOderSo/OderSoReleases/releases/";
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;

            HttpClient releaseClient = new HttpClient(handler);
            var latestRelease = releaseClient.GetAsync(downloadBase + "latest");
            var location = latestRelease.Result.Headers.Location;

            if (location == null)
            {
                MessageBox.Show ("Error, no \"Location\" found in response header");
                return "";
            }

            string str = location.ToString();
            return str.Substring(str.LastIndexOf('/') + 1);
        }

        public static bool DownloadClient()
        {
            string downloadBase = "https://github.com/MasonOderSo/OderSoReleases/releases/";
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;

            HttpClient releaseClient = new HttpClient(handler);
            var latestRelease = releaseClient.GetAsync(downloadBase + "latest");
            var location = latestRelease.Result.Headers.Location;

            if (location == null)
            {
                MessageBox.Show("Error, no \"Location\" found in response header");
                return false;
            }

            string str = location.ToString();
            string releaseName = str.Substring(str.LastIndexOf('/') + 1);

            HttpClient download = new HttpClient();
            var downloadRequest = download.GetStreamAsync(downloadBase + "download/" + releaseName + "/OderSo.dll");
            FileStream fileStream = new FileStream("OderSo.dll", FileMode.OpenOrCreate);
            downloadRequest.Result.CopyTo(fileStream);

            return true;
        }

        public static bool CheckConnection()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://github.com");
                request.KeepAlive = false;
                request.Timeout = 1000;
                using (request.GetResponse()) return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
