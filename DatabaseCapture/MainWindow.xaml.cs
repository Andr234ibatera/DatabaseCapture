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


using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DatabaseCapture
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {

        KinectSensor kinectSensor;
        MultiSourceFrameReader reader;

        //Depth Parameters
        DepthFrameReader depthFrameReader;
        FrameDescription depthFrameDescription;
        WriteableBitmap depthImage;
        ushort[] depthBuffer;
        byte[] depthBitmapBuffer;
        Int32Rect depthRect;
        int depthStride;
        Point depthPoint;
        const int R = 20;

        //Control Variables
        bool capturing = false;
        bool connected =false;

        public MainWindow()
        {
            kinectSensor = KinectSensor.GetDefault();
            InitializeComponent();
        }

        private void btConnect_Click(object sender, RoutedEventArgs e)
        {
            connected = !connected;
            if (connected)
            {
                kinectSensor.Open();
                btConnect.Content = "Disconnect";

                //Depth calling
                depthFrameDescription = kinectSensor.DepthFrameSource.FrameDescription;

                depthImage = new WriteableBitmap(depthFrameDescription.Width, depthFrameDescription.Height, 96, 96, PixelFormats.Gray8, null);
                depthBuffer = new ushort[depthFrameDescription.LengthInPixels];
                depthBitmapBuffer = new byte[depthFrameDescription.LengthInPixels];
                depthRect = new Int32Rect(0, 0, depthFrameDescription.Width, depthFrameDescription.Height);
                depthStride = (int)depthFrameDescription.Width;

                imgDepth.Source = depthImage;

                depthPoint = new Point(depthFrameDescription.Width / 2, depthFrameDescription.Height / 2);
                //LblStatus.Content = "X " +depthPoint.X+ " Y "+depthPoint.Y;

                depthFrameReader = kinectSensor.DepthFrameSource.OpenReader();
                depthFrameReader.FrameArrived += depthFrameReader_FrameArrived;

                //Infrared calling
                reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Infrared);
                reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
            else
            {
                kinectSensor.Close();
                btConnect.Content = "Connect";
            }
        }

        private void btCapture_Click(object sender, RoutedEventArgs e)
        {
            capturing = !capturing;
            if (capturing)
            {
                btCapture.Content = "Stop";
            }
            else
            {
                btCapture.Content = "Start";
            }
        }

        private void depthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            try
            {
                using (var depthFrame = e.FrameReference.AcquireFrame())
                {
                    if (depthFrame == null)
                    {
                        return;
                    }
                    depthFrame.CopyFrameDataToArray(depthBuffer);
                }

                for (int i = 0; i < depthBuffer.Length; i++)
                {
                    depthBitmapBuffer[i] = (byte)(depthBuffer[i] % 255);
                }
                depthImage.WritePixels(depthRect, depthBitmapBuffer, depthStride, 0);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                Close();
            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    int width = frame.FrameDescription.Width;
                    int height = frame.FrameDescription.Height;
                    PixelFormat format = PixelFormats.Bgr32;

                    ushort[] frameData = new ushort[width * height];
                    byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

                    frame.CopyFrameDataToArray(frameData);

                    int colorIndex = 0;
                    for (int infraredIndex = 0; infraredIndex < frameData.Length; infraredIndex++)
                    {
                        ushort ir = frameData[infraredIndex];

                        byte intensity = (byte)(ir >> 7);

                        pixels[colorIndex++] = (byte)(intensity / 0.7); // Blue
                        pixels[colorIndex++] = (byte)(intensity / 1); // Green   
                        pixels[colorIndex++] = (byte)(intensity / 1); // Red

                        colorIndex++;
                    }

                    int stride = width * format.BitsPerPixel / 8;

                    BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);

                    imgInfrared.Source = bitmap;
                }
            }
        }
    }
}
