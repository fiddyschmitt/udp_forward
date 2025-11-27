using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using udpforward.Configs;
using udpforward.Utils;

namespace udpforward.UDP
{
    public class Listener
    {
        public Listener(ForwardCfg config)
        {
            Listeners = config
                            .Listeners
                            .Select(listenEndpointStr => IPEndPoint.Parse(listenEndpointStr))
                            .Select(listenEndpoint =>
                            {
                                var client = new UdpClient()
                                {
                                    EnableBroadcast = true
                                };

                                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                                client.Client.Bind(listenEndpoint);

                                config
                                    .JoinMulticastGroups
                                    .ForEach(multicastAddressStr =>
                                    {
                                        var multicastAddress = IPAddress.Parse(multicastAddressStr);
                                        client.JoinMulticastGroup(multicastAddress, listenEndpoint.Address);

                                        Program.Log($"{listenEndpoint} joined multicast group {multicastAddressStr}");
                                    });

                                return client;
                            })
                            .ToList();

            Senders = config
                        .Senders
                        .Select(sender => new Sender(sender, Listeners))
                        .ToList();

            foreach (var listener in Listeners)
            {
                foreach (var sender in Senders)
                {
                    foreach (var destination in sender.Destinations)
                    {
                        string str;
                        if (sender.UsedExistingListener)
                        {
                            str = $"Initialised: Client -> {listener.Client.LocalEndPoint} -> {destination}";
                        }
                        else
                        {
                            str = $"Initialised: Client -> {listener.Client.LocalEndPoint} -> {sender.SendClient.Client.LocalEndPoint} -> {destination}";
                        }
                        Program.Log(str);
                    }

                    if (config.Bidirectional)
                    {
                        if (sender.UsedExistingListener)
                        {
                            //str = $"Initialised {listener.Client.LocalEndPoint} <- {destination}";
                        }
                        else
                        {
                            var str = $"Initialised: {listener.Client.LocalEndPoint} <- {sender.SendClient.Client.LocalEndPoint} <- Client";
                            Program.Log(str);
                        }
                    }
                }
            }
            Config = config;
        }

        readonly List<UdpClient> Listeners;
        readonly Dictionary<IPEndPoint, UdpClient> OriginToReceivedOn = [];
        readonly List<Sender> Senders;

        public ForwardCfg Config { get; }

        public void Start()
        {
            StartListeners();

            if (Config.Bidirectional)
            {
                ListenForResponses();
            }
        }

        void StartListeners()
        {
            var recentDatagrams = new MemoryCache(Guid.NewGuid().ToString());

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

                                if (IsDuplicate(Config.DedupeWindowMilliseconds, recentDatagrams, data))
                                {
                                    var packetStr = $"[{remoteIpEndPoint}] -> [{listener.Client.LocalEndPoint}] [{data.Length:N0} bytes payload]";
                                    Program.Log($"{packetStr} Dropping duplicate packet.");
                                }
                                else
                                {
                                    OriginToReceivedOn.TryAdd(remoteIpEndPoint, listener);
                                    Senders
                                        .ForEach(sender =>
                                        {
                                            var receiveChain = $"{remoteIpEndPoint} -> {listener.Client.LocalEndPoint}";
                                            sender.Send(receiveChain, listener, data);
                                        });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.Log($"StartListeners error: {ex}");
                        }
                    });
                });
        }

        void ListenForResponses()
        {
            var recentDatagrams = new MemoryCache(Guid.NewGuid().ToString());

            Senders
                .ToList()
                .Where(sender => !sender.UsedExistingListener)
                .ToList()
                .ForEach(sender =>
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                                //var remoteIpEndPoint = destination;

                                byte[] data;
                                try
                                {
                                    data = sender.SendClient.Receive(ref remoteIpEndPoint);
                                }
                                catch
                                {
                                    //this is normal. It's likely an ICMP coming back saying that the destination (sent to earlier) didn't exist
                                    continue;
                                }

                                if (IsDuplicate(Config.DedupeWindowMilliseconds, recentDatagrams, data))
                                {
                                    var packetStr = $"[{remoteIpEndPoint}] -> [{sender.SendClient.Client.LocalEndPoint}] [{data.Length:N0} bytes payload]";
                                    Program.Log($"{packetStr} Dropping duplicate packet.");
                                }
                                else
                                {
                                    Listeners
                                    .ForEach(listener =>
                                    {
                                        var originsForListener = OriginToReceivedOn
                                                                    .Where(o => o.Key.Equals(remoteIpEndPoint))
                                                                    .Select(o => o.Key)
                                                                    .ToList();

                                        originsForListener
                                            .ForEach(originEndpoint =>
                                            {
                                                var receiveChain = $"{originEndpoint} <- {listener.Client.LocalEndPoint} <- {sender.SendClient.Client.LocalEndPoint} <- {remoteIpEndPoint}";
                                                Program.Log($"{receiveChain}    {data.Length:N0} bytes");
                                                listener.Send(data, data.Length, originEndpoint);
                                            });
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Program.Log($"ListenForResponses error: {ex}");
                        }
                    });
                });
        }

        static bool IsDuplicate(int? dedupeWindowMilliseconds, MemoryCache recentDatagrams, byte[] payload)
        {
            if (dedupeWindowMilliseconds == null) return false;

            var payloadHex = payload.ToHexString();
            var isDuplicate = recentDatagrams.Contains(payloadHex);

            if (!isDuplicate)
            {
                recentDatagrams.Set(payloadHex, new object(), DateTimeOffset.UtcNow.AddMilliseconds(dedupeWindowMilliseconds.Value));
            }

            return isDuplicate;
        }
    }
}
