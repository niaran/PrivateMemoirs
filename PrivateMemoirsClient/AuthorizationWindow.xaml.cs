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
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PrivateMemoirsClient
{
    public partial class AuthorizationWindow : Window
    {
        private AgentRelay agent;
        private SHA256Cng sha256;

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            agent = new AgentRelay();
            sha256 = new SHA256Cng();
            textBoxPassword1.Focus();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonEnter_Click(object sender, RoutedEventArgs e)
        {


            agent.Connect(textBoxServer.Text, Convert.ToInt32(textBoxPort.Text));
            var mainWindow = new MainWindow(agent, textBoxServer.Text);
            mainWindow.Show();
            Close();
        }

        private void buttonRegistration_Click(object sender, RoutedEventArgs e)
        {


            StackPanelMain.Visibility = Visibility.Hidden;
            StackPanelRegistration.Visibility = Visibility.Visible;
            textBoxLogin2.Focus();
        }

        private void Agent_OnNewPacketReceived(AgentRelay.Packet packet, AgentRelay agentRelay)
        {
            switch (packet.Command)
            {
                case (byte)TcpCommands.ServerFailed:
                    Dispatcher.BeginInvoke(new Action(() => { labelStatus.Content = "Failed"; }));
                    break;
                case (byte)TcpCommands.ServerOK:
                    Dispatcher.BeginInvoke(new Action(() => { labelStatus.Content = "OK"; }));
                    break;
            }
        }

        private void textBoxPassword1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonEnter_Click(sender, e);
            }
        }
        
        private void textBoxPassword2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonRegistrationOK_Click(sender, e);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            StackPanelRegistration.Visibility = Visibility.Hidden;
            buttonRegistrationOK.IsEnabled = true;
            StackPanelMain.Visibility = Visibility.Visible;
            textBoxLogin1.Focus();
        }

        private void buttonRegistrationOK_Click(object sender, RoutedEventArgs e)
        {

            buttonRegistrationOK.IsEnabled = false;
            byte[] pass = Encoding.Default.GetBytes(textBoxPassword3.Password);
            byte[] hashByte = sha256.ComputeHash(pass);
            string hash = BitConverter.ToString(hashByte).Replace("-", "").ToLower();
            
            agent.SendMessage((byte)TcpCommands.ClientHello, textBoxLogin2.Text);
            agent.SendMessage((byte)TcpCommands.ClientRegistration, hash);

            StackPanelRegistration.Visibility = Visibility.Hidden;
            buttonRegistrationOK.IsEnabled = true;
            StackPanelMain.Visibility = Visibility.Visible;
            textBoxLogin1.Focus();
        }
    }
}