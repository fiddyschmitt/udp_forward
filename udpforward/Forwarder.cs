using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace udpforward
{
    public class Forwarder
    {
        public Forwarder(UdpClient listener, UdpClient forwarder, List<IPEndPoint> destinations)
        {
            Listener = listener;
            ForwarderClient = forwarder;
            Destinations = destinations;
        }

        public UdpClient Listener { get; }
        public UdpClient ForwarderClient { get; }
        public List<IPEndPoint> Destinations { get; }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                        var data = Listener.Receive(ref remoteIpEndPoint);

                        Destinations
                            .ForEach(destination =>
                            {
                                Program.Log($"{remoteIpEndPoint} -> {Listener.Client.LocalEndPoint} -> {ForwarderClient.Client.LocalEndPoint} -> {destination}    {data.Length:N0} bytes");
                                ForwarderClient.Send(data, data.Length, destination);
                            });
                    }
                }
                catch (Exception ex)
                {
                    Program.Log($"UdpServer error: {ex}");
                }
            });
        }
    }
}
