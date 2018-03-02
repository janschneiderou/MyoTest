using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using System.Net.Sockets;
using System.Net;
using System.Windows.Media.Media3D;
using ConnectorHub;

namespace MyoTest.MyoManager
{
    class MyoManagerClass
    {
        IChannel channel;
        public IHub hub;
        MainWindow mWindow;

        

        Socket sending_socket;
        IPAddress send_to_address;
        //assign default values
        private Int32 gripEMG=0;
        private float orientationW=0;
        private float orientationX=0;
        private float orientationY=0;
        private float orientationZ=0;
        private double myoRoll;
        private double myoYaw;
        private double myoPitch;
        int frameNumber = 0;

        Int32[] firstPreEmgValue = new Int32[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        Int32[] secPreEmgValue = new Int32[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public void InitMyoManagerHub(MainWindow m)
        {
            this.mWindow = m;
            channel = Channel.Create(
                ChannelDriver.Create(ChannelBridge.Create(),
                MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            hub = Hub.Create(channel);

            // listen for when the Myo connects
            hub.MyoConnected += (sender, e) =>
            {
                Debug.WriteLine("Myo {0} has connected!", e.Myo.Handle);
                e.Myo.Vibrate(VibrationType.Short);
                e.Myo.EmgDataAcquired += Myo_EmgDataAcquired;
                e.Myo.OrientationDataAcquired += Myo_OrientationAcquired;
                e.Myo.PoseChanged += Myo_PoseChanged;
                e.Myo.SetEmgStreaming(true);
            };

            // listen for when the Myo disconnects
            hub.MyoDisconnected += (sender, e) =>
            {
                Debug.WriteLine("Oh no! It looks like {0} arm Myo has disconnected!", e.Myo.Arm);
                e.Myo.SetEmgStreaming(false);
                e.Myo.EmgDataAcquired -= Myo_EmgDataAcquired;
            };

            // start listening for Myo data
            channel.StartListening();

        }

        private void Myo_PoseChanged(object sender, PoseEventArgs e)
        {
            string grip = e.Pose.ToString();
            mWindow.UpdateGrip(grip);
        }

        private void Myo_EmgDataAcquired(object sender, EmgDataEventArgs e)
        {

            CalculateGripPressure(e);
            if(gripEMG>2)
            {
                if (mWindow.myConnector.startedByHub==true)
                {
                    mWindow.myConnector.sendFeedback(gripEMG.ToString());
                }
               
            }
            if(mWindow.start==true)
            {
                SendDataConnector();
            }
        }

        private void SendDataConnector()
        {
            try
            {
                List<string> values = new List<string>();
                values.Add(gripEMG.ToString());
                values.Add(orientationW.ToString());
                values.Add(orientationX.ToString());
                values.Add(orientationY.ToString());
                values.Add(orientationZ.ToString());
                values.Add(myoRoll.ToString());
                values.Add(myoPitch.ToString());
                values.Add(myoYaw.ToString());
                mWindow.myConnector.storeFrame(values);
            }
            catch(Exception e)
            {

            }
            

        }

        private void Myo_OrientationAcquired(object sender, OrientationDataEventArgs e)
        {

            CalculateOrientation(e);
           // SendData();
        }


        /// <summary>
        /// Method to broadcast packets of data
        /// </summary>
        /// <param name="pressure"></param>
        public void SendData()
        {

            sending_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 11002);

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = sending_end_point;

            string s = "{ \"sensorName\":\"Myo\",\"attributes\":[{\"attributeName\":\"GripEMG\",\"attributteValue\":\"" + gripEMG +
                "\"},{\"attributeName\":\"orientationW\",\"attributteValue\":\"" + orientationW +
                "\" }, { \"attributeName\":\"orientationX\", \"attributteValue\":\"" + orientationX + 
                "\"},{\"attributeName\":\"orientationY\",\"attributteValue\":\"" + orientationY +
                "\" },{\"attributeName\":\"orientationZ\",\"attributteValue\":\"" + orientationZ +
                "\" },{\"attributeName\":\"myoRoll\",\"attributteValue\":\"" + myoRoll +
                "\" },{\"attributeName\":\"myoPitch\",\"attributteValue\":\"" + myoPitch +
                "\" },{\"attributeName\":\"myoYaw\",\"attributteValue\":\"" + myoYaw +
                "\" }] }";
            s = "FrameSENT:" + frameNumber + s;
            frameNumber++;
            byte[] send_buffer = Encoding.UTF8.GetBytes(s);

            try
            {
                socketEventArg.SetBuffer(send_buffer, 0, send_buffer.Length);
                sending_socket.SendToAsync(socketEventArg);
                Debug.WriteLine("text sent");
            }
            catch
            {
                Debug.WriteLine("not initialized");
            }

            gripEMG = 0;
            //orientationW = 0;
            //orientationX = 0;
            //orientationY = 0;
            //orientationZ = 0;
    }

        /// <summary>
        /// Iterate through each emg sensor in myo and assign 1 if the sum of the first and second frame of emg has a sum of more than 20.
        /// else assign 0. It means that much variation(100 to -100) was observed propotional to higher tension in muscle. 
        /// 
        /// </summary>
        /// <param name="e"></param>
        void CalculateGripPressure(EmgDataEventArgs e)
        {
            gripEMG = 0;
            int[] emgTension = new int[8];
            for (int i = 0; i < 7; i++)
            {
                try
                {
                    if ((firstPreEmgValue[i] + secPreEmgValue[i] + Math.Abs(e.EmgData.GetDataForSensor(i))) <= 20)
                    {
                        emgTension[i] = 0;

                    }
                    else
                    {
                        emgTension[i] = 1;
                    }

                }
                catch
                {
                    Debug.WriteLine("Myo not connceted");
                }
            }

            //add all value from emgTension and assign it to avgTension
            Array.ForEach(emgTension, delegate (int i) { gripEMG += i; });
            mWindow.UpdateGripPressure(gripEMG);

            try
            {
                for (int i = 0; i < 7; i++)
                {
                    secPreEmgValue[i] = firstPreEmgValue[i];
                    firstPreEmgValue[i] = Math.Abs(e.EmgData.GetDataForSensor(i));
                }
            }
            catch
            {
                Debug.WriteLine("No emg value");
            }
        }

        /// <summary>
        /// Method called upon receiving the even myodata received. It passes on the orientation data to the UpdateOrientation class in Mainwindow
        /// </summary>
        /// <param name="e"></param>
        public void CalculateOrientation(OrientationDataEventArgs e)
        {
            orientationW = e.Orientation.W;
            orientationX = e.Orientation.X;
            orientationY = e.Orientation.Y;
            orientationZ = e.Orientation.Z;
            mWindow.UpdateOrientation(orientationW, orientationX, orientationY, orientationZ);

            myoRoll = e.Roll;
            mWindow.UpdateRoll(myoRoll);
            myoPitch = e.Pitch;
            mWindow.UpdatePitch(myoPitch);
            myoYaw = e.Yaw;
            mWindow.UpdateYaw(myoYaw);
        }
    }
}
