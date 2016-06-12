using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
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
using PrivateMemoirsUser;
using static PrivateMemoirs.General;


namespace PrivateMemoirsClient
{
    public partial class MainWindow : Window
    {
        private AgentRelay agent;
        private string userLogin;
        private User user;

        public MainWindow(AgentRelay agent, string userLogin)
        {
            this.agent = agent;
            this.userLogin = userLogin;
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            agent.SendPacket((byte)TcpCommands.ClientBye);
            agent.Disconnect();
            agent.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            user = new User();
            DataContext = user;
            agent.OnNewPacketReceived += agent_OnNewPacketReceived;
            agent.SendPacket((byte)TcpCommands.ClientGetDataQuery);
            Title = "Личные воспоминания | Сегодня: " +
                    DateTime.Today.ToLongDateString() + " | Пользователь: " + userLogin;
            StartAnimation();
        }

        private void StartAnimation()
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

        private void agent_OnNewPacketReceived(AgentRelay.Packet packet, AgentRelay agentRelay)
        {
            try
            {
                switch (packet.Command)
                {
                    case (byte)TcpCommands.ServerOK:
                        break;

                    case (byte)TcpCommands.ServerGetDataMarkResponse:
                        byte mark = packet.Content[0];
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            switch (mark)
                            {
                                case (byte)CurrentMemoirField.MEMOIR_TEXT:
                                    user.CurrentField = CurrentMemoirField.MEMOIR_TEXT;
                                    break;

                                case (byte)CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                    user.CurrentField = CurrentMemoirField.MEMOIR_DATE_CHANGE;
                                    break;
                            }
                        }));
                        break;

                    case (byte)TcpCommands.ServerGetDataMarkMemoirResponse:
                        int curMemoir = Convert.ToInt32(AgentRelay.MakeStringFromPacketContents(packet));
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (curMemoir == user.CurentMemoir)
                            {
                                user.Memoirs.Add(new Memoir());
                                user.CurentMemoir++;
                            }
                        }));
                        break;

                    case (byte)TcpCommands.ServerGetDataResponse:
                        string content = AgentRelay.MakeStringFromPacketContents(packet);
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            switch (user.CurrentField)
                            {
                                case CurrentMemoirField.MEMOIR_TEXT:
                                    user.Memoirs[user.CurentMemoir].MEMOIR_TEXT = content;
                                    break;

                                case CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                    user.Memoirs[user.CurentMemoir].MEMOIR_DATE_CHANGE = content;
                                    break;
                            }
                        }));
                        break;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(user.Memoirs.Last().MEMOIR_TEXT);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}