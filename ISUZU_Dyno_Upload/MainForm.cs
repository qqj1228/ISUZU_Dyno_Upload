using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ISUZU_Dyno_Upload {
    public partial class MainForm : Form {
        public const int RETRY_NUM = 10;
        public Logger m_log;
        public Logger m_logTCP;
        public Config m_cfg;
        public Model m_db;
        public ModelOracle m_dbOracle;
        public TCPImplement m_dynoServer;
        public int m_iCNLenb;
        readonly System.Timers.Timer m_timer;
        public DataTable m_dtInfo;
        public DataTable m_dtEnv;
        public DataTable m_dtResult;
        public DataTable m_dtDevice;
        public DataTable m_dt51;
        public DataTable m_dt52;
        public DataTable m_dt53;
        public DataTable m_dt54;
        public DataTable m_dt55;
        public DataTable m_dt56;
        public Color m_backColor;
        public Color m_foreColor;
        public Dictionary<string, int> m_VehicleRetryDic;

        public MainForm() {
            InitializeComponent();
            this.Text = "ISUZU_Dyno_Upload Ver: " + MainFileVersion.AssemblyVersion;
            m_log = new Logger("Upload", ".\\log", EnumLogLevel.LogLevelAll, true, 100);
            m_log.TraceInfo("==================================================================");
            m_log.TraceInfo("==================== START Ver: " + MainFileVersion.AssemblyVersion + " ====================");
            m_cfg = new Config(m_log);
            m_db = new Model(m_cfg.DB.Data.SqlServer, m_log);
            m_dbOracle = new ModelOracle(m_cfg.DB.Data.Oracle, m_cfg.DB.Data.Dyno, m_log);
            m_iCNLenb = 3;
            m_VehicleRetryDic = new Dictionary<string, int>();
            Task.Factory.StartNew(new Action(() => {
                try {
                    m_iCNLenb = m_dbOracle.GetCNLenb();
                } catch (Exception ex) {
                    m_log.TraceError("Can't connect with MES: " + ex.Message);
                    MessageBox.Show("无法与MES通讯，请检查设置\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
            Task.Factory.StartNew(new Action(() => {
                try {
                    m_db.TestConnect();
                    m_db.AddSkipField();
                } catch (Exception ex) {
                    m_log.TraceError("Can't connect with dyno database: " + ex.Message);
                    MessageBox.Show("无法与测功机数据库通讯，请检查设置\n" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
#if DEBUG
            m_timer = new System.Timers.Timer(m_cfg.DB.Data.Interval * 1000);
            m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeUpload);
            m_timer.AutoReset = false;
            m_timer.Enabled = true;
#else
            m_timer = new System.Timers.Timer(m_cfg.DB.Data.Interval * 60 * 1000);
            m_timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeUpload);
            m_timer.AutoReset = true;
            m_timer.Enabled = true;
#endif
            if (m_cfg.DynoParam.Data.Enable) {
                StartDynoServer();
            }
        }

        private void StartDynoServer() {
            m_logTCP = new Logger("DynoServer", ".\\log", EnumLogLevel.LogLevelAll, true, 100);
            m_dynoServer = new TCPImplement(this, this.txtBoxDynoParam, m_cfg.DynoParam.Data, m_cfg.DynoSimData.Data, m_dbOracle, m_logTCP);
        }

        private bool Upload(UploadField result, out string errorMsg) {
            bool bRet = false;
            errorMsg = "";
            if (result.EPASS != "1") {
                m_log.TraceInfo("Skip this VIN[" + result.VIN + "] because of EPASS != 1");
                errorMsg = "排放结果不合格，数据不上传";
                return bRet;
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
                    m_db.SetUpload(1, result.JCLSH);
                    bRet = true;
                } else {
                    m_log.TraceWarning("Skip this VIN[" + result.VIN + "] because it is not in MES");
                    errorMsg = "在MES中未检测到记录，数据不上传";
                }
            } catch (Exception ex) {
                m_log.TraceError("Upload error: " + ex.Message);
                errorMsg = "Exception|" + ex.Message;
            } finally {
                dt2.Dispose();
                dt3.Dispose();
                dt51.Dispose();
                dt52.Dispose();
                dt53.Dispose();
                dt54.Dispose();
                dt6.Dispose();
            }
            return bRet;
        }

        private void OnTimeUpload(object source, System.Timers.ElapsedEventArgs e) {
            m_log.TraceInfo(">>>>>>>> Upload dyno data OnTime. Ver: " + MainFileVersion.AssemblyVersion + " <<<<<<<<");
            List<UploadField> resultList = new List<UploadField>();
            try {
                resultList = m_db.GetDynoData(m_cfg.FieldUL.Data, m_iCNLenb);
            } catch (Exception ex) {
                m_log.TraceError("GetDynoData error: " + ex.Message);
            }
            foreach (UploadField result in resultList) {
                if (Upload(result, out string errorMsg)) {
                    this.Invoke((EventHandler)delegate {
                        this.txtBoxAutoUpload.BackColor = Color.LightGreen;
                        this.txtBoxAutoUpload.ForeColor = m_foreColor;
                        this.txtBoxAutoUpload.Text = "VIN[" + result.VIN + "]数据已自动上传";
                    });
                } else {
                    if (errorMsg.Contains("Exception|")) {
                        MessageBox.Show(errorMsg.Split('|')[1], "上传出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    } else {
                        if (m_VehicleRetryDic.ContainsKey(result.JCLSH)) {
                            if (m_VehicleRetryDic[result.JCLSH]++ >= RETRY_NUM) {
                                m_db.SetSkip(1, result.JCLSH);
                                m_log.TraceWarning(string.Format("SetSkip at VIN: {0}, JCLSH: {1}", result.VIN, result.JCLSH));
                            }
                        } else {
                            m_VehicleRetryDic.Add(result.JCLSH, 1);
                        }
                        this.Invoke((EventHandler)delegate {
                            this.txtBoxAutoUpload.BackColor = Color.Red;
                            this.txtBoxAutoUpload.ForeColor = Color.White;
                            this.txtBoxAutoUpload.Text = "VIN[" + result.VIN + "]" + errorMsg;
                        });
                    }
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

        private void SetGridViewColumnsSortMode(DataGridView gridView, DataGridViewColumnSortMode sortMode) {
            for (int i = 0; i < gridView.Columns.Count; i++) {
                gridView.Columns[i].SortMode = sortMode;
            }
        }

        private bool ShowData(string strVIN) {
            DataRow dr;
            string JCLSH;
            Dictionary<string, string> result = m_db.GetJCLSH(strVIN);
            if (result != null && result.ContainsKey("检测流水号") && result["检测流水号"] != null) {
                JCLSH = result["检测流水号"];
            } else {
                return false;
            }
            m_dtInfo.Clear();
            foreach (string key in result.Keys) {
                dr = m_dtInfo.NewRow();
                dr[0] = key;
                dr[1] = result[key];
                m_dtInfo.Rows.Add(dr);
            }

            result = m_db.GetEnv(JCLSH);
            m_dtEnv.Clear();
            foreach (string key in result.Keys) {
                dr = m_dtEnv.NewRow();
                dr[0] = key;
                dr[1] = result[key];
                m_dtEnv.Rows.Add(dr);
            }

            result = m_db.GetResult(m_cfg.FieldUL.Data, JCLSH);
            m_dtResult.Clear();
            foreach (string key in result.Keys) {
                dr = m_dtResult.NewRow();
                dr[0] = key;
                dr[1] = result[key];
                m_dtResult.Rows.Add(dr);
            }

            result = m_db.GetDevice(m_cfg.FieldUL.Data, m_iCNLenb);
            m_dtDevice.Clear();
            foreach (string key in result.Keys) {
                dr = m_dtDevice.NewRow();
                dr[0] = key;
                dr[1] = result[key];
                m_dtDevice.Rows.Add(dr);
            }

            switch (m_dtResult.Rows[0][1]) {
            case "1":
                result = m_db.Get51(m_cfg.FieldUL.Data, JCLSH);
                m_dt51.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt51.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt51.Rows.Add(dr);
                }
                break;
            case "2":
                result = m_db.Get52(m_cfg.FieldUL.Data, JCLSH);
                m_dt52.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt52.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt52.Rows.Add(dr);
                }
                break;
            case "3":
                result = m_db.Get53(m_cfg.FieldUL.Data, JCLSH);
                m_dt53.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt53.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt53.Rows.Add(dr);
                }
                break;
            case "4":
                result = m_db.Get54(m_cfg.FieldUL.Data, JCLSH);
                m_dt54.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt54.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt54.Rows.Add(dr);
                }
                break;
            }
            return true;
        }

        private void ManualUpload() {
            UploadField result = null;
            try {
                result = m_db.GetDynoDataByVIN(m_cfg.FieldUL.Data, m_iCNLenb, this.txtBoxVIN.Text);
            } catch (Exception ex) {
                m_log.TraceError("GetDynoData error: " + ex.Message);
            }
            if (result == null) {
                this.Invoke((EventHandler)delegate {
                    this.txtBoxManualUpload.BackColor = Color.Red;
                    this.txtBoxManualUpload.ForeColor = Color.White;
                    this.txtBoxManualUpload.Text = "VIN[" + this.txtBoxVIN.Text + "]未获取到排放数据";
                });
            } else {
                if (Upload(result, out string errorMsg)) {
                    this.Invoke((EventHandler)delegate {
                        this.txtBoxManualUpload.BackColor = Color.LightGreen;
                        this.txtBoxManualUpload.ForeColor = m_foreColor;
                        this.txtBoxManualUpload.Text = "VIN[" + result.VIN + "]数据已手动上传";
                    });
                } else {
                    if (errorMsg.Contains("Exception|")) {
                        MessageBox.Show(errorMsg.Split('|')[1], "上传出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    } else {
                        this.Invoke((EventHandler)delegate {
                            this.txtBoxManualUpload.BackColor = Color.Red;
                            this.txtBoxManualUpload.ForeColor = Color.White;
                            this.txtBoxManualUpload.Text = "VIN[" + result.VIN + "]" + errorMsg;
                        });
                    }
                }
            }
        }

        private void TxtBoxVIN_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                TextBox tb = sender as TextBox;
                string[] codes = tb.Text.Split('*');
                if (codes != null) {
                    if (codes.Length > 2) {
                        this.txtBoxVIN.Text = codes[2];
                    } else if (codes.Length == 1) {
                        this.txtBoxVIN.Text = codes[0];
                    }
                    if (this.txtBoxVIN.Text.Length == 17) {
                        this.txtBoxManualUpload.BackColor = m_backColor;
                        this.txtBoxManualUpload.ForeColor = m_foreColor;
                        this.txtBoxManualUpload.Text = "手动上传就绪";
                        if (ShowData(this.txtBoxVIN.Text)) {
                            this.txtBoxManualUpload.BackColor = m_backColor;
                            this.txtBoxManualUpload.ForeColor = m_foreColor;
                            this.txtBoxManualUpload.Text = "数据显示完毕";
                        } else {
                            this.txtBoxManualUpload.BackColor = Color.Red;
                            this.txtBoxManualUpload.ForeColor = Color.White;
                            this.txtBoxManualUpload.Text = "VIN[" + this.txtBoxVIN.Text + "]未获取到排放数据";
                        }
                        this.txtBoxVIN.SelectAll();
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e) {
            m_dtInfo = new DataTable();
            m_dtInfo.Columns.Add("名称");
            m_dtInfo.Columns.Add("数值");
            this.GridViewInfo.DataSource = m_dtInfo;
            SetGridViewColumnsSortMode(this.GridViewInfo, DataGridViewColumnSortMode.Programmatic);
            m_dtEnv = new DataTable();
            m_dtEnv.Columns.Add("名称");
            m_dtEnv.Columns.Add("数值");
            this.GridViewEnv.DataSource = m_dtEnv;
            SetGridViewColumnsSortMode(this.GridViewEnv, DataGridViewColumnSortMode.Programmatic);
            m_dtResult = new DataTable();
            m_dtResult.Columns.Add("名称");
            m_dtResult.Columns.Add("数值");
            this.GridViewResult.DataSource = m_dtResult;
            SetGridViewColumnsSortMode(this.GridViewResult, DataGridViewColumnSortMode.Programmatic);
            m_dtDevice = new DataTable();
            m_dtDevice.Columns.Add("名称");
            m_dtDevice.Columns.Add("数值");
            this.GridViewDevice.DataSource = m_dtDevice;
            SetGridViewColumnsSortMode(this.GridViewDevice, DataGridViewColumnSortMode.Programmatic);
            m_dt51 = new DataTable();
            m_dt51.Columns.Add("名称");
            m_dt51.Columns.Add("数值");
            this.GridView51.DataSource = m_dt51;
            SetGridViewColumnsSortMode(this.GridView51, DataGridViewColumnSortMode.Programmatic);
            m_dt52 = new DataTable();
            m_dt52.Columns.Add("名称");
            m_dt52.Columns.Add("数值");
            this.GridView52.DataSource = m_dt52;
            SetGridViewColumnsSortMode(this.GridView52, DataGridViewColumnSortMode.Programmatic);
            m_dt53 = new DataTable();
            m_dt53.Columns.Add("名称");
            m_dt53.Columns.Add("数值");
            this.GridView53.DataSource = m_dt53;
            SetGridViewColumnsSortMode(this.GridView53, DataGridViewColumnSortMode.Programmatic);
            m_dt54 = new DataTable();
            m_dt54.Columns.Add("名称");
            m_dt54.Columns.Add("数值");
            this.GridView54.DataSource = m_dt54;
            SetGridViewColumnsSortMode(this.GridView54, DataGridViewColumnSortMode.Programmatic);
            m_dt55 = new DataTable();
            m_dt55.Columns.Add("名称");
            m_dt55.Columns.Add("数值");
            this.GridView55.DataSource = m_dt55;
            SetGridViewColumnsSortMode(this.GridView55, DataGridViewColumnSortMode.Programmatic);
            m_dt56 = new DataTable();
            m_dt56.Columns.Add("名称");
            m_dt56.Columns.Add("数值");
            this.GridView56.DataSource = m_dt56;
            SetGridViewColumnsSortMode(this.GridView56, DataGridViewColumnSortMode.Programmatic);
            m_backColor = this.txtBoxAutoUpload.BackColor;
            m_foreColor = this.txtBoxAutoUpload.ForeColor;
        }

        private void BtnUpload_Click(object sender, EventArgs e) {
            m_log.TraceInfo(">>>>>>>> Manual Upload dyno data. Ver: " + MainFileVersion.AssemblyVersion + " <<<<<<<<");
            Task.Factory.StartNew(ManualUpload);
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
