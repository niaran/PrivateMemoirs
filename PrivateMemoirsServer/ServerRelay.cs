using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PrivateMemoirs
{
    /// <summary>
    /// This class is provided to handle any connection requests from clients 
    /// This class is capable of sending a broadcast message to all connected agents...
    /// </summary>
    public class ServerRelay : NetComm
    {
        public delegate void NewAgentConnected(AgentRelay agentRelay);
        public delegate void ServerFailedToAcceptConnection(Exception ex);
        public event NewAgentConnected OnNewAgentConnected;                             // Handle this event immedietly
        public event ServerFailedToAcceptConnection OnServerFailedToAcceptConnection;   // Handle this event immedietly

        private static List<AgentRelay> m_Agents = new List<AgentRelay>();

        private Thread m_MainThread = null;
        private AutoResetEvent m_evStop;
        private bool m_AcceptIncommingConnections = false;
        private TcpListener m_tcpListener = null;

        private bool m_bEnableAutoHandshake = false;
        private bool m_bAutoCleanupConnections = false;

        public ServerRelay(bool bEnableAutoHandshake)
            : base(false)
        {
            m_bEnableAutoHandshake = bEnableAutoHandshake;
        }

        /// <summary>
        /// This function starts server on the given port and ip address
        /// </summary>
        /// <param name="listeningIPAddress">if you pasas null, it will start listening on ANY ip address that is available</param>
        public void StartServer(string listeningIPAddress, int listeningPort)
        {
            m_tcpListener = new TcpListener(listeningIPAddress == null ? System.Net.IPAddress.Any : System.Net.IPAddress.Parse(listeningIPAddress), listeningPort);
            m_evStop = new AutoResetEvent(false);

            m_MainThread = new Thread(new ThreadStart(WorkerThread));
            m_MainThread.Name = "ServerRelay";

            m_MainThread.Start();
        }

        public override void Dispose()
        {
            StopServer(true);
            base.Dispose();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        /// <param name="bAutoCleanupConnections">which means the server will close all currently active agents</param>
        public void StopServer(bool bAutoCleanupConnections)
        {
            if (m_MainThread == null || !m_MainThread.IsAlive)
                return;

            m_bAutoCleanupConnections = bAutoCleanupConnections;

            m_evStop.Set();
            m_MainThread.Join();    // Wait till it ends...

            try { m_tcpListener.Stop(); }
            catch { }

            m_tcpListener = null;
            m_MainThread = null;
        }

        /// <summary>
        /// Main server thread
        /// </summary>
        private void WorkerThread()
        {
            DateTime dtLastActivityReportSent = DateTime.Now;
            TimeSpan tsGuardTime = TimeSpan.FromSeconds(15);

            while (!m_evStop.WaitOne(500))
            {
                lock (m_tcpListener)
                {
                    if (m_AcceptIncommingConnections)
                    {
                        if (m_tcpListener.Pending())
                        {
                            try
                            {
                                AgentRelay agent = AgentRelay.FromClient(m_tcpListener.AcceptTcpClient());
                                if (agent.IsFaulty)
                                {
                                    agent.Disconnect();
                                    agent = null;
                                }
                                else
                                {
                                    if (OnNewAgentConnected != null)
                                        OnNewAgentConnected(agent);

                                    // Append it to agents list
                                    lock (m_Agents)
                                    {
                                        m_Agents.Add(agent);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (OnServerFailedToAcceptConnection != null)
                                    OnServerFailedToAcceptConnection(ex);
                            }
                        }
                    }
                } // lock

                // Remove Faulties 91/08/01
                for (int i = 0; i < m_Agents.Count; i++)
                {
                    if (m_Agents[i].IsFaulty)
                    {
                        m_Agents[i].Disconnect();
                        m_Agents[i] = null;
                        m_Agents.RemoveAt(i);
                        --i;
                    }
                }

                // Report clients about us
                if (m_bEnableAutoHandshake && (DateTime.Now - dtLastActivityReportSent > tsGuardTime))
                {
                    for (int i = 0; i < m_Agents.Count; i++)
                    {
                        // It is clear, if the other side is not accessable, it will become Faulty 
                        // so there is no need to check handshake result for that.
                        try { m_Agents[i].StartHandshakeAsync(); }
                        catch { }
                    }
                    dtLastActivityReportSent = DateTime.Now;
                }
            }

            // Close all connections and cleanup...
            if (m_bAutoCleanupConnections)
            {
                lock (m_Agents)
                {
                    for (int i = 0; i < m_Agents.Count; i++)
                    {
                        m_Agents[i].Dispose();
                        m_Agents[i] = null;
                    }
                    m_Agents.Clear();
                }
            }
        }

        /// <summary>
        /// AcceptIncommingConnections
        /// </summary>
        public bool AcceptIncommingConnections
        {
            get { return m_AcceptIncommingConnections; }
            set
            {
                lock (m_tcpListener)
                {
                    m_AcceptIncommingConnections = value;
                    if (m_AcceptIncommingConnections)
                        m_tcpListener.Start(5);
                    else
                        m_tcpListener.Stop();
                }
            }
        }


        /// <summary>
        /// It will send all connected clients a specific message
        /// </summary>
        /// <param name="cmdCode">can be one of the <code>eResponseTypes</code> codes or any user defined codes</param>
        /// <param name="content">can be null</param>
        /// <param name="excludedAgent">message will not be sent to this specific agent, can be null</param>
        /// <param name="desc"></param>
        public void BroadcastMessage(int cmdCode, string content, AgentRelay excludedAgent)
        {
            lock (m_Agents)
            {
                foreach (AgentRelay agent in m_Agents)
                {
                    if (agent == null || agent.IsFaulty)
                        continue;

                    try
                    {
                        if (excludedAgent != null && excludedAgent == agent)
                            continue;
                        if (content == null)
                            agent.SendPacket(cmdCode);
                        else
                            agent.SendPacket(cmdCode, content);
                    }
                    catch { }
                }
            }
        }
    }
}