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
            Text += " Ver: " + MainFileVersion.AssemblyVersion;
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
                    m_db.AddUploadField();
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
            m_dynoServer = new TCPImplement(this, txtBoxDynoParam, m_cfg.DynoParam.Data, m_cfg.DynoSimData.Data, m_dbOracle, m_logTCP);
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
            DataTable dt55 = new DataTable("IF_EM_WQPF_5_5");
            DataTable dt56 = new DataTable("IF_EM_WQPF_5_6");
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
                    case "6":
                        SetDataTable55Oracle(ID_MES[0], dt55, result);
                        break;
                    case "8":
                        SetDataTable56Oracle(ID_MES[0], dt56, result);
                        break;
                    }
                    SetDataTable6Oracle(ID_MES[0], dt6, result);
                    m_db.SetUpload(1, result.F_KEY);
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
                dt55.Dispose();
                dt56.Dispose();
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
                    Invoke((EventHandler)delegate {
                        txtBoxAutoUpload.BackColor = Color.LightGreen;
                        txtBoxAutoUpload.ForeColor = m_foreColor;
                        txtBoxAutoUpload.Text = "VIN[" + result.VIN + "]数据已自动上传";
                    });
                } else {
                    if (errorMsg.Contains("Exception|")) {
                        MessageBox.Show(errorMsg.Split('|')[1], "上传出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    } else {
                        if (m_VehicleRetryDic.ContainsKey(result.F_KEY.Value)) {
                            if (m_VehicleRetryDic[result.F_KEY.Value]++ >= RETRY_NUM) {
                                m_db.SetSkip(1, result.F_KEY);
                                m_log.TraceWarning(string.Format("SetSkip at VIN: {0}, {1}: {2}", result.VIN, result.F_KEY.Name, result.F_KEY.Value));
                            }
                        } else {
                            m_VehicleRetryDic.Add(result.F_KEY.Value, 1);
                        }
                        Invoke((EventHandler)delegate {
                            txtBoxAutoUpload.BackColor = Color.Red;
                            txtBoxAutoUpload.ForeColor = Color.White;
                            txtBoxAutoUpload.Text = "VIN[" + result.VIN + "]" + errorMsg;
                        });
                    }
                }
            }
        }

        private void SetDataTable2Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("RH", typeof(string));
            dt.Columns.Add("ET", typeof(string));
            dt.Columns.Add("AP", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["RH"] = result.RH;
            dr["ET"] = result.ET;
            dr["AP"] = result.AP;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("TESTTYPE", typeof(string));
            dt.Columns.Add("TESTNO", typeof(string));
            dt.Columns.Add("TESTDATE", typeof(string));
            dt.Columns.Add("APASS", typeof(string));
            dt.Columns.Add("OPASS", typeof(string));
            dt.Columns.Add("OTESTDATE", typeof(string));
            dt.Columns.Add("EPASS", typeof(string));
            dt.Columns.Add("RESULT", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["TESTTYPE"] = result.TESTTYPE;
            dr["TESTDATE"] = result.TESTDATE;
            dr["EPASS"] = result.EPASS;
            //dr["RESULT"] = result.EPASS;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("REAC", typeof(string));
            dt.Columns.Add("LEACMAX", typeof(string));
            dt.Columns.Add("LEACMIN", typeof(string));
            dt.Columns.Add("LRCO", typeof(string));
            dt.Columns.Add("LLCO", typeof(string));
            dt.Columns.Add("LRHC", typeof(string));
            dt.Columns.Add("LLHC", typeof(string));
            dt.Columns.Add("HRCO", typeof(string));
            dt.Columns.Add("HLCO", typeof(string));
            dt.Columns.Add("HRHC", typeof(string));
            dt.Columns.Add("HLHC", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["REAC"] = result.REAC;
            dr["LEACMAX"] = result.LEACMAX;
            dr["LEACMIN"] = result.LEACMIN;
            dr["LRCO"] = result.LRCO;
            dr["LLCO"] = result.LLCO;
            dr["LRHC"] = result.LRHC;
            dr["LLHC"] = result.LLHC;
            dr["HRCO"] = result.HRCO;
            dr["HLCO"] = result.HLCO;
            dr["HRHC"] = result.HRHC;
            dr["HLHC"] = result.HLHC;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("ARHC5025", typeof(string));
            dt.Columns.Add("ALHC5025", typeof(string));
            dt.Columns.Add("ARCO5025", typeof(string));
            dt.Columns.Add("ALCO5025", typeof(string));
            dt.Columns.Add("ARNOX5025", typeof(string));
            dt.Columns.Add("ALNOX5025", typeof(string));
            dt.Columns.Add("ARHC2540", typeof(string));
            dt.Columns.Add("ALHC2540", typeof(string));
            dt.Columns.Add("ARCO2540", typeof(string));
            dt.Columns.Add("ALCO2540", typeof(string));
            dt.Columns.Add("ARNOX2540", typeof(string));
            dt.Columns.Add("ALNOX2540", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["ARHC5025"] = result.ARHC5025;
            dr["ALHC5025"] = result.ALHC5025;
            dr["ARCO5025"] = result.ARCO5025;
            dr["ALCO5025"] = result.ALCO5025;
            dr["ARNOX5025"] = result.ARNOX5025;
            dr["ALNOX5025"] = result.ALNOX5025;
            dr["ARHC2540"] = result.ARHC2540;
            dr["ALHC2540"] = result.ALHC2540;
            dr["ARCO2540"] = result.ARCO2540;
            dr["ALCO2540"] = result.ALCO2540;
            dr["ARNOX2540"] = result.ARNOX2540;
            dr["ALNOX2540"] = result.ALNOX2540;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("VRHC", typeof(string));
            dt.Columns.Add("VLHC", typeof(string));
            dt.Columns.Add("VRCO", typeof(string));
            dt.Columns.Add("VLCO", typeof(string));
            dt.Columns.Add("VRNOX", typeof(string));
            dt.Columns.Add("VLNOX", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["VRHC"] = result.VRHC;
            dr["VLHC"] = result.VLHC;
            dr["VRCO"] = result.VRCO_53;
            dr["VLCO"] = result.VLCO_53;
            dr["VRNOX"] = result.VRNOX;
            dr["VLNOX"] = result.VLNOX;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("RATEREVUP", typeof(string));
            dt.Columns.Add("RATEREVDOWN", typeof(string));
            dt.Columns.Add("REV100", typeof(string));
            dt.Columns.Add("MAXPOWER", typeof(string));
            dt.Columns.Add("MAXPOWERLIMIT", typeof(string));
            dt.Columns.Add("SMOKE100", typeof(string));
            dt.Columns.Add("SMOKE80", typeof(string));
            dt.Columns.Add("SMOKELIMIT", typeof(string));
            dt.Columns.Add("NOX", typeof(string));
            dt.Columns.Add("NOXLIMIT", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["RATEREVUP"] = result.RATEREVUP;
            dr["RATEREVDOWN"] = result.RATEREVDOWN;
            dr["REV100"] = result.REV100;
            dr["MAXPOWER"] = result.MAXPOWER;
            dr["MAXPOWERLIMIT"] = result.MAXPOWERLIMIT;
            dr["SMOKE100"] = result.SMOKE100;
            dr["SMOKE80"] = result.SMOKE80;
            dr["SMOKELIMIT"] = result.SMOKELIMIT;
            dr["NOX"] = result.NOX;
            dr["NOXLIMIT"] = result.NOXLIMIT;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("RATEREV", typeof(string));
            dt.Columns.Add("REV", typeof(string));
            dt.Columns.Add("SMOKEK1", typeof(string));
            dt.Columns.Add("SMOKEK2", typeof(string));
            dt.Columns.Add("SMOKEK3", typeof(string));
            dt.Columns.Add("SMOKEAVG", typeof(string));
            dt.Columns.Add("SMOKEKLIMIT", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["RATEREV"] = result.RATEREV;
            dr["REV"] = result.REV;
            dr["SMOKEK1"] = result.SMOKEK1;
            dr["SMOKEK2"] = result.SMOKEK2;
            dr["SMOKEK3"] = result.SMOKEK3;
            dr["SMOKEAVG"] = result.SMOKEAVG;
            dr["SMOKEKLIMIT"] = result.SMOKEKLIMIT;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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

        private void SetDataTable56Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("VRCO", typeof(string));
            dt.Columns.Add("VLCO", typeof(string));
            dt.Columns.Add("VRHCNOX", typeof(string));
            dt.Columns.Add("VLHCNOX", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["VRCO"] = result.VRCO_56;
            dr["VLCO"] = result.VLCO_56;
            dr["VRHCNOX"] = result.VRHCNOX;
            dr["VLHCNOX"] = result.VLHCNOX;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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

        private void SetDataTable6Oracle(string strKeyID, DataTable dt, UploadField result) {
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("WQPF_ID", typeof(string));
            dt.Columns.Add("ANALYMANUF", typeof(string));
            dt.Columns.Add("ANALYNAME", typeof(string));
            dt.Columns.Add("ANALYMODEL", typeof(string));
            dt.Columns.Add("ANALYDATE", typeof(string));
            dt.Columns.Add("DYNOMODEL", typeof(string));
            dt.Columns.Add("DYNOMANUF", typeof(string));
            dt.Columns.Add("CREATIONTIME", typeof(DateTime));
            dt.Columns.Add("CREATOR", typeof(string));
            dt.Columns.Add("LASTMODIFICATIONTIME", typeof(DateTime));
            dt.Columns.Add("LASTMODIFIER", typeof(string));
            dt.Columns.Add("DELETIONTIME", typeof(DateTime));
            dt.Columns.Add("ISDELETED", typeof(string));
            dt.Columns.Add("DELETER", typeof(string));

            DataRow dr = dt.NewRow();
            dr["WQPF_ID"] = strKeyID;
            dr["ANALYMANUF"] = result.ANALYMANUF;
            dr["ANALYNAME"] = result.ANALYNAME;
            dr["ANALYMODEL"] = result.ANALYMODEL;
            dr["ANALYDATE"] = result.ANALYDATE;
            dr["DYNOMODEL"] = result.DYNOMODEL;
            dr["DYNOMANUF"] = result.DYNOMANUF;
            dr["LASTMODIFICATIONTIME"] = DateTime.Now.ToLocalTime();
            dr["LASTMODIFIER"] = m_cfg.DB.Data.Name;
            dr["ISDELETED"] = "0";
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
            Dictionary<string, string> result = m_db.GetKeyData(strVIN, m_cfg.FieldUL.Data.F_KEY.Name);
            if (result != null && result.ContainsKey("外检报告编号") && result["外检报告编号"] != null) {
                m_cfg.FieldUL.Data.F_KEY.Value = result["外检报告编号"];
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

            result = m_db.GetEnv(m_cfg.FieldUL.Data.F_KEY);
            m_dtEnv.Clear();
            foreach (string key in result.Keys) {
                dr = m_dtEnv.NewRow();
                dr[0] = key;
                dr[1] = result[key];
                m_dtEnv.Rows.Add(dr);
            }

            result = m_db.GetResult(m_cfg.FieldUL.Data);
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
                result = m_db.Get51(m_cfg.FieldUL.Data);
                m_dt51.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt51.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt51.Rows.Add(dr);
                }
                break;
            case "2":
                result = m_db.Get52(m_cfg.FieldUL.Data);
                m_dt52.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt52.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt52.Rows.Add(dr);
                }
                break;
            case "3":
                result = m_db.Get53(m_cfg.FieldUL.Data);
                m_dt53.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt53.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt53.Rows.Add(dr);
                }
                break;
            case "4":
                result = m_db.Get54(m_cfg.FieldUL.Data);
                m_dt54.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt54.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt54.Rows.Add(dr);
                }
                break;
            case "6":
                result = m_db.Get55(m_cfg.FieldUL.Data);
                m_dt55.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt55.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt55.Rows.Add(dr);
                }
                break;
            case "8":
                result = m_db.Get56(m_cfg.FieldUL.Data);
                m_dt56.Clear();
                foreach (string key in result.Keys) {
                    dr = m_dt56.NewRow();
                    dr[0] = key;
                    dr[1] = result[key];
                    m_dt56.Rows.Add(dr);
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
                Invoke((EventHandler)delegate {
                    txtBoxManualUpload.BackColor = Color.Red;
                    txtBoxManualUpload.ForeColor = Color.White;
                    txtBoxManualUpload.Text = "VIN[" + this.txtBoxVIN.Text + "]未获取到排放数据";
                });
            } else {
                if (Upload(result, out string errorMsg)) {
                    Invoke((EventHandler)delegate {
                        txtBoxManualUpload.BackColor = Color.LightGreen;
                        txtBoxManualUpload.ForeColor = m_foreColor;
                        txtBoxManualUpload.Text = "VIN[" + result.VIN + "]数据已手动上传";
                    });
                } else {
                    if (errorMsg.Contains("Exception|")) {
                        MessageBox.Show(errorMsg.Split('|')[1], "上传出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    } else {
                        Invoke((EventHandler)delegate {
                            txtBoxManualUpload.BackColor = Color.Red;
                            txtBoxManualUpload.ForeColor = Color.White;
                            txtBoxManualUpload.Text = "VIN[" + result.VIN + "]" + errorMsg;
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
                        txtBoxVIN.Text = codes[2];
                    } else if (codes.Length == 1) {
                        txtBoxVIN.Text = codes[0];
                    }
                    if (txtBoxVIN.Text.Length == 17) {
                        txtBoxManualUpload.BackColor = m_backColor;
                        txtBoxManualUpload.ForeColor = m_foreColor;
                        txtBoxManualUpload.Text = "手动上传就绪";
                        if (ShowData(txtBoxVIN.Text)) {
                            txtBoxManualUpload.BackColor = m_backColor;
                            txtBoxManualUpload.ForeColor = m_foreColor;
                            txtBoxManualUpload.Text = "数据显示完毕";
                        } else {
                            txtBoxManualUpload.BackColor = Color.Red;
                            txtBoxManualUpload.ForeColor = Color.White;
                            txtBoxManualUpload.Text = "VIN[" + this.txtBoxVIN.Text + "]未获取到排放数据";
                        }
                        txtBoxVIN.SelectAll();
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
