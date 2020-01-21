using DynoRecvDll;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecvDemo {
    public partial class Form1 : Form {
        CancellationTokenSource m_tokenSource;

        public Form1() {
            InitializeComponent();
        }

        /// <summary>
        /// 数据接收 DynoRecvDll.dll 使用方法：
        /// 1、使用之前需要先引入 DynoRecvDll 命名空间
        /// 2、先新建一个 DynoRecv 对象，构造函数原型 GetDynoInfo(string hostName, int port)
        ///    string hostName：服务器IP地址
        ///    int port：服务器端口号
        /// 3、在构造函数中会连接服务器，如果没有抛出异常说明已经成功连上服务器了
        /// 4、然后需要注册 RecvDynoInfo 事件处理函数，该事件表示 DynoRecv 对象已经接收到服务器返回的信息了
        /// 5、RecvDynoInfo 事件处理函数原型 void OnRecvDynoInfo(object sender, RecvDynoInfoEventArgs e)
        ///    object sender：事件发送者对象
        ///    RecvDynoInfoEventArgs e：发送该事件的参数
        /// 6、RecvDynoInfoEventArgs 参数原型类包含两个属性
        ///    string Code：表示返回状态，可能的取值：200、400、500、600
        ///         200：成功接收 JSON 数据
        ///         400：发送的 VIN 号格式错误，目前服务端只判断 VIN 号长度是否为17
        ///         500：服务器端从工厂 MES 取数据时发生错误
        ///         600：接收 JSON 数据时在本地发生异常
        ///    string Msg：Code 为 200 时存放成功接收到的 JSON 数据，Code 为其他值时存放错误信息
        /// 4、然后使用 SendVIN 函数来发送VIN号，函数原型为 void SendVIN(string strVIN)
        ///    string strVIN：VIN 号
        /// 5、SendVIN 函数执行后，DynoRecv 对象会自动开始接收 JSON 数据，其为异步过程，当接收完成后会发送 RecvDynoInfo 事件
        ///    客户端需在 RecvDynoInfo 事件处理函数内处理接收到的JSON数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSend_Click(object sender, EventArgs e) {
            DynoRecv recv = null;
            try {
                // 新建DynoRecv对象，同时连接服务器
                recv = new DynoRecv("127.0.0.1", 50001);
            } catch (Exception ex) {
                this.txtBoxRecv.Text = ex.Message;
                return;
            }

            // 没有异常说明已经连上服务器了，注册接收事件处理函数
            recv.RecvDynoInfo += OnRecvDynoInfo;

            //开始发送VIN号并接收返回信息
            recv.SendVIN(this.txtBoxVIN.Text);

            // 开个工作线程显示进度
            m_tokenSource = new CancellationTokenSource();
            CancellationToken token = m_tokenSource.Token;
            Task.Factory.StartNew(() => {
                int count = 0;
                string progress = "";
                while (!token.IsCancellationRequested) {
                    switch (count % 4) {
                    case 0:
                        progress = "-";
                        break;
                    case 1:
                        progress = "\\";
                        break;
                    case 2:
                        progress = "|";
                        break;
                    case 3:
                        progress = "/";
                        break;
                    }
                    count++;
                    this.Invoke(new Action(() => {
                        this.txtBoxRecv.Text = "正在接收数据 " + progress;
                    }));
                    Thread.Sleep(500);
                }
            }, token);
        }

        private void OnRecvDynoInfo(object sender, RecvDynoInfoEventArgs e) {
            // 模拟接收消息时的耗时操作
            Thread.Sleep(5000);

            // 取消界面上的进度显示
            if (m_tokenSource != null) {
                m_tokenSource.Cancel();
            }

            // 处理接收到的数据
            if (e.Code == "200") {
                // 成功拿到JSON数据
                this.Invoke(new Action(() => {
                    this.txtBoxRecv.Text = e.Msg;
                }));
            } else {
                // 过程中发生错误
                this.Invoke(new Action(() => {
                    this.txtBoxRecv.Text = "Code: " + e.Code + "\r\nError: " + e.Msg;
                }));
            }
        }
    }
}
