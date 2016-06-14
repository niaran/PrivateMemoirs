using System;
using CommandLine;
using CommandLine.Text;

namespace PrivateMemoirs
{
    class PrivateMemoirsConsole
    {
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                PrivateMemoirsServer memoirsServer;
                var options = new Options();
                if (Parser.Default.ParseArguments(args, options))
                {
                    memoirsServer = new PrivateMemoirsServer(options.msSqlHostNameOrAddress,
                        options.listeningIPAddress, options.listeningPort, options.loginMsSql,
                        options.passMsSql, options.dbName);
                    memoirsServer.NewAgentСonnected += MemoirsServer_NewAgentСonnected;
                    memoirsServer.AgentDisconnected += MemoirsServer_AgentDisconnected;
                    memoirsServer.PackageOn += MemoirsServer_PackageOn;
                    ;
                    Console.CancelKeyPress += delegate
                    {
                        Console.WriteLine("Private Memoirs Server has stopping...");
                        memoirsServer.Stop();
                    };
                    Console.WriteLine("Private Memoirs Server has been started!\nPress Ctrl+C to Stop it.");
                    memoirsServer.Start();
                }
            }
            else
            {
                Environment.Exit(1);
            }
        }

        private static void MemoirsServer_PackageOn(General.TcpCommands com, string cont)
        {
            Console.WriteLine("Команда -> " + com + ", Содержимое ->" + cont);
        }

        private static void MemoirsServer_AgentDisconnected(string disAgent)
        {
            Console.WriteLine(disAgent);
        }

        private static void MemoirsServer_NewAgentСonnected(string conAgent)
        {
            Console.WriteLine(conAgent);
        }

        class Options
        {
            [Option('s', "msSqlHostNameOrAddress", DefaultValue = "127.0.0.1",
                HelpText = "The IP address or domain name of microsoft sql server")]
            public string msSqlHostNameOrAddress { get; set; }

            [Option('i', "listeningIPAddress", Required = true,
                HelpText = "The local IP address on which to listen for incoming connection.")]
            public string listeningIPAddress { get; set; }

            [Option('o', "listeningPort", Required = true,
                HelpText = "Local port on which to listen for incoming connection.")]
            public short listeningPort { get; set; }

            [Option('l', "loginMsSql", DefaultValue = "PrivateNotes",
                HelpText = "Login to enter to the microsoft sql server")]
            public string loginMsSql { get; set; }
            
            [Option('p', "passMsSql", DefaultValue = "PrivateNotes",
                HelpText = "Password to enter to the microsoft sql server")]
            public string passMsSql { get; set; }

            [Option('d', "dbName", DefaultValue = "MEMOIRS_DB",
                HelpText = "Password to enter to the microsoft sql server")]
            public string dbName { get; set; }

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