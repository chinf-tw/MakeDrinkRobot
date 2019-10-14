using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Runtime.InteropServices;
using TreadingTimer = System.Threading.Timer;

using Microsoft.Kinect;

using Alturos;
using Alturos.Yolo;
using Alturos.Yolo.Model;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;

using PerspectiveTransformSetting;


namespace multimodal
{
    public partial class Form1 : Form
    {
        Class_URcontrol_left URclass_left = new Class_URcontrol_left();
        Class_URcontrol_2g URclass_2g = new Class_URcontrol_2g();


        YoloWrapper gestureWrapper;
        YoloWrapper objectWrapper;

        IEnumerable<YoloItem> items1;
        IEnumerable<YoloItem> items2;

        private  VideoCapture cap = null;
        Bitmap cap_img;
        Mat img1;

        static int OpenCamIndex = 1;
        const int MAXCAM = 2;
        Mat[] t_matrix;
        Form_camSetting cam_set = new Form_camSetting();



        int cx1, cy1, cx2, cy2, cx3, cy3; //杯子x,y
        int bx1, by1, bx2, by2, bx3, by3; //瓶子x,y

        int cw1, cw2, cw3, ch1, ch2, ch3; //杯子width, height
        int bw1, bw2, bw3, bh1, bh2, bh3;  //瓶子width, height
        int brx, bry, bcx, bcy, blx, bly; //瓶子右、中、左 x ,y
        int brw, brh, bcw, bch, blw, blh; //瓶子右、中、左 width, height

        int line = 160; //放置區及瓶子區分隔線

        bool mode_1 = true; //選取飲料(true為未選)
        bool mode_2 = true; //選取糖度(true為未選)
        bool mode_3 = true; //偵測杯子(true為未執行)
        bool mode_4 = true; //飲料倒完為false
        bool cup_detect = false;


        bool cup_1, cup_2, cup_3 = false; // 杯子個數
        bool bottle_1, bottle_2, bottle_3 = false; //瓶子個數
        bool drink_1, drink_2, drink_3 = false; //飲料種類(所選取的飲料類別為true)

        int count_t = 0; //計數
        int count, count1, count2, count3 = 0; //計數
        System.Windows.Forms.Timer timer;

        System.Drawing.PointF bottle_right;
        System.Drawing.PointF bottle_center;
        System.Drawing.PointF bottle_left;
        System.Drawing.PointF cup_one;
        System.Drawing.PointF cup_two;
        System.Drawing.PointF cup_three;

        double Obj_X;
        double Obj_Y;
        double[] Obj_Pos = new double[2];




        private void button2_Click(object sender, EventArgs e)
        {
            URclass_2g.ServerOn_2g("192.168.1.102", 21);
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        

        private void button_settingFrom_Click(object sender, EventArgs e)
        {
            Form_PtSetting form_Pt = new Form_PtSetting();
            form_Pt.Show();

        }





        public Form1()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, EventArgs e)
        {
            //URclass_left.ServerOn("192.168.1.102", 22);
            //URclass_2g.ServerOn_2g("192.168.1.102", 21);



            main_run();
        }


        void main_run()
        {
            clear_all();
         //   cam_set.readSettings(MAXCAM);

            //URclass_2g.robotGoPos_2g(new URCoordinate_2g(-0.288, 0.069, 0.322, 1.23, -2.59, -0.83, 0, 0), true);



            read_py();

            gesture_run();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;


            
        //    System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_open);
        }

        void clear_all()
        {
            mode_1 = true;
            mode_2 = true;
            mode_3 = true;
            mode_4 = true;

            cup_1 = false;
            cup_2 = false;
            cup_3 = false;
            bottle_1 = false;
            bottle_2 = false;
            bottle_3 = false;
            drink_1 = false;
            drink_2 = false;
            drink_3 = false;


            count_t = 0;
            count = 0;
            count1 = 0;
            count2 = 0;
            count3 = 0;

        }


        void read_py()
        {
            //實體檔案名稱
           // string fileName = string.Format("username.txt");

            //起一個Process執行Python程式
            Process pyProc = new Process();
            pyProc.EnableRaisingEvents = true;
            pyProc.StartInfo.UseShellExecute = false;
            pyProc.StartInfo.RedirectStandardOutput = true;
            pyProc.StartInfo.FileName = "python";
            pyProc.StartInfo.CreateNoWindow = true;

            string sArguments = @"test_noGUI.py";
            //將必要的參數丟進Python，其中test.py的路徑是放在與此UI執行路徑的同一個資料夾中
            string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + sArguments;// 获得python文件的绝对路径（将文件放在c#的debug文件夹中可以这样操作）
                                                                                                       // string path = @"D:\tensorflow-facenet\test" + sArguments;//(因为我没放debug下，所以直接写的绝对路径,替换掉上面的路径了)
            pyProc.StartInfo.FileName = @"C:\anaconda\python.exe";//没有配环境变量的话，可以像我这样写python.exe的绝对路径。如果配了，直接写"python.exe"即可
            string sArgu = path;
            pyProc.StartInfo.Arguments = sArgu;

            pyProc.Start();
            pyProc.BeginOutputReadLine();
            pyProc.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            Console.ReadLine();
            pyProc.WaitForExit();
            pyProc.Close();
        //    string user = System.IO.File.ReadAllText(fileName);
            //  File.Delete(fileName);
        }

        static void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                AppendText(e.Data + Environment.NewLine);
            }
        }

        public static void AppendText(string text)
        {
            Console.WriteLine(text);     //此处在控制台输出.py文件print的结果

        }

        void Show_capture_open(object sender, EventArgs e)
        {
            textBox2.Text = "Please place the number of cups you want.";
            textBox5.Text = "(3 cups at most)";
            if (count == 0)
            {
                timer.Enabled = true;
                count++;
            }

            else if (count >= 15)
            {
                timer.Enabled = false;
                cup_detect = true;

                stop_run();

            }
            else
                count++;

        }

        void gesture_run()
        {
          //  System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_open);
            count = 0;
            textBox2.Text = "What kinds of drink do you want?";
            textBox5.Text = "(1: Black Tea 2: Lemon Water 3: Leamon Tea)";
            

            
         //   cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 640);
        //    cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
            cap = new Emgu.CV.VideoCapture(1, VideoCapture.API.DShow);

            gestureWrapper = new YoloWrapper("yolov2-tiny_number_test.cfg", "yolov2-tiny_number_90000.weights", "number.names");
            
            System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);
        }


   

        private void button1_Click(object sender, EventArgs e)
        {
            // URclass_2g.ServerOn_2g("192.168.1.102", 21);

            URclass_2g.robotGoPos_2g(new URCoordinate_2g(-0.288, 0.069, 0.322, 1.23, -2.59, -0.83, 0, 0), true);

            //mode_1 = false;
            //mode_2 = false;
            //  mode_3 = false;

            cap = new Emgu.CV.VideoCapture(0);
            gestureWrapper = new YoloWrapper("yolov2-tiny_number_test.cfg", "yolov2-tiny_number_90000.weights", "number.names");

            System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);


        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        public byte[] BmpToBytes(Bitmap bmp)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            byte[] b = ms.GetBuffer();
            return b;
        }

        void stop_run()
        {
            count_t = 0;
            count1 = 0;
            count2 = 0;
            count3 = 0;

            count = 0;

            if (mode_1 == true)
            {
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_g);
                textBox2.Text = "Do you need sugar?";
                textBox5.Text = "(1: Yes 2: No)";
                mode_1 = false;

                System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);
            }
            else if (mode_2 == true)
            {
                if (cup_detect == false)
                {
                    System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_g);
                    System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_open);
                }

                else
                {
                    System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_open);
                    textBox2.Text = "Detect the cups";
                    textBox5.Text = "";
                    mode_2 = false;
                    objectWrapper = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");

                    System.Windows.Forms.Application.Idle += new EventHandler(Show_capture);
                }
                

            }

            else if (mode_3 == true)
            {
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture);
                textBox2.Text = "Prepare your drink. Please wait a moment";
                mode_3 = false;
                cam_set.readSettings(MAXCAM);

                if (cup_1 == true)
                {
                    var c1x = cx1 + cw1 / 2;
                    var c1y = cy1 + ch1 / 2;

                    cup_one = cam_set.I2W(c1x, c1y, OpenCamIndex);

                    textBox2.Text = "Test";
                   // URclass_2g.robotGoPos_2g(new URCoordinate_2g(cup_one.Y, 0.250, cup_one.X, 2.2, -2.2, 0, 0, 0), true);



                }

                     System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_b);

            }
            else if (mode_4 == true)
            {
                cam_set.readSettings(MAXCAM);


                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_b);

                var prx = brx + brw / 2;
                var pry = bry + brh / 2;



                bottle_right = cam_set.I2W(prx, pry, OpenCamIndex);  //轉正後的世界座標


                var pcx = bcx + bcw / 2;
                var pcy = bcy + bch / 2;

                bottle_center = cam_set.I2W(pcx, pcy, OpenCamIndex);   //轉正後的世界座標


                mode_4 = false;

                if (drink_1 == true)
                {

                    textBox2.Text = "Black Tea";
                }
                if (drink_2 == true)
                {
                    textBox2.Text = "Lemon Water";
                }
                if (drink_3 == true)
                {
                    textBox2.Text = "Leamon Tea";
                }
                else
                {
                    textBox2.Text = "Test";
                    URclass_2g.robotGoPos_2g(new URCoordinate_2g(bottle_right.Y, 0.250, bottle_right.X, 2.2, -2.2, 0, 0, 0), true);
                }
            }


            else
            {
                var plx = blx + blw / 2;
                var ply = bly + blh / 2;

                bottle_left = cam_set.I2W(plx, ply, OpenCamIndex);   //轉正後的點
            }
        }


        void Show_capture_g(object sender, EventArgs e)
        {
            cap_img = cap.QueryFrame().Bitmap;

            System.Drawing.Rectangle rect1;

            Image<Bgr, Byte> imageCV1 = new Image<Bgr, byte>(cap_img);
            Mat img1 = imageCV1.Mat;


            imageBox1.SizeMode = PictureBoxSizeMode.Zoom;

            byte[] img2 = BmpToBytes(cap_img);
            items1 = gestureWrapper.Detect(img2);
            cap_img.Dispose();
            cap_img = null;

            if (mode_1 == true)
            {
                if (count1 >= 5 || count2 >= 5 || count3 >= 5)
                {
                    if (count1 >= 5)
                    {
                        textBox1.Text = "Black Tea";
                        drink_1 = true;
                    }
                    else if (count2 >= 5)
                    {
                        textBox1.Text = "Lemon Water";
                        drink_2 = true;
                    }
                    else if (count3 >= 5)
                    {
                        textBox1.Text = "Lemon Tea";
                        drink_3 = true;
                    }


                    if (count == 0)
                    {
                        timer.Enabled = true;
                        count++;
                    }

                    else if (count >= 5)
                    {
                        timer.Enabled = false;
                        stop_run();
                    }

                    else
                        count++;

                }
                else
                { 
                    foreach (YoloItem item in items1)
                    {
                        int confidence1 = (int)(item.Confidence * 100);
                        rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                        CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                        CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));

                        if (item.Type == "one")
                            count1++;
                        else if (item.Type == "two")
                            count2++;
                        else if (item.Type == "three")
                            count3++;

                    }
                }
            }

            else
            {
                if (count1 >= 5 || count2 >= 5)
                {
                    if (count1 >= 5)
                        textBox4.Text = "Yes";
                    else if (count2 >= 5)
                        textBox4.Text = "No";



                    if (count == 0)
                    {
                        timer.Enabled = true;
                        count++;
                    }

                    else if (count >= 5)
                    {
                        timer.Enabled = false;
                        stop_run();
                    }

                    else
                        count++;

                }
                else
                {
                    foreach (YoloItem item in items1)
                    {
                        int confidence1 = (int)(item.Confidence * 100);
                        rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                        CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                        CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));

                        if (item.Type == "one")
                            count1++;
                        else if (item.Type == "two")
                            count2++;


                    }
                }
            }

            imageBox1.Image = img1;
            
        }




    


        void Show_capture(object sender, EventArgs e)
        {
            cap_img = cap.QueryFrame().Bitmap;
           
            System.Drawing.Rectangle rect1;

            Image<Bgr, Byte> imageCV1 = new Image<Bgr, byte>(cap_img);
            Mat img1 = imageCV1.Mat;
            

            imageBox1.SizeMode = PictureBoxSizeMode.Zoom;

            byte[] img2 = BmpToBytes(cap_img);
            items2 = objectWrapper.Detect(img2);
            cap_img.Dispose();
            cap_img = null;

            if (count_t<8)
            {

                foreach (YoloItem item in items2)
                {
                    int ry = item.Y + item.Height / 2;

                    if (ry > line)
                    {


                        if (item.Type == "bowl" || item.Type == "cup" || item.Type == "mouse")
                        {
                            int confidence1 = (int)(item.Confidence * 100);
                            rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                            CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                            CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));

                            if (count == 0)
                            {
                                cx1 = item.X;
                                cy1 = item.Y;
                                cw1 = item.Width;
                                ch1 = item.Height;
                                count++;
                                cup_1 = true;
                            }
                            else if (count == 1)
                            {
                                cx2 = item.X;
                                cy2 = item.Y;
                                cw2 = item.Width;
                                ch2 = item.Height;
                                if ((cx1 + 20 > cx2 && cx2 > cx1 - 20) && (cy1 + 20 > cy2 && cy2 > cy1 - 20))
                                {
                                    cx1 = cx2;
                                    cy1 = cy2;
                                    cw1 = cw2;
                                    ch1 = ch2;
                                }
                                else
                                {
                                    count++;
                                    cup_2 = true;
                                }
                            }
                            else if (count == 2)
                            {
                                cx3 = item.X;
                                cy3 = item.Y;
                                cw3 = item.Width;
                                ch3 = item.Height;
                                if ((cx1 + 20 > cx3 && cx3 > cx1 -20) && (cy1 + 20 > cy3 && cy3 > cy1 - 20))
                                {
                                    cx1 = cx3;
                                    cy1 = cy3;
                                    cw1 = cw3;
                                    ch1 = ch3;
                                }

                                else if ((cx2 + 20 > cx3 && cx3 > cx2 - 20) && (cy2 + 20 > cy3 && cy3 > cy2 - 20))
                                {
                                    cx2 = cx3;
                                    cy2 = cy3;
                                    cw2 = cw3;
                                    ch2 = ch3;
                                }
                                else
                                {
                                    count++;
                                    cup_3 = true;
                                }
                            }

                            count_t++;

                        }
                    }
                    
                  
                }

            }
            else
            {
                textBox7.Text = count.ToString();
                stop_run();
            }



            imageBox1.Image = img1;
        }
        void Show_capture_b(object sender, EventArgs e)
        {
            cap_img = cap.QueryFrame().Bitmap;
            Image<Bgr, Byte> imageCV1 = new Image<Bgr, byte>(cap_img);
            Mat img1 = imageCV1.Mat;
            System.Drawing.Rectangle rect1;

            imageBox1.SizeMode = PictureBoxSizeMode.Zoom;

            byte[] img2 = BmpToBytes(cap_img);
            items2 = objectWrapper.Detect(img2);
            cap_img.Dispose();
            cap_img = null;

             if (count<3)
            {
                foreach (YoloItem item in items2)
                {
                    int ry = item.Y + item.Height / 2;

                    if (ry < line)
                    {
                        if (item.Type == "bottle" || item.Type == "cup")
                        {
                            int confidence1 = (int)(item.Confidence * 100);
                            rect1 = new System.Drawing.Rectangle(item.X, item.Y, item.Width, item.Height);
                            CvInvoke.Rectangle(img1, rect1, new MCvScalar(65, 105, 255, 255), 3);
                            CvInvoke.PutText(img1, item.Type + "  " + confidence1, new System.Drawing.Point(item.X, item.Y - 10), 0, 0.5, new MCvScalar(65, 105, 255, 255));
                            if (count == 0)
                            {
                                bx1 = item.X;
                                by1 = item.Y;
                                bw1 = item.Width;
                                bh1 = item.Height;

                                count++;
                                bottle_1 = true;
                            }
                            else if (count == 1)
                            {
                                bx2 = item.X;
                                by2 = item.Y;
                                bw2 = item.Width;
                                bh2 = item.Height;

                                if ((bx1 + 20 > bx2 && bx2 > bx1 - 20) && (by1 + 20 > by2 && by2 > by1 - 20))
                                {
                                    bx1 = bx2;
                                    by1 = by2;
                                    bw1 = bw2;
                                    bh1 = bh2;
                                }
                                else
                                {
                                    count++;
                                    bottle_2 = true;
                                }
                            }
                            else if (count == 2)
                            {
                                bx3 = item.X;
                                by3 = item.Y;
                                bw3 = item.Width;
                                bh3 = item.Height;

                                if ((bx1 + 20 > bx3 && bx3 > bx1 - 20) && (by1 + 20 > by3 && by3 > by1 - 20))
                                {
                                    bx1 = bx3;
                                    by1 = by3;
                                    bw1 = bw3;
                                    bh1 = bh3;

                                }

                                else if ((bx2 + 20 > bx3 && bx3 > bx2 - 20) && (by2 + 20 > by3 && by3 > by2 - 20))
                                {
                                    bx2 = bx3;
                                    by2 = by3;
                                    bw2 = bw3;
                                    bh2 = bh3;
                                }
                                else
                                {
                                    count++;
                                    bottle_3 = true;
                                }
                            }

                        }
                    }

                }
            }

            else
            {
                textBox2.Text = "Done";
                int[] array = new int[] { by1, by2, by3 };
                var max = array.Max();
                var min = array.Min();

                if (max == by1)
                {
                    brx = bx1;
                    bry = by1;
                    brw = bw1;
                    brh = bh1;

                    if (min == by2)
                    {
                        blx = bx2;
                        bly = by2;
                        blw = bw2;
                        blh = bh2;

                        bcx = bx3;
                        bcy = by3;
                        bcw = bw3;
                        bch = bh3;
                    }
                    else
                    {
                        blx = bx3;
                        bly = by3;
                        blw = bw3;
                        blh = bh3;

                        bcx = bx2;
                        bcy = by2;
                        bcw = bw2;
                        bch = bh2;
                    }
                }
                else if (max == by2)
                {
                    brx = bx2;
                    bry = by2;
                    brw = bw2;
                    brh = bh2;
                    if (min == by1)
                    {
                        blx = bx1;
                        bly = by1;
                        blw = bw1;
                        blh = bh1;

                        bcx = bx3;
                        bcy = by3;
                        bcw = bw3;
                        bch = bh3;
                    }
                    else
                    {
                        blx = bx3;
                        bly = by3;
                        blw = bw3;
                        blh = bh3;

                        bcx = bx1;
                        bcy = by1;
                        bcw = bw1;
                        bch = bh1;
                    }

                }
                else
                {
                    brx = bx3;
                    bry = by3;
                    brw = bw3;
                    brh = bh3;

                    if (min == by1)
                    {
                        blx = bx1;
                        bly = by1;
                        blw = bw1;
                        blh = bh1;

                        bcx = bx2;
                        bcy = by2;
                        bcw = bw2;
                        bch = bh2;
                    }
                    else
                    {
                        blx = bx2;
                        bly = by2;
                        blw = bw2;
                        blh = bh2;

                        bcx = bx1;
                        bcy = by1;
                        bcw = bw1;
                        bch = bh1;
                    }
                }




                stop_run();
            }

            


            imageBox1.Image = img1;
        }
    }
}
