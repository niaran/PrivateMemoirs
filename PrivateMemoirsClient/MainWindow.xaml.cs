using System;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using PrivateMemoirsUser;
using DialogManagement;
using static PrivateMemoirs.General;


namespace PrivateMemoirsClient
{
    public partial class MainWindow : Window
    {
        private string userLogin;
        private bool close = false;
        private bool save = false;
        private AgentRelay agent;
        private User user;
        private Message message;

        public MainWindow(AgentRelay agent, string userLogin)
        {
            this.agent = agent;
            this.userLogin = userLogin;
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
            {
                e.Cancel = true;
                var mes1 = new Message("Предупреждение", "Вы действительно хотите выйти?");
                mes1.CreateQuestioningDialog(new DialogManager(this, Dispatcher), () =>
                {
                    if (save)
                    {
                        mes1.Close();
                        var mes2 = new Message("Предупреждение", "Сохранить воспаминания?");
                        mes2.CreateQuestioningDialog(new DialogManager(this, Dispatcher), () =>
                        {
                            Record();
                            close = true;
                            Close();
                        }, () =>
                        {
                            close = true;
                            Close();
                        });
                    }
                    else
                    {
                        close = true;
                        Close();
                    }
                }, () => { return; });
            }
            else
            {
                agent.SendPacket((byte)TcpCommands.ClientBye);
                agent.Disconnect();
                agent.Dispose();
            }
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
            sampleEditor.TextInput += SampleEditor_TextInput;
        }

        private void SampleEditor_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            save = true;
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

                                case (byte)CurrentMemoirField.MEMOIR_ID:
                                    user.CurrentField = CurrentMemoirField.MEMOIR_ID;
                                    break;

                                case (byte)CurrentMemoirField.MEMOIR_TITLE:
                                    user.CurrentField = CurrentMemoirField.MEMOIR_TITLE;
                                    break;
                            }
                        }));
                        break;

                    case (byte)TcpCommands.ServerGetDataMarkMemoirResponse:
                        int curMemoir = Convert.ToInt32(AgentRelay.MakeStringFromPacketContents(packet));
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (user.TotalMemoir < curMemoir)
                            {
                                user.Memoirs.Add(new Memoir());
                                user.TotalMemoir++;
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
                                    user.Memoirs[user.Memoirs.Count - 1].MEMOIR_TEXT = content;
                                    break;

                                case CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                    user.Memoirs[user.Memoirs.Count - 1].MEMOIR_DATE_CHANGE = content;
                                    break;

                                case CurrentMemoirField.MEMOIR_ID:
                                    user.Memoirs[user.Memoirs.Count - 1].MEMOIR_ID = content;
                                    break;

                                case CurrentMemoirField.MEMOIR_TITLE:
                                    user.Memoirs[user.Memoirs.Count - 1].MEMOIR_TITLE = content;
                                    break;
                            }
                        }));
                        break;

                    case (byte)TcpCommands.ServerFailed:
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (message != null)
                            {
                                message.Close();
                                var mes = new Message("Ошибка", AgentRelay.MakeStringFromPacketContents(packet));
                                mes.CreateMessageDialog(new DialogManager(this, Dispatcher));
                                buttonRecord.IsEnabled = true;
                            }
                        }));
                        break;
                    case (byte)TcpCommands.ServerOK:
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (message != null)
                            {
                                message.Close();
                                var mes = new Message("Успешно", AgentRelay.MakeStringFromPacketContents(packet));
                                mes.CreateMessageDialog(new DialogManager(this, Dispatcher));
                                buttonRecord.IsEnabled = true;
                            }
                        }));
                        break;
                }
            }
            catch (Exception ex)
            {
                var mes = new Message("Ошибка", ex.Message);
                mes.CreateMessageDialog(new DialogManager(this, Dispatcher));
            }
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            user.Memoirs.Add(new Memoir() { MEMOIR_DATE_CHANGE = DateTime.Now.ToString(),
                MEMOIR_TEXT = "Без текста", MEMOIR_TITLE = "Без темы" });
            user.TotalMemoir++;
            agent.SendPacket((byte)TcpCommands.ClientMarkTotalMemoirQuery, user.TotalMemoir.ToString());
            save = true;
        }

        private void buttonRecord_Click(object sender, RoutedEventArgs e)
        {
            Record();
        }

        private void Record()
        {
            buttonRecord.IsEnabled = false;

            message = new Message("Соеднинение с сервером...", "Подождите, пожалуйста," +
            Environment.NewLine + "пока идет запись данных.");
            try
            {
                message.CreateWaitDialog(new DialogManager(this, Dispatcher),
                () =>
                {
                    int counter = 0;
                    foreach (var memoir in user.Memoirs)
                    {
                        agent.SendPacket((byte)TcpCommands.ClientMarkCurrentMemoirQuery, counter.ToString());
                        agent.SendPacket((byte)TcpCommands.ClientMarkFieldQuery, new byte[] { (byte)CurrentMemoirField.MEMOIR_TEXT });
                        agent.SendPacket((byte)TcpCommands.ClientUpdateDataQuery, memoir.MEMOIR_TEXT);
                        agent.SendPacket((byte)TcpCommands.ClientMarkFieldQuery, new byte[] { (byte)CurrentMemoirField.MEMOIR_DATE_CHANGE });
                        agent.SendPacket((byte)TcpCommands.ClientUpdateDataQuery, memoir.MEMOIR_DATE_CHANGE);
                        agent.SendPacket((byte)TcpCommands.ClientMarkFieldQuery, new byte[] { (byte)CurrentMemoirField.MEMOIR_TITLE });
                        agent.SendPacket((byte)TcpCommands.ClientUpdateDataQuery, memoir.MEMOIR_TITLE);
                        counter++;
                    }
                    agent.SendPacket((byte)TcpCommands.ClientUpdateEndQuery);
                    save = false;
                });
            }
            catch (Exception ex) { }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            int sel = listBox.SelectedIndex;

            if (sel == -1)
            {
                var mes1 = new Message("Ошибка", "Для удаления, выделите сначала воспаминание.");
                mes1.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return;
            }

            var mes2 = new Message("Предупреждение", "Вы действительно хотите удалить воспаминание?");
            mes2.CreateQuestioningDialog(new DialogManager(this, Dispatcher), () =>
            {
                if (user.Memoirs[sel].MEMOIR_ID != null)
                {
                    agent.SendPacket((byte)TcpCommands.ClientDeleteDataQuery, user.Memoirs[sel].MEMOIR_ID);
                    user.TotalMemoir--;
                    user.Memoirs.RemoveAt(sel);
                }
                else
                {
                    agent.SendPacket((byte)TcpCommands.ClientDeleteDataLastQuery);
                    user.TotalMemoir--;
                    user.Memoirs.RemoveAt(sel);
                    save = true;
                }
            }, () =>
            {
                return;
            }
            );
        }
    }
}