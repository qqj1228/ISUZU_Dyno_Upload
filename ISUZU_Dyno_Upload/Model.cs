﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace ISUZU_Dyno_Upload {
    public class Model {
        public string StrConn { get; set; }
        public readonly Logger m_log;
        public SqlServerNative m_sqlSetting;

        public Model(SqlServerNative settings, Logger log) {
            m_log = log;
            m_sqlSetting = settings;
            this.StrConn = "";
            ReadConfig();
        }

        void ReadConfig() {
            StrConn = "user id=" + m_sqlSetting.UserName + ";";
            StrConn += "password=" + m_sqlSetting.PassWord + ";";
            StrConn += "database=" + m_sqlSetting.DBName + ";";
            StrConn += "data source=" + m_sqlSetting.IP + "," + m_sqlSetting.Port;
        }

        public void ShowDB(string strTable) {
            string strSQL = "select * from " + strTable;

            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                sqlConn.Open();
                SqlCommand sqlCmd = new SqlCommand(strSQL, sqlConn);
                SqlDataReader sqlData = sqlCmd.ExecuteReader();
                string str = "";
                int c = sqlData.FieldCount;
                while (sqlData.Read()) {
                    for (int i = 0; i < c; i++) {
                        object obj = sqlData.GetValue(i);
                        if (obj.GetType() == typeof(DateTime)) {
                            str += ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss") + "\t";
                        } else {
                            str += obj.ToString() + "\t";
                        }
                    }
                    str += "\n";
                }
                m_log.TraceInfo(str);
                sqlCmd.Dispose();
                sqlConn.Close();
            }
        }

        public string[] GetTableColumns(string strTable) {
            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                try {
                    sqlConn.Open();
                    DataTable schema = sqlConn.GetSchema("Columns", new string[] { null, null, strTable });
                    schema.DefaultView.Sort = "ORDINAL_POSITION";
                    schema = schema.DefaultView.ToTable();
                    int count = schema.Rows.Count;
                    string[] columns = new string[count];
                    for (int i = 0; i < count; i++) {
                        DataRow row = schema.Rows[i];
                        foreach (DataColumn col in schema.Columns) {
                            if (col.Caption == "COLUMN_NAME") {
                                if (col.DataType.Equals(typeof(DateTime))) {
                                    columns[i] = string.Format("{0:d}", row[col]);
                                } else if (col.DataType.Equals(typeof(decimal))) {
                                    columns[i] = string.Format("{0:C}", row[col]);
                                } else {
                                    columns[i] = string.Format("{0}", row[col]);
                                }
                            }
                        }
                    }
                    return columns;
                } catch (Exception ex) {
                    m_log.TraceError("==> SQL ERROR: " + ex.Message);
                } finally {
                    sqlConn.Close();
                }
            }
            return new string[] { };
        }

        public Dictionary<string, int> GetTableColumnsDic(string strTable) {
            Dictionary<string, int> colDic = new Dictionary<string, int>();
            string[] cols = GetTableColumns(strTable);
            for (int i = 0; i < cols.Length; i++) {
                colDic.Add(cols[i], i);
            }
            return colDic;
        }

        public void InsertDB(string strTable, DataTable dt) {
            string columns = " (";
            for (int i = 0; i < dt.Columns.Count; i++) {
                columns += dt.Columns[i].ColumnName + ",";
            }
            columns = columns.Substring(0, columns.Length - 1) + ")";

            for (int i = 0; i < dt.Rows.Count; i++) {
                string row = " values ('";
                for (int j = 0; j < dt.Columns.Count; j++) {
                    row += dt.Rows[i][j].ToString() + "','";
                }
                row = row.Substring(0, row.Length - 2) + ")";
                string strSQL = "insert into " + strTable + columns + row;

                using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                    SqlCommand sqlCmd = new SqlCommand(strSQL, sqlConn);
                    try {
                        sqlConn.Open();
                        m_log.TraceInfo(string.Format("==> T-SQL: {0}", strSQL));
                        m_log.TraceInfo(string.Format("==> Insert {0} record(s)", sqlCmd.ExecuteNonQuery()));
                    } catch (Exception ex) {
                        m_log.TraceError("==> SQL ERROR: " + ex.Message);
                    } finally {
                        sqlCmd.Dispose();
                        sqlConn.Close();
                    }
                }
            }
        }

        public void UpdateDB(string strTable, DataTable dt, Dictionary<string, string> whereDic) {
            for (int i = 0; i < dt.Rows.Count; i++) {
                string strSQL = "update " + strTable + " set ";
                for (int j = 0; j < dt.Columns.Count; j++) {
                    strSQL += dt.Columns[j].ColumnName + " = '" + dt.Rows[i][j].ToString() + "', ";
                }
                strSQL = strSQL.Substring(0, strSQL.Length - 2);
                strSQL += " where ";
                foreach (string key in whereDic.Keys) {
                    strSQL += key + " = '" + whereDic[key] + "' and ";
                }
                strSQL = strSQL.Substring(0, strSQL.Length - 5);

                using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                    SqlCommand sqlCmd = new SqlCommand(strSQL, sqlConn);
                    try {
                        sqlConn.Open();
                        m_log.TraceInfo(string.Format("==> T-SQL: {0}", strSQL));
                        m_log.TraceInfo(string.Format("==> Update {0} record(s)", sqlCmd.ExecuteNonQuery()));
                    } catch (Exception ex) {
                        m_log.TraceError("==> SQL ERROR: " + ex.Message);
                    } finally {
                        sqlCmd.Dispose();
                        sqlConn.Close();
                    }
                }

            }
        }

        int RunSQL(string strSQL) {
            int count = 0;
            if (strSQL.Length == 0) {
                return -1;
            }
            try {
                using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                    SqlCommand sqlCmd = new SqlCommand(strSQL, sqlConn);
                    try {
                        sqlConn.Open();
                        count = sqlCmd.ExecuteNonQuery();
                        m_log.TraceInfo(string.Format("==> T-SQL: {0}", strSQL));
                        m_log.TraceInfo(string.Format("==> {0} record(s) affected", count));
                    } catch (Exception ex) {
                        m_log.TraceError("==> SQL ERROR: " + ex.Message);
                    } finally {
                        sqlCmd.Dispose();
                        sqlConn.Close();
                    }
                }
            } catch (Exception ex) {
                m_log.TraceError("==> SQL ERROR: " + ex.Message);
            }
            return count;
        }

        string[,] SelectDB(string strSQL) {
            string[,] records = null;
            try {
                int count = 0;
                List<string[]> rowList;
                using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                    SqlCommand sqlCmd = new SqlCommand(strSQL, sqlConn);
                    sqlConn.Open();
                    SqlDataReader sqlData = sqlCmd.ExecuteReader();
                    count = sqlData.FieldCount;
                    rowList = new List<string[]>();
                    while (sqlData.Read()) {
                        string[] items = new string[count];
                        for (int i = 0; i < count; i++) {
                            object obj = sqlData.GetValue(i);
                            if (obj.GetType() == typeof(DateTime)) {
                                items[i] = ((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss");
                            } else {
                                items[i] = obj.ToString();
                            }
                        }
                        rowList.Add(items);
                    }
                    sqlCmd.Dispose();
                    sqlConn.Close();
                }
                records = new string[rowList.Count, count];
                for (int i = 0; i < rowList.Count; i++) {
                    for (int j = 0; j < count; j++) {
                        records[i, j] = rowList[i][j];
                    }
                }
                return records;
            } catch (Exception ex) {
                m_log.TraceError("==> SQL ERROR: " + ex.Message);
            }
            return records;
        }

        public int GetRecordCount(string strTable, Dictionary<string, string> whereDic) {
            string strSQL = "select * from " + strTable + " where ";
            foreach (string key in whereDic.Keys) {
                strSQL += key + " = '" + whereDic[key] + "' and ";
            }
            strSQL = strSQL.Substring(0, strSQL.Length - 5);
            m_log.TraceInfo("==> T-SQL: " + strSQL);
            string[,] strArr = SelectDB(strSQL);
            if (strArr != null) {
                return strArr.GetLength(0);
            } else {
                return -1;
            }
        }

        public string[,] GetRecords(string strTable, Dictionary<string, string> whereDic) {
            string strSQL = "select * from " + strTable + " where ";
            foreach (string key in whereDic.Keys) {
                strSQL += key + " = '" + whereDic[key] + "' and ";
            }
            strSQL = strSQL.Substring(0, strSQL.Length - 5);
            m_log.TraceInfo("==> T-SQL: " + strSQL);
            return SelectDB(strSQL);
        }

        public bool ModifyDB(string strTable, DataTable dt) {
            for (int i = 0; i < dt.Rows.Count; i++) {
                Dictionary<string, string> whereDic = new Dictionary<string, string> {
                    { "VIN", dt.Rows[i][0].ToString() },
                    { "ECU_ID", dt.Rows[i][1].ToString() }
                };
                string strSQL = "";
                int count = GetRecordCount(strTable, whereDic);
                if (count > 0) {
                    strSQL = "update " + strTable + " set ";
                    for (int j = 0; j < dt.Columns.Count; j++) {
                        strSQL += dt.Columns[j].ColumnName + " = '" + dt.Rows[i][j].ToString() + "', ";
                    }
                    strSQL += "WriteTime = '" + DateTime.Now.ToLocalTime().ToString() + "' where ";
                    foreach (string key in whereDic.Keys) {
                        strSQL += key + " = '" + whereDic[key] + "' and ";
                    }
                    strSQL = strSQL.Substring(0, strSQL.Length - 5);
                } else if (count == 0) {
                    strSQL = "insert " + strTable + " (";
                    for (int j = 0; j < dt.Columns.Count; j++) {
                        strSQL += dt.Columns[j].ColumnName + ", ";
                    }
                    strSQL = strSQL.Substring(0, strSQL.Length - 2) + ") values ('";

                    for (int j = 0; j < dt.Columns.Count; j++) {
                        strSQL += dt.Rows[i][j].ToString() + "', '";
                    }
                    strSQL = strSQL.Substring(0, strSQL.Length - 3) + ")";
                } else if (count < 0) {
                    return false;
                }
                RunSQL(strSQL);
            }
            return true;
        }

        private string GetFieldValue(string field, string JCLSH, bool bNumber = false, int precision = 0, int scale = 0) {
            string[,] vals = null;
            string strRet = "";
            if (field.Length > 0 && field != "--") {
                string strSQL = "select [" + field.Split('.')[1] + "] from [" + field.Split('.')[0] + "] where [JCLSH] = '" + JCLSH + "'";
                //m_log.TraceInfo("==> T-SQL: " + strSQL);
                vals = SelectDB(strSQL);
            }
            if (vals != null && vals.GetLength(0) > 0) {
                if (bNumber) {
                    strRet = Normalize(vals[0, 0], precision, scale);
                    if (strRet != vals[0, 0]) {
                        m_log.TraceError("original value\"" + vals[0, 0] + "\" larger than specified precision\"" + precision.ToString() + "\" and scale\"" + scale.ToString() + "\"");
                    }
                } else {
                    strRet = vals[0, 0];
                }
            }
            return strRet;
        }

        private string Normalize(string strNum, int precision, int scale) {
            if (strNum == null || strNum.Length == 0) {
                return strNum;
            }
            if (strNum.Split('.')[0].Length > precision - scale) {
                return new string('9', precision - scale) + "." + new string('9', scale);
            } else {
                return strNum;
            }
        }

        private string GetTestType(string strValue) {
            string strRet = "9";
            if (strValue.Contains("双怠速")) {
                strRet = "1";
            } else if (strValue.Contains("稳态工况")) {
                strRet = "2";
            } else if (strValue.Contains("简易瞬态")) {
                strRet = "3";
            } else if (strValue.Contains("加载减速")) {
                strRet = "4";
            } else if (strValue.Contains("自由加速")) {
                strRet = "6";
            } else if (strValue.Contains("林格曼")) {
                strRet = "7";
            } else if (strValue.Contains("瞬态工况")) {
                strRet = "8";
            }
            return strRet;
        }

        /// <summary>
        /// 获取环境数据，返回值为[RH, ET, AP]数组
        /// </summary>
        /// <param name="JCLSH">检测流水号</param>
        /// <returns></returns>
        private string[] GetEnvValue(string JCLSH) {
            string[,] vals;
            string[] rets = { "", "", "" };
            EnvStructure[] envs = new EnvStructure[3];
            envs[0] = new EnvStructure("SD", 4, 2);
            envs[1] = new EnvStructure("WD", 5, 2);
            envs[2] = new EnvStructure("DQY", 5, 2);
            string[] tables = { "双怠速法结果库", "稳态工况结果库", "简易瞬态结果库", "加载减速结果库" };

            for (int j = 0; j < tables.Length; j++) {
                string strSQL = "select [" + envs[0].Column + "], [" + envs[1].Column + "], [" + envs[2].Column + "] from [" + tables[j] + "] where [JCLSH] = '" + JCLSH + "'";
                //m_log.TraceInfo("==> T-SQL: " + strSQL);
                vals = SelectDB(strSQL);
                if (vals != null && vals.GetLength(0) > 0) {
                    for (int i = 0; i < envs.Length; i++) {
                        rets[i] = Normalize(vals[0, i], envs[i].Precision, envs[i].Scale);
                        if (rets[i] != vals[0, i]) {
                            m_log.TraceError("original value\"" + vals[0, 0] + "\" larger than specified precision\"" + envs[0].Precision.ToString() + "\" and scale\"" + envs[0].Scale.ToString() + "\"");
                        }
                    }
                    break;
                }
            }
            return rets;
        }

        private string[] GetDeviceInfo(UploadField FieldUL, int iCNLenb) {
            string[] ret = new string[5];
            string strTable = FieldUL.ANALYMANUF.Split('.')[0];
            string ANALYMANUF = FieldUL.ANALYMANUF.Split('.')[1];
            string ANALYNAME = FieldUL.ANALYNAME.Split('.')[1];
            string ANALYMODEL = FieldUL.ANALYMODEL.Split('.')[1];
            string DYNOMODEL = FieldUL.DYNOMODEL.Split('.')[1];
            string DYNOMANUF = FieldUL.DYNOMANUF.Split('.')[1];

            string strSQL = "select [" + ANALYMANUF + "], ";
            strSQL += "[" + ANALYNAME + "], ";
            strSQL += "[" + ANALYMODEL + "], ";
            strSQL += "[" + DYNOMODEL + "], ";
            strSQL += "[" + DYNOMANUF + "] from [" + strTable + "]";

            //m_log.TraceInfo("==> T-SQL: " + strSQL);
            string[,] vals = SelectDB(strSQL);
            if (vals != null && vals.GetLength(0) > 0) {
                for (int i = 0; i < vals.GetLength(1); i++) {
                    ret[i] = vals[0, i];
                }
            }
            if (ret[0].Length > 30 / iCNLenb) {
                ret[0] = ret[0].Substring(0, 30 / iCNLenb);
            }
            if (ret[1].Length > 30 / iCNLenb) {
                ret[1] = ret[0].Substring(0, 30 / iCNLenb);
            }
            if (ret[2].Length > 50) {
                ret[2] = ret[0].Substring(0, 50);
            }
            if (ret[3].Length > 50) {
                ret[3] = ret[0].Substring(0, 50);
            }
            if (ret[4].Length > 50 / iCNLenb) {
                ret[4] = ret[0].Substring(0, 50 / iCNLenb);
            }
            return ret;
        }

        public List<UploadField> GetDynoData(UploadField FieldUL, int iCNLenb) {
            List<UploadField> resultList = new List<UploadField>();
            string strSQL = "select [VIN], [JCLSH] from [已检车辆库] where [Upload] = '0' and [JCJG] = '合格' and [JCLX] = '初检'";
            m_log.TraceInfo("==> T-SQL: " + strSQL);
            string[,] cars = SelectDB(strSQL);
            m_log.TraceInfo(cars.GetLength(0) + " record(s) will upload");
            for (int i = 0; i < cars.GetLength(0); i++) {
                UploadField result = new UploadField {
                    VIN = cars[i, 0],
                    JCLSH = cars[i, 1]
                };
                string strLog = "===== Record " + (i + 1).ToString() + ": [VIN: " + result.VIN + ", JCLSH: " + result.JCLSH + "] =====";
                m_log.TraceInfo(strLog);

                // IF_EM_WQPF_2
                //m_log.TraceInfo("Start Get IF_EM_WQPF_2 Data");
                string[] env = GetEnvValue(result.JCLSH);
                result.RH = env[0];
                result.ET = env[1];
                result.AP = env[2];
                strLog = "IF_EM_WQPF_2 Data: [RH: " + result.RH + ", ET: " + result.ET + ", AP: " + result.AP + "]";
                m_log.TraceInfo(strLog);

                // IF_EM_WQPF_3
                //m_log.TraceInfo("Get IF_EM_WQPF_3 Data");
                result.TESTTYPE = GetFieldValue(FieldUL.TESTTYPE, result.JCLSH);
                result.TESTTYPE = GetTestType(result.TESTTYPE);
                result.TESTDATE = GetFieldValue(FieldUL.TESTDATE, result.JCLSH);
                if (result.TESTDATE.Length == 0) {
                    result.TESTDATE = DateTime.Now.ToLocalTime().ToString("yyyyMMdd");
                } else {
                    result.TESTDATE = result.TESTDATE.Substring(0, 10).Replace("-", "");
                }
                result.EPASS = GetFieldValue(FieldUL.EPASS, result.JCLSH);
                result.EPASS = result.EPASS == "合格" ? "1" : "0";
                strLog = "IF_EM_WQPF_3 Data: [TESTTYPE: " + result.TESTTYPE + ", TESTDATE: " + result.TESTDATE + ", EPASS: " + result.EPASS + "]";
                m_log.TraceInfo(strLog);

                // IF_EM_WQPF_5_1
                //m_log.TraceInfo("Get IF_EM_WQPF_5_1 Data");
                result.REAC = GetFieldValue(FieldUL.REAC, result.JCLSH, true, 3, 2);
                result.LEACMAX = GetFieldValue(FieldUL.LEACMAX, result.JCLSH, true, 3, 2);
                result.LEACMIN = GetFieldValue(FieldUL.LEACMIN, result.JCLSH, true, 3, 2);
                result.LRCO = GetFieldValue(FieldUL.LRCO, result.JCLSH, true, 3, 2);
                result.LLCO = GetFieldValue(FieldUL.LLCO, result.JCLSH, true, 3, 2);
                result.LRHC = GetFieldValue(FieldUL.LRHC, result.JCLSH, true, 4);
                result.LLHC = GetFieldValue(FieldUL.LLHC, result.JCLSH, true, 4);
                result.HRCO = GetFieldValue(FieldUL.HRCO, result.JCLSH, true, 3, 2);
                result.HLCO = GetFieldValue(FieldUL.HLCO, result.JCLSH, true, 3, 2);
                result.HRHC = GetFieldValue(FieldUL.HRHC, result.JCLSH, true, 4);
                result.HLHC = GetFieldValue(FieldUL.HLHC, result.JCLSH, true, 4);
                strLog = "IF_EM_WQPF_5_1 Data: [REAC: " + result.REAC + ", LEACMAX: " + result.LEACMAX + ", LEACMIN: " + result.LEACMIN;
                strLog += ", LRCO: " + result.LRCO;
                strLog += ", LLCO: " + result.LLCO;
                strLog += ", LRHC: " + result.LRHC;
                strLog += ", LLHC: " + result.LLHC;
                strLog += ", HRCO: " + result.HRCO;
                strLog += ", HLCO: " + result.HLCO;
                strLog += ", HRHC: " + result.HRHC;
                strLog += ", HLHC: " + result.HLHC + "]";
                m_log.TraceInfo(strLog);

                // IF_EM_WQPF_5_2
                //m_log.TraceInfo("Get IF_EM_WQPF_5_2 Data");
                result.ARHC5025 = GetFieldValue(FieldUL.ARHC5025, result.JCLSH, true, 4);
                result.ALHC5025 = GetFieldValue(FieldUL.ALHC5025, result.JCLSH, true, 4);
                result.ARCO5025 = GetFieldValue(FieldUL.ARCO5025, result.JCLSH, true, 3, 2);
                result.ALCO5025 = GetFieldValue(FieldUL.ALCO5025, result.JCLSH, true, 3, 2);
                result.ARNOX5025 = GetFieldValue(FieldUL.ARNOX5025, result.JCLSH, true, 4);
                result.ALNOX5025 = GetFieldValue(FieldUL.ALNOX5025, result.JCLSH, true, 4);
                result.ARHC2540 = GetFieldValue(FieldUL.ARHC2540, result.JCLSH, true, 4);
                result.ALHC2540 = GetFieldValue(FieldUL.ALHC2540, result.JCLSH, true, 4);
                result.ARCO2540 = GetFieldValue(FieldUL.ARCO2540, result.JCLSH, true, 3, 2);
                result.ALCO2540 = GetFieldValue(FieldUL.ALCO2540, result.JCLSH, true, 3, 2);
                result.ARNOX2540 = GetFieldValue(FieldUL.ARNOX2540, result.JCLSH, true, 4);
                result.ALNOX2540 = GetFieldValue(FieldUL.ALNOX2540, result.JCLSH, true, 4);
                strLog = "IF_EM_WQPF_5_2 Data: [ARHC5025: " + result.ARHC5025;
                strLog += ", ALHC5025: " + result.ALHC5025;
                strLog += ", ARCO5025: " + result.ARCO5025;
                strLog += ", ALCO5025: " + result.ALCO5025;
                strLog += ", ARNOX5025: " + result.ARNOX5025;
                strLog += ", ALNOX5025: " + result.ALNOX5025;
                strLog += ", ARHC2540: " + result.ARHC2540;
                strLog += ", ALHC2540: " + result.ALHC2540;
                strLog += ", ARCO2540: " + result.ARCO2540;
                strLog += ", ALCO2540: " + result.ALCO2540;
                strLog += ", ARNOX2540: " + result.ARNOX2540;
                strLog += ", ALNOX2540: " + result.ALNOX2540 + "]";
                m_log.TraceInfo(strLog);

                // IF_EM_WQPF_5_3
                //m_log.TraceInfo("Get IF_EM_WQPF_5_3 Data");
                result.VRHC = GetFieldValue(FieldUL.VRHC, result.JCLSH, true, 3, 2);
                result.VLHC = GetFieldValue(FieldUL.VLHC, result.JCLSH, true, 3, 2);
                result.VRCO_53 = GetFieldValue(FieldUL.VRCO_53, result.JCLSH, true, 3, 2);
                result.VLCO_53 = GetFieldValue(FieldUL.VLCO_53, result.JCLSH, true, 3, 2);
                result.VRNOX = GetFieldValue(FieldUL.VRNOX, result.JCLSH, true, 3, 2);
                result.VLNOX = GetFieldValue(FieldUL.VLNOX, result.JCLSH, true, 3, 2);
                strLog = "IF_EM_WQPF_5_3 Data: [VRHC: " + result.VRHC;
                strLog += ", VLHC: " + result.VLHC;
                strLog += ", VRCO_53: " + result.VRCO_53;
                strLog += ", VLCO_53: " + result.VLCO_53;
                strLog += ", VRNOX: " + result.VRNOX;
                strLog += ", VLNOX: " + result.VLNOX + "]";
                m_log.TraceInfo(strLog);

                // IF_EM_WQPF_5_4
                //m_log.TraceInfo("Get IF_EM_WQPF_5_4 Data");
                result.RATEREVUP = GetFieldValue(FieldUL.RATEREVUP, result.JCLSH, true, 5);
                result.RATEREVDOWN = GetFieldValue(FieldUL.RATEREVDOWN, result.JCLSH, true, 5);
                result.REV100 = GetFieldValue(FieldUL.REV100, result.JCLSH, true, 5);
                result.MAXPOWER = GetFieldValue(FieldUL.MAXPOWER, result.JCLSH, true, 4, 1);
                result.MAXPOWERLIMIT = GetFieldValue(FieldUL.MAXPOWERLIMIT, result.JCLSH, true, 4, 1);
                result.SMOKE100 = GetFieldValue(FieldUL.SMOKE100, result.JCLSH, true, 3, 2);
                result.SMOKE80 = GetFieldValue(FieldUL.SMOKE80, result.JCLSH, true, 3, 2);
                result.SMOKELIMIT = GetFieldValue(FieldUL.SMOKELIMIT, result.JCLSH, true, 3, 2);
                result.NOX = GetFieldValue(FieldUL.NOX, result.JCLSH, true, 4);
                result.NOXLIMIT = GetFieldValue(FieldUL.NOXLIMIT, result.JCLSH, true, 4);
                strLog = "IF_EM_WQPF_5_4 Data: [RATEREVUP: " + result.RATEREVUP;
                strLog += ", RATEREVDOWN: " + result.RATEREVDOWN;
                strLog += ", REV100: " + result.REV100;
                strLog += ", MAXPOWER: " + result.MAXPOWER;
                strLog += ", MAXPOWERLIMIT: " + result.MAXPOWERLIMIT;
                strLog += ", SMOKE100: " + result.SMOKE100;
                strLog += ", SMOKE80: " + result.SMOKE80;
                strLog += ", SMOKELIMIT: " + result.SMOKELIMIT;
                strLog += ", NOX: " + result.NOX;
                strLog += ", NOXLIMIT: " + result.NOXLIMIT + "]";
                m_log.TraceInfo(strLog);

                // IF_EM_WQPF_6
                //m_log.TraceInfo("Get IF_EM_WQPF_6 Data");
                string[] deviceInfo = GetDeviceInfo(FieldUL, iCNLenb);
                result.ANALYMANUF = deviceInfo[0];
                result.ANALYNAME = deviceInfo[1];
                result.ANALYMODEL = deviceInfo[2];
                result.DYNOMODEL = deviceInfo[3];
                result.DYNOMANUF = deviceInfo[4];
                strLog = "IF_EM_WQPF_6 Data: [ANALYMANUF: " + result.ANALYMANUF;
                strLog += ", ANALYNAME: " + result.ANALYNAME;
                strLog += ", ANALYMODEL: " + result.ANALYMODEL;
                strLog += ", DYNOMODEL: " + result.DYNOMODEL;
                strLog += ", DYNOMANUF: " + result.DYNOMANUF + "]";
                m_log.TraceInfo(strLog);

                resultList.Add(result);
            }
            return resultList;
        }

        public int SetUpload(int value, string JCLSH) {
            string strSQL = "update [已检车辆库] set [Upload] = '" + value.ToString() + "' where [JCLSH] = '" + JCLSH + "'";
            return RunSQL(strSQL);
        }
    }

    public class EnvStructure {
        public string Column { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }

        public EnvStructure(string column, int precision, int scale) {
            this.Column = column;
            this.Precision = precision;
            this.Scale = scale;
        }
    }
}