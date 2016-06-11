using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PrivateMemoirEnum.PrivateMemoirEnum;


namespace PrivateMemoirsClient
{
    public partial class MainWindow : Window
    {
        private AgentRelay agent;
        private string userLogin;
        private ObservableCollection<LogMessage> LogMessages { get; set; }

        public MainWindow(AgentRelay agent, string userLogin)
        {
            InitializeComponent();
            this.agent = agent;
            this.userLogin = userLogin;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            agent.SendMessage((byte)TcpCommands.ClientBye);
            agent.Disconnect();
            agent.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            agent.OnNewPacketReceived += Agent_OnNewPacketReceived;
            label3.Content = userLogin;
            Start();
        }

        private void Agent_OnNewPacketReceived(AgentRelay.Packet packet, AgentRelay agentRelay)
        {
            switch (packet.Command)
            {
                case (byte)TcpCommands.ServerFailed:
                    Dispatcher.BeginInvoke(new Action(() => { label1.Content = AgentRelay.MakeStringFromPacketContents(packet); } ));
                    break;
                case (byte)TcpCommands.ServerOK:
                    Dispatcher.BeginInvoke(new Action(() => { label1.Content = AgentRelay.MakeStringFromPacketContents(packet); }));
                    break;
            }
        }

        private void Start()
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation da = new DoubleAnimation();
            da.Duration = TimeSpan.FromSeconds(0.5);
            da.From = 743;
            da.To = 0;
            Storyboard.SetTarget(da, Grid1);
            Storyboard.SetTargetProperty(da, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(da);
            storyboard.Begin();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            textBox1.Clear();
            sampleEditor.Text = "";
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {

        }

        private class LogMessage
        {
            public string Msg { get; set; }
            public int Severity { get; set; }
            //code cut...
        }

        private enum CurrentMemoirField
        {
            NONE = 0,
            MEMOIR_TEXT = 1,
            MEMOIR_TITLE = 2,
            MEMOIR_DATE_CHANGE = 3
        }
    }
}