using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Utilities
{
    public static class SocketCheck
    {
        private static ILogger _logger = ServiceLocator.GetService<ILogger>();

        public static bool TestConnection(EndPoint endPoint, int millisecondsTimeout = 500)
        {
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = millisecondsTimeout })
            {
                try
                {
                    socket.Connect(endPoint);
                    isHealth = true;
                }
                catch
                {

                }
                return isHealth;

            }
        }

        public static bool TestConnection(string host, int port, int millisecondsTimeout = 500) 
        {
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = millisecondsTimeout })
            {
                try
                {
                    socket.Connect(host, port);
                    isHealth = true;
                }
                catch
                {

                }
                return isHealth;

            }
        }

        public static bool TestConnection(IPAddress iPAddress, int port, int millisecondsTimeout = 50)
        {
            
            bool isHealth = false;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { SendTimeout = millisecondsTimeout }) 
            {
                try
                {
                    socket.Connect(iPAddress,port);
                    isHealth = true;
                }
                catch
                {

                }
                return isHealth;

            }

        }



    }
}
