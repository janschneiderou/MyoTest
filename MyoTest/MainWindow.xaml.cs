using System;
using System.Collections.Generic;
using System.Linq;
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

using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Poses;
using System.Diagnostics;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows.Media.Media3D;

namespace MyoTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MyoManager.MyoManagerClass myoManager = new MyoManager.MyoManagerClass();
        public ConnectorHub.ConnectorHub myConnector;
        public bool start = false;
        public ConnectorHub.FeedbackHub myFeedback;

        public MainWindow()
        {
            InitializeComponent();
            myoManager.InitMyoManagerHub(this);
            try
            {
                myConnector = new ConnectorHub.ConnectorHub();
                myConnector.init();
                myConnector.startRecordingEvent += MyConnector_startRecordingEvent;
                myConnector.stopRecordingEvent += MyConnector_stopRecordingEvent;
                myConnector.sendReady();
                setValueNames();

                myFeedback = new ConnectorHub.FeedbackHub();
                myFeedback.init();
                myFeedback.feedbackReceivedEvent += MyFeedback_feedbackReceivedEvent;
            }
            catch (Exception e)
            {

            }

        }

        private void MyFeedback_feedbackReceivedEvent(object sender, string feedback)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            GripLabel.Content = feedback;
                        }));
        }

        private void setValueNames()
        {
            List<string> names = new List<string>();
            names.Add("GripPressure");
            names.Add("OrientationW");
            names.Add("OrientationX");
            names.Add("OrientationY");
            names.Add("OrientationZ");
            names.Add("Roll");
            names.Add("Pitch");
            names.Add("Yaw");
            myConnector.setValuesName(names);
        }

        private void MyConnector_stopRecordingEvent(object sender)
        {
            start = false;

        }

        private void MyConnector_startRecordingEvent(object sender)
        {
            start = true;
        }


       
        /// <summary>
        /// Method to update the grip textbox and assign the value to gripPressure var
        /// </summary>
        /// <param name="g"></param>
        public void UpdateGripPressure(Int32 g)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            GripTxt.Text = g.ToString();
                        }));
        }

        /// <summary>
        /// Method to update the orientation textbox and assign the value of orientation
        /// </summary>
        /// <param name="ww"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void UpdateOrientation(float w, float x, float y, float z)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            OrientationTxt.Text = w.ToString() + " " + x.ToString() + " " + y.ToString() + " " + z.ToString(); ;
                        }));
        }

        /// <summary>
        /// Method to update the grip textbox and assign the value to gripPressure var
        /// </summary>
        /// <param name="g"></param>
        public void UpdateRoll(double roll)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            RollTxt.Text = roll.ToString();
                        }));
        }

        /// <summary>
        /// Method to update the grip textbox and assign the value to gripPressure var
        /// </summary>
        /// <param name="g"></param>
        public void UpdatePitch(double pitch)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            PitchTxt.Text = pitch.ToString();
                        }));
        }

        /// <summary>
        /// Method to update the grip textbox and assign the value to gripPressure var
        /// </summary>
        /// <param name="g"></param>
        public void UpdateYaw(double yaw)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            YawTxt.Text = yaw.ToString();
                        }));
        }

        public void UpdateGrip(string grip)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                        () =>
                        {
                            GripLabel.Content = grip;
                        }));
        }
    }
}
