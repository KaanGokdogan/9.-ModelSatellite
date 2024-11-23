using Accord.Video.FFMPEG;
using Accord.Video;
using Accord.Video.DirectShow;
using GMap.NET.MapProviders;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Xml;
using System.Security.Policy;


namespace B_Dispate_Yer_İstasyonu_v2._0
{
    public partial class Form1 : Form
    {
        string[] dataParts;
        private FilterInfoCollection VideoCaptureDevices;
        private VideoCaptureDevice FinalVideo = null;
        private VideoCaptureDeviceForm captureDevice;
        //private AVIWriter AVIwriter = new AVIWriter();
        private VideoFileWriter FileWriter = new VideoFileWriter();
        string receivedData;
        bool kayit = false;
        double hizeski = 0;
        long max = 21, min = 0;
        double x, y, z;
        bool button_flag = true;
        double pilgerilimi, latDouble, longDouble;
        int pil_yüzde;
        int paket;
        string statu;
        string hatakodu;
        string[]tarih;
        int saniye, dakika;
        string bas1,bas2,yuk1,yuk2,irtifa,hiz,sıcaklık,pil,gps_lat,gps_long,gps_alt,pitch,roll,yaw,rhrh,iot;
        public Form1()
        {
            
            InitializeComponent();
            InitializeSerialPort();
            using (StreamWriter sw = File.AppendText("C:\\Users\\MONSTER\\Desktop\\klasöer\\TMUY2024_270757_TLM.csv"))
            {
                sw.Write("PAKET NUMARASI;UYDU STATUSU;HATA KODU;GONDERME SAATI;BASINC1;BASINC2;YUKSEKLIK1;YUKSEKLIK2;IRTIFA FARKI;INIS HIZI;SICAKLIK;PIL GERILIMI;GPS1 LATITUDE;GPS1 LONGITUDE;GPS1 ALTITUDE;PITCH;ROLL;YAW;RHRH;IoT DATA;TAKIM NO");
            }
            Control.CheckForIllegalCrossThreadCalls = false;
           
            kayıt1.Visible = false;
            ayarlar1.Visible = true;
            grafikler1.Visible = false;
            harita1.Visible = false;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            
            dataGridView1.EnableHeadersVisualStyles = true;
            
            dataGridView1.ColumnHeadersHeight = 55;
            ayarlar1.pictureBox1.Click += new EventHandler(Acma_kapama);
           ayarlar1.pictureBox2.Click += new EventHandler(Com_port_yenileme);

            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            captureDevice = new VideoCaptureDeviceForm();
        }
        private void InitializeSerialPort()
        {
            serialPort1 = new SerialPort();

            ayarlar1.comboBox1.Items.AddRange(SerialPort.GetPortNames());
            if (ayarlar1.comboBox1.Items.Count > 0)
            {
                ayarlar1.comboBox1.SelectedIndex = 0;
            }

            ayarlar1.comboBox3.Items.AddRange(new object[] { "2400", "4800", "9600", "115200" });
            ayarlar1.comboBox3.SelectedIndex = 0;

        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

         


             receivedData = serialPort1.ReadExisting();

            if (receivedData.Length < 120)
            {
                if(string.IsNullOrEmpty(receivedData))
            {
                    
                }
            


                receivedData = (paket + 1).ToString() + ";" + statu + ";" + hatakodu + ";" + tarih[0] + "/" + tarih[1] + "/" + tarih[2] +"/"+dakika.ToString()+"/"+saniye.ToString()+";"+bas1+";"+bas2+";"+yuk1+";"+yuk2+";"+irtifa+";"+hiz+";"+sıcaklık+";"+pil+";"+gps_lat+";"+gps_long+";"+gps_alt+";"+pitch+";"+roll+";"+yaw+";"+rhrh+";"+iot+";"+270757+"\n";

            }
            dataParts = receivedData.Split(';');

            paket = Convert.ToInt32(dataParts[0]);
            statu =(dataParts[1]);
            hatakodu =(dataParts[2]);
            tarih = dataParts[3].Split('/');
            saniye = Convert.ToInt32(tarih[4]);
            dakika= Convert.ToInt32(tarih[3]);
            bas1 = dataParts[4];
            bas2 = dataParts[5];
            yuk1 = dataParts[6];
            yuk2 = dataParts[7];
            irtifa = dataParts[8];
            hiz = dataParts[9];
            sıcaklık = dataParts[10];
            pil = dataParts[11];
            gps_lat = dataParts[12];
            gps_long = dataParts[13];
            gps_alt = dataParts[14];
            pitch = dataParts[15];
            roll = dataParts[16];
            yaw = dataParts[17];
            rhrh = dataParts[18];
            iot = dataParts[19];

            if (saniye == 59)
            {
                saniye = 0;
                dakika++;
            }
            else saniye++;
             


            AppendToCSV(receivedData);
         
       

           


            if (string.IsNullOrEmpty(dataParts[15]))
            {
                x = 0.00;
            }
            else { x = -(Convert.ToDouble(dataParts[15].Trim()) / 100.00); }
            if (string.IsNullOrEmpty(dataParts[16]))
            {
                y = 0.00;
            }
            else { y = -(Convert.ToDouble(dataParts[16].Trim()) / 100.00); }
            if (string.IsNullOrEmpty(dataParts[17]))
            {
                z = 0.00;
            }
            else { z = -(Convert.ToDouble(dataParts[17].Trim()) / 100.00); }

           
            
           
            BeginInvoke(new Action(() =>
            {

                yukseklik1.ChartAreas[0].AxisX.Minimum = min;
                yukseklik1.ChartAreas[0].AxisX.Maximum = max;
                yukseklik1.ChartAreas[0].AxisY.Minimum = 0;
                yukseklik1.ChartAreas[0].AxisY.Maximum = 1000;
               
                grafikler1.yukseklik1_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.yukseklik1_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.yukseklik1_user.ChartAreas[0].AxisY.Minimum = 0;
                grafikler1.yukseklik1_user.ChartAreas[0].AxisY.Maximum = 1000;
               

                yükseklik2.ChartAreas[0].AxisX.Minimum = min;
                yükseklik2.ChartAreas[0].AxisX.Maximum = max;
                yükseklik2.ChartAreas[0].AxisY.Minimum = 0;
                yükseklik2.ChartAreas[0].AxisY.Maximum = 1000;

                grafikler1.yukseklik2_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.yukseklik2_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.yukseklik2_user.ChartAreas[0].AxisY.Minimum = 0;
                grafikler1.yukseklik2_user.ChartAreas[0].AxisY.Maximum = 1000;
               
               
                irtifa_farki.ChartAreas[0].AxisX.Minimum = min;
                irtifa_farki.ChartAreas[0].AxisX.Maximum = max;
                irtifa_farki.ChartAreas[0].AxisY.Minimum = 0;
                irtifa_farki.ChartAreas[0].AxisY.Maximum = 700;

                grafikler1.irtifa_farki_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.irtifa_farki_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.irtifa_farki_user.ChartAreas[0].AxisY.Minimum = 0;
                grafikler1.irtifa_farki_user.ChartAreas[0].AxisY.Maximum = 700;
               

                grafikler1.inis_hizi_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.inis_hizi_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.inis_hizi_user.ChartAreas[0].AxisY.Minimum = 0;
                grafikler1.inis_hizi_user.ChartAreas[0].AxisY.Maximum = 50;
                

                sicaklik.ChartAreas[0].AxisX.Minimum = min;
                sicaklik.ChartAreas[0].AxisX.Maximum = max;
                sicaklik.ChartAreas[0].AxisY.Minimum = 0;
                sicaklik.ChartAreas[0].AxisY.Maximum = 50;

                grafikler1.sicaklik_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.sicaklik_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.sicaklik_user.ChartAreas[0].AxisY.Minimum = 0;
                grafikler1.sicaklik_user.ChartAreas[0].AxisY.Maximum = 50;
               

                iot_data.ChartAreas[0].AxisX.Minimum = min;
                iot_data.ChartAreas[0].AxisX.Maximum = max;
                iot_data.ChartAreas[0].AxisY.Minimum = 0;
                iot_data.ChartAreas[0].AxisY.Maximum = 50;

                grafikler1.iot_data_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.iot_data_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.iot_data_user.ChartAreas[0].AxisY.Minimum = 0;
                grafikler1.iot_data_user.ChartAreas[0].AxisY.Maximum = 50;
              

                basinc_1.ChartAreas[0].AxisX.Minimum = min;
                basinc_1.ChartAreas[0].AxisX.Maximum = max;
                basinc_1.ChartAreas[0].AxisY.Minimum = 900;
                basinc_1.ChartAreas[0].AxisY.Maximum = 1200;

                grafikler1.basinc1_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.basinc1_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.basinc1_user.ChartAreas[0].AxisY.Minimum = 900;
                grafikler1.basinc1_user.ChartAreas[0].AxisY.Maximum = 1200;
               

                basinc2.ChartAreas[0].AxisX.Minimum = min;
                basinc2.ChartAreas[0].AxisX.Maximum = max;
                basinc2.ChartAreas[0].AxisY.Minimum = 900;
                basinc2.ChartAreas[0].AxisY.Maximum = 1200;

                grafikler1.basinc2_user.ChartAreas[0].AxisX.Minimum = min;
                grafikler1.basinc2_user.ChartAreas[0].AxisX.Maximum = max;
                grafikler1.basinc2_user.ChartAreas[0].AxisY.Minimum = 900;
                grafikler1.basinc2_user.ChartAreas[0].AxisY.Maximum = 1200;
               
                
                iniş_hizi.ChartAreas[0].AxisX.Minimum = min;
                iniş_hizi.ChartAreas[0].AxisX.Maximum = max;
                iniş_hizi.ChartAreas[0].AxisY.Minimum = 0;
                iniş_hizi.ChartAreas[0].AxisY.Maximum = 50;

             

              //gMapControl1.Position = new PointLatLng(latDouble, longDouble); 
              




                pil_yüzde = Convert.ToInt32(((pilgerilimi - 11.10) / 1.50) * 100.00);
                if (pil_yüzde <= 0)
                {
                    pil_yüzde = 0;
                }
                else if (pil_yüzde >= 100)
                {
                    pil_yüzde = 100;
                }

                glControl1.Invalidate();

             typeof(DataGridView).InvokeMember("DoubleBuffered",
             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
             System.Reflection.BindingFlags.SetProperty, null, dataGridView1, new object[] { true });

                    progressBar1.Value = pil_yüzde;
                    pil_label.Text = "%" + pil_yüzde.ToString();

                latDouble = Convert.ToDouble(dataParts[12], CultureInfo.InvariantCulture);
                longDouble = Convert.ToDouble(dataParts[13], CultureInfo.InvariantCulture);

                Console.WriteLine(latDouble);
               

                gMapControl1.MapProvider = GMapProviders.BingSatelliteMap;
                GMaps.Instance.Mode = AccessMode.CacheOnly;
                //GMaps.Instance.Mode = AccessMode.ServerOnly;
                //GMaps.Instance.CacheLocation = @"C:\GMapCache";  // Cache dizinini belirleyin
                
                gMapControl1.Position = new PointLatLng(latDouble, longDouble);
                    
                    gMapControl1.MinZoom = 5;
                    gMapControl1.MaxZoom = 100;
                    gMapControl1.Zoom = 16;

                harita1.gMapControl1.MapProvider = GMapProviders.BingSatelliteMap;
                GMaps.Instance.Mode = AccessMode.ServerOnly;
                harita1.gMapControl1.Position = new PointLatLng(latDouble, longDouble);
                harita1.gMapControl1.Zoom = 18;
                harita1.gMapControl1.MinZoom = 5;
                harita1.gMapControl1.MaxZoom = 100;








                yukseklik1.Series[0].Points.AddXY(dataParts[0], dataParts[6]);
                    if (yukseklik1.Series[0].Points.Count > 14)
                    {
                        yukseklik1.Series[0].Points.RemoveAt(0);
                    }
                yukseklik1.ChartAreas[0].RecalculateAxesScale();

                grafikler1.yukseklik1_user.Series[0].Points.AddXY(dataParts[0], dataParts[6]);
                    if (grafikler1.yukseklik1_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.yukseklik1_user.Series[0].Points.RemoveAt(0);
                    }

                   
                    yükseklik2.Series[0].Points.AddXY(dataParts[0], dataParts[7]);
                    if (yükseklik2.Series[0].Points.Count > 14)
                    {
                        yükseklik2.Series[0].Points.RemoveAt(0);
                    }


                
                    grafikler1.yukseklik2_user.Series[0].Points.AddXY(dataParts[0], dataParts[7]);
                    if (grafikler1.yukseklik2_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.yukseklik2_user.Series[0].Points.RemoveAt(0);
                    }
                   

                    irtifa_farki.Series[0].Points.AddXY(dataParts[0], dataParts[8]);
                    if (irtifa_farki.Series[0].Points.Count > 14)
                    {
                        irtifa_farki.Series[0].Points.RemoveAt(0);
                    }



                  
                    grafikler1.irtifa_farki_user.Series[0].Points.AddXY(dataParts[0], dataParts[8]);
                    if (grafikler1.irtifa_farki_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.irtifa_farki_user.Series[0].Points.RemoveAt(0);
                    }

                    


                    iniş_hizi.Series[0].Points.AddXY(dataParts[0], dataParts[9]);
                
                if (iniş_hizi.Series[0].Points.Count > 14)
                    {
                        iniş_hizi.Series[0].Points.RemoveAt(0);
                    }


               
                    grafikler1.inis_hizi_user.Series[0].Points.AddXY(dataParts[0], dataParts[9]);
                    if (grafikler1.inis_hizi_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.inis_hizi_user.Series[0].Points.RemoveAt(0);
                    }
                 


                 
                    sicaklik.Series[0].Points.AddXY(dataParts[0], dataParts[10]);
                    if (sicaklik.Series[0].Points.Count > 14)
                    {
                        sicaklik.Series[0].Points.RemoveAt(0);
                    }
                 

                    grafikler1.sicaklik_user.Series[0].Points.AddXY(dataParts[0], dataParts[10]);
                    if (grafikler1.sicaklik_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.sicaklik_user.Series[0].Points.RemoveAt(0);
                    }

                    iot_data.Series[0].Points.AddXY(dataParts[0], dataParts[19]);
                    if (iot_data.Series[0].Points.Count > 14)
                    {
                        iot_data.Series[0].Points.RemoveAt(0);
                    }

                    grafikler1.iot_data_user.Series[0].Points.AddXY(dataParts[0], dataParts[19]);
                    if (grafikler1.iot_data_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.iot_data_user.Series[0].Points.RemoveAt(0);
                    }
                
                    basinc_1.Series[0].Points.AddXY(dataParts[0], dataParts[4]);
                    if (basinc_1.Series[0].Points.Count > 14)
                    {
                        basinc_1.Series[0].Points.RemoveAt(0);
                    }

                    grafikler1.basinc1_user.Series[0].Points.AddXY(dataParts[0], dataParts[4]);
                    if (grafikler1.basinc1_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.basinc1_user.Series[0].Points.RemoveAt(0);
                    }
                 
                    basinc2.Series[0].Points.AddXY(dataParts[0], dataParts[5]);
                    if (basinc2.Series[0].Points.Count > 14)
                    {
                        basinc2.Series[0].Points.RemoveAt(0);
                    }

                    grafikler1.basinc2_user.Series[0].Points.AddXY(dataParts[0], dataParts[5]);
                    if (grafikler1.basinc2_user.Series[0].Points.Count > 14)
                    {
                        grafikler1.basinc2_user.Series[0].Points.RemoveAt(0);
                    }



                if (dataParts[2][0] == '1')
                    {
                        pictureBox6.BackColor = Color.Red;
                        label6.ForeColor = Color.Red;

                    }
                    else
                    {
                        pictureBox6.BackColor = Color.Green;
                        label6.ForeColor = Color.Green;
                    }

                    if (dataParts[2][1] == '1')
                    {
                        pictureBox7.BackColor = Color.Red;
                        label7.ForeColor = Color.Red;
                    }
                    else
                    {
                        pictureBox7.BackColor = Color.Green;
                        label7.ForeColor = Color.Green;
                    }

                    if (dataParts[2][2] == '1')
                    {
                        pictureBox8.BackColor = Color.Red;
                        label8.ForeColor = Color.Red;
                    }
                    else
                    {
                        pictureBox8.BackColor = Color.Green;
                        label8.ForeColor = Color.Green;
                    }

                    if (dataParts[2][3] == '1')
                    {
                        pictureBox9.BackColor = Color.Red;
                        label9.ForeColor = Color.Red;
                    }
                    else
                    {
                        pictureBox9.BackColor = Color.Green;
                        label9.ForeColor = Color.Green;
                    }

                    if (dataParts[2][4] == '1')
                    {
                        pictureBox10.BackColor = Color.Red;
                        label10.ForeColor = Color.Red;
                    }
                    else
                    {
                        pictureBox10.BackColor = Color.Green;
                        label10.ForeColor = Color.Green;
                    }

                    //////////// UYDU STATÜ
                    if (Convert.ToInt32(dataParts[1]) == 0)
                    {
                   
                        label13.Text = "    UÇUŞA HAZIR";
                    }
                    else if (Convert.ToInt32(dataParts[1]) == 1)
                    {
                        label13.Text = "      YÜKSELME";
                        statu0.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.zero;
                        statu1.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_1__1_;

                    }
                    else if (Convert.ToInt32(dataParts[1]) == 2)
                    {
                        label13.Text = "MODEL UYDU İNİŞ";
                        statu1.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_1;
                        statu2.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_2__1_;
                    }
                    else if (Convert.ToInt32(dataParts[1]) == 3)
                    {
                        label13.Text = "      AYRILMA";
                        statu2.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_2;
                        statu3.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_3__1_;
                    }
                    else if (Convert.ToInt32(dataParts[1]) == 4)
                    {
                        label13.Text = "GÖREV YÜKÜ İNİŞ";
                        statu3.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_3; 
                        statu4.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_4__1_;
                    }
                    else if (Convert.ToInt32(dataParts[1]) == 5)
                    {
                        label13.Text = "      KURTARMA";
                        statu4.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_4;
                        statu5.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.number_5__1_;
                    }


                dataGridView1.Rows.Add(dataParts[0], dataParts[1], dataParts[2], dataParts[3], dataParts[4], dataParts[5], dataParts[6], dataParts[7], dataParts[8], dataParts[9], dataParts[10], dataParts[11], dataParts[12], dataParts[13], dataParts[14], dataParts[15], dataParts[16], dataParts[17], dataParts[18], dataParts[19], "270757");
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                dataGridView1.Columns["saat"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["yuksek1"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns["yuksek2"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                pilgerilimi = Convert.ToDouble(dataParts[11]) / 100.00;
               

            }
            ));
        }

        private void AppendToCSV(string data)
        {
           
            try
            {
                // CSV dosyasına veriyi ekleyin
                using (StreamWriter sw = File.AppendText("C:\\Users\\MONSTER\\Desktop\\klasöer\\TMUY2024_270757_TLM.csv"))
                {
                    sw.Write(data);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("CSV dosyasına veri eklenirken bir hata oluştu: " + ex.Message);
            }
        }


        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (serialPort1 != null && serialPort1.IsOpen)
            {
                serialPort1.Close();
            }

            if (FinalVideo == null)
            { return; }
            if (FinalVideo.IsRunning)
            {
                this.FinalVideo.Stop();
                FileWriter.Close();
                //this.AVIwriter.Close();
            }

        }



        private void Com_port_yenileme(object sender, EventArgs e)
        {

            ayarlar1.comboBox1.Items.Clear();
            ayarlar1.comboBox1.Items.AddRange(SerialPort.GetPortNames());
            if (ayarlar1.comboBox1.Items.Count > 0)
            {
                ayarlar1.comboBox1.SelectedIndex = 0;
            }
            else if (ayarlar1.comboBox1.Items.Count == 0)
            {
                ayarlar1.comboBox1.SelectedIndex = -1;
            }

        }
        private void Acma_kapama(object sender, EventArgs e)
        {
            if (button_flag)
            {
               ayarlar1.pictureBox1.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.switch_on;
                if (captureDevice.ShowDialog(this) == DialogResult.OK)
                {

                    //VideoCaptureDevice videoSource = captureDevice.VideoDevice;
                    FinalVideo = captureDevice.VideoDevice;
                    
                    FinalVideo.Start();
                    FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
               
                    FileWriter.Open("C:\\Users\\MONSTER\\Desktop\\klasöer\\TMUY2024_270757_VIDEO.avi", 640, 480,60);
                    //FileWriter.WriteVideoFrame(video);
                   
                    kayıt1.Visible = false;
                    ayarlar1.Visible = false;
                    grafikler1.Visible = false;
                    harita1.Visible = false;
                    kayit = true;
                }
                if (!serialPort1.IsOpen)
                {
                    try
                    {
                       if (ayarlar1.comboBox1.SelectedIndex == -1)
                        {
                            MessageBox.Show("COM GİRİŞİ SEÇİLMEDİ", "B-Dispate");
                        }
                        else
                        {
                            serialPort1.PortName = ayarlar1.comboBox1.SelectedItem.ToString();
                            serialPort1.BaudRate = int.Parse(ayarlar1.comboBox3.SelectedItem.ToString());
                       
                            serialPort1.Open();
                           
                            serialPort1.DataReceived += SerialPort_DataReceived;
                            dataGridView1.Visible = true;
                            
                         
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Bağlantı hatası: " + ex.Message);
                    }  }
                
                   

            }
            else
            {
                ayarlar1.pictureBox1.Image = B_Dispate_Yer_İstasyonu_v2._0.Properties.Resources.switch_off;

                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();      
                }
                if (kayit == true)
                {
                    kayit = false;
                    if (FinalVideo == null)
                    { return; }
                    if (FinalVideo.IsRunning)
                    {
                        //this.FinalVideo.Stop();
                        FileWriter.Close();
                        //this.AVIwriter.Close();
                        kamera.Image = null;
                    }
                }
                else
                {
                    this.FinalVideo.Stop();
                    FileWriter.Close();
                    //this.AVIwriter.Close();
                    kamera.Image = null;
                }

            }




            button_flag = !button_flag;

        }



        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (kayit == true)
            {
                //video = (Bitmap)eventArgs.Frame.Clone();
                kamera.Image = (Bitmap)eventArgs.Frame.Clone();
              
                kayıt1.pictureBox5.Image= (Bitmap)eventArgs.Frame.Clone();
                //AVIwriter.Quality = 0;
                FileWriter.WriteVideoFrame((Bitmap)eventArgs.Frame.Clone());
                //AVIwriter.AddFrame(video);
            }
           
        }

        
        /// komut

        private void button3_Click(object sender, EventArgs e)
        {
            String disk = "DISK" + textBox1.Text + ",";
            //textBox2.Text= disk;
            serialPort1.WriteLine(disk);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("AYRIL,");
        }

        //////////////////////////        OPEN GL //////////////////////////////////
        
        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(System.Drawing.Color.FromArgb(224,224,224));
            GL.Enable(EnableCap.DepthTest);
            
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
           
                float step = 1.0f;
                float topla = step;
                float radius = 5.0f;  // 16 birim çap için yarıçap = 8
                float dikey1 = 10.0f;  // Yükseklik = 10 için üst kapak konumu
                float dikey2 = -10.0f; // Alt kapak konumu

                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(1.04f, 4 / 3, 1, 10000);
                Matrix4 lookat = Matrix4.LookAt(25, 0, 0, 0, 0, 0, 0, 1, 0);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.LoadMatrix(ref perspective);
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.LoadMatrix(ref lookat);
                GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Less);

                GL.Rotate(x, 1.0, 0.0, 0.0);
                GL.Rotate(z, 0.0, 1.0, 0.0);
                GL.Rotate(y, 0.0, 0.0, 1.0);

                // 16x10 silindiri çiz
                silindir(step, topla, radius, dikey1, dikey2);

                GL.Begin(BeginMode.Lines);

                GL.Color3(Color.FromArgb(250, 0, 0));
                GL.Vertex3(-30.0, 0.0, 0.0);
                GL.Vertex3(30.0, 0.0, 0.0);

                GL.Color3(Color.FromArgb(0, 0, 0));
                GL.Vertex3(0.0, 30.0, 0.0);
                GL.Vertex3(0.0, -30.0, 0.0);

                GL.Color3(Color.FromArgb(0, 0, 50));
                GL.Vertex3(0.0, 0.0, 30.0);
                GL.Vertex3(0.0, 0.0, -30.0);

                GL.End();
                glControl1.SwapBuffers();

           
        }


