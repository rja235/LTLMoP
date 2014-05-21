// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using Coding4Fun.Kinect;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace SkeletalTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public int XR;
        public int YR;
        public int XL;
        public int YL;
        public int HY;

        public MainWindow()
        {
            InitializeComponent(); //initialize kinect
        }

        bool closing = false;
        const int skeletonCount = 6;  //number of skeletons able to read
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
            //this launches the kinect            
        }
        //check


        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor old = (KinectSensor)e.OldValue;
            StopKinect(old); //makes sure we are connected to kinect
            KinectSensor sensor = (KinectSensor)e.NewValue; //here is the new sensor

            if (sensor == null)
            {
                return;
            }

            var parameters = new TransformSmoothParameters
            { //basic parameters 
                Smoothing = 0.3f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 1.0f,
                MaxDeviationRadius = 0.5f
            };
            sensor.SkeletonStream.Enable(parameters); //enables parameters for the skeleton
            sensor.SkeletonStream.Enable(); //turns on skeleton stream 

            sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady); //starts event handler
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30); //enables the depth stream at resolution set
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30); // enables color stream at resolution set

            try
            {
                sensor.Start(); //starts sensor
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred(); //if an error, alert
            }
        }
        //check 
        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            //Get a skeleton
            Skeleton first = GetFirstSkeleton(e); //first is now our skeleton

            if (first == null)
            {
                return; //if no skeleton located, return
            }

            GetCameraPoint(first, e);

            //set scaled positions
            ScalePosition(Head, first.Joints[JointType.Head]);
            ScalePosition(LeftHand, first.Joints[JointType.HandLeft]);
            ScalePosition(RightHand, first.Joints[JointType.HandRight]);


            XR = ProcessGestureXR(first.Joints[JointType.Head], first.Joints[JointType.HandLeft], first.Joints[JointType.HandRight]);
            XL = ProcessGestureXL(first.Joints[JointType.Head], first.Joints[JointType.HandLeft], first.Joints[JointType.HandRight]);
            YR = ProcessGestureYR(first.Joints[JointType.Head], first.Joints[JointType.HandLeft], first.Joints[JointType.HandRight]);
            YL = ProcessGestureYL(first.Joints[JointType.Head], first.Joints[JointType.HandLeft], first.Joints[JointType.HandRight]);
           

            

           MainSender(XR,XL,YR,YL);
           
        }
        
        
        static public int ProcessGestureYR(Joint head, Joint handleft, Joint handright)
        {
            int YR;
            float HY1 = head.Position.Y;
            float YR1 = handright.Position.Y;

            return YR = (int)((YR1-HY1) * 10000);
        }
        static public int ProcessGestureYL(Joint head, Joint handleft, Joint handright)
        {
            int YL;
            float HY1 = head.Position.Y;
            float YL1 = handleft.Position.Y;

            return YL = (int)((YL1-HY1) * 10000);
        }
        static public int ProcessGestureXL(Joint head, Joint handleft, Joint handright)
        {
            int XL;
            float XL1 = handleft.Position.X;

            return XL = (int)(XL1 * 10000);
        }

       static public int ProcessGestureXR(Joint head, Joint handleft, Joint handright)
        {
           
           int XR;
           
           float XR1 = handright.Position.X;
                     
           return XR = (int)(XR1*10000);          

        }


        void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null ||
                    kinectSensorChooser1.Kinect == null)
                {
                    return; //If sensor is off or cannot locate the depth then
                }
                //Map a joint location to a point on the depth map
                //head
                DepthImagePoint headDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                //left hand
                DepthImagePoint leftDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                //right hand
                DepthImagePoint rightDepthPoint =
                    depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);
                                
                //Map a depth point to a point on the color image
                //head
                ColorImagePoint headColorPoint =
                    depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //left hand
                ColorImagePoint leftColorPoint =
                    depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //right hand
                ColorImagePoint rightColorPoint =
                    depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y,
                    ColorImageFormat.RgbResolution640x480Fps30);
                //Set location
                CameraPosition(Head, headColorPoint);
                CameraPosition(LeftHand, leftColorPoint);
                CameraPosition(RightHand, rightColorPoint);
            }
        }


        Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }


                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //get the first tracked skeleton
                Skeleton first = (from s in allSkeletons
                                  where s.TrackingState == SkeletonTrackingState.Tracked
                                  select s).FirstOrDefault();

                return first;



            }
        }


        private void btnangle_Click(object sender, RoutedEventArgs e)
        {
            if (kinectSensorChooser1.Kinect.ElevationAngle != (int)slider1.Value)
            {
                kinectSensorChooser1.Kinect.ElevationAngle = (int)slider1.Value;
            }



        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int n = (int)slider1.Value;

            Degree.Content = n.ToString();

        }

        private void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                if (sensor.IsRunning)
                {
                    //stop sensor 
                    sensor.Stop();

                    //stop audio if not null
                    if (sensor.AudioSource != null)
                    {
                        sensor.AudioSource.Stop();
                    }


                }
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            //Divide by 2 for width and height so point is right in the middle 
            // instead of in top/left corner
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);

        }


        private void ScalePosition(FrameworkElement element, Joint joint)
        {
            //convert the value to X/Y
            Joint scaledJoint = joint.ScaleTo(1280, 720);

            //convert & scale (.3 = means 1/3 of joint distance)
            //Joint scaledJoint = joint.ScaleTo(1280, 720, .3f, .3f);

            Canvas.SetLeft(element, scaledJoint.Position.X);
            Canvas.SetTop(element, scaledJoint.Position.Y);

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            StopKinect(kinectSensorChooser1.Kinect);
        }
        //check 
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        const int PORT = 2222;
        static byte[] buffer = new byte[4096];


        static void MainSender(int XR, int XL, int YR, int YL)
        {
            
            UdpClient client = new UdpClient();

            client.ExclusiveAddressUse = false;
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 2222);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;

            client.Client.Bind(localEp);
                      
            char one = '#'; //header
            char two = '@'; //header
                byte[] fullMsg = new byte[18];

                fullMsg[0] = (byte)(one);
                fullMsg[1] = (byte)(two);
                fullMsg[2] = (byte)((XR >> 24) & 0xFF);
                fullMsg[3] = (byte)((XR >> 16) & 0xFF);
                fullMsg[4] = (byte)((XR >> 8) & 0xFF);
                fullMsg[5] = (byte)((XR) & 0xFF);
                fullMsg[6] = (byte)((XL >> 24) & 0xFF);
                fullMsg[7] = (byte)((XL >> 16) & 0xFF);
                fullMsg[8] = (byte)((XL >> 8) & 0xFF);
                fullMsg[9] = (byte)((XL) & 0xFF);
                fullMsg[10] = (byte)((YR >> 24) & 0xFF);
                fullMsg[11] = (byte)((YR >> 16) & 0xFF);
                fullMsg[12] = (byte)((YR >> 8) & 0xFF);
                fullMsg[13] = (byte)((YR) & 0xFF);
                fullMsg[14] = (byte)((YL >> 24) & 0xFF);
                fullMsg[15] = (byte)((YL >> 16) & 0xFF);
                fullMsg[16] = (byte)((YL >> 8) & 0xFF);
                fullMsg[17] = (byte)((YL) & 0xFF);

                buffer = fullMsg; //GetBytes(sendMe);
                int len = buffer.Length;

                string ipAddress = "239.0.0.222";
                client.Send(buffer, len, ipAddress, PORT);
            //}

            
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        static void ReceiveCallback(IAsyncResult ar)
        {
            Console.WriteLine("Got a message! it says: " + System.Text.Encoding.Default.GetString(buffer));
            socket.BeginReceive(buffer, 0, 4096, SocketFlags.None, ReceiveCallback, null);
        }

    }
}
