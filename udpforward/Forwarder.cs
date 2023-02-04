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
        public Forwarder(string listenEndpointString, string? forwarderEndpointString, string destinationEndpointString)
        {
            try
            {
                var listenerEndpoint = IPEndPoint.Parse(listenEndpointString);
                Listener = new UdpClient(listenerEndpoint)
                {
                    EnableBroadcast = true
                };
            }
            catch (Exception ex)
            {
                Program.Log($"FATAL. Could not initialise local listener: {listenEndpointString}");
                Program.Log(ex.ToString());
                Environment.Exit(1);
                throw;
            }

            if (string.IsNullOrEmpty(forwarderEndpointString))
            {
                ForwarderClient = Listener;
            }
            else
            {
                try
                {
                    var forwarderEndpoint = IPEndPoint.Parse(forwarderEndpointString);
                    ForwarderClient = new UdpClient(forwarderEndpoint)
                    {
                        EnableBroadcast = true
                    };
                }
                catch (Exception ex)
                {
                    Program.Log($"FATAL. Could not initialise forwarder: {forwarderEndpointString}");
                    Program.Log(ex.ToString());
                    Environment.Exit(1);
                    throw;
                }
            }

            Destination = IPEndPoint.Parse(destinationEndpointString);

            if (string.IsNullOrEmpty(forwarderEndpointString))
            {
                Program.Log($"Initialised forwarder: {listenEndpointString} -> {destinationEndpointString}");
            }
            else
            {
                Program.Log($"Initialised forwarder: {listenEndpointString} -> {forwarderEndpointString} -> {destinationEndpointString}");
            }
        }

        public UdpClient Listener { get; }
        public UdpClient ForwarderClient { get; }
        public IPEndPoint Destination { get; }

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

                        Program.Log($"{remoteIpEndPoint} -> {Listener.Client.LocalEndPoint} -> {ForwarderClient.Client.LocalEndPoint} -> {Destination}    {data.Length:N0} bytes");
                        ForwarderClient.Send(data, data.Length, Destination);
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
