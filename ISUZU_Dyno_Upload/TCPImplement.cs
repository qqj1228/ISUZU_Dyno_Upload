using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ISUZU_Dyno_Upload {
    public class TCPImplement {
        public readonly Logger m_log;
        public DynoParameter m_dynoParam;
        public EmissionInfo m_emiInfo;
        public ModelOracle m_dbOracle;
        public TcpListener m_listener;
        private const int BufSize = 1024;

        public TCPImplement(DynoParameter dynoParam, EmissionInfo emiInfo, ModelOracle dbOracle, Logger log) {
            this.m_log = log;
            this.m_dynoParam = dynoParam;
            this.m_emiInfo = emiInfo;
            this.m_dbOracle = dbOracle;
            this.m_listener = new TcpListener(IPAddress.Any, this.m_dynoParam.TCPPort);
            Task.Factory.StartNew(ListenForClients);
        }

        private void ListenForClients() {
            this.m_listener.Start();
            IPEndPoint serverAddress = (IPEndPoint)m_listener.LocalEndpoint;
            m_log.TraceInfo(string.Format("DynoServer start listenning on {0}:{1}", serverAddress.Address, serverAddress.Port));
            while (true) {
                try {
                    TcpClient client = this.m_listener.AcceptTcpClient();
                    Task.Factory.StartNew(HandleClientComm, client);
                } catch (Exception ex) {
                    m_log.TraceError("TCP listener occur error: " + ex.Message);
                }
            }
        }

        private void HandleClientComm(object param) {
            TcpClient client = (TcpClient)param;
            NetworkStream clientStream = client.GetStream();
            byte[] recv = new byte[BufSize];
            string strRecv;
            int bytesRead;
            while (true) {
                try {
                    bytesRead = clientStream.Read(recv, 0, BufSize);
                } catch (Exception ex) {
                    m_log.TraceError("TCP client occur error: " + ex.Message);
                    return;
                }
                if (bytesRead == 0) {
                    break;
                }
                m_log.TraceInfo(">>>>>>>> Start to handle client request. Ver: " + MainFileVersion.AssemblyVersion + " <<<<<<<<");
                strRecv = Encoding.UTF8.GetString(recv, 0, bytesRead);
                IPEndPoint remoteAddress = (IPEndPoint)client.Client.RemoteEndPoint;
                m_log.TraceInfo(string.Format("Received message[{0}], from {1}:{2}", strRecv, remoteAddress.Address, remoteAddress.Port));
                byte[] sendMessage;
                string strVIN = "";
                if (strRecv != null) {
                    strVIN = strRecv.Split(',')[0].Trim();
                }
                if (strVIN.Length == 17) {
                    sendMessage = Encoding.UTF8.GetBytes("200");
                    m_log.TraceInfo("Received VIN is OK");
                } else {
                    sendMessage = Encoding.UTF8.GetBytes("400");
                    m_log.TraceError("Received VIN is Illegal");
                }
                clientStream.Write(sendMessage, 0, sendMessage.Length);
                clientStream.Flush();
                if (strVIN.Length == 17) {
                    EmissionInfo ei = new EmissionInfo();
                    if (m_dynoParam.UseSimData) {
                        ei = m_emiInfo;
                        ei.VehicleInfo1.VIN = strVIN;
                        ei.VehicleInfo2.VIN = strVIN;
                    } else {
                        try {
                            m_dbOracle.GetEmissionInfo(strVIN, ei);
                        } catch (Exception ex) {
                            m_log.TraceError("GetEmissionInfo() error: " + ex.Message);
                        }
                    }
                    string strSend = JsonConvert.SerializeObject(ei);
                    m_log.TraceInfo("Send dyno information: " + strSend);
                    sendMessage = Encoding.UTF8.GetBytes(strSend);
                    clientStream.Write(sendMessage, 0, sendMessage.Length);
                    clientStream.Flush();
                }
            }
            clientStream.Close();
            client.Close();
        }
    }

}
