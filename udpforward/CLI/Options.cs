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

        [Option('c', "config", Required = true, HelpText = "The config file specifying the forwards to perform.")]
        public string ConfigFilename { get; set; } = string.Empty;
    }
}
