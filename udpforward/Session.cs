using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using udpforward.CLI;

namespace udpforward
{
    public class Session
    {
        public Session(string listenIP, string forwarderIP, List<IPAddress> destinationIPs, List<int> ports)
        {
            ListenIP = listenIP;
            ForwarderIP = forwarderIP;
            DestinationIPs = destinationIPs;
            Ports = ports;
        }

        public string ListenIP { get; }
        public string ForwarderIP { get; }
        public List<IPAddress> DestinationIPs { get; }
        public List<int> Ports { get; }

        public void Start()
        {
            var forwarders = Ports
                                .Select(port =>
                                {
                                    UdpClient listener;
                                    try
                                    {
                                        var listenerEndpoint = IPEndPoint.Parse($"{ListenIP}:{port}");
                                        listener = new UdpClient(listenerEndpoint)
                                        {
                                            EnableBroadcast = true
                                        };
                                    }
                                    catch (Exception ex)
                                    {
                                        Program.Log($"FATAL. Could not initialise local listener: {ListenIP}");
                                        Program.Log(ex.ToString());
                                        Environment.Exit(1);
                                        throw;
                                    }

                                    Program.Log($"Created listener: {listener.Client.LocalEndPoint}");

                                    UdpClient forwarderClient;

                                    if (string.IsNullOrEmpty(ForwarderIP))
                                    {
                                        forwarderClient = listener;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var forwarderEndpoint = IPEndPoint.Parse($"{ForwarderIP}:{port}");
                                            forwarderClient = new UdpClient(forwarderEndpoint)
                                            {
                                                EnableBroadcast = true
                                            };
                                        }
                                        catch (Exception ex)
                                        {
                                            Program.Log($"FATAL. Could not initialise forwarder: {ForwarderIP}");
                                            Program.Log(ex.ToString());
                                            Environment.Exit(1);
                                            throw;
                                        }
                                    }

                                    var destinations = DestinationIPs
                                                        .Select(destinationIP => new IPEndPoint(destinationIP, port))
                                                        .ToList();

                                    var forwarder = new Forwarder(listener, forwarderClient, destinations);
                                    return forwarder;
                                })
                                .ToList();

            Program.Log($"Initialised {forwarders.Count:N0} forwarders.");

            forwarders
                .ForEach(forward =>
                {
                    forward.Start();
                });
        }
    }
}
