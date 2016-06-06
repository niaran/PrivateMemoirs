using System;
using CommandLine;
using CommandLine.Text;
using System.ServiceProcess;

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
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                memoirsServer = new PrivateMemoirsServer(options.msSqlHostNameOrAddress,
                    options.listeningIPAddress, options.listeningPort);
                memoirsServer.Start();
            }
        }

        protected override void OnStop()
        {
            memoirsServer.Stop();
        }

        class Options
        {
            [Option('m', "msSqlHostNameOrAddress", DefaultValue = "127.0.0.1",
                HelpText = "The IP address or domain name of microsoft sql server")]
            public string msSqlHostNameOrAddress { get; set; }

            [Option('l', "listeningIPAddress", DefaultValue = "127.0.0.1",
                HelpText = "The local IP address on which to listen for incoming connection.")]
            public string listeningIPAddress { get; set; }

            [Option('p', "listeningPort", DefaultValue = (short)8877,
                HelpText = "Local port on which to listen for incoming connection.")]
            public short listeningPort { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                    (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
    }
}