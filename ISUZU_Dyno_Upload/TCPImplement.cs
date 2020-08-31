using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ISUZU_Dyno_Upload {
    public class TCPImplement {
        private const int BUFSIZE = 1024;
        private readonly MainForm m_mainForm;
        private readonly TextBox m_textBox;
        private readonly Logger m_log;
        public DynoParameter m_dynoParam;
        public EmissionInfo m_emiInfoSim;
        public ModelOracle m_dbOracle;
        public TcpListener m_listener;

        public TCPImplement(MainForm mainForm, TextBox textBox, DynoParameter dynoParam, EmissionInfo emiInfoSim, ModelOracle dbOracle, Logger log) {
            this.m_mainForm = mainForm;
            this.m_textBox = textBox;
            this.m_log = log;
            this.m_dynoParam = dynoParam;
            this.m_emiInfoSim = emiInfoSim;
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
                    m_mainForm.Invoke((EventHandler)delegate {
                        m_textBox.BackColor = Color.Red;
                        m_textBox.ForeColor = Color.White;
                        m_textBox.Text = "TCP listener error: " + ex.Message;
                    });
                }
            }
        }

        private void HandleClientComm(object param) {
            TcpClient client = (TcpClient)param;
            NetworkStream clientStream = client.GetStream();
            byte[] recv = new byte[BUFSIZE];
            string strRecv;
            int bytesRead;
            while (true) {
                try {
                    bytesRead = clientStream.Read(recv, 0, BUFSIZE);
                } catch (Exception ex) {
                    m_log.TraceError("TCP client occur error: " + ex.Message);
                    m_mainForm.Invoke((EventHandler)delegate {
                        m_textBox.BackColor = Color.Red;
                        m_textBox.ForeColor = Color.White;
                        m_textBox.Text = "TCP client error: " + ex.Message;
                    });
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
                    m_mainForm.Invoke((EventHandler)delegate {
                        m_textBox.BackColor = Color.Red;
                        m_textBox.ForeColor = Color.White;
                        m_textBox.Text = "VIN号长度不为17位";
                    });
                }
                clientStream.Write(sendMessage, 0, sendMessage.Length);
                clientStream.Flush();
                if (strVIN.Length == 17) {
                    EmissionInfo ei = new EmissionInfo();
                    string errMsg = string.Empty;
                    if (m_dynoParam.UseSimData) {
                        ei = m_emiInfoSim;
                        ei.VehicleInfo1.VIN = strVIN;
                        ei.VehicleInfo2.VIN = strVIN;
                    } else {
                        try {
                            m_dbOracle.GetEmissionInfo(strVIN, ei, out errMsg);
                        } catch (Exception ex) {
                            m_log.TraceError("GetEmissionInfo() error: " + ex.Message);
                            m_mainForm.Invoke((EventHandler)delegate {
                                m_textBox.BackColor = Color.Red;
                                m_textBox.ForeColor = Color.White;
                                m_textBox.Text = "GetEmissionInfo() error: " + ex.Message;
                            });
                        }
                    }
                    if (errMsg.Length < 0) {
                        m_log.TraceError("USP_GET_ENVIRONMENT_DATA() return error: " + errMsg);
                        m_mainForm.Invoke((EventHandler)delegate {
                            m_textBox.BackColor = Color.Red;
                            m_textBox.ForeColor = Color.White;
                            m_textBox.Text = errMsg;
                        });
                    } else {
                        m_mainForm.Invoke((EventHandler)delegate {
                            m_textBox.BackColor = Color.LightGreen;
                            m_textBox.ForeColor = Color.Black;
                            m_textBox.Text = "VIN[" + strVIN + "]测功机参数匹配成功";
                        });
                    }
                    string JsonFormatted = JsonConvert.SerializeObject(ei, Newtonsoft.Json.Formatting.Indented);
                    m_log.TraceInfo("Send dyno information: " + Environment.NewLine + JsonFormatted);
                    string strSend = JsonConvert.SerializeObject(ei);
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
