using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using udpforward.Configs;

namespace udpforward.UDP
{
    public class Sender
    {
        public Sender(SenderCfg config)
        {
            if (!string.IsNullOrEmpty(config.SenderFromAddress))
            {
                var sendFrom = IPEndPoint.Parse(config.SenderFromAddress);
                SendClient = new UdpClient(sendFrom)
                {
                    EnableBroadcast = true
                };
            }

            Destinations = config
                                .Destinations
                                .Select(destinationEndpointStr => IPEndPoint.Parse(destinationEndpointStr))
                                .ToList();
        }

        public void Send(string receiveChain, UdpClient receivedBy, byte[] data)
        {
            Destinations
                .ForEach(dest =>
                {
                    string str;
                    if (SendClient == null)
                    {
                        //use the client that received this message, as a sender
                        str = $"{receiveChain} -> {dest}";
                        receivedBy.Send(data, data.Length, dest);
                    }
                    else
                    {
                        str = $"{receiveChain} -> {SendClient.Client.LocalEndPoint} -> {dest}";
                        SendClient.Send(data, data.Length, dest);
                    }

                    Program.Log(str);

                });
        }

        public UdpClient? SendClient;
        public List<IPEndPoint> Destinations;
    }
}
