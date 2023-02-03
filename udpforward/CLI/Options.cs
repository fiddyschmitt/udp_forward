using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace udpforward.CLI
{
    public class Options
    {
        [Option('l', "listen", Required = true, HelpText = "The local IP on which to listen for UDP data. Example --listen 127.0.0.1")]
        public string ListenIP { get; set; }

        [Option('f', "forwarder", Required = true, HelpText = "The local IP from which to send data to the destination. Example --forwarder 192.168.1.1")]
        public string ForwarderIP { get; set; }

        [Option('d', "destinations", Required = true, HelpText = "A list of remote IPs to forward the data to. Example --destinations 192.168.1.10-20")]
        public string DestinationIPs { get; set; }

        [Option('p', "ports", Required = true, HelpText = "The ports to listen and forward. Example --ports 11000-11050")]
        public string Ports { get; set; }
    }
}
