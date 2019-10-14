using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace multimodal
{
    struct URCoordinate_2g
    {
        public double X;
        public double Y;
        public double Z;
        public double Rx;
        public double Ry;
        public double Rz;
        public double Gripper;
        public double Speed;


        public URCoordinate_2g(double _x, double _y, double _z, double _Rx, double _Ry, double _Rz, double _G, double _sp)
        {
            X = _x;
            Y = _y;
            Z = _z;
            Rx = _Rx;
            Ry = _Ry;
            Rz = _Rz;
            Gripper = _G;
            Speed = _sp;

        }
        public string toMsg()
        {
            string msg = "(" + X.ToString() + "," + Y.ToString() + "," + Z.ToString() + "," + Rx.ToString() + "," + Ry.ToString() + "," + Rz.ToString() + "," + Gripper.ToString() + "," + Speed.ToString() + ")";
            return msg;
        }

    }
    enum state_2g
    {
        unconnect = 0,
        connect = 1,
        onPos = 2,
        readyToGo = 3,
        moving = 4
    }


    class Class_URcontrol_2g
    {
        private TcpListener tcpListener;
        private IPAddress IPAddress;
        private TcpClient UR_Client_2g;
        private NetworkStream stream;
        private string sMsg_robotToPos_2g = string.Empty;
        private string sMsg_robotNowPos_2g = string.Empty;
        private bool fServerOn_2g = false;
        private state Robot_state_2g = 0;
        private URCoordinate_2g goPos_2g = new URCoordinate_2g();


        //thread
        private void theServer_2g()
        {
            Console.WriteLine("in the server");
            UR_Client_2g = tcpListener.AcceptTcpClient();
            stream = UR_Client_2g.GetStream();
            if (UR_Client_2g.Client.Connected)
            {
                Console.WriteLine("connect");
                OnConnect_2g(EventArgs.Empty);
                Robot_state_2g = state.connect;
                while (fServerOn_2g)
                {
                    byte[] arrayBytesRequest = new byte[UR_Client_2g.Available];
                    int nRead = stream.Read(arrayBytesRequest, 0, arrayBytesRequest.Length);//讀取UR手臂的數值
                    if (nRead > 0)//確認有收到東西後
                    {
                        Robot_state_2g = state.onPos;
                        sMsg_robotNowPos_2g = ASCIIEncoding.ASCII.GetString(arrayBytesRequest);
                        UR3OnPosArgs_2g args_2g = new UR3OnPosArgs_2g();
                        args_2g.sMsg_RobotPos_2g = sMsg_robotNowPos_2g;
                        OnWaitCommand_2g(args_2g);
                        while (Robot_state_2g != state.readyToGo)//等待"可移動"的狀態
                        {
                            Thread.Sleep(50);
                            if (fServerOn_2g == false)
                                break;//如果中途關掉...就退出
                        }
                        Thread.Sleep(10);
                        Console.WriteLine("go moving");
                        OnMoving_2g(EventArgs.Empty);
                        Robot_state_2g = state.moving;
                        sMsg_robotToPos_2g = goPos_2g.toMsg();
                        byte[] arrayBytesAnswer = ASCIIEncoding.ASCII.GetBytes(sMsg_robotToPos_2g);
                        stream.Write(arrayBytesAnswer, 0, arrayBytesAnswer.Length);
                    }//read>0 //所以如果沒有收到的話就繼續再收一次
                }//while 
                OnServerClose_2g(EventArgs.Empty);
                stream.Close();
                Robot_state_2g = state.unconnect;
            }
        }
        public void ServerOn_2g(string IP, int port)
        {
            if (fServerOn_2g == false)//確認沒有開啟server，不然會重複開執行續
            {
                try
                {
                    IPAddress = IPAddress.Parse(IP);
                    tcpListener = new TcpListener(IPAddress, port);
                    tcpListener.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    MessageBox.Show(ex.ToString());
                    return;
                }

                OnServerOn_2g(EventArgs.Empty);
                //button_ServerOn.Text = "Server On";
                fServerOn_2g = true;
                Thread thread_server_2g;
                thread_server_2g = new Thread(() => theServer_2g());
                thread_server_2g.Start();
            }


        }
        public void ServerClose_2g()
        {
            if (fServerOn_2g == true)
            {
                fServerOn_2g = false;
            }
            else
            {
                MessageBox.Show("already close");
            }
        }

        public bool robotGoPos_2g(URCoordinate_2g pos, bool waitMoving = false)
        {
            if (Robot_state_2g == state.onPos && fServerOn_2g == true)
            {
                goPos_2g = pos;
                Robot_state_2g = state.readyToGo;
                if (waitMoving == true)
                    while (Robot_state_2g != state.onPos)
                        Application.DoEvents();//等到手臂狀態是"on pos"為止
                return true;
            }
            else
            {
                return false;
            }

        }


        //event
        public event EventHandler UR3Handler_serverOn_2g;
        public event EventHandler UR3Handler_serverClose_2g;
        public event EventHandler UR3Handler_connect_2g;
        /// <summary>代表已經在Pos上，並且等待命令(注意!! Args是帶參數的)</summary>
        public event UR3OnPosHandler_2g UR3Handler_WaitCommand_2g;
        /// <summary>代表接到命令，正要與控制器溝通</summary>
        public event EventHandler UR3Handler_Moving_2g;
        protected virtual void OnServerOn_2g(EventArgs e)
        {
            EventHandler handler = UR3Handler_serverOn_2g;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnServerClose_2g(EventArgs e)
        {
            EventHandler handler = UR3Handler_serverClose_2g;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnConnect_2g(EventArgs e)
        {
            EventHandler handler = UR3Handler_connect_2g;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnWaitCommand_2g(UR3OnPosArgs_2g e)
        {
            UR3OnPosHandler_2g handler = UR3Handler_WaitCommand_2g;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnMoving_2g(EventArgs e)
        {
            EventHandler handler = UR3Handler_Moving_2g;
            if (handler != null)
                handler(this, e);
        }
    }//class ur3

    public delegate void UR3OnPosHandler_2g(object sender, UR3OnPosArgs_2g e);
    public class UR3OnPosArgs_2g : EventArgs
    {
        public string sMsg_RobotPos_2g { get; set; }
    }
}

