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

namespace AxConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AOSes.ItemsSource = CodeCrib.AX.Config.Server.GetAOSInstances();
        }

        private void AOSes_Selected(object sender, RoutedEventArgs e)
        {
            string aosName = (string)AOSes.SelectedItem;

            CodeCrib.AX.Config.Server config = CodeCrib.AX.Config.Server.GetConfigFromRegistry(aosName);
        }
    }
}
