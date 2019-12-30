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
        public ModelOracle m_dbOracle;
        public TcpListener m_listener;
        private readonly int m_iRecvBufSize;

        public TCPImplement(DynoParameter dynoParam, ModelOracle dbOracle, Logger log) {
            this.m_log = log;
            this.m_dynoParam = dynoParam;
            this.m_dbOracle = dbOracle;
            this.m_listener = new TcpListener(IPAddress.Any, this.m_dynoParam.TCPPort);
            m_iRecvBufSize = 1024;
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
            byte[] recv = new byte[m_iRecvBufSize];
            string strRecv;
            int bytesRead;
            while (true) {
                try {
                    bytesRead = clientStream.Read(recv, 0, m_iRecvBufSize);
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
                    SetEmissionInfo(ei, emissionInfo);
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
            eiOUT.LimitValue.SmokeK = "0.5";
            eiOUT.LimitValue.SmokeNO = "0.5";
        }

    }

    public class VehicleInfo1Class {
        public string License { get; set; }
        public string VIN { get; set; }
        public string RegisterDate { get; set; }
        public string ISQZ { get; set; }
        public string VehicleType { get; set; }
        public string CLXH { get; set; }
        public string FDJXH { get; set; }
        public string HasOBD { get; set; }
        public string FuelType { get; set; }
        public string Standard { get; set; }
        public string OBDCommCL { get; set; }
        public string OBDCommCX { get; set; }
    }

    public class VehicleInfo2Class {
        public string VehicleKind { get; set; }
        public string License { get; set; }
        public string VIN { get; set; }
        public string RegisterDate { get; set; }
        public string VehicleType { get; set; }
        public string Model { get; set; }
        public string GearBoxType { get; set; }
        public string AdmissionMode { get; set; }
        public string Volume { get; set; }
        public string Odometer { get; set; }
        public string FuelType { get; set; }
        public string SupplyMode { get; set; }
        public string RatedRev { get; set; }
        public string RatedPower { get; set; }
        public string DriveMode { get; set; }
        public string Owner { get; set; }
        public string Address { get; set; }
        public string MaxMass { get; set; }
        public string RefMass { get; set; }
        public string HasODB { get; set; }
        public string Phone { get; set; }
        public string HasPurge { get; set; }
        public string IsEFI { get; set; }
        public string MaxLoad { get; set; }
        public string CarOrTruck { get; set; }
        public string Cylinder { get; set; }
        public string IsTransform { get; set; }
        public string StandardID { get; set; }
        public string IsAsm { get; set; }
        public string QCZZCJ { get; set; }
        public string FDJZZC { get; set; }
        public string DDJXH { get; set; }
        public string XNZZXH { get; set; }
        public string CHZHQXH { get; set; }
        public string HPYS { get; set; }
        public string SCR { get; set; }
        public string SCRXH { get; set; }
        public string DPF { get; set; }
        public string DPFXH { get; set; }

    }

    public class LimitValueClass {
        public string AmbientCOUp { get; set; }
        public string AmbientCO2Up { get; set; }
        public string AmbientHCUp { get; set; }
        public string AmbientNOUp { get; set; }
        public string BackgroundCOUp { get; set; }
        public string BackgroundCO2Up { get; set; }
        public string BackgroundHCUp { get; set; }
        public string BackgroundNOUp { get; set; }
        public string ResidualHCUp { get; set; }
        public string CO5025 { get; set; }
        public string HC5025 { get; set; }
        public string NO5025 { get; set; }
        public string Lambda5025up { get; set; }
        public string Lambda5025below { get; set; }
        public string CO2540 { get; set; }
        public string HC2540 { get; set; }
        public string NO2540 { get; set; }
        public string Lambda2540up { get; set; }
        public string Lambda2540below { get; set; }
        public string COAndCO2 { get; set; }
        public string HighIdleCO { get; set; }
        public string HighIdleHC { get; set; }
        public string IdleCO { get; set; }
        public string IdleHC { get; set; }
        public string FASmokeHSU { get; set; }
        public string FASmokeK { get; set; }
        public string SmokeK { get; set; }
        public string SmokeHSU { get; set; }
        public string SmokeNO { get; set; }
        public string MaxPower { get; set; }
    }

    public class EmissionInfo {
        public VehicleInfo1Class VehicleInfo1 { get; set; }
        public VehicleInfo2Class VehicleInfo2 { get; set; }
        public LimitValueClass LimitValue { get; set; }
        public EmissionInfo() {
            VehicleInfo1 = new VehicleInfo1Class();
            VehicleInfo2 = new VehicleInfo2Class();
            LimitValue = new LimitValueClass();
        }
    }
}
