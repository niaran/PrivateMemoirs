using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Net;
using static PrivateMemoirEnum.PrivateMemoirEnum;
using DialogManagement;

namespace PrivateMemoirsClient
{
    public partial class AuthorizationWindow : Window
    {
        private AgentRelay agent;
        private Message message;

        public AuthorizationWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            agent = new AgentRelay();
            agent.OnNewPacketReceived += Agent_OnNewPacketReceived;
            
            textBoxPassword1.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (message != null)
            {
                message.Close();
                message = null;
            }
            agent.Disconnect();
            agent.Dispose();
        }

        private void Agent_OnNewPacketReceived(AgentRelay.Packet packet, AgentRelay agentRelay)
        {
            switch (packet.Command)
            {
                case (byte)TcpCommands.ServerLoginFailed:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        message.Close();
                        var mes = new Message("Ошибка", AgentRelay.MakeStringFromPacketContents(packet));
                        mes.CreateMessageDialog(new DialogManager(this, Dispatcher));
                        buttonEnter.IsEnabled = true;
                    }));
                    break;
                case (byte)TcpCommands.ServerLoginOK:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var mainWindow = new MainWindow(agent, textBoxLogin1.Text);
                        message.Close();
                        mainWindow.Show();
                        Close();
                    }));
                    break;
                case (byte)TcpCommands.ServerRegistrationFailed:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        message.Close();
                        var mes = new Message("Ошибка", AgentRelay.MakeStringFromPacketContents(packet));
                        mes.CreateMessageDialog(new DialogManager(this, Dispatcher));
                        buttonRegistrationOK.IsEnabled = true;
                        textBoxLogin2.Text = "";
                        textBoxPassword2.Password = "";
                        textBoxPassword3.Password = "";
                    }));
                    break;
                case (byte)TcpCommands.ServerRegistrationOK:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        message.Close();
                        var mes = new Message("Успешно", "Ваша регистрация прошла успешно," +
                            Environment.NewLine + "         теперь можете войти.");
                        mes.CreateMessageDialog(new DialogManager(this, Dispatcher));
                        StackPanelRegistration.Visibility = Visibility.Hidden;
                        buttonRegistrationOK.IsEnabled = true;
                        textBoxLogin1.Text = textBoxLogin2.Text;
                        textBoxPassword1.Password = textBoxPassword3.Password;
                        textBoxLogin2.Text = "";
                        textBoxPassword2.Password = "";
                        textBoxPassword3.Password = "";
                        StackPanelMain.Visibility = Visibility.Visible;
                        textBoxLogin1.Focus();
                    }));
                    break;
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void buttonRegistration_Click(object sender, RoutedEventArgs e)
        {
            StackPanelMain.Visibility = Visibility.Hidden;
            StackPanelRegistration.Visibility = Visibility.Visible;
            textBoxLogin2.Focus();
        }

        private void buttonEnter_Click(object sender, RoutedEventArgs e)
        {
            buttonEnter.IsEnabled = false;

            string serverNameOrAddress = textBoxServer.Text;
            string serverPort = textBoxPort.Text;
            string login = textBoxLogin1.Text.ToLower();
            string pass = textBoxPassword1.Password;

            if (Validation(serverNameOrAddress, serverPort, login, pass))
            {
                message = new Message("Соеднинение с сервером...", "Подождите, пожалуйста," +
                Environment.NewLine + "пока осуществляется вход.");
                message.CreateWaitDialog(new DialogManager(this, Dispatcher),
                    () =>
                    {
                        Connect(serverNameOrAddress, Convert.ToInt32(serverPort));
                        agent.SendMessage((byte)TcpCommands.ClientHello, login);
                        agent.SendMessage((byte)TcpCommands.ClientLoginQuery, pass);
                    });
                buttonEnter.IsEnabled = true;
            }
            else
            {
                buttonEnter.IsEnabled = true;
            }
        }

        private void buttonRegistrationOK_Click(object sender, RoutedEventArgs e)
        {
            buttonRegistrationOK.IsEnabled = false;
            string serverNameOrAddress = textBoxServer2.Text;
            string serverPort = textBoxPort2.Text;
            string login = textBoxLogin2.Text.ToLower();
            string pass = textBoxPassword3.Password;
            if (textBoxPassword3.Password != textBoxPassword2.Password)
            {
                message = new Message("Не правильно", "Пороли должны совпадать.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                textBoxPassword2.Password = "";
                textBoxPassword3.Password = "";
                buttonRegistrationOK.IsEnabled = true;
                return;
            }

            if (Validation(serverNameOrAddress, serverPort, login, pass))
            {
                message = new Message("Соеднинение с сервером...", "Подождите, пожалуйста," +
                Environment.NewLine + "пока происходит регистрация.");
                message.CreateWaitDialog(new DialogManager(this, Dispatcher),
                    () =>
                    {
                        Connect(serverNameOrAddress, Convert.ToInt32(serverPort));
                        agent.SendMessage((byte)TcpCommands.ClientHello, login);
                        agent.SendMessage((byte)TcpCommands.ClientRegistrationQuery, pass);
                    });
                buttonRegistrationOK.IsEnabled = true;
            }
            else
            {
                buttonRegistrationOK.IsEnabled = true;
            }
        }

        private void Connect(string serverNameOrAddress, int serverPort)
        {
            if (agent.IsConnected)
                agent.Disconnect();
            
            foreach (var ipAddrV4 in Dns.GetHostAddresses(serverNameOrAddress))
            {
                if (ipAddrV4.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    agent.Connect(ipAddrV4.ToString(), serverPort);
                    break;
                }
            }
        }

        private bool Validation(string serverNameOrAddress, string serverPort, string login, string pass)
        {
            if (login == "")
            {
                message = new Message("Не правильно", "Введите логин.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }
            if (!(login.Length < 50 && login.Length > 2))
            {
                message = new Message("Не правильно", "Длина логина должна быть от 3 до 50 символов.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }
            if (!new Regex(@"^([a-zA-Z](?:(?:(?:\w[\._]?)*)\w)+)([a-zA-Z0-9])$").IsMatch(login))
            {
                message = new Message("Не правильно", "Логин может начинаться только с буквы и может" + Environment.NewLine +
                    "содержать латинские буквы, цифры, \".\" и \"_\" .");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }

            if (pass == "")
            {
                message = new Message("Не правильно", "Введите пороль.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }
            if (!(pass.Length < 50 && pass.Length > 7))
            {
                message = new Message("Не правильно", "Длина пороля должна быть от 8 до 50 символов.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }

            if (serverPort == "")
            {
                message = new Message("Не правильно", "Введите номер порта.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }
            if (!new Regex(@"^[\d]{1,5}$").IsMatch(serverPort))
            {
                message = new Message("Не правильно", "Порт это набор цифр от 1 до 65536.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }

            if (serverNameOrAddress == "")
            {
                message = new Message("Не правильно", "Введите имя или ip адрес сервера.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }
            try
            {
                IPAddress.Parse(Dns.GetHostAddresses(serverNameOrAddress)[0].ToString());
            }
            catch (System.Net.Sockets.SocketException)
            {
                message = new Message("Не правильно", "Этот сервер не известен.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }
            catch (FormatException)
            {
                message = new Message("Не правильно", "Этот сервер не известен.");
                message.CreateMessageDialog(new DialogManager(this, Dispatcher));
                return false;
            }

            return true;
        }
    }
}