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
                        options.listeningIPAddress, options.listeningPort);
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

        class Options
        {
            [Option('m', "msSqlHostNameOrAddress", DefaultValue = "127.0.0.1",
                HelpText = "The IP address or domain name of microsoft sql server")]
            public string msSqlHostNameOrAddress { get; set; }

            [Option('l', "listeningIPAddress", Required = true,
                HelpText = "The local IP address on which to listen for incoming connection.")]
            public string listeningIPAddress { get; set; }

            [Option('p', "listeningPort", Required = true,
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