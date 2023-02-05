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
        public Sender(SenderCfg config, List<UdpClient> listeners)
        {
            var sendFrom = IPEndPoint.Parse(config.SenderFromAddress);

            var existingClient = listeners
                                    .FirstOrDefault(listener => listener.Client.LocalEndPoint.Equals(sendFrom));

            if (existingClient == null)
            {
                UsedExistingListener = false;

                SendClient = new UdpClient(sendFrom)
                {
                    EnableBroadcast = true
                };
            }
            else
            {
                UsedExistingListener = true;
                SendClient = existingClient;
            }

            Destinations = config
                                .Destinations
                                .Select(destinationEndpointStr => IPEndPoint.Parse(destinationEndpointStr))
                                .ToList();
        }

        public readonly bool UsedExistingListener;

        public void Send(string receiveChain, UdpClient receivedBy, byte[] data)
        {
            Destinations
                .ForEach(dest =>
                {
                    string str;
                    if (UsedExistingListener)
                    {
                        //use the client that received this message, as a sender
                        str = $"{receiveChain} -> {dest}     {data.Length:N0} bytes";
                        receivedBy.Send(data, data.Length, dest);
                    }
                    else
                    {
                        str = $"{receiveChain} -> {SendClient.Client.LocalEndPoint} -> {dest}    {data.Length:N0} bytes";
                        SendClient.Send(data, data.Length, dest);
                    }

                    Program.Log(str);
                });
        }

        public UdpClient SendClient;
        public List<IPEndPoint> Destinations;
    }
}