        private void silindir(float step, float topla, float radius, float dikey1, float dikey2)
        {
            float eski_step = 0.1f;
            GL.Begin(BeginMode.Quads);//Y EKSEN CIZIM DAİRENİN
            while (step <= 360)
            {
                if (step < 90)
                    GL.Color3(Color.FromArgb(26, 102, 102));
                else if (step < 180)
                    GL.Color3(Color.FromArgb(255, 255,255));
                else if (step < 270)
                    GL.Color3(Color.FromArgb(26, 102, 102));
                else if (step < 360)
                    GL.Color3(Color.FromArgb(255, 255,255));
               


                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 2) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 2) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();
            GL.Begin(BeginMode.Lines);
            step = eski_step;
            topla = step;
            while (step <= 180)// UST KAPAK
            {
                if (step < 90)
                    GL.Color3(Color.FromArgb(26,102,102));
                else if (step < 180)
                    GL.Color3(Color.FromArgb(255,255, 255));
                else if (step < 270)
                    GL.Color3(Color.FromArgb(26, 102, 102));
                else if (step < 360)
                    GL.Color3(Color.FromArgb(255, 255, 255));


                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);

                GL.Vertex3(ciz1_x, dikey1, ciz1_y);
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);
                step += topla;
            }
            step = eski_step;
            topla = step;
            while (step <= 180)//ALT KAPAK
            {
                if (step < 90)
                    GL.Color3(Color.FromArgb(26, 102, 102));
                else if (step < 180)
                    GL.Color3(Color.FromArgb(255, 255, 255));
                else if (step < 270)
                    GL.Color3(Color.FromArgb(26, 102, 102));
                else if (step < 360)
                    GL.Color3(Color.FromArgb(255, 255, 255));

                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey2, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();
        }
        


        //////////////////////////YAN BUTONLAR //////////////////////////////////

        private void anasayfa_button_Click(object sender, EventArgs e)
        {
            
           
            kayıt1.Visible = false;
            ayarlar1.Visible= false;
            grafikler1.Visible = false;
            harita1.Visible = false;
            anasayfa_panel.BackColor = Color.FromArgb(224, 224, 224);
            grafikler_panel.BackColor = Color.FromArgb(26, 102, 102);
            gps_panel.BackColor = Color.FromArgb(26, 102, 102);
            kayit_panel.BackColor = Color.FromArgb(26, 102, 102);
           
            ayarlar_panel.BackColor = Color.FromArgb(26, 102, 102);

            anasayfa_button.Enabled = false;
            grafikler_button.Enabled = true;
            gps_button.Enabled = true;
            kayit_button.Enabled = true;

           
            ayarlar_button.Enabled = true;
        }

        private void grafikler_button_Click(object sender, EventArgs e)
        {
          
            
            kayıt1.Visible = false;
            ayarlar1.Visible = false;
            grafikler1.Visible = true;
            harita1.Visible = false;
            anasayfa_panel.BackColor = Color.FromArgb(26, 102, 102);
            grafikler_panel.BackColor = Color.FromArgb(224, 224, 224);
            gps_panel.BackColor = Color.FromArgb(26, 102, 102);
            kayit_panel.BackColor = Color.FromArgb(26, 102, 102);
           
            ayarlar_panel.BackColor = Color.FromArgb(26, 102, 102);

            anasayfa_button.Enabled = true;
            grafikler_button.Enabled = false;
            gps_button.Enabled = true;
            kayit_button.Enabled = true;
           
            ayarlar_button.Enabled = true;
        }

        

        private void gps_button_Click(object sender, EventArgs e)
        {
            
            
            kayıt1.Visible = false;
            ayarlar1.Visible = false;
            grafikler1.Visible = false;
            harita1.Visible = true;
            anasayfa_panel.BackColor = Color.FromArgb(26, 102, 102);
            grafikler_panel.BackColor = Color.FromArgb(26, 102, 102);
            gps_panel.BackColor = Color.FromArgb(224, 224, 224);
            kayit_panel.BackColor = Color.FromArgb(26, 102, 102);
           
            ayarlar_panel.BackColor = Color.FromArgb(26, 102, 102);

            anasayfa_button.Enabled = true;
            grafikler_button.Enabled = true;
            gps_button.Enabled = false;
            kayit_button.Enabled = true;
           
            ayarlar_button.Enabled = true;
        }
        private void kayit_button_Click(object sender, EventArgs e)
        {
            
           
            kayıt1.Visible = true;
            ayarlar1.Visible = false;
            grafikler1.Visible = false;
            harita1.Visible = false;

            anasayfa_panel.BackColor = Color.FromArgb(26, 102, 102);
            grafikler_panel.BackColor = Color.FromArgb(26, 102, 102);
            gps_panel.BackColor = Color.FromArgb(26, 102, 102);
            kayit_panel.BackColor = Color.FromArgb(224, 224, 224); 
                    
            ayarlar_panel.BackColor = Color.FromArgb(26, 102, 102);

            anasayfa_button.Enabled = true;
            grafikler_button.Enabled = true;
            gps_button.Enabled = true;
            kayit_button.Enabled = false;
         
            ayarlar_button.Enabled = true;
        }

        private void telemetri_button_Click(object sender, EventArgs e)
        {
           
           
            kayıt1.Visible = false;
            ayarlar1.Visible = false;
            grafikler1.Visible = false;
            harita1.Visible = false;
            anasayfa_panel.BackColor = Color.FromArgb(26, 102, 102);
            grafikler_panel.BackColor = Color.FromArgb(26, 102, 102);
            gps_panel.BackColor = Color.FromArgb(26, 102, 102);
            kayit_panel.BackColor = Color.FromArgb(26, 102, 102);
           
            ayarlar_panel.BackColor = Color.FromArgb(26, 102, 102);

            anasayfa_button.Enabled = true;
            grafikler_button.Enabled = true;
            gps_button.Enabled = true;
            kayit_button.Enabled = true;
           
            ayarlar_button.Enabled = true;
        }
        private void ayarlar_button_Click(object sender, EventArgs e)
        {
           
            kayıt1.Visible = false;
            ayarlar1.Visible = true;
            grafikler1.Visible = false;
            harita1.Visible = false;
            anasayfa_panel.BackColor = Color.FromArgb(26, 102, 102);
            grafikler_panel.BackColor = Color.FromArgb(26, 102, 102);
            gps_panel.BackColor = Color.FromArgb(26, 102, 102);
            kayit_panel.BackColor = Color.FromArgb(26, 102, 102);
            
            ayarlar_panel.BackColor = Color.FromArgb(224, 224, 224);

            anasayfa_button.Enabled = true;
            grafikler_button.Enabled = true;
            gps_button.Enabled = true;
            kayit_button.Enabled = true;
           
           
            ayarlar_button.Enabled = false;
        }
      
      ////////////////////////////
      
        private void kapatma_button_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            this.FinalVideo.Stop();
            FileWriter.Close();
            this.Close();
            Application.Exit();
        }
    }
}
