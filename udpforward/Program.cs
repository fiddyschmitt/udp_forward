using CommandLine;
using System.Net;
using System.Text.RegularExpressions;
using udpforward.CLI;

namespace udpforward
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(o =>
               {
                   var destinationIPs = Utils.RangeUtils.ExtractIPs(o.DestinationIPs).ToList();
                   var ports = Utils.RangeUtils.ExtractPorts(o.Ports).ToList();

                   var session = new Session(o.ListenIP, o.ForwarderIP, destinationIPs, ports);
                   session.Start();

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