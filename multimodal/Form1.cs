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

using dynamixel_sdk;


using Alturos;
using Alturos.Yolo;
using Alturos.Yolo.Model;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;

using PerspectiveTransformSetting;
using URControler2;



namespace multimodal
{
    public partial class Form1 : Form
    {

        URServerAction uRServerAction_left;
        URServerAction uRServerAction_right;


        // parameter ///////
        float[] robot_initial_pos_r = { -0.288F, 0.069F, 0.322F, 1.23F, -2.59F, -0.83F };
       // float[] robot_initial_pos_l2 = { 0.395F, 0.030F, 0.172F, 0.86F, 3.19F, 0.76F };
        float[] robot_initial_pos_l = { 0.395F, 0.030F, 0.172F, 0.86F, 3.19F, 0.76F };
        float[] robot_initial_pos_rc = { -0.337F, 0.110F, -0.123F, 2.174F, -2.233F, 0.002F };
        float[] robot_initial_pos_lc = { 0.335F, -0.100F, -0.153F, 2.447F, 2.367F, 2.475F };


        float image_right_x = -0.065F;
        float image_right_y = 0.01F;

        float image_left_x = 0.05F;
        float image_left_y = 0.04F;

        ///////////////////////////////////////////////

        SocketTool mipy_socket;
        MxMotor motor1 = new MxMotor();
        int test;
        int initial_pos = 2020;
        int obj_pos = 1350;


        YoloWrapper gestureWrapper;
        YoloWrapper objectWrapper;

        IEnumerable<YoloItem> items1;
        IEnumerable<YoloItem> items2;

        private  VideoCapture cap = null;
        private VideoCapture cap2 = null;
        Bitmap cap_img;
        Mat img1;

        static int OpenCamIndex_right = 0;
        static int OpenCamIndex_right_top = 2;
        static int OpenCamIndex_left = 1;
        static int OpenCamIndex_left_top = 3;


        const int MAXCAM = 2;
        Mat[] t_matrix;
        Form_PtSetting cam_set_right = new Form_PtSetting();
        Form_PtSetting cam_set_left = new Form_PtSetting();



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

        bool move_1 = false;
        bool move_2 = false;
        bool move_3 = false;

        bool mipy = false;


        bool cup_1, cup_2, cup_3 = false; // 杯子個數
        bool bottle_1, bottle_2, bottle_3 = false; //瓶子個數
        bool drink_1, drink_2, drink_3, drink_4 = false; //飲料種類(所選取的飲料類別為true)

        int count_t = 0; //計數
        int count, count1, count2, count3, count4 = 0; //計數

        string user_name;

        System.Windows.Forms.Timer timer;

        float[] bottle_right_r = new float[] { 0, 0 };
        float[] bottle_right_l = new float[] { 0, 0 };
        //float[] bottle_right = new float[] { 0, 0 };
        float[] bottle_center_r = new float[] { 0, 0 };
        float[] bottle_center_l = new float[] { 0, 0 };
        float[]  bottle_left_r = new float[] { 0, 0 };
        float[] bottle_left_l = new float[] { 0, 0 };
        float[] cup_one_r = new float[] { 0,0};
        float[] cup_one_l = new float[] { 0, 0 };
        float[] cup_two = new float[] { 0, 0 };
        float[] cup_three = new float[] { 0, 0 };

        //float[] bottle_right_f;
        //float[] bottle_left_f;
        float[] cup_one_rf;
        float[] cup_two_rf;
        float[] cup_three_rf;

        float[] cup_one_lf;
        float[] cup_two_lf;
        float[] cup_three_lf;


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (Mipy.Checked)
            {
                mipy = true;
            }
            else
            {
                mipy = false;
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            int port_r = 1100;
            string addr = "0.0.0.0";
            ListenerBaseTcpListener listener_r = new ListenerBaseTcpListener(port_r, addr);
            Thread t_r = new Thread(new ThreadStart(listener_r.RunServer));
            t_r.Start();
            var stream_r = listener_r.GetNetworkStream();

            if (stream_r == null)
            {
                Console.Write("Waiting for a connection... ");
            }
            while (stream_r == null)
            {
                stream_r = listener_r.GetNetworkStream();
            }
            Console.WriteLine("Connected!");
            uRServerAction_right = new URServerAction(stream_r);



            ///////////////////////////////////////////////////////////////////////////////////////////

            int port_l = 1101;
            string addr_l = "0.0.0.0";
            ListenerBaseTcpListener listener_l = new ListenerBaseTcpListener(port_l, addr_l);
            Thread t_l = new Thread(new ThreadStart(listener_l.RunServer));
            t_l.Start();
            var stream_l = listener_l.GetNetworkStream();

            if (stream_l == null)
            {
                Console.Write("Waiting for a connection... ");
            }
            while (stream_l == null)
            {
                stream_l = listener_l.GetNetworkStream();
            }
            Console.WriteLine("Connected!");
            uRServerAction_left = new URServerAction(stream_l);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            uRServerAction_left.Move(robot_initial_pos_lc);
            uRServerAction_left.TurnJoint(4, -15, 2);
            Thread.Sleep(2000);

            uRServerAction_left.Move(robot_initial_pos_lc);

            uRServerAction_left.GripperCloseForceMIN();

            robot_initial_pos_lc[1] -= 0.020F;

            uRServerAction_left.Move(robot_initial_pos_lc);

            Thread.Sleep(2000);

            robot_initial_pos_lc[1] += 0.020F;
            uRServerAction_left.Move(robot_initial_pos_lc);
            uRServerAction_left.GripperOpen();

            Thread.Sleep(2000);

            robot_initial_pos_lc[1] -= 0.01F;
            uRServerAction_left.Move(robot_initial_pos_lc);

            uRServerAction_left.TurnJoint(-4, 25, 2);

            Thread.Sleep(5000);
            robot_initial_pos_rc[1] += 0.02F;
            uRServerAction_right.Move(robot_initial_pos_rc);
            //robot_initial_pos_lc[1] -= 0.02F;
            //uRServerAction_left.Move(robot_initial_pos_lc);
            Thread.Sleep(2000);

            uRServerAction_left.Move(robot_initial_pos_l);


            uRServerAction_right.Move(robot_initial_pos_r);

        }



        private void button_settingFrom_Click(object sender, EventArgs e)
        {
            //motor1 = new MxMotor();

            test = motor1.ReadMxPosition();

            motor1.MxMotorSetPosition(initial_pos);
            System.Threading.Thread.Sleep(1000);
            motor1.MxMotorSetPosition(obj_pos);

            Form_PtSetting form_Pt = new Form_PtSetting();
            form_Pt.Show();

        }





        public Form1()
        {
            InitializeComponent();
            
        }

        private void Start_Click(object sender, EventArgs e)
        {

            //motor1 = new MxMotor();                    
            main_run();
        }


        void main_run()
        {
            clear_all();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;

            motor1.MxMotorSetPosition(initial_pos);
            //uRServerAction_right.Move(new float[] { -0.288F, 0.069F, 0.322F, 1.23F, -2.59F, -0.83F });
            //   uRServerAction_left.Move(new float[] { 0.418F, -0.120F, 0.216F, 2.5F, 2.28F, 0.35F });
            
            if (Mipy.Checked)
            {
                cap2 = new Emgu.CV.VideoCapture(2, VideoCapture.API.DShow);

                mipy_socket = new SocketTool();
                mipy_socket.creatClient("127.0.0.1", 8888);
                Application.Idle += new EventHandler(Mipy_read_user); 
                         
            }
            else
            {
                read_py();
                stop_Face();
            }

           

           
        }


        void Mipy_read_user(object sender, EventArgs e)
        {

            mipy_socket.client_SendData("FaceNameRequest");
            Bitmap cap_mipy = cap2.QueryFrame().Bitmap;
            Image<Bgr, Byte> image_mipy = new Image<Bgr, byte>(cap_mipy);
            Mat img_mipy = image_mipy.Mat;
            imageBox1.Image= img_mipy;



            user_name = mipy_socket.client_ReadData();
            if (user_name != "None" && user_name != "Stranger!" && user_name != "")
            {
                stop_Face();
            }

            

        }

