using System.Configuration;
using System.ServiceProcess;
using System;

namespace PrivateMemoirs
{
    partial class PrivateMemoirsService : ServiceBase
    {
        private PrivateMemoirsServer memoirsServer;

        public PrivateMemoirsService()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            string msSqlHostNameOrAddress = ConfigurationManager.AppSettings["msSqlHostNameOrAddress"];
            string listeningIPAddress = ConfigurationManager.AppSettings["listeningIPAddress"];
            string listeningPort = ConfigurationManager.AppSettings["listeningPort"];
            string loginMsSql = ConfigurationManager.AppSettings["loginMsSql"];
            string passMsSql = ConfigurationManager.AppSettings["passMsSql"];
            string dbName = ConfigurationManager.AppSettings["dbName"];

            memoirsServer = new PrivateMemoirsServer(msSqlHostNameOrAddress, listeningIPAddress,
                Convert.ToInt16(listeningPort), loginMsSql, passMsSql, dbName);
            memoirsServer.Start();
        }

        protected override void OnStop()
        {
            memoirsServer.Stop();
        }
    }
}