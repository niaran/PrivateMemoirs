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
using DialogManagement;
using DialogManagement.Contracts;

namespace PrivateMemoirsClient
{
    public class Message
    {
        private IMessageDialog _dialogManager;
        private string _caption;
        private string _message;


        public Message(string caption, string message)
        {
            _caption = caption;
            _message = message;
        }

        public void Close()
        {
            _dialogManager.Close();
        }

        public void CreateWaitDialog(IDialogManager dialogManager, Action worker = null, Action workerReady = null)
        {
            var waitDialog = dialogManager.CreateWaitDialog(_message, DialogMode.None);
            waitDialog.Caption = _caption;
            if (workerReady != null)
                waitDialog.WorkerReady += workerReady;
            waitDialog.CloseWhenWorkerFinished = false;
            _dialogManager = waitDialog;
            if (worker != null)
                waitDialog.Show(worker);
            else
                waitDialog.Show();
        }

        public void CreateMessageDialog(IDialogManager dialogManager, Action workerReady = null)
        {
            var messageDialog = dialogManager.CreateMessageDialog(_message, _caption, DialogMode.Ok);
            if (workerReady != null)
                messageDialog.Ok += workerReady;
            _dialogManager = messageDialog;
            messageDialog.Show();
        }
    }

        public partial class MainWindow : Window
    {
        private AgentRelay agent;
        private string userLogin;
        private ObservableCollection<LogMessage> LogMessages { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(AgentRelay agent, string userLogin)
        {
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
            agent = new AgentRelay();
            agent.OnNewPacketReceived += Agent_OnNewPacketReceived;
            label3.Content = userLogin;
            /*Message.ViewWaitDialog_WorkerReady(
               new DialogManager(this, Dispatcher),
               "Соеднинение с БД...",
               "Подождите, пожалуйста, пока загрузиться данные из базы данных.",
               () => System.Threading.Thread.Sleep(5000), Start);*/
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
            {/*
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Message.ErorMessage(new DialogManager(this, Dispatcher), "ПАкет мессадже", "Ошибка", Error);
                })).Priority = DispatcherPriority.ContextIdle;*/
            }
        }

        private void Error()
        { }

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