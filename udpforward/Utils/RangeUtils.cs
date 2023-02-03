using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace udpforward.Utils
{
    public static class RangeUtils
    {
        public static IEnumerable<IPAddress> ExtractIPs(string ipItemList)
        {
            var listRegex = new Regex("(.*?)-(.*)");
            IEnumerable<IPAddress> result = new List<IPAddress>();

            string[] tokens = ipItemList.Split(',');

            tokens
                .Select(t => t.Trim())
                .ToList()
                .ForEach(item =>
                {
                    if (IPAddress.TryParse(item, out IPAddress IP))
                    {
                        //It's a normal IP
                        var l = new List<IPAddress> { IP };
                        result = result.Concat(l.AsEnumerable());
                    }
                    else
                    {
                        Match m = listRegex.Match(item);

                        if (m.Success)
                        {
                            IPAddress fromIP = IPAddress.Parse(m.Groups[1].Value.Trim());
                            IPAddress toIP = IPAddress.Parse(m.Groups[2].Value.Trim());

                            uint fromIPNumeric = fromIP.ToUInt32();
                            uint toIPIntNumeric = toIP.ToUInt32();

                            var range = ToRange(fromIPNumeric, toIPIntNumeric).Select(n => n.ToIPAddress());
                            result = result.Concat(range);
                        }
                    }
                });

            return result;
        }

        public static IEnumerable<int> ExtractPorts(string portItemList)
        {
            var listRegex = new Regex("(.*?)-(.*)");
            IEnumerable<int> result = new List<int>();

            string[] tokens = portItemList.Split(',');

            tokens
                .Select(t => t.Trim())
                .ToList()
                .ForEach(item =>
                {
                    if (int.TryParse(item, out int port))
                    {
                        //It's a normal IP
                        var l = new List<int> { port };
                        result = result.Concat(l.AsEnumerable());
                    }
                    else
                    {
                        Match m = listRegex.Match(item);

                        if (m.Success)
                        {
                            int startPort = int.Parse(m.Groups[1].Value.Trim());
                            int endPort = int.Parse(m.Groups[2].Value.Trim());

                            result = result.Concat(Enumerable.Range(startPort, endPort - startPort + 1));
                        }
                    }
                });

            return result;
        }

        public static IEnumerable<uint> ToRange(uint minInclusive, uint maxInclusive)
        {
            for (uint i = minInclusive; i <= maxInclusive; i++)
            {
                yield return i;
            }
        }
    }
}
