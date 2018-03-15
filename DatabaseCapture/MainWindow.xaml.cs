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

namespace DatabaseCapture
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        bool capturing = false;
        bool connected =false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btConnect_Click(object sender, RoutedEventArgs e)
        {
            connected = !connected;
            if (connected)
            {
                btConnect.Content = "Disconnect";
            }
            else
            {
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
    }
}
