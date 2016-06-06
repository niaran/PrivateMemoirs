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
using System.Windows.Shapes;

namespace PrivateMemoirsClient
{
    public partial class AuthorizationWindow : Window
    {
        private short portServer;
        private string iPAddressServer;
        private AgentRelay agent;

        public AuthorizationWindow()
        {
            InitializeComponent();
            
        }

        private void AuthorizationWindow_Closed(object sender, EventArgs e)
        {
            var mw = new MainWindow(agent);
            mw.Show();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonEnter_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonRegistration_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Agent_OnNewPacketReceived(AgentRelay.Packet packet, AgentRelay agentRelay)
        {
            switch (packet.Command)
            {
                case (byte)TcpCommands.ServerFailed:
                    Dispatcher.BeginInvoke(new Action(() => { label1.Content = "Failed"; }));
                    break;
                case (byte)TcpCommands.ServerOK:
                    Dispatcher.BeginInvoke(new Action(() => { label1.Content = "OK"; }));
                    break;
            }
        }

        private void textBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonEnter_Click(sender, e);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Closed += AuthorizationWindow_Closed;
            textBoxPassword.Focus();
        }
    }
}