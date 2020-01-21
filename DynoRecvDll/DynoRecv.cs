using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynoRecvDll {
    public class DynoRecv {
        private readonly TcpClient m_client;
        private readonly NetworkStream m_clientStream;
        private readonly int m_bufSize;
        private readonly byte[] m_recvBuf;
        private string m_strRecv;
        public event EventHandler<RecvDynoInfoEventArgs> RecvDynoInfo;

        public DynoRecv(string hostName, int port) {
            try {
                m_client = new TcpClient(hostName, port);
                m_clientStream = m_client.GetStream();
            } catch (Exception) {
                if (m_clientStream != null) {
                    m_clientStream.Close();
                }
                if (m_client != null) {
                    m_client.Close();
                }
                throw;
            }
            m_bufSize = 4096;
            m_recvBuf = new byte[m_bufSize];
            m_strRecv = "";
        }

        ~DynoRecv() {
            if (m_clientStream != null) {
                m_clientStream.Close();
            }
            if (m_client != null) {
                m_client.Close();
            }
        }

        public void SendVIN(string strVIN) {
            // 发送VIN号和客户端dll版本，以“,”分隔
            byte[] sendMessage = Encoding.UTF8.GetBytes(strVIN + ",dll" + DllVersion<DynoRecv>.AssemblyVersion);
            m_clientStream.Write(sendMessage, 0, sendMessage.Length);
            m_clientStream.Flush();
            Task.Factory.StartNew(RecvMsg);
        }

        private void RecvMsg() {
            RecvDynoInfoEventArgs args = new RecvDynoInfoEventArgs();
            int bytesRead;
            m_strRecv = "";
            try {
                do {
                    bytesRead = m_clientStream.Read(m_recvBuf, 0, m_bufSize);
                    m_strRecv += Encoding.UTF8.GetString(m_recvBuf, 0, bytesRead);
                } while (m_clientStream.DataAvailable);
            } catch (Exception ex) {
                args.Code = "600";
                args.Msg = "发送VIN号后，接收返回信息出错：" + ex.Message;
                RecvDynoInfo?.Invoke(this, args);
                return;
            }

            // TCP接收的数据会有粘包现象，需要拆包操作
            if (m_strRecv.StartsWith("200")) {
                if (m_strRecv.Length == 3) {
                    m_strRecv = "";
                    try {
                        do {
                            bytesRead = m_clientStream.Read(m_recvBuf, 0, m_bufSize);
                            m_strRecv += Encoding.UTF8.GetString(m_recvBuf, 0, bytesRead);
                        } while (m_clientStream.DataAvailable);
                    } catch (Exception ex) {
                        args.Code = "600";
                        args.Msg = "接收测功机参数出错：" + ex.Message;
                        RecvDynoInfo?.Invoke(this, args);
                        return;
                    }
                } else {
                    m_strRecv = m_strRecv.Substring(3);
                }
                args.Code = "200";
                args.Msg = m_strRecv;
            } else {
                if (m_strRecv.Length >= 3) {
                    args.Code = m_strRecv.Substring(0, 3);
                    if (args.Code == "400") {
                        args.Msg = "VIN号格式错误";
                    } else {
                        args.Msg = m_strRecv.Substring(3);
                    }
                } else {
                    args.Code = "600";
                    args.Msg = "未知错误";
                }
            }
            RecvDynoInfo?.Invoke(this, args);
        }
    }

    public class RecvDynoInfoEventArgs : EventArgs {
        public string Code { get; set; }
        public string Msg { get; set; }
    }

    public static class DllVersion<T> {
        public static Version AssemblyVersion {
            get { return ((Assembly.GetAssembly(typeof(T))).GetName()).Version; }
        }
    }

}
