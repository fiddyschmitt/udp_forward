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
        [Option('v', "version", Required = false, HelpText = "Print the version and exit.")]
        public bool PrintVersion { get; set; }

        [Option('f', "forward", Required = true, HelpText = "local [forwarder] remote\n" +
            "    local = The local endpoint on which to listen for UDP data.\n" +
            "    forwarder = The local endpoint from which to send data to the destination.\n" +
            "    remote = The remote endpoint to forward the data to.\n\n" +
            "Example -f \"127.0.0.1:11000 192.168.1.1:15000 192.168.1.20:11000\"")]
        public IEnumerable<string> Forwards { get; set; } = new List<string>();
    }
}
