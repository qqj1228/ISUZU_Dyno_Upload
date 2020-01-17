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
        public Form1() {
            InitializeComponent();
        }

        /// <summary>
        /// 数据接收DynoRecvDll.dll使用方法：
        /// 1、使用之前需要先引入DynoRecvDll命名空间
        /// 2、先新建一个DynoRecv对象，构造函数原型 GetDynoInfo(string hostName, int port)
        ///    hostName：服务器IP地址
        ///    port：服务器端口号
        /// 3、如果没有抛出异常说明已经成功连上服务器了
        /// 4、然后可以使用GetDynoInfo函数来获取服务器返回的JSON字符串
        /// 5、函数原型为 bool GetDynoInfo(string strVIN, out string strRecvMsg)
        ///    strVIN：VIN号
        ///    strRecvMsg：存放拿到的JSON字符串或者返回的错误信息“400”
        ///    若函数执行期间发生异常，则strRecvMsg为空字符串
        ///    函数返回值为bool类型：
        ///    true：表示成功拿到了JSON字符串，strRecvMsg为拿到的JSON字符串
        ///    false：表示传入的VIN号长度不对，strRecvMsg为字符串“400”
        /// 6、GetDynoInfo函数为同步函数，执行时会阻塞代码，在生产环境中使用时应在单独的线程中执行，以免影响程序其他部分
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSend_Click(object sender, EventArgs e) {
            DynoRecv recv = null;
            try {
                // 新建DynoRecv对象
                recv = new DynoRecv("127.0.0.1", 50001);
            } catch (Exception ex) {
                this.txtBoxRecv.Text = ex.Message;
                return;
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            // 开个工作线程显示进度
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

            // 新开工作线程用于接收JSON字符串，并显示在界面上
            Task.Factory.StartNew(() => {
                string strRecv = "";
                try {
                    if (recv != null) {
                        if (recv.GetDynoInfo(this.txtBoxVIN.Text, out strRecv)) {
                            // 模拟耗时操作
                            Thread.Sleep(5000);
                        }
                    }
                } catch (Exception ex) {
                    strRecv = ex.Message;
                } finally {
                    // 操作结束后关闭进度显示
                    tokenSource.Cancel();
                }
                this.Invoke(new Action(() => {
                    this.txtBoxRecv.Text = strRecv;
                }));
            });
        }
    }
}
