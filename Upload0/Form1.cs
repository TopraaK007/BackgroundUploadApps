using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Upload0
{

    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();

        }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string UploadTime { get; set; }
        public string DocumentPath { get; set; }
        public int WaitTime { get; set; }
        private async void Form1_Load(object sender, EventArgs e)
        {

            string xmlPath = "v2.xml";
            await LoadXml(xmlPath);

        }
        private async Task LoadXml(string xml)
        {

            if (!File.Exists(xml))
            {
                MessageBox.Show("Dosya Bulunamadı");
            }
            else
            {
                try
                {
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.Load(xml);
                 
                    XmlNode xmlNode = xmldoc.SelectSingleNode("/file");

                    Server = xmlNode.SelectSingleNode("server").InnerText;
                    Username = xmlNode.SelectSingleNode("username").InnerText;
                    Password = xmlNode.SelectSingleNode("password").InnerText;
                    DocumentPath = xmlNode.SelectSingleNode("document").InnerText;
                    WaitTime = int.Parse(xmlNode.SelectSingleNode("time").InnerText);
                    UploadTime = xmlNode.SelectSingleNode("lastupload").InnerText;

                   

                    //Username = Username.Substring(10);
                    //Password = Password.Substring(10);
                    await EditFiles();
                }
                catch (Exception ex)
                {
                    BackUpload.BalloonTipIcon = ToolTipIcon.Error;
                    BackUpload.BalloonTipTitle = "Xml Hatası!";
                    BackUpload.BalloonTipText = ex.Message.ToString();
                    BackUpload.ShowBalloonTip(5000);
                    
                }

            }

        }

        private DateTime uploadTime = DateTime.MinValue;
        private async Task EditFiles()
        {

            while (true)
            {

                int waitTime = WaitTime;

                BackUpload.Text = "Yükleniyor....";
               

                try
                {
                    string LocalFilePath = DocumentPath;
                    string RemoteFile = "/";
                    string[] files = Directory.GetFiles(LocalFilePath);
                    using (WebClient client = new WebClient())
                    {
                        client.Credentials = new NetworkCredential(Username, Password);

                        foreach (string file in files)
                        {
                            string fileName = Path.GetFileName(file);
                            string remoteFilePath = RemoteFile + fileName;
                            string ftpUrl = $"ftp://{Server}/{remoteFilePath}";

                            client.UploadFile(ftpUrl, WebRequestMethods.Ftp.UploadFile, file);
                        }
                        uploadTime = DateTime.Now;
                        SaveLastUploadTimeToXml(uploadTime);
                        BackUpload.Text = "Yükeleme Tamam Bir Sonraki Bekleniyor";
                        await Task.Delay(waitTime * 1000);
                        
                    }


                }
                catch (WebException ex)
                {
                    if (ex.Message.Contains("530"))
                    {
                        BackUpload.BalloonTipIcon = ToolTipIcon.Error;
                        BackUpload.BalloonTipTitle = "Kullanıcı Adı veya Şifre Hatalı!";
                        BackUpload.BalloonTipText = ex.Message.ToString();
                        BackUpload.ShowBalloonTip(5000);
                        BackUpload.Text = "Yükleme Yapılamadı!";
                        break;
                    }
                    else
                    {
                        BackUpload.BalloonTipIcon = ToolTipIcon.Error;
                        BackUpload.BalloonTipTitle = "Ftp Sunucu Hatası!";
                        BackUpload.BalloonTipText = ex.Message.ToString();
                        BackUpload.ShowBalloonTip(5000);
                        BackUpload.Text = "Yükleme Yapılamadı!";
                        break;
                    }

                }
            }
        }

        private void SaveLastUploadTimeToXml(DateTime lastuploadTime)
        {
         
                string xmlPath = "v2.xml";

                try
                {
                    XDocument xmlDocument = XDocument.Load(xmlPath);
                    xmlDocument.Root.Element("lastupload").Value = lastuploadTime.ToString("dd.MM.yyyy HH:mm:ss");
                    xmlDocument.Save(xmlPath);
                }
                catch (Exception ex)
                {
                    // XML kaydetme hatası oluştuğunda işlem yapılabilir
                    MessageBox.Show("XML kaydetme hatası: " + ex.Message);
                }
            
        }

        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit(); 
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        { 
                this.Hide();
                
        }
        private void BackUpload_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string xmlPath = "v2.xml";

            if (File.Exists(xmlPath))
            {
                try
                {
                   XDocument document = XDocument.Load(xmlPath);


                    XElement uploadTimeElement = document.Root.Element("lastupload");
                    UploadTime= uploadTimeElement.Value;
                    BackUpload.ShowBalloonTip(3000, "Bilgilendirme.", $"Son Yükleme{UploadTime}", ToolTipIcon.Info);
                }
                catch (Exception ex)
                { 
                    BackUpload.ShowBalloonTip(3000, $"Yükleme Yapılamadı!+{ex.ToString()}", $"Son Yükleme{UploadTime}", ToolTipIcon.Warning);
                }
            }
        }
    }
}
