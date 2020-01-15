using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace RecvDemo {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e) {
            TcpClient client = new TcpClient("127.0.0.1", 50001);
            NetworkStream clientStream = client.GetStream();
            byte[] sendMessage;
            byte[] recv = new byte[4096];
            int bytesRead;
            string strRecv;

            // 发送VIN号
            sendMessage = Encoding.UTF8.GetBytes(this.txtBoxVIN.Text);
            clientStream.Write(sendMessage, 0, sendMessage.Length);
            clientStream.Flush();

            // 接收服务器返回消息
            // 本demo采用同步接收函数，代码会阻塞在接收函数这里，直到收到返回服务器消息
            // 实际使用的时候需要调用异步接收函数，或者另开一个接收线程
            bytesRead = clientStream.Read(recv, 0, 4096);
            if (bytesRead == 0) {
                this.txtBoxVIN.Text = "接收服务器返回VIN号格式出错";
                return;
            }
            strRecv = Encoding.UTF8.GetString(recv, 0, bytesRead);
            // 这里收到的“200”字符串不需要解析
            if (strRecv == "200") {
                bytesRead = clientStream.Read(recv, 0, 4096);
                if (bytesRead == 0) {
                    this.txtBoxVIN.Text = "接收服务器返回参数数据出错";
                    return;
                }
                strRecv = Encoding.UTF8.GetString(recv, 0, bytesRead);
            }
            // 本demo只是简单的将收到的JSON字符串显示在界面上，实际使用时需要解析这个JSON字符串
            this.txtBoxRecv.Text = strRecv;
        }
    }
}
