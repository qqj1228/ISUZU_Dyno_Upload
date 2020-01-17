using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DynoRecvDll {
    public class DynoRecv {
        private readonly TcpClient m_client;
        private readonly NetworkStream m_clientStream;
        private readonly int m_bufSize;
        private readonly byte[] m_recvBuf;
        private string m_strRecv;

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

        public bool GetDynoInfo(string strVIN, out string strRecvMsg) {
            bool bRet = false;
            strRecvMsg = "";
            byte[] sendMessage = Encoding.UTF8.GetBytes(strVIN);
            m_clientStream.Write(sendMessage, 0, sendMessage.Length);
            m_clientStream.Flush();

            int bytesRead;
            m_strRecv = "";
            try {
                do {
                    bytesRead = m_clientStream.Read(m_recvBuf, 0, m_bufSize);
                    m_strRecv += Encoding.UTF8.GetString(m_recvBuf, 0, bytesRead);
                } while (m_clientStream.DataAvailable);
            } catch (Exception ex) {
                throw new ApplicationException("发送VIN号后，接收返回信息出错：" + ex.Message);
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
                        throw new ApplicationException("接收Dyno参数出错：" + ex.Message);
                    }
                } else {
                    m_strRecv = m_strRecv.Substring(3);
                }
                bRet = true;
            }
            strRecvMsg = m_strRecv;
            return bRet;
        }
    }
}
