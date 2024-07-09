using CommandLine;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;
using udpforward.CLI;
using udpforward.Configs;
using udpforward.UDP;

namespace udpforward
{
    internal class Program
    {
        const string PROGRAM_NAME = "udpforward";
        const string VERSION = "1.4.0";

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Options))]
        static void Main(string[] args)
        {
            Log($"{PROGRAM_NAME} {VERSION}");

            /*
            var exampleCfg = new Config()
            {
                Forwards = new List<ForwardCfg>() {
                    new ForwardCfg() {
                        Listeners = new List<string>() { "192.168.56.1:11000" },
                        JoinMulticastGroups = new List<string>() {"239.0.0.1"},
                        Senders = new List<SenderCfg>()
                        {
                            new SenderCfg()
                            {
                                SenderFromAddress = "192.168.56.1:12000",
                                Destinations = new List<string>()
                                {
                                    "192.168.56.120:13000"
                                }
                            }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(exampleCfg, Formatting.Indented);

            */


            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(o =>
               {
                   if (o.PrintVersion)
                   {
                       Log($"{PROGRAM_NAME} {VERSION}");
                       Environment.Exit(0);
                   }

                   var configJson = File.ReadAllText(o.ConfigFilename);
                   var config = JsonConvert.DeserializeObject<Config>(configJson);

                   if (config == null)
                   {
                       Program.Log("FATAL: Config not loaded.");
                       Environment.Exit(1);
                   }

                   var listeners = config
                                    .Forwards
                                    .Select(fwd => new Listener(fwd))
                                    .ToList();

                   listeners
                        .ForEach(lst => lst.Start());

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