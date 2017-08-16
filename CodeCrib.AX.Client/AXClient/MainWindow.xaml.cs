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

namespace AXClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //CodeCrib.AX.Client.AutoRun.AxaptaAutoRun autoRun = new CodeCrib.AX.Client.AutoRun.AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\ImportLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };
            //autoRun.Steps.Add(new CodeCrib.AX.Client.AutoRun.XpoImport() { File = xpoFile });

            //string autoRunFile = string.Format(@"{0}\AutoRun-ImportXPO-{1}.xml", Environment.GetEnvironmentVariable("temp"), Guid.NewGuid());
            //CodeCrib.AX.Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(autoRun, autoRunFile);
        }
    }
}
