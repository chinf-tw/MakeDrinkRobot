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


    struct URCoordinate
    {
        public double X;
        public double Y;
        public double Z;
        public double Rx;
        public double Ry;
        public double Rz;
        public double Gripper;
        public double Speed;

        public URCoordinate(double _x, double _y, double _z, double _Rx, double _Ry, double _Rz, double _G, double _sp)
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
    enum state
    {
        unconnect = 0,
        connect = 1,
        onPos = 2,
        readyToGo = 3,
        moving = 4
    }

    class Class_URcontrol_left
    {
        private TcpListener tcpListener;
        private IPAddress IPAddress;
        private TcpClient UR_Client;
        private NetworkStream stream;
        private string sMsg_robotToPos = string.Empty;
        private string sMsg_robotNowPos = string.Empty;
        private bool fServerOn = false;
        private state Robot_state = 0;
        private URCoordinate goPos = new URCoordinate();

        public Class_URcontrol_left()
        {

        }
        //thread
        private void theServer()
        {
            Console.WriteLine("in the server");
            UR_Client = tcpListener.AcceptTcpClient();
            stream = UR_Client.GetStream();
            if (UR_Client.Client.Connected)
            {
                Console.WriteLine("connect");
                OnConnect(EventArgs.Empty);
                Robot_state = state.connect;
                while (fServerOn)
                {
                    byte[] arrayBytesRequest = new byte[UR_Client.Available];
                    int nRead = stream.Read(arrayBytesRequest, 0, arrayBytesRequest.Length);//讀取UR手臂的數值
                    if (nRead > 0)//確認有收到東西後
                    {
                        Robot_state = state.onPos;
                        sMsg_robotNowPos = ASCIIEncoding.ASCII.GetString(arrayBytesRequest);
                        UR3OnPosArgs args = new UR3OnPosArgs();
                        args.sMsg_RobotPos = sMsg_robotNowPos;
                        OnWaitCommand(args);
                        while (Robot_state != state.readyToGo)//等待"可移動"的狀態
                        {
                            Thread.Sleep(50);
                            if (fServerOn == false)
                                break;//如果中途關掉...就退出
                        }
                        Thread.Sleep(10);
                        Console.WriteLine("go moving");
                        OnMoving(EventArgs.Empty);
                        Robot_state = state.moving;
                        sMsg_robotToPos = goPos.toMsg();
                        byte[] arrayBytesAnswer = ASCIIEncoding.ASCII.GetBytes(sMsg_robotToPos);
                        stream.Write(arrayBytesAnswer, 0, arrayBytesAnswer.Length);
                    }//read>0 //所以如果沒有收到的話就繼續再收一次
                }//while 
                OnServerClose(EventArgs.Empty);
                stream.Close();
                Robot_state = state.unconnect;
            }
        }
        public void ServerOn(string IP, int port)
        {
            if (fServerOn == false)//確認沒有開啟server，不然會重複開執行續
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

                OnServerOn(EventArgs.Empty);
                //button_ServerOn.Text = "Server On";
                fServerOn = true;
                Thread thread_server;
                thread_server = new Thread(() => theServer());
                thread_server.Start();
            }


        }
        public void ServerClose()
        {
            if (fServerOn == true)
            {
                fServerOn = false;
            }
            else
            {
                MessageBox.Show("already close");
            }
        }

        public bool robotGoPos(URCoordinate pos, bool waitMoving = false)
        {
            if (Robot_state == state.onPos && fServerOn == true)
            {
                goPos = pos;
                Robot_state = state.readyToGo;
                if (waitMoving == true)
                    while (Robot_state != state.onPos)
                        Application.DoEvents();//等到手臂狀態是"on pos"為止
                return true;
            }
            else
            {
                return false;
            }

        }


        //event
        public event EventHandler UR3Handler_serverOn;
        public event EventHandler UR3Handler_serverClose;
        public event EventHandler UR3Handler_connect;
        /// <summary>代表已經在Pos上，並且等待命令(注意!! Args是帶參數的)</summary>
        public event UR3OnPosHandler UR3Handler_WaitCommand;
        /// <summary>代表接到命令，正要與控制器溝通</summary>
        public event EventHandler UR3Handler_Moving;
        protected virtual void OnServerOn(EventArgs e)
        {
            EventHandler handler = UR3Handler_serverOn;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnServerClose(EventArgs e)
        {
            EventHandler handler = UR3Handler_serverClose;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnConnect(EventArgs e)
        {
            EventHandler handler = UR3Handler_connect;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnWaitCommand(UR3OnPosArgs e)
        {
            UR3OnPosHandler handler = UR3Handler_WaitCommand;
            if (handler != null)
                handler(this, e);
        }
        protected virtual void OnMoving(EventArgs e)
        {
            EventHandler handler = UR3Handler_Moving;
            if (handler != null)
                handler(this, e);
        }
    }//class ur3

    public delegate void UR3OnPosHandler(object sender, UR3OnPosArgs e);
    public class UR3OnPosArgs : EventArgs
    {
        public string sMsg_RobotPos { get; set; }
    }
}
