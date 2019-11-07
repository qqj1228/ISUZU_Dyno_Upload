using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace ISUZU_Dyno_Upload {
    public partial class MainForm : Form {
        public Logger m_log;
        public Config m_cfg;
        public Model m_db;
        public ModelOracle m_dbOracle;
        public int m_iCNLenb;
        readonly System.Timers.Timer m_timer;

        public MainForm() {
            InitializeComponent();
            this.Text = "ISUZU_Dyno_Upload Ver: " + MainFileVersion.AssemblyVersion;
            m_log = new Logger(".\\log", EnumLogLevel.LogLevelAll, true, 100);
            m_log.TraceInfo("==================================================================");
            m_log.TraceInfo("==================== START Ver: " + MainFileVersion.AssemblyVersion + " ====================");
            m_cfg = new Config(m_log);
            m_db = new Model(m_cfg.DB.SqlServer, m_log);
            m_dbOracle = new ModelOracle(m_cfg.DB.Oracle, m_log);
            m_iCNLenb = 3;
            try {
                m_iCNLenb = m_dbOracle.GetCNLenb();
            } catch (Exception ex) {
                m_log.TraceError("Can't connect with MES: " + ex.Message);
                MessageBox.Show("无法与MES通讯，请检查设置\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#if DEBUG
            m_timer = new System.Timers.Timer(m_cfg.DB.Interval * 1000);
            m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeUpload);
            m_timer.AutoReset = false;
            m_timer.Enabled = true;
#else
            m_timer = new System.Timers.Timer(m_cfg.DB.Interval * 60 * 1000);
            m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeUpload);
            m_timer.AutoReset = true;
            m_timer.Enabled = true;
#endif

        }

        private void MainForm_Resize(object sender, EventArgs e) {
            int margin = this.grpBoxInfo.Location.X;
            this.grpBoxInfo.Width = (this.Width - margin * 6) / 4;
            this.grpBoxInfo.Height = (this.Height - this.grpBoxInfo.Location.Y - margin * 7) / 4;
            this.grpBoxEnv.Location = new Point(this.grpBoxInfo.Location.X, this.grpBoxInfo.Height + this.grpBoxInfo.Location.Y + margin);
            this.grpBoxEnv.Width = this.grpBoxInfo.Width;
            this.grpBoxEnv.Height = this.grpBoxInfo.Height;
            this.grpBoxResult.Location = new Point(this.grpBoxInfo.Location.X, this.grpBoxEnv.Height + this.grpBoxEnv.Location.Y + margin);
            this.grpBoxResult.Width = this.grpBoxInfo.Width;
            this.grpBoxResult.Height = this.grpBoxInfo.Height;
            this.grpBoxDevice.Location = new Point(this.grpBoxInfo.Location.X, this.grpBoxResult.Height + this.grpBoxResult.Location.Y + margin);
            this.grpBoxDevice.Width = this.grpBoxInfo.Width;
            this.grpBoxDevice.Height = this.grpBoxInfo.Height;

            this.grpBox51.Location = new Point(this.grpBoxInfo.Location.X + this.grpBoxInfo.Width + margin, this.grpBoxInfo.Location.Y);
            this.grpBox51.Width = this.grpBoxInfo.Width;
            this.grpBox51.Height = (this.Height - this.grpBoxInfo.Location.Y - margin * 5) / 2;
            this.grpBox52.Location = new Point(this.grpBox51.Location.X, this.grpBox51.Location.Y + this.grpBox51.Height + margin);
            this.grpBox52.Width = this.grpBox51.Width;
            this.grpBox52.Height = this.grpBox51.Height;

            this.grpBox53.Location = new Point(this.grpBox51.Location.X + this.grpBox51.Width + margin, this.grpBoxInfo.Location.Y);
            this.grpBox53.Width = this.grpBox51.Width;
            this.grpBox53.Height = this.grpBox51.Height;
            this.grpBox54.Location = new Point(this.grpBox53.Location.X, this.grpBox53.Location.Y + this.grpBox53.Height + margin);
            this.grpBox54.Width = this.grpBox51.Width;
            this.grpBox54.Height = this.grpBox51.Height;

            this.grpBox55.Location = new Point(this.grpBox53.Location.X + this.grpBox53.Width + margin, this.grpBoxInfo.Location.Y);
            this.grpBox55.Width = this.grpBox51.Width;
            this.grpBox55.Height = this.grpBox51.Height;
            this.grpBox56.Location = new Point(this.grpBox55.Location.X, this.grpBox55.Location.Y + this.grpBox55.Height + margin);
            this.grpBox56.Width = this.grpBox51.Width;
            this.grpBox56.Height = this.grpBox51.Height;

        }

        private void OnTimeUpload(object source, System.Timers.ElapsedEventArgs e) {
            m_log.TraceInfo("Upload dyno data OnTime. Ver: " + MainFileVersion.AssemblyVersion);
            List<UploadField> resultList = new List<UploadField>();
            try {
                resultList = m_db.GetDynoData(m_cfg.FieldUL, m_iCNLenb);
            } catch (Exception ex) {
                m_log.TraceError("GetDynoData error: " + ex.Message);
            }
            foreach (UploadField result in resultList) {
                if (result.EPASS != "1") {
                    m_log.TraceInfo("Skip this VIN[" + result.VIN + "] because of EPASS != 1");
                    this.Invoke((EventHandler)delegate {
                        this.lblAutoUpload.ForeColor = Color.Red;
                        this.lblAutoUpload.Text = "VIN[" + result.VIN + "] 排放结果不合格，数据不上传";
                    });
                    continue;
                }
                DataTable dt2 = new DataTable("IF_EM_WQPF_2");
                DataTable dt3 = new DataTable("IF_EM_WQPF_3");
                DataTable dt51 = new DataTable("IF_EM_WQPF_5_1");
                DataTable dt52 = new DataTable("IF_EM_WQPF_5_2");
                DataTable dt53 = new DataTable("IF_EM_WQPF_5_3");
                DataTable dt54 = new DataTable("IF_EM_WQPF_5_4");
                DataTable dt6 = new DataTable("IF_EM_WQPF_6");
                try {
                    string[] ID_MES = m_dbOracle.GetValue("IF_EM_WQPF_1", "ID", "VIN", result.VIN);
                    if (ID_MES != null && ID_MES.Length > 0) {
                        SetDataTable2Oracle(ID_MES[0], dt2, result);
                        SetDataTable3Oracle(ID_MES[0], dt3, result);
                        switch (result.TESTTYPE) {
                        case "1":
                            SetDataTable51Oracle(ID_MES[0], dt51, result);
                            break;
                        case "2":
                            SetDataTable52Oracle(ID_MES[0], dt52, result);
                            break;
                        case "3":
                            SetDataTable53Oracle(ID_MES[0], dt53, result);
                            break;
                        case "4":
                            SetDataTable54Oracle(ID_MES[0], dt54, result);
                            break;
                        default:
                            break;
                        }
                        SetDataTable6Oracle(ID_MES[0], dt6, result);
                        this.Invoke((EventHandler)delegate {
                            this.lblAutoUpload.ForeColor = Color.ForestGreen;
                            this.lblAutoUpload.Text = "VIN[" + result.VIN + "] 数据已上传";
                        });
                        m_db.SetUpload(1, result.JCLSH);
                    } else {
                        m_log.TraceWarning("Skip this VIN[" + result.VIN + "] because it is not in MES");
                        this.Invoke((EventHandler)delegate {
                            this.lblAutoUpload.ForeColor = Color.Red;
                            this.lblAutoUpload.Text = "VIN[" + result.VIN + "] 在MES中未检测到记录，数据不上传";
                        });
                    }
                } catch (Exception ex) {
                    m_log.TraceError("Upload error: " + ex.Message);
                    MessageBox.Show(ex.Message, "上传出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } finally {
                    dt2.Dispose();
                    dt3.Dispose();
                    dt51.Dispose();
                    dt52.Dispose();
                    dt53.Dispose();
                    dt54.Dispose();
                    dt6.Dispose();
                }
            }
        }

        private void SetDataTable2Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("RH", typeof(string));                       // 2
            dt.Columns.Add("ET", typeof(string));                       // 3
            dt.Columns.Add("AP", typeof(string));                       // 4

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 5
            dt.Columns.Add("CREATOR", typeof(string));                  // 6
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 7
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 8
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 9
            dt.Columns.Add("ISDELETED", typeof(string));                // 10
            dt.Columns.Add("DELETER", typeof(string));                  // 11

            DataRow dr = dt.NewRow();
            dr[1] = strKeyID;

            dr[2] = result.RH;
            dr[3] = result.ET;
            dr[4] = result.AP;

            dr[7] = DateTime.Now.ToLocalTime();
            dr[8] = m_cfg.DB.Name;
            dr[10] = "0";
            dt.Rows.Add(dr);

            int iRet;
            try {
                string[] strVals = m_dbOracle.GetValue(dt.TableName, "ID", "WQPF_ID", strKeyID);
                if (strVals.Length == 0) {
                    iRet = m_dbOracle.InsertRecords(dt.TableName, dt);
                } else {
                    iRet = m_dbOracle.UpdateRecords(dt.TableName, dt, "ID", strVals);
                }
            } catch (Exception) {
                throw;
            }
            if (iRet <= 0) {
                throw new Exception("插入或更新 MES 数据出错，返回的影响行数: " + iRet.ToString());
            }
        }

        private void SetDataTable3Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("TESTTYPE", typeof(string));                 // 2
            dt.Columns.Add("TESTNO", typeof(string));                   // 3
            dt.Columns.Add("TESTDATE", typeof(string));                 // 4
            dt.Columns.Add("APASS", typeof(string));                    // 5
            dt.Columns.Add("OPASS", typeof(string));                    // 6
            dt.Columns.Add("OTESTDATE", typeof(string));                // 7
            dt.Columns.Add("EPASS", typeof(string));                    // 8
            dt.Columns.Add("RESULT", typeof(string));                   // 9

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 10
            dt.Columns.Add("CREATOR", typeof(string));                  // 11
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 12
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 13
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 14
            dt.Columns.Add("ISDELETED", typeof(string));                // 15
            dt.Columns.Add("DELETER", typeof(string));                  // 16

            DataRow dr = dt.NewRow();
            dr[1] = strKeyID;
            dr[2] = result.TESTTYPE;
            dr[4] = result.TESTDATE;
            dr[8] = result.EPASS;
            //dr[9] = result.EPASS;

            dr[12] = DateTime.Now.ToLocalTime();
            dr[13] = m_cfg.DB.Name;
            dr[15] = "0";
            dt.Rows.Add(dr);
            int iRet;
            try {
                string[] strVals = m_dbOracle.GetValue(dt.TableName, "ID", "WQPF_ID", strKeyID);
                if (strVals.Length == 0) {
                    iRet = m_dbOracle.InsertRecords(dt.TableName, dt);
                } else {
                    iRet = m_dbOracle.UpdateRecords(dt.TableName, dt, "ID", strVals);
                }
            } catch (Exception) {
                throw;
            }
            if (iRet <= 0) {
                throw new Exception("插入或更新 MES 数据出错，返回的影响行数: " + iRet.ToString());
            }
        }

        private void SetDataTable51Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("REAC", typeof(string));                     // 2
            dt.Columns.Add("LEACMAX", typeof(string));                  // 3
            dt.Columns.Add("LEACMIN", typeof(string));                  // 4
            dt.Columns.Add("LRCO", typeof(string));                     // 5
            dt.Columns.Add("LLCO", typeof(string));                     // 6
            dt.Columns.Add("LRHC", typeof(string));                     // 7
            dt.Columns.Add("LLHC", typeof(string));                     // 8
            dt.Columns.Add("HRCO", typeof(string));                     // 9
            dt.Columns.Add("HLCO", typeof(string));                     // 10
            dt.Columns.Add("HRHC", typeof(string));                     // 11
            dt.Columns.Add("HLHC", typeof(string));                     // 12

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 13
            dt.Columns.Add("CREATOR", typeof(string));                  // 14
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 15
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 16
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 17
            dt.Columns.Add("ISDELETED", typeof(string));                // 18
            dt.Columns.Add("DELETER", typeof(string));                  // 19

            DataRow dr = dt.NewRow();
            dr[1] = strKeyID;
            dr[2] = result.REAC;
            dr[3] = result.LEACMAX;
            dr[4] = result.LEACMIN;
            dr[5] = result.LRCO;
            dr[6] = result.LLCO;
            dr[7] = result.LRHC;
            dr[8] = result.LLHC;
            dr[9] = result.HRCO;
            dr[10] = result.HLCO;
            dr[11] = result.HRHC;
            dr[12] = result.HLHC;

            dr[15] = DateTime.Now.ToLocalTime();
            dr[16] = m_cfg.DB.Name;
            dr[18] = "0";
            dt.Rows.Add(dr);
            int iRet;
            try {
                string[] strVals = m_dbOracle.GetValue(dt.TableName, "ID", "WQPF_ID", strKeyID);
                if (strVals.Length == 0) {
                    iRet = m_dbOracle.InsertRecords(dt.TableName, dt);
                } else {
                    iRet = m_dbOracle.UpdateRecords(dt.TableName, dt, "ID", strVals);
                }
            } catch (Exception) {
                throw;
            }
            if (iRet <= 0) {
                throw new Exception("插入或更新 MES 数据出错，返回的影响行数: " + iRet.ToString());
            }

        }

        private void SetDataTable52Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("ARHC5025", typeof(string));                 // 2
            dt.Columns.Add("ALHC5025", typeof(string));                 // 3
            dt.Columns.Add("ARCO5025", typeof(string));                 // 4
            dt.Columns.Add("ALCO5025", typeof(string));                 // 5
            dt.Columns.Add("ARNOX5025", typeof(string));                // 6
            dt.Columns.Add("ALNOX5025", typeof(string));                // 7
            dt.Columns.Add("ARHC2540", typeof(string));                 // 8
            dt.Columns.Add("ALHC2540", typeof(string));                 // 9
            dt.Columns.Add("ARCO2540", typeof(string));                 // 10
            dt.Columns.Add("ALCO2540", typeof(string));                 // 11
            dt.Columns.Add("ARNOX2540", typeof(string));                // 12
            dt.Columns.Add("ALNOX2540", typeof(string));                // 13

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 14
            dt.Columns.Add("CREATOR", typeof(string));                  // 15
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 16
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 17
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 18
            dt.Columns.Add("ISDELETED", typeof(string));                // 19
            dt.Columns.Add("DELETER", typeof(string));                  // 20

            DataRow dr = dt.NewRow();
            dr[1] = strKeyID;
            dr[2] = result.ARHC5025;
            dr[3] = result.ALHC5025;
            dr[4] = result.ARCO5025;
            dr[5] = result.ALCO5025;
            dr[6] = result.ARNOX5025;
            dr[7] = result.ALNOX5025;
            dr[8] = result.ARHC2540;
            dr[9] = result.ALHC2540;
            dr[10] = result.ARCO2540;
            dr[11] = result.ALCO2540;
            dr[12] = result.ARNOX2540;
            dr[13] = result.ALNOX2540;

            dr[16] = DateTime.Now.ToLocalTime();
            dr[17] = m_cfg.DB.Name;
            dr[19] = "0";
            dt.Rows.Add(dr);
            int iRet;
            try {
                string[] strVals = m_dbOracle.GetValue(dt.TableName, "ID", "WQPF_ID", strKeyID);
                if (strVals.Length == 0) {
                    iRet = m_dbOracle.InsertRecords(dt.TableName, dt);
                } else {
                    iRet = m_dbOracle.UpdateRecords(dt.TableName, dt, "ID", strVals);
                }
            } catch (Exception) {
                throw;
            }
            if (iRet <= 0) {
                throw new Exception("插入或更新 MES 数据出错，返回的影响行数: " + iRet.ToString());
            }
        }

        private void SetDataTable53Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("VRHC", typeof(string));                     // 2
            dt.Columns.Add("VLHC", typeof(string));                     // 3
            dt.Columns.Add("VRCO", typeof(string));                     // 4
            dt.Columns.Add("VLCO", typeof(string));                     // 5
            dt.Columns.Add("VRNOX", typeof(string));                    // 6
            dt.Columns.Add("VLNOX", typeof(string));                    // 7

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 8
            dt.Columns.Add("CREATOR", typeof(string));                  // 9
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 10
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 11
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 12
            dt.Columns.Add("ISDELETED", typeof(string));                // 13
            dt.Columns.Add("DELETER", typeof(string));                  // 14

            DataRow dr = dt.NewRow();
            dr[1] = strKeyID;
            dr[2] = result.VRHC;
            dr[3] = result.VLHC;
            dr[4] = result.VRCO_53;
            dr[5] = result.VLCO_53;
            dr[6] = result.VRNOX;
            dr[7] = result.VLNOX;

            dr[10] = DateTime.Now.ToLocalTime();
            dr[11] = m_cfg.DB.Name;
            dr[13] = "0";
            dt.Rows.Add(dr);
            int iRet;
            try {
                string[] strVals = m_dbOracle.GetValue(dt.TableName, "ID", "WQPF_ID", strKeyID);
                if (strVals.Length == 0) {
                    iRet = m_dbOracle.InsertRecords(dt.TableName, dt);
                } else {
                    iRet = m_dbOracle.UpdateRecords(dt.TableName, dt, "ID", strVals);
                }
            } catch (Exception) {
                throw;
            }
            if (iRet <= 0) {
                throw new Exception("插入或更新 MES 数据出错，返回的影响行数: " + iRet.ToString());
            }
        }

        private void SetDataTable54Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("RATEREVUP", typeof(string));                // 2
            dt.Columns.Add("RATEREVDOWN", typeof(string));              // 3
            dt.Columns.Add("REV100", typeof(string));                   // 4
            dt.Columns.Add("MAXPOWER", typeof(string));                 // 5
            dt.Columns.Add("MAXPOWERLIMIT", typeof(string));            // 6
            dt.Columns.Add("SMOKE100", typeof(string));                 // 7
            dt.Columns.Add("SMOKE80", typeof(string));                  // 8
            dt.Columns.Add("SMOKELIMIT", typeof(string));               // 9
            dt.Columns.Add("NOX", typeof(string));                      // 10
            dt.Columns.Add("NOXLIMIT", typeof(string));                 // 11

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 12
            dt.Columns.Add("CREATOR", typeof(string));                  // 13
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 14
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 15
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 16
            dt.Columns.Add("ISDELETED", typeof(string));                // 17
            dt.Columns.Add("DELETER", typeof(string));                  // 18

            DataRow dr = dt.NewRow();
            dr[1] = strKeyID;
            dr[2] = result.RATEREVUP;
            dr[3] = result.RATEREVDOWN;
            dr[4] = result.REV100;
            dr[5] = result.MAXPOWER;
            dr[6] = result.MAXPOWERLIMIT;
            dr[7] = result.SMOKE100;
            dr[8] = result.SMOKE80;
            dr[9] = result.SMOKELIMIT;
            dr[10] = result.NOX;
            dr[11] = result.NOXLIMIT;

            dr[14] = DateTime.Now.ToLocalTime();
            dr[15] = m_cfg.DB.Name;
            dr[17] = "0";
            dt.Rows.Add(dr);
            int iRet;
            try {
                string[] strVals = m_dbOracle.GetValue(dt.TableName, "ID", "WQPF_ID", strKeyID);
                if (strVals.Length == 0) {
                    iRet = m_dbOracle.InsertRecords(dt.TableName, dt);
                } else {
                    iRet = m_dbOracle.UpdateRecords(dt.TableName, dt, "ID", strVals);
                }
            } catch (Exception) {
                throw;
            }
            if (iRet <= 0) {
                throw new Exception("插入或更新 MES 数据出错，返回的影响行数: " + iRet.ToString());
            }
        }

        private void SetDataTable55Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("RATEREV", typeof(string));                  // 2
            dt.Columns.Add("REV", typeof(string));                      // 3
            dt.Columns.Add("SMOKEK1", typeof(string));                  // 4
            dt.Columns.Add("SMOKEK2", typeof(string));                  // 5
            dt.Columns.Add("SMOKEK3", typeof(string));                  // 6
            dt.Columns.Add("SMOKEAVG", typeof(string));                 // 7
            dt.Columns.Add("SMOKEKLIMIT", typeof(string));              // 8

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 9
            dt.Columns.Add("CREATOR", typeof(string));                  // 10
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 11
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 12
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 13
            dt.Columns.Add("ISDELETED", typeof(string));                // 14
            dt.Columns.Add("DELETER", typeof(string));                  // 15
        }

        private void SetDataTable56Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("VRCO", typeof(string));                     // 2
            dt.Columns.Add("VLCO", typeof(string));                     // 3
            dt.Columns.Add("VRHCNOX", typeof(string));                  // 4
            dt.Columns.Add("VLHCNOX", typeof(string));                  // 5

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 6
            dt.Columns.Add("CREATOR", typeof(string));                  // 7
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 8
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 9
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 10
            dt.Columns.Add("ISDELETED", typeof(string));                // 11
            dt.Columns.Add("DELETER", typeof(string));                  // 12
        }

        private void SetDataTable6Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));                       // 0
            dt.Columns.Add("WQPF_ID", typeof(string));                  // 1

            dt.Columns.Add("ANALYMANUF", typeof(string));               // 2
            dt.Columns.Add("ANALYNAME", typeof(string));                // 3
            dt.Columns.Add("ANALYMODEL", typeof(string));               // 4
            dt.Columns.Add("ANALYDATE", typeof(string));                // 5
            dt.Columns.Add("DYNOMODEL", typeof(string));                // 6
            dt.Columns.Add("DYNOMANUF", typeof(string));                // 7

            dt.Columns.Add("CREATIONTIME", typeof(DateTime));           // 8
            dt.Columns.Add("CREATOR", typeof(string));                  // 9
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));   // 10
            dt.Columns.Add("LASTMODIFIER", typeof(string));             // 11
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));           // 12
            dt.Columns.Add("ISDELETED", typeof(string));                // 13
            dt.Columns.Add("DELETER", typeof(string));                  // 14

            DataRow dr = dt.NewRow();
            dr[1] = strKeyID;
            dr[2] = result.ANALYMANUF;
            dr[3] = result.ANALYNAME;
            dr[4] = result.ANALYMODEL;
            dr[5] = result.ANALYDATE;
            dr[6] = result.DYNOMODEL;
            dr[7] = result.DYNOMANUF;

            dr[10] = DateTime.Now.ToLocalTime();
            dr[11] = m_cfg.DB.Name;
            dr[13] = "0";
            dt.Rows.Add(dr);
            int iRet;
            try {
                string[] strVals = m_dbOracle.GetValue(dt.TableName, "ID", "WQPF_ID", strKeyID);
                if (strVals.Length == 0) {
                    iRet = m_dbOracle.InsertRecords(dt.TableName, dt);
                } else {
                    iRet = m_dbOracle.UpdateRecords(dt.TableName, dt, "ID", strVals);
                }
            } catch (Exception) {
                throw;
            }
            if (iRet <= 0) {
                throw new Exception("插入或更新 MES 数据出错，返回的影响行数: " + iRet.ToString());
            }
        }

    }

    public static class MainFileVersion {
        public static Version AssemblyVersion {
            get { return ((Assembly.GetEntryAssembly()).GetName()).Version; }
        }

        public static Version AssemblyFileVersion {
            get { return new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion); }
        }

        public static string AssemblyInformationalVersion {
            get { return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion; }
        }
    }

}
