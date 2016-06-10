using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static PrivateMemoirEnum.PrivateMemoirEnum;

namespace PrivateMemoirs
{
    public class PrivateMemoirsServer
    {
        private Model context;

        private short listeningPort;
        private string listeningIPAddress;
        private string msSqlIp;
        private SHA256Cng sha256;
        private ServerRelay server;
        private ConcurrentDictionary<Guid, User> dictionaryAgents;
        
        public PrivateMemoirsServer()
            : this("127.0.0.1", "127.0.0.1")
        {
        }
        public PrivateMemoirsServer(string sqlServerNameOrAddress, string listenerlocalAddress)
            : this(sqlServerNameOrAddress, listenerlocalAddress, 8877)
        {
        }
        public PrivateMemoirsServer(string msSqlHostNameOrAddress, string listeningIPAddress, short listeningPort)
        {
            this.listeningIPAddress = listeningIPAddress;
            this.listeningPort = listeningPort;
            this.msSqlIp = System.Net.Dns.GetHostAddresses(msSqlHostNameOrAddress).First().ToString();
        }

        public void Start()
        {
            dictionaryAgents = new ConcurrentDictionary<Guid, User>();
            context = new Model("data source=" + msSqlIp +
                "\\SQLEXPRESS;initial catalog=MEMOIRS_DB;persist security info=True;user id=PrivateNotes;password=PrivateNotes;MultipleActiveResultSets=True;App=EntityFramework");

            server = new ServerRelay(true);
            server.StartServer(listeningIPAddress, listeningPort);
            server.AcceptIncommingConnections = true;
            server.OnNewAgentConnected += Server_OnNewAgentConnected;
            sha256 = new SHA256Cng();
        }
        
        private void Listener_OnNewPacketReceived(AgentRelay.Packet packet, AgentRelay listener)
        {
            switch (packet.Command)
            {
                case (byte)TcpCommands.ClientHello:
                    if (!dictionaryAgents.ContainsKey(listener.Guid))
                    {
                        var _login = AgentRelay.MakeStringFromPacketContents(packet);
                        dictionaryAgents.TryAdd(listener.Guid, null);
                        var _user = context.USERS.ToList().Find(u => u.USER_LOGIN == _login);
                        if (_user != null)
                        {
                            dictionaryAgents[listener.Guid] = new User
                            {
                                Login = _user.USER_LOGIN,
                                Hash = _user.USER_HASH,
                                Content = _user
                            };
                        }
                        else
                        {
                            dictionaryAgents[listener.Guid] = new User
                            {
                                Login = _login
                            };
                        }
                    }
                    break;

                case (byte)TcpCommands.ClientLoginQuery:
                    string hash = GetHash(AgentRelay.MakeStringFromPacketContents(packet));
                    if (dictionaryAgents[listener.Guid].Hash != null)
                    {
                        if (dictionaryAgents[listener.Guid].Hash == hash)
                        {
                            dictionaryAgents[listener.Guid].Verified = true;
                            listener.SendMessage((byte)TcpCommands.ServerLoginOK);
                            return;
                        }
                        else
                        {
                            listener.SendMessage((byte)TcpCommands.ServerLoginFailed, "Неправильный пароль.");
                            return;
                        }
                    }
                    listener.SendMessage((byte)TcpCommands.ServerLoginFailed, "Такой пользователь не зарегистрирован.");
                    break;

                case (byte)TcpCommands.ClientGetDataQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        foreach (var memoir in dictionaryAgents[listener.Guid].Content.MEMOIRS)
                        {
                            listener.SendMessage((byte)TcpCommands.ServerGetDataResponse, memoir.MEMOIR_TITLE);
                            listener.SendMessage((byte)TcpCommands.ServerGetDataResponse,
                                string.Format("{0:HH:mm:ss d.M.yyyy}", memoir.MEMOIR_DATE_CHANGE));
                            listener.SendMessage((byte)TcpCommands.ServerGetDataResponse, memoir.MEMOIR_TEXT);
                        }
                        listener.SendMessage((byte)TcpCommands.ServerOK);
                        return;
                    }
                    listener.SendMessage((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientMarkFieldQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        byte mark = Convert.ToByte(AgentRelay.MakeStringFromPacketContents(packet));
                        switch (mark)
                        {
                            case (byte)CurrentMemoirField.MEMOIR_TITLE:
                                dictionaryAgents[listener.Guid].CurrentField = CurrentMemoirField.MEMOIR_TITLE;
                                break;

                            case (byte)CurrentMemoirField.MEMOIR_TEXT:
                                dictionaryAgents[listener.Guid].CurrentField = CurrentMemoirField.MEMOIR_TEXT;
                                break;

                            case (byte)CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                dictionaryAgents[listener.Guid].CurrentField = CurrentMemoirField.MEMOIR_DATE_CHANGE;
                                break;
                        }
                        listener.SendMessage((byte)TcpCommands.ServerOK);
                        return;
                    }
                    listener.SendMessage((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientMarkMemoirQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        int mark = Convert.ToInt32(AgentRelay.MakeStringFromPacketContents(packet));
                        if (dictionaryAgents[listener.Guid].CurentMemoir > dictionaryAgents[listener.Guid].Content.MEMOIRS.Count)
                        {
                            dictionaryAgents[listener.Guid].Content.MEMOIRS.Add(new MEMOIRS());
                        }
                        dictionaryAgents[listener.Guid].CurentMemoir = mark;
                        listener.SendMessage((byte)TcpCommands.ServerOK);
                        return;
                    }
                    listener.SendMessage((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientAddDataQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        string content = AgentRelay.MakeStringFromPacketContents(packet);
                        var memoir = dictionaryAgents[listener.Guid].Content.MEMOIRS.Last();
                        
                        switch (dictionaryAgents[listener.Guid].CurrentField)
                        {
                            case CurrentMemoirField.MEMOIR_TITLE:
                                memoir.MEMOIR_TITLE = content;
                                break;

                            case CurrentMemoirField.MEMOIR_TEXT:
                                memoir.MEMOIR_TEXT = content;
                                break;

                            case CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                memoir.MEMOIR_DATE_CHANGE = Convert.ToDateTime(content);
                                break;
                        }
                        context.MEMOIRS.Add(memoir);
                        context.SaveChanges();
                        listener.SendMessage((byte)TcpCommands.ServerOK);
                        return;
                    }
                    listener.SendMessage((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientUpdateDataQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        string content = AgentRelay.MakeStringFromPacketContents(packet);
                        var memoir = context.MEMOIRS.ElementAt(dictionaryAgents[listener.Guid].CurentMemoir);

                        switch (dictionaryAgents[listener.Guid].CurrentField)
                        {
                            case CurrentMemoirField.MEMOIR_TITLE:
                                memoir.MEMOIR_TITLE = content;
                                break;

                            case CurrentMemoirField.MEMOIR_TEXT:
                                memoir.MEMOIR_TEXT = content;
                                break;

                            case CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                memoir.MEMOIR_DATE_CHANGE = Convert.ToDateTime(content);
                                break;
                        }
                        
                        context.SaveChanges();
                        listener.SendMessage((byte)TcpCommands.ServerOK);
                        return;
                    }
                    listener.SendMessage((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientDeleteDataQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        var memoir = from mem in dictionaryAgents[listener.Guid].Content.MEMOIRS
                                     where mem.MEMOIR_ID == Convert.ToInt64(AgentRelay.MakeStringFromPacketContents(packet))
                                     select mem;

                        if (memoir.Count() == 0)
                        {
                            listener.SendMessage((byte)TcpCommands.ServerFailed, "Такая запись не существует.");
                            return;
                        }

                        context.MEMOIRS.Remove(memoir.First());
                        context.SaveChanges();
                        listener.SendMessage((byte)TcpCommands.ServerOK);
                        return;
                    }
                    listener.SendMessage((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientRegistrationQuery:
                    string login = dictionaryAgents[listener.Guid].Login;
                    if (context.USERS.ToList().Exists(u => u.USER_LOGIN == login))
                    {
                        listener.SendMessage((byte)TcpCommands.ServerRegistrationFailed, "Такой пользователь уже зарегестрирован.");
                        return;
                    }

                    context.USERS.Add(new USERS { USER_LOGIN = login,
                        USER_HASH = GetHash(AgentRelay.MakeStringFromPacketContents(packet)),
                        REGISTRATION_DATE = DateTime.Now, USER_GUID = Guid.NewGuid() });
                    context.SaveChanges();
                    listener.SendMessage((byte)TcpCommands.ServerRegistrationOK);
                    break;

                case (byte)TcpCommands.ClientBye:
                    listener.SendMessage((byte)TcpCommands.ServerBye);
                    listener.Disconnect();
                    break;
            }
        }

        private void Server_OnNewAgentConnected(AgentRelay agentRelay)
        {
            agentRelay.OnNewPacketReceived += Listener_OnNewPacketReceived;
        }

        private string GetHash(string pass)
        {
            byte[] passByte = Encoding.Default.GetBytes(pass);
            byte[] hashByte = sha256.ComputeHash(passByte);
            return BitConverter.ToString(hashByte).Replace("-", "").ToLower();
        }

        public void Stop()
        {
            server.StopServer(true);
            server.Dispose();
            sha256.Dispose();
            context.Dispose();
        }

        private class User
        {
            public string Login { get; set; }
            public string Hash { get; set; }
            public bool Verified { get; set; } = false;

            public USERS Content { get; set; }
            public int CurentMemoir { get; set; }
            public CurrentMemoirField CurrentField { get; set; } = CurrentMemoirField.NONE;
        }
    }
}