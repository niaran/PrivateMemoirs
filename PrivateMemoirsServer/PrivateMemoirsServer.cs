using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static PrivateMemoirs.General;

namespace PrivateMemoirs
{
    public class PrivateMemoirsServer
    {
        private short listeningPort;
        private string listeningIPAddress;
        private string msSqlIp;
        private string loginMsSql;
        private string passMsSql;
        private string dbName;
        private Model context;
        private SHA256Cng sha256;
        private ServerRelay server;
        private ConcurrentDictionary<Guid, User> dictionaryAgents;

        public delegate void NewAgentСonnectedHandler(string conAgent);
        public event NewAgentСonnectedHandler NewAgentСonnected;
        public delegate void AgentDisconnectedHandler(string disAgent);
        public event AgentDisconnectedHandler AgentDisconnected;

        public PrivateMemoirsServer(string msSqlHostNameOrAddress, string listeningIPAddress, short listeningPort,
            string loginMsSql, string passMsSql, string dbName)
        {
            this.listeningIPAddress = listeningIPAddress;
            this.listeningPort = listeningPort;
            this.msSqlIp = System.Net.Dns.GetHostAddresses(msSqlHostNameOrAddress).First().ToString();
            this.loginMsSql = loginMsSql;
            this.passMsSql = passMsSql;
            this.dbName = dbName;
        }

        public void Start()
        {
            dictionaryAgents = new ConcurrentDictionary<Guid, User>();
            context = new Model($"data source={msSqlIp}\\SQLEXPRESS;initial catalog={dbName};persist security info=True;user id={loginMsSql};password={passMsSql};MultipleActiveResultSets=True;App=EntityFramework");

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
                                Content = _user,
                                CurrentMemoir = 0,
                                TotalMemoir = _user.MEMOIRS.Count - 1
                            };
                        }
                        else
                        {
                            dictionaryAgents[listener.Guid] = new User
                            {
                                Login = _login,
                                CurrentMemoir = 0,
                                TotalMemoir = 0
                            };
                        }
                        NewAgentСonnected("New Agent Сonnected! GUID -> " + listener.Guid
                            + ", User login -> " + dictionaryAgents[listener.Guid].Login
                            + ", Date -> " + DateTime.Now);
                        listener.SendPacket((byte)TcpCommands.ServerHello);
                    }
                    break;

                case (byte)TcpCommands.ClientLoginQuery:
                    string hash = GetHash(AgentRelay.MakeStringFromPacketContents(packet));
                    if (dictionaryAgents[listener.Guid].Hash != null)
                    {
                        if (dictionaryAgents[listener.Guid].Hash == hash)
                        {
                            dictionaryAgents[listener.Guid].Verified = true;
                            listener.SendPacket((byte)TcpCommands.ServerLoginOK);
                            return;
                        }
                        else
                        {
                            listener.SendPacket((byte)TcpCommands.ServerLoginFailed, "Неправильный пароль.");
                            return;
                        }
                    }
                    listener.SendPacket((byte)TcpCommands.ServerLoginFailed, "Такой пользователь не зарегистрирован.");
                    break;

                case (byte)TcpCommands.ClientGetDataQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        int counter = 0;
                        foreach (var memoir in dictionaryAgents[listener.Guid].Content.MEMOIRS)
                        {
                            listener.SendPacket((byte)TcpCommands.ServerGetDataMarkMemoirResponse, counter.ToString());
                            listener.SendPacket((byte)TcpCommands.ServerGetDataMarkResponse, new byte[] { (byte)CurrentMemoirField.MEMOIR_DATE_CHANGE });
                            listener.SendPacket((byte)TcpCommands.ServerGetDataResponse, memoir.MEMOIR_DATE_CHANGE.ToString());
                            listener.SendPacket((byte)TcpCommands.ServerGetDataMarkResponse, new byte[] { (byte)CurrentMemoirField.MEMOIR_TEXT });
                            listener.SendPacket((byte)TcpCommands.ServerGetDataResponse, memoir.MEMOIR_TEXT);
                            listener.SendPacket((byte)TcpCommands.ServerGetDataMarkResponse, new byte[] { (byte)CurrentMemoirField.MEMOIR_ID });
                            listener.SendPacket((byte)TcpCommands.ServerGetDataResponse, memoir.MEMOIR_ID.ToString());
                            listener.SendPacket((byte)TcpCommands.ServerGetDataMarkResponse, new byte[] { (byte)CurrentMemoirField.MEMOIR_TITLE });
                            listener.SendPacket((byte)TcpCommands.ServerGetDataResponse, memoir.MEMOIR_TITLE);
                            counter++;
                        }
                        listener.SendPacket((byte)TcpCommands.ServerOK);
                        return;
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientMarkFieldQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        byte mark = packet.Content[0];
                        switch (mark)
                        {
                            case (byte)CurrentMemoirField.MEMOIR_TEXT:
                                dictionaryAgents[listener.Guid].CurrentField = CurrentMemoirField.MEMOIR_TEXT;
                                break;

                            case (byte)CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                dictionaryAgents[listener.Guid].CurrentField = CurrentMemoirField.MEMOIR_DATE_CHANGE;
                                break;

                            case (byte)CurrentMemoirField.MEMOIR_ID:
                                dictionaryAgents[listener.Guid].CurrentField = CurrentMemoirField.MEMOIR_ID;
                                break;

                            case (byte)CurrentMemoirField.MEMOIR_TITLE:
                                dictionaryAgents[listener.Guid].CurrentField = CurrentMemoirField.MEMOIR_TITLE;
                                break;
                        }
                        return;
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientMarkTotalMemoirQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        int total = Convert.ToInt32(AgentRelay.MakeStringFromPacketContents(packet));
                        if (dictionaryAgents[listener.Guid].TotalMemoir < total)
                        {
                            dictionaryAgents[listener.Guid].Content.MEMOIRS.Add(new MEMOIRS() { MEMOIR_DATE_RECORD = DateTime.Now });
                            dictionaryAgents[listener.Guid].TotalMemoir++;
                        }
                        return;
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientMarkCurrentMemoirQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        int current = Convert.ToInt32(AgentRelay.MakeStringFromPacketContents(packet));
                        dictionaryAgents[listener.Guid].CurrentMemoir = current;
                        return;
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientUpdateEndQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        try
                        {
                            context.SaveChanges();
                            listener.SendPacket((byte)TcpCommands.ServerOK, "Данные записаны.");
                            return;
                        }
                        catch (Exception ex) { }
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientUpdateDataQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        string content = AgentRelay.MakeStringFromPacketContents(packet);
                        var listMemoir = dictionaryAgents[listener.Guid].Content.MEMOIRS.ToList();
                        var memoir = listMemoir[dictionaryAgents[listener.Guid].CurrentMemoir];

                        switch (dictionaryAgents[listener.Guid].CurrentField)
                        {
                            case CurrentMemoirField.MEMOIR_TEXT:
                                memoir.MEMOIR_TEXT = content;
                                break;

                            case CurrentMemoirField.MEMOIR_DATE_CHANGE:
                                memoir.MEMOIR_DATE_CHANGE = Convert.ToDateTime(content);
                                break;

                            case CurrentMemoirField.MEMOIR_TITLE:
                                memoir.MEMOIR_TITLE = content;
                                break;
                        }
                        return;
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientDeleteDataQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        var memoir = from mem in dictionaryAgents[listener.Guid].Content.MEMOIRS
                                     where mem.MEMOIR_ID == Convert.ToInt64(AgentRelay.MakeStringFromPacketContents(packet))
                                     select mem;

                        if (memoir.Count() == 0)
                        {
                            listener.SendPacket((byte)TcpCommands.ServerFailed, "Такая запись не существует.");
                            return;
                        }

                        context.MEMOIRS.Remove(memoir.First());
                        context.SaveChanges();
                        dictionaryAgents[listener.Guid].TotalMemoir--;
                        listener.SendPacket((byte)TcpCommands.ServerOK, "Воспаминание удаленно.");
                        return;
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;
                case (byte)TcpCommands.ClientDeleteDataLastQuery:
                    if (dictionaryAgents[listener.Guid].Verified)
                    {
                        var memoir = dictionaryAgents[listener.Guid].Content.MEMOIRS;

                        context.MEMOIRS.Remove(memoir.Last());
                        context.SaveChanges();
                        dictionaryAgents[listener.Guid].TotalMemoir--;
                        listener.SendPacket((byte)TcpCommands.ServerOK, "Воспаминание удаленно.");
                        return;
                    }
                    listener.SendPacket((byte)TcpCommands.ServerFailed, "Необходимо авторизоваться.");
                    break;

                case (byte)TcpCommands.ClientRegistrationQuery:
                    string login = dictionaryAgents[listener.Guid].Login;
                    if (context.USERS.ToList().Exists(u => u.USER_LOGIN == login))
                    {
                        listener.SendPacket((byte)TcpCommands.ServerRegistrationFailed, "Такой пользователь уже зарегестрирован.");
                        return;
                    }

                    context.USERS.Add(new USERS
                    {
                        USER_LOGIN = login,
                        USER_HASH = GetHash(AgentRelay.MakeStringFromPacketContents(packet)),
                        REGISTRATION_DATE = DateTime.Now,
                        USER_GUID = Guid.NewGuid()
                    });
                    context.SaveChanges();
                    listener.SendPacket((byte)TcpCommands.ServerRegistrationOK);
                    break;

                case (byte)TcpCommands.ClientBye:
                    AgentDisconnected("Agent Disconnected! GUID -> " + listener.Guid
                            + ", User login -> " + dictionaryAgents[listener.Guid].Login
                            + ", Date -> " + DateTime.Now);
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
            public int TotalMemoir { get; set; }
            public int CurrentMemoir { get; set; }
            public CurrentMemoirField CurrentField { get; set; } = CurrentMemoirField.NONE;
        }
    }
}