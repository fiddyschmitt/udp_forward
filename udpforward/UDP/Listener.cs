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
    public class Listener
    {
        public Listener(ForwardCfg config)
        {
            Listeners = config
                            .Listeners
                            .Select(listenEndpointStr => IPEndPoint.Parse(listenEndpointStr))
                            .Select(listenEndpoint => new UdpClient(listenEndpoint)
                            {
                                EnableBroadcast = true
                            })
                            .ToList();

            Senders = config
                        .Senders
                        .Select(sender => new Sender(sender))
                        .ToList();

            foreach (var listener in Listeners)
            {
                foreach (var sender in Senders)
                {
                    foreach (var destination in sender.Destinations)
                    {
                        string str;
                        if (sender.SendClient == null)
                        {
                            str = $"Initialised {listener.Client.LocalEndPoint} -> {destination}";
                        }
                        else
                        {
                            str = $"Initialised {listener.Client.LocalEndPoint} -> {sender.SendClient.Client.LocalEndPoint} -> {destination}";
                        }
                        Program.Log(str);
                    }
                }
            }
        }

        readonly List<UdpClient> Listeners;
        readonly List<Sender> Senders;

        public void Start()
        {
            Listeners
                .ForEach(listener =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                                var data = listener.Receive(ref remoteIpEndPoint);

                                Senders
                                    .ForEach(sender =>
                                    {
                                        var receiveChain = $"{remoteIpEndPoint} -> {listener.Client.LocalEndPoint}";
                                        sender.Send(receiveChain, listener, data);
                                    });
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.Log($"Listener error: {ex}");
                        }
                    });
                });
        }
    }
}
