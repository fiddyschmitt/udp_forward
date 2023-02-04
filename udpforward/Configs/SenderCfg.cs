using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace udpforward.Configs
{
    public class SenderCfg
    {
        public string SenderFromAddress { get; set; } = string.Empty;
        public List<string> Destinations { get; set; } = new();
    }
}
