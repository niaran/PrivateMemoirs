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
using DialogManagement;
using DialogManagement.Contracts;

namespace PrivateMemoirsClient
{
    public enum TcpCommands
    {
        ServerFailed = 1,
        ServerOK = 0,
        ServerBye = 7,
        ServerGetDataResponse = 10,
        ClientHello = 100,
        ClientRegistration = 150,
        ClientBye = 200,
        ClientLoginQuery = 20,
        ClientGetDataQuery = 30,
        ClientMarkFieldQuery = 40,
        ClientMarkMemoirQuery = 50,
        ClientAddDataQuery = 60,
        ClientUpdateDataQuery = 70,
        ClientDeleteDataQuery = 80,
        InvalidCommand = 123
    };

    public partial class MainWindow : Window
    {
        private AgentRelay agent;
        private ObservableCollection<LogMessage> LogMessages { get; set; }

        public MainWindow(AgentRelay agent)
        {
            InitializeComponent();
            this.agent = agent;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            agent.SendMessage((byte)TcpCommands.ClientBye);
            agent.Disconnect();
            agent.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            agent = new AgentRelay();
            agent.OnNewPacketReceived += Agent_OnNewPacketReceived;
            ViewWaitDialog_WorkerReady(
               new DialogManager(this, Dispatcher),
               "Соеднинение с БД...",
               "Подождите, пожалуйста, пока загрузиться данные из базы данных.",
               () => System.Threading.Thread.Sleep(5000), Start);
        }

        private void Connect()
        {
            agent.Connect("127.0.0.1", 8877);
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

        private void ViewWaitDialog_WorkerReady(IDialogManager dialogManager, string caption, string message, Action A, Action W)
        {
            var _dialogManager = dialogManager
                .CreateWaitDialog(message, DialogMode.None);
            _dialogManager.Caption = caption;
            _dialogManager.WorkerReady += W;
            _dialogManager.Show(A);
        }

        private void ViewMessageDialog(IDialogManager dialogManager, string Message, string Caption)
        {
            var _dialogManager = dialogManager
                .CreateMessageDialog(Message, Caption, DialogMode.Ok);
            _dialogManager.Show();
        }

        private void ErorMessageConnectDB(IDialogManager dialogManager, string Message, string Caption)
        {
            var _dialogManager = dialogManager
                .CreateMessageDialog(Message + "\nПриложение будет закрыто", Caption, DialogMode.Ok);
            _dialogManager.Ok += Exit;
            _dialogManager.Show();
        }

        private void Exit()
        {
            Environment.Exit(1);
        }

        private void GetDATA()
        {
            try
            {

            }
            catch
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ErorMessageConnectDB(new DialogManager(this, Dispatcher), "ПАкет мессадже", "Ошибка");
                })).Priority = DispatcherPriority.ContextIdle;
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
            agent.SendMessage((byte)TcpCommands.ClientHello, "Kirill");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Connect();
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