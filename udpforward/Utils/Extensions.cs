using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace udpforward.Utils
{
    public static class Extensions
    {
        public static uint ToUInt32(this IPAddress ip)
        {
            byte[] fromIPBytes = ip.GetAddressBytes();
            Array.Reverse(fromIPBytes);
            uint result = BitConverter.ToUInt32(fromIPBytes, 0);
            return result;
        }

        public static IPAddress ToIPAddress(this uint val)
        {
            byte[] bytes = BitConverter.GetBytes(val);
            Array.Reverse(bytes); // flip little-endian to big-endian(network order)
            var result = new IPAddress(bytes);
            return result;
        }
    }
}
