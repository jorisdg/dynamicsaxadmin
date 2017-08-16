//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

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
using Microsoft.Win32;

namespace AxSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<CodeCrib.AX.Setup.Parameter> parameters = new List<CodeCrib.AX.Setup.Parameter>();

            ParameterListView.ItemsSource = parameters;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "All Files|*.*";
            if (dialog.ShowDialog() == true)
            {
                ParameterListView.ItemsSource = CodeCrib.AX.Setup.ParameterFile.Open(dialog.FileName);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ParameterListView.ItemsSource != null && ParameterListView.ItemsSource is List<CodeCrib.AX.Setup.Parameter>)
            {
                SaveFileDialog dialog = new SaveFileDialog();

                dialog.Filter = "All Files|*.*";
                dialog.OverwritePrompt = true;
                if (dialog.ShowDialog() == true)
                {
                    CodeCrib.AX.Setup.ParameterFile.Save(ParameterListView.ItemsSource as List<CodeCrib.AX.Setup.Parameter>, dialog.FileName);
                }
            }
            else
            {
                MessageBox.Show("No parameters to save");
            }
        }

        private void ParseLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Log Files|*.txt";

            if (System.IO.Directory.Exists(@"C:\Program Files\Microsoft Dynamics AX\60\Setup Logs\"))
            {
                dialog.InitialDirectory = @"C:\Program Files\Microsoft Dynamics AX\60\Setup Logs\";
            }

            if (dialog.ShowDialog() == true)
            {
                ParameterListView.ItemsSource = CodeCrib.AX.Setup.LogParser.LogExtract(dialog.FileName);
            }
        }

        private void RunSetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (ParameterListView.ItemsSource != null && ParameterListView.ItemsSource is List<CodeCrib.AX.Setup.Parameter>)
            {
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Filter = "Dynamics AX Setup|setup.exe";

                if (dialog.ShowDialog() == true)
                {
                    CodeCrib.AX.Setup.Setup setup = new CodeCrib.AX.Setup.Setup();
                    setup.ExecutablePath = dialog.FileName;
                    setup.Run(ParameterListView.ItemsSource as List<CodeCrib.AX.Setup.Parameter>);
                }
            }
            else
            {
                MessageBox.Show("No parameters");
            }
        }
    }
}
