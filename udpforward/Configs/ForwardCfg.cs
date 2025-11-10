using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace udpforward.Configs
{
    public class ForwardCfg
    {
        public List<string> Listeners { get; set; } = new();
        public List<string> JoinMulticastGroups { get; set; } = new();
        public List<SenderCfg> Senders { get; set; } = new();

        public bool Bidirectional = false;

        public int? DedupeWindowMilliseconds { get; set; } = null;
    }
}