        void stop_Face()
        {
            if (Mipy.Checked)
            {
                Application.Idle -= new EventHandler(Mipy_read_user);
                textBox4.Text = user_name;
            }
            

            gesture_run();
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
            drink_4 = false;

            move_1 = false;
            move_2 = false;
            move_3 = false;


            count_t = 0;
            count = 0;
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;




        }


        void read_py()
        {
            //實體檔案名稱
            string fileName = "username.txt";

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
            string user = System.IO.File.ReadAllText(fileName);
            textBox4.Text = user;
            //File.Delete(fileName);
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
            textBox2.Text = "請放置杯子";
            textBox5.Text = "";
            if (count == 0)
            {
                timer.Enabled = true;
                count++;
            }

            else if (count >= 3)
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
            textBox2.Text = "請問您想喝哪種飲料?";
            textBox5.Text = "(1): 左邊瓶子 (2): 右邊瓶子 (3): 左+右邊瓶子";



            //   cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, 640);
            //    cap.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);

            cap = new Emgu.CV.VideoCapture(0, VideoCapture.API.DShow);
            // = new VideoCapture(1);

            gestureWrapper = new YoloWrapper("yolov2-tiny_number_test.cfg", "yolov2-tiny_number_90000.weights", "number.names");
            
            System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);
        }


   

        private void button1_Click(object sender, EventArgs e)
        {
            //uRServerAction_right.Move(new float[] { -0.288F, 0.069F, 0.322F, 1.23F, -2.59F, -0.83F });
            //uRServerAction_left.Move(new float[] { 0.418F, -0.120F, 0.216F, 2.5F, 2.28F, 0.35F });

            clear_all();

            mode_1 = false;
            mode_2 = false;
            //mode_3 = false;

            drink_2 = true;

            cap = new Emgu.CV.VideoCapture(0, VideoCapture.API.DShow);
            //gestureWrapper = new YoloWrapper("yolov2-tiny_number_test.cfg", "yolov2-tiny_number_90000.weights", "number.names");

            //System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);
            objectWrapper = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");

            System.Windows.Forms.Application.Idle += new EventHandler(Show_capture);

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
                //textBox2.Text = "Do you need sugar?";
                //textBox5.Text = "(1: Yes 2: No)";
                mode_1 = false;
                motor1.MxMotorSetPosition(obj_pos);
                System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_open);
                //System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_g);

            }
            else if (mode_2 == true)
            {

                    System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_open);
                    textBox2.Text = "偵測杯子中";
                    textBox5.Text = "";
                    mode_2 = false;
                    objectWrapper = new YoloWrapper("yolov3.cfg", "yolov3.weights", "coco.names");

                    System.Windows.Forms.Application.Idle += new EventHandler(Show_capture);
                

            }

            else if (mode_3 == true)
            {
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture);
                textBox2.Text = "準備您的飲料中，請稍後~";
                mode_3 = false;
                

                if (cup_1 == true)
                {
                    var c1x = cx1 + cw1 / 2;
                    var c1y = cy1 + ch1;

                    cup_one_rf = cam_set_right.I2W(c1x, c1y, OpenCamIndex_right,true);
                    cup_one_r[0] = cup_one_rf[0] / 1000;
                    cup_one_r[1] = cup_one_rf[2] / 1000;

                    cup_one_lf = cam_set_left.I2W(c1x, c1y, OpenCamIndex_left, true);
                    cup_one_l[0] = cup_one_lf[0] / 1000;
                    cup_one_l[1] = cup_one_lf[2] / 1000;


                }

                     System.Windows.Forms.Application.Idle += new EventHandler(Show_capture_b);

            }
            else if (mode_4 == true)
            {              
                System.Windows.Forms.Application.Idle -= new EventHandler(Show_capture_b);
                var prx = brx + brw / 2;
                var pry = bry + brh;

                var plx = blx + blw / 2;
                var ply = bly + blh;

                var pcx = bcx + bcw / 2;
                var pcy = bcy + bch;

                float[] bottle_right_rf = cam_set_right.I2W(prx, pry, OpenCamIndex_right, true);  //轉正後的世界座標

                bottle_right_r[0] = bottle_right_rf[0] / 1000;
                bottle_right_r[1] = bottle_right_rf[2] / 1000;

                float[] bottle_right_lf = cam_set_left.I2W(prx, pry, OpenCamIndex_left, true);

                bottle_right_l[0] = bottle_right_lf[0] / 1000;
                bottle_right_l[1] = bottle_right_lf[2] / 1000;


                float[] bottle_left_rf = cam_set_right.I2W(plx, ply, OpenCamIndex_right, true);  //轉正後的世界座標

                bottle_left_r[0] = bottle_left_rf[0] / 1000;
                bottle_left_r[1] = bottle_left_rf[2] / 1000;

                float[] bottle_left_lf = cam_set_left.I2W(plx, ply, OpenCamIndex_left, true);

                bottle_left_l[0] = bottle_left_lf[0] / 1000;
                bottle_left_l[1] = bottle_left_lf[2] / 1000;



                //float[] bottle_center_rf = cam_set_right.I2W(pcx, pcy, OpenCamIndex_right, true);  //轉正後的世界座標

                //bottle_center_r[0] = bottle_center_rf[0] / 1000;
                //bottle_center_r[1] = bottle_center_rf[2] / 1000;

                //float[] bottle_center_lf = cam_set_left.I2W(pcx, pcy, OpenCamIndex_left, true);

                //bottle_center_l[0] = bottle_center_lf[0] / 1000;
                //bottle_center_l[1] = bottle_center_lf[2] / 1000;

                if (drink_1 == true)
                {
                    textBox2.Text = "左邊瓶子";


                    mode_4 = false;
                    rotate_bottle(bottle_right_r, cup_one_r, bottle_right_l);

                }
                else if (drink_2 == true)
                {
                    textBox2.Text = "右邊瓶子";

                    mode_4 = false;

                    pick_bottle(bottle_left_l, cup_one_l);
                    //open_bottle(bottle_left_r, cup_one_r, cup_one_l, bottle_left_l);

                }
                else if (drink_3 == true)
                {
                    textBox2.Text = "左+右邊瓶子";

                    if (move_1 == false)
                    {
                        rotate_bottle(bottle_right_r, cup_one_r, bottle_right_l);
                    }

                    else if (move_3 == false)
                    {
                        pick_bottle(bottle_left_l, cup_one_l);
                        //open_bottle(bottle_center_r, cup_one_r, cup_one_l, bottle_center_l);
                    }

                    else
                    {
                        mode_4 = false;
                        stop_run();
                    }
                

                    //     mode_4 = false;

                    //     pick_bottle(bottle_left_l,cup_one_l);

                    // }      

                    //else if(drink_4 == true)
                    //{

                    //    textBox2.Text = "中間+右邊瓶子";


                    //    if (move_1 == false)
                    //    {
                    //        rotate_bottle(bottle_right_r, cup_one_r, bottle_right_l);
                    //    }

                    //    else if (move_2 == false)
                    //    {
                    //        open_bottle(bottle_center_r, cup_one_r, cup_one_l, bottle_center_l);
                    //    }

                    //    else
                    //    {
                    //        mode_4 = false;
                    //        stop_run();
                    //    }
                }

            }

            else if (mode_4 == false)
            {               
                textBox2.Text = "您的飲料已經準備好了，請拿取，謝謝~";
              //  pick_cup(cup_one_r,cup_one_l);

                cap.Stop();
                cap.Dispose();

            }

        }


        void Show_capture_g(object sender, EventArgs e)
        {
            while (true)
            {
                Mat img;
                img = cap.QueryFrame();
                if (img != null && img.Width != 0)
                    break;
            }
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
                        textBox1.Text = "左邊瓶子";
                        drink_1 = true;
                    }
                    else if (count2 >= 5)
                    {
                        textBox1.Text = "右邊瓶子";
                        drink_2 = true;
                    }
                    else if (count3 >= 5)
                    {
                        textBox1.Text = "左+右邊瓶子";
                        drink_3 = true;
                    }

                    //else if (count4 >=5)
                    //{
                    //    textBox1.Text = "中間+右邊瓶子";
                    //    drink_4 = true;
                    //}


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
                        if (item.Type == "two")
                            count2++;
                        if (item.Type == "three")
                            count3++;
                        if (item.Type == "four")
                            count4++;

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

            if (count_t<3)
            {

                foreach (YoloItem item in items2)
                {
                    int ry = item.Y + item.Height / 2;

                    if (ry < line)
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

             if (count<2)
            {
                foreach (YoloItem item in items2)
                {
                    int ry = item.Y + item.Height / 2;

                    if (ry > line)
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


                int[] array = new int[] { bx1, bx2 };
                var max = array.Max();
                var min = array.Min();

                if (max == bx1)
                {
                    brx = bx1;
                    bry = by1;
                    brw = bw1;
                    brh = bh1;

                    //if (min == bx2)
                    //{

                        blx = bx2;
                        bly = by2;
                        blw = bw2;
                        blh = bh2;


                        //bcx = bx3;
                        //bcy = by3;
                        //bcw = bw3;
                        //bch = bh3;
                   // }

                    //else
                    //{
                    //    blx = bx3;
                    //    bly = by3;
                    //    blw = bw3;
                    //    blh = bh3;


                    //    bcx = bx2;
                    //    bcy = by2;
                    //    bcw = bw2;
                    //    bch = bh2;
                    //}

                }
                else if (max == bx2)
                {
                    brx = bx2;
                    bry = by2;
                    brw = bw2;
                    brh = bh2;

                    //if (min == bx1)
                    //{
                        blx = bx1;
                        bly = by1;
                        blw = bw1;
                        blh = bh1;

                        //bcx = bx3;
                        //bcy = by3;
                        //bcw = bw3;
                    //    bch = bh3;
                    //}

                    //else
                    //{
                    //    blx = bx3;
                    //    bly = by3;
                    //    blw = bw3;
                    //    blh = bh3;

                    //    bcx = bx1;
                    //    bcy = by1;
                    //    bcw = bw1;
                    //    bch = bh1;
                    //}
                }

                //else if (max == bx3)
                //{

                //    brx = bx3;
                //    bry = by3;
                //    brw = bw3;
                //    brh = bh3;

                //    if (min == bx1)
                //    {
                //        blx = bx1;
                //        bly = by1;
                //        blw = bw1;
                //        blh = bh1;

                //        bcx = bx2;
                //        bcy = by2;
                //        bcw = bw2;
                //        bch = bh2;
                //    }

                //    else
                //    {
                //        blx = bx2;
                //        bly = by2;
                //        blw = bw2;
                //        blh = bh2;

                //        bcx = bx1;
                //        bcy = by1;
                //        bcw = bw1;
                //        bch = bh1;
                //    }
                //}          

                stop_run();
            }

            


            imageBox1.Image = img1;
        }

        void rotate_bottle(float [] bottle_r, float [] cup_r, float [] bottle_l)
        {
            textBox2.Text = "Test";
            if (uRServerAction_right == null)
            {
                throw new DriveNotFoundException("完蛋了。");
            }

            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(robot_initial_pos_r);
            uRServerAction_left.Move(robot_initial_pos_l);

            uRServerAction_left.GripperCloseForceMIN();

            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x, 0.01F, bottle_l[1] + image_left_y - 0.04F, 2.4F, 2.5F, 1.5F });
            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x, 0.04F, bottle_l[1] + image_left_y - 0.04F, 2.4F, 2.5F, 1.5F });

            uRServerAction_left.ForceMode(0, 30);

            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x, 0.250F, bottle_r[1] + image_right_y + 0.02F, 2.174F, -2.233F, 0.002F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x, 0.250F, bottle_r[1] + image_right_y, 2.174F, -2.233F, 0.002F });
            Thread.Sleep(2000);

            uRServerAction_left.EndForceMode();
            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x, 0.01F, bottle_l[1] + image_left_y - 0.04F, 2.4F, 2.5F, 1.5F });
            uRServerAction_left.Move(robot_initial_pos_l);
            uRServerAction_left.GripperOpen();


            uRServerAction_right.GripperCloseForceMAX();

            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x, 0.110F, bottle_r[1] + image_right_y, 2.174F, -2.233F, 0.002F });
            uRServerAction_right.Move(new float[] { robot_initial_pos_rc[0], 0.110F, bottle_r[1] + image_right_y, 2.174F, -2.233F, 0.002F });
            uRServerAction_right.Move(robot_initial_pos_rc);

            Thread.Sleep(2000);
            robot_initial_pos_lc[1] -= 0.02F;
            uRServerAction_left.Move(robot_initial_pos_lc);
            robot_initial_pos_lc[1] += 0.02F;
            uRServerAction_left.Move(robot_initial_pos_lc);
            Thread.Sleep(1000);


            uRServerAction_left.TurnJoint(4, -15, 2);
            Thread.Sleep(2000);

            uRServerAction_left.Move(robot_initial_pos_lc);

            uRServerAction_left.GripperCloseForceMIN();

            robot_initial_pos_lc[1] -= 0.020F;

            uRServerAction_left.Move(robot_initial_pos_lc);

            Thread.Sleep(2000);

            uRServerAction_right.Move(new float[] { cup_r[0], 0.110F, cup_r[1] + 0.040F, 2.26F, -2.27F, -0.06F });
            uRServerAction_right.Move(new float[] { cup_r[0], 0.180F, cup_r[1] + 0.040F, 2.26F, -2.27F, -0.06F });

            uRServerAction_right.MoveJoint(5, 1.75F);
            Thread.Sleep(3000);
            uRServerAction_right.MoveJoint(5, -1.75F);


            uRServerAction_right.Move(robot_initial_pos_rc);
            Thread.Sleep(2000);

            robot_initial_pos_lc[1] += 0.020F;
            uRServerAction_left.Move(robot_initial_pos_lc);
            uRServerAction_left.GripperOpen();

            Thread.Sleep(2000);

            robot_initial_pos_lc[1] -= 0.01F;
            uRServerAction_left.Move(robot_initial_pos_lc);

            uRServerAction_left.TurnJoint(-4, 25, 2);
            
            Thread.Sleep(5000);
            robot_initial_pos_rc[1] += 0.02F;
            uRServerAction_right.Move(robot_initial_pos_rc);
            Thread.Sleep(2000);

            uRServerAction_left.Move(robot_initial_pos_l);

            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x, 0.100F, bottle_r[1] + image_right_y, 2.26F, -2.27F, -0.06F });

            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x, 0.242F, bottle_r[1] + image_right_y, 2.26F, -2.27F, -0.06F });
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x, 0.242F, bottle_r[1] + image_right_y + 0.03F, 2.26F, -2.27F, -0.06F });

            uRServerAction_right.Move(robot_initial_pos_r);
            move_1 = true;
            stop_run();
        }



        void open_bottle(float[] bottle_r, float[] cup_r, float[] cup_l, float[] bottle_l)
        {
            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(robot_initial_pos_r);
            uRServerAction_left.Move(robot_initial_pos_l);


            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x , 0.09F, bottle_r[1] + image_right_y + 0.050F, 2.37F, -2.54F, -1.07F });
            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x , 0.09F, bottle_r[1] + image_right_y + 0.015F, 2.37F, -2.54F, -1.07F });

            uRServerAction_right.GripperCloseForceMAX();
            Thread.Sleep(1000);

            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x , 0.220F, bottle_l[1] + image_left_y + 0.02F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x , 0.220F, bottle_l[1] + image_left_y - 0.01F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperCloseForceMAX();
            Thread.Sleep(2000);

            uRServerAction_right.GripperOpen();

            uRServerAction_right.Move(new float[] { bottle_r[0] + image_right_x - 0.017F, 0.070F, bottle_r[1] + image_right_y + 0.050F, 2.37F, -2.54F, -1.07F });
            uRServerAction_right.Move(robot_initial_pos_r);
            Thread.Sleep(2000);

            uRServerAction_left.Move(new float[] { cup_l[0] + image_left_x - 0.08F, 0.200F, cup_l[1] + image_left_y, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.MoveJoint(5, -1.5F);
            Thread.Sleep(5000);
            uRServerAction_left.MoveJoint(5, 1.5F);



            uRServerAction_right.GripperCloseForceMIN();
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x + 0.180F, 0.165F, cup_r[1] + image_right_y + 0.035F, 2.21F, -2.21F, -0.035F });
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x + 0.180F, 0.105F, cup_r[1] + image_right_y + 0.035F, 2.21F, -2.21F, -0.035F });
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x + 0.100F, 0.105F, cup_r[1] + image_right_y + 0.035F, 2.21F, -2.21F, -0.035F });

            uRServerAction_right.ForceMode(0, -25);
            Thread.Sleep(2000);
            uRServerAction_right.EndForceMode();
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x + 0.100F, 0.105F, cup_r[1] + image_right_y + 0.06F, 2.21F, -2.21F, -0.035F });
            uRServerAction_right.Move(robot_initial_pos_r);
            uRServerAction_right.GripperOpen();

            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x  , 0.220F, bottle_l[1] + image_left_y - 0.01F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperOpen();
            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x  , 0.220F, bottle_l[1] + image_left_y + 0.02F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.Move(robot_initial_pos_l);

            move_2 = true;
            stop_run();

        }



        void pick_bottle(float[] bottle_l, float [] cup_l)
        {
            uRServerAction_left.GripperOpen();
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(robot_initial_pos_r);
            uRServerAction_left.Move(robot_initial_pos_l);

            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x + 0.002F, 0.250F, bottle_l[1] + image_left_y + 0.01F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x + 0.002F, 0.250F, bottle_l[1] + image_left_y - 0.04F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperCloseForceMIN();
            Thread.Sleep(1000);
            uRServerAction_left.Move(new float[] { cup_l[0] + image_left_x - 0.08F, 0.190F, cup_l[1] + image_left_y - 0.016F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.MoveJoint(5, -1.5F);
            if (drink_2 == true)
            {
                Thread.Sleep(700);
            }
            else if (drink_3 == true)
            {
                //  Thread.Sleep(100);
            }

            uRServerAction_left.MoveJoint(5, 1.5F);

            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x + 0.002F, 0.250F, bottle_l[1] + image_left_y - 0.04F, 0.06F, -3.127F, 0.093F });
            uRServerAction_left.GripperOpen();
            uRServerAction_left.Move(new float[] { bottle_l[0] + image_left_x + 0.002F, 0.250F, bottle_l[1] + image_left_y + 0.01F, 0.06F, -3.127F, 0.093F });

            uRServerAction_left.Move(robot_initial_pos_l);

            move_3 = true;
            stop_run();
        }

        void pick_cup(float[] cup_r, float[] cup_l)
        {
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x+0.027F, 0.200F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x+0.027F, 0.275F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_right.GripperCloseForceMIN();

            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x+0.027F, 0.050F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_left.GripperCloseForceMIN();
            uRServerAction_left.Move(new float[] { cup_l[0] + image_left_x - 0.02F, 0.150F, cup_l[1] + image_left_y, 0.071F, 2.94F, -0.067F});
            uRServerAction_left.Move(new float[] { cup_l[0] + image_left_x - 0.02F, 0.135F, cup_l[1] + image_left_y, 0.071F, 2.94F, -0.067F });
            Thread.Sleep(1000);


            uRServerAction_right.Move(new float[] {-0.500F,0.050F,-0.085F,2.1F,-2.1F,-0.38F });
            uRServerAction_left.Move(new float[] { 0.490F, 0.135F, -0.045F, 0.012F, -3.54F, 0.038F });
            Thread.Sleep(3000);

            uRServerAction_left.Move(new float[] { 0.500F, 0.135F, 0.050F, 0.0126F, 2.742F, -0.045F });
            uRServerAction_left.Move(robot_initial_pos_l);
            uRServerAction_left.GripperOpen();
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x+0.027F, 0.275F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            Thread.Sleep(500);
            uRServerAction_right.GripperOpen();
            uRServerAction_right.Move(new float[] { cup_r[0] + image_right_x + 0.027F, 0.2F, cup_r[1] + 0.045F, 2.1F, -2.1F, -0.38F });
            uRServerAction_right.Move(robot_initial_pos_r);


        }
    }


}
