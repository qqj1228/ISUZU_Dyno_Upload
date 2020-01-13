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
                strRecv = Encoding.UTF8.GetString(recv, 0, bytesRead);
                IPEndPoint remoteAddress = (IPEndPoint)client.Client.RemoteEndPoint;
                m_log.TraceInfo(string.Format("Received message[{0}], from {1}:{2}", strRecv, remoteAddress.Address, remoteAddress.Port));
                byte[] sendMessage;
                string strVIN = "";
                if (strRecv != null) {
                    strVIN = strRecv.Trim();
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
                    string[] emissionInfo = m_dbOracle.GetEmissionInfo(strVIN);
                    EmissionInfo ei = new EmissionInfo();
                    if (m_dynoParam.UseSimData) {
                        ei = m_emiInfo;
                        ei.VehicleInfo1.VIN = emissionInfo[0];
                        ei.VehicleInfo2.VIN = emissionInfo[0];
                    } else {
                        SetEmissionInfo(ei, emissionInfo);
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

        private void SetEmissionInfo(EmissionInfo eiOUT, string[] eiIN) {
            eiOUT.VehicleInfo1.VIN = eiIN[0];
            eiOUT.VehicleInfo1.RegisterDate = DateTime.Now.ToLocalTime().ToString("yyyyMMdd");
            eiOUT.VehicleInfo1.ISQZ = "0";
            eiOUT.VehicleInfo1.VehicleType = "0";
            eiOUT.VehicleInfo1.CLXH = "testvehicletype";
            eiOUT.VehicleInfo1.FDJXH = "testenginetype";
            eiOUT.VehicleInfo1.HasOBD = "1";
            eiOUT.VehicleInfo1.FuelType = "0";
            eiOUT.VehicleInfo1.Standard = "6";
            eiOUT.VehicleInfo1.OBDCommCL = "1";
            eiOUT.VehicleInfo1.OBDCommCX = "1";
            eiOUT.VehicleInfo2.VehicleKind = "0";
            eiOUT.VehicleInfo2.VIN = eiIN[0];
            eiOUT.VehicleInfo2.RegisterDate = DateTime.Now.ToLocalTime().ToString("yyyyMMdd");
            eiOUT.VehicleInfo2.VehicleType = "0";
            eiOUT.VehicleInfo2.Model = "testmodel";
            eiOUT.VehicleInfo2.GearBoxType = "0";
            eiOUT.VehicleInfo2.AdmissionMode = "0";
            eiOUT.VehicleInfo2.Volume = "2.4";
            eiOUT.VehicleInfo2.FuelType = "0";
            eiOUT.VehicleInfo2.SupplyMode = "0";
            eiOUT.VehicleInfo2.RatedRev = "3000";
            eiOUT.VehicleInfo2.RatedPower = "2000.2";
            eiOUT.VehicleInfo2.DriveMode = "0";
            eiOUT.VehicleInfo2.MaxMass = "3000";
            eiOUT.VehicleInfo2.RefMass = "3000";
            eiOUT.VehicleInfo2.HasODB = "1";
            eiOUT.VehicleInfo2.HasPurge = "1";
            eiOUT.VehicleInfo2.IsEFI = "0";
            eiOUT.VehicleInfo2.MaxLoad = "5";
            eiOUT.VehicleInfo2.CarOrTruck = "0";
            eiOUT.VehicleInfo2.Cylinder = "4";
            eiOUT.VehicleInfo2.IsTransform = "0";
            eiOUT.VehicleInfo2.StandardID = "6";
            eiOUT.VehicleInfo2.IsAsm = "0";
            eiOUT.VehicleInfo2.QCZZCJ = "江西五十铃";
            eiOUT.VehicleInfo2.FDJZZC = "江西五十铃";
            eiOUT.VehicleInfo2.DDJXH = "XXXX-YYY-ZZ";
            eiOUT.VehicleInfo2.XNZZXH = "XXXX-YYY-ZZ";
            eiOUT.VehicleInfo2.CHZHQXH = "XXXX-YYY-ZZ";
            eiOUT.VehicleInfo2.HPYS = "蓝牌";
            eiOUT.VehicleInfo2.SCR = "有";
            eiOUT.VehicleInfo2.SCRXH = "XXXX";
            eiOUT.VehicleInfo2.DCRL = "60";
            eiOUT.VehicleInfo2.JCFF = "加载减速";
            eiOUT.LimitValue.SmokeK = "0.5";
            eiOUT.LimitValue.SmokeNO = "0.5";
        }

    }

}
