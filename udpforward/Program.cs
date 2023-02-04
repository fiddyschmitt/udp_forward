using CommandLine;
using System.Net;
using System.Text.RegularExpressions;
using udpforward.CLI;

namespace udpforward
{
    internal class Program
    {
        const string PROGRAM_NAME = "udpforwarder";
        const string VERSION = "1.0";

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(o =>
               {
                   if (o.PrintVersion)
                   {
                       Log($"{PROGRAM_NAME} {VERSION}");
                       Environment.Exit(0);
                   }

                   var forwarders = o
                                    .Forwards
                                    .Select(forward =>
                                    {
                                        var tokens = forward.Split(' ');

                                        Forwarder forwarder;
                                        if (tokens.Length == 2)
                                        {
                                            forwarder = new Forwarder(tokens[0], null, tokens[1]);
                                        }
                                        else
                                        {
                                            forwarder = new Forwarder(tokens[0], tokens[1], tokens[2]);
                                        }

                                        return forwarder;
                                    })
                                    .ToList();

                   forwarders
                    .ForEach(f => f.Start());

                   while (true)
                   {
                       Thread.Sleep(1000);
                   }
               });
        }

        public static void Log(string str)
        {
            var logEntry = $"{DateTime.Now}: {str}";
            Console.WriteLine(logEntry);
        }
    }
}