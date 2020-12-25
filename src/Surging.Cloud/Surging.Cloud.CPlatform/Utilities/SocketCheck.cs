using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Surging.Cloud.CPlatform.Utilities
{
    public static class SocketCheck
    {
        private static ILogger _logger = ServiceLocator.GetService<ILogger<IHealthCheckService>>();

        public static bool TestConnection(string host, int port, int millisecondsTimeout = 500)
        {
            Socket socket = null;
            try
            {
                var timeoutObject = new ManualResetEvent(false);
                timeoutObject.Reset();
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = socket.BeginConnect(host, port, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                return socket.Connected;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{host}:{port}连接异常,原因：{ex.Message}");
                return false;
            }
            finally
            {
                if (socket != null)
                {
                    socket.Dispose();
                }
            }

        }



        public static bool TestConnection(IPAddress iPAddress, int port, int millisecondsTimeout = 50)
        {
            Socket socket = null;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ar = socket.BeginConnect(iPAddress, port, null, null);
                ar.AsyncWaitHandle.WaitOne(millisecondsTimeout);
                return socket.Connected;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{iPAddress.ToString()}:{port}连接异常,原因：{ex.Message}");
                return false;
            }
            finally
            {
                if (socket != null)
                {
                    socket.Dispose();
                }
            }

        }



    }
}
