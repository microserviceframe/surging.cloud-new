using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Surging.Core.CPlatform.Utilities
{
    public static class SocketCheck
    {
        private static ILogger _logger = ServiceLocator.GetService<ILogger<IHealthCheckService>>();

        public static bool TestConnection(EndPoint endPoint, int millisecondsTimeout = 500)
        {

            Socket socket = null;
            try
            {
                bool isHealth = false;
                var timeoutObject = new ManualResetEvent(false);
                timeoutObject.Reset();
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
            }
            catch(Exception ex)
            {
                _logger.LogError($"{endPoint}连接异常,原因：{ex.Message}");
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

        public static bool TestConnection(string host, int port, int millisecondsTimeout = 500) 
        {
            Socket socket = null;
            try 
            {
                var isHealth = TestConnectionByPing(host, millisecondsTimeout);
                if (isHealth)
                {
                    var timeoutObject = new ManualResetEvent(false);
                    timeoutObject.Reset();
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                bool isHealth = TestConnectionByPing(iPAddress, millisecondsTimeout);
                if (isHealth)
                {
                    var timeoutObject = new ManualResetEvent(false);
                    timeoutObject.Reset();
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
