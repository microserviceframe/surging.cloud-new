using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    public static class SocketCheck
    {
        private static ILogger _logger = ServiceLocator.GetService<ILogger>();

        public static bool TestConnection(EndPoint endPoint, int millisecondsTimeout = 500)
        {
            try 
            {
                bool isHealth = false;
                var timeoutObject = new ManualResetEvent(false);
                timeoutObject.Reset();
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.BeginConnect(endPoint, ar => timeoutObject.Set(), socket);
                if (timeoutObject.WaitOne(millisecondsTimeout, false))
                {
                    isHealth = true;
                }
                else
                {
                    isHealth = false;
                }
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                return isHealth;
            } catch 
            {
                return false;
            }
        }

        public static bool TestConnection(string host, int port, int millisecondsTimeout = 500) 
        {
            try 
            {
                var isHealth = TestConnectionByPing(host, millisecondsTimeout);
                if (isHealth)
                {
                    var timeoutObject = new ManualResetEvent(false);
                    timeoutObject.Reset();
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.BeginConnect(host, port, ar => timeoutObject.Set(), socket);
                    if (timeoutObject.WaitOne(millisecondsTimeout, false))
                    {
                        isHealth = true;
                    }
                    else
                    {
                        isHealth = false;
                    }
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                }
                return isHealth;
            } catch 
            {
                return false;
            }

        }

        public static bool TestConnection(IPAddress iPAddress, int port, int millisecondsTimeout = 50)
        {
            try 
            {
                bool isHealth = TestConnectionByPing(iPAddress, millisecondsTimeout);
                if (isHealth)
                {
                    var timeoutObject = new ManualResetEvent(false);
                    timeoutObject.Reset();
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.BeginConnect(iPAddress, port, ar => timeoutObject.Set(), socket);
                    if (timeoutObject.WaitOne(millisecondsTimeout, false))
                    {
                        isHealth = true;
                    }
                    else
                    {
                        isHealth = false;
                    }
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                }
                return isHealth;
            } catch 
            {
                return false;
            }
          

        }

        private static bool TestConnectionByPing(string ip, int millisecondsTimeout = 50) 
        {
            var ping = new Ping();
            var pingStatus = ping.Send(ip, millisecondsTimeout).Status;
            return pingStatus == IPStatus.Success;

        }

        private static bool TestConnectionByPing(IPAddress iPAddress, int millisecondsTimeout = 50)
        {
            var ping = new Ping();
            var pingStatus = ping.Send(iPAddress, millisecondsTimeout).Status;
            return pingStatus == IPStatus.Success;
        }

    }
}
