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
        public static bool TestConnection(string host, int port, int timeout = 5) 
        {
            var client = new TcpClient();
            try
            {
                var ar = client.BeginConnect(host, port, null, null);
                ar.AsyncWaitHandle.WaitOne(timeout);
                return client.Connected;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                client.Close();
            }
        }

        public static bool TestConnection(IPAddress iPAddress, int port, int millisecondsTimeout = 5)
        {
            var client = new TcpClient();
            try
            {
                var ar = client.BeginConnect(iPAddress, port, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                return client.Connected;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                client.Close();
            }
        }



    }
}
