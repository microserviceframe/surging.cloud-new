using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;

namespace Surging.Cloud.CPlatform.Address
{
    /// <summary>
    /// ip��ַģ�͡�
    /// </summary>
    public sealed class IpAddressModel : AddressModel
    {
        #region Constructor

        /// <summary>
        /// ��ʼ��һ���µ�ip��ַģ��ʵ����
        /// </summary>
        public IpAddressModel()
        {
        }

        /// <summary>
        /// ��ʼ��һ���µ�ip��ַģ��ʵ����
        /// </summary>
        /// <param name="ip">ip��ַ��</param>
        /// <param name="port">�˿ڡ�</param>
        public IpAddressModel(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }


        #endregion Constructor

        #region Property

        /// <summary>
        /// ip��ַ��
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// �˿ڡ�
        /// </summary>
        public int Port { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string WanIp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? WsPort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MqttPort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? HttpPort { get; set; }

        #endregion Property

        #region Overrides of AddressModel

        /// <summary>
        /// �����ս�㡣
        /// </summary>
        /// <returns></returns>
        public override EndPoint CreateEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(AddressHelper.GetIpFromAddress(Ip)), Port);
        }


        public override string ToString()
        {
            return string.Concat(new string[] { AddressHelper.GetIpFromAddress(Ip), ":", Port.ToString() });
        }

        #endregion Overrides of AddressModel
    }
}