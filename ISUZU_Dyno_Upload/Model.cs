using System;
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

        private void GetUploadData(UploadField FieldUL, int iCNLenb, UploadField result_out) {
            // IF_EM_WQPF_2
            string[] env = GetEnvValue(result_out.JCLSH);
            result_out.RH = env[0];
            result_out.ET = env[1];
            result_out.AP = env[2];
            string strLog = "IF_EM_WQPF_2 Data: [RH: " + result_out.RH + ", ET: " + result_out.ET + ", AP: " + result_out.AP + "]";
            m_log.TraceInfo(strLog);

            // IF_EM_WQPF_3
            result_out.TESTTYPE = GetFieldValue(FieldUL.TESTTYPE, result_out.JCLSH);
            result_out.TESTTYPE = GetTestType(result_out.TESTTYPE);
            result_out.TESTDATE = GetFieldValue(FieldUL.TESTDATE, result_out.JCLSH);
            if (result_out.TESTDATE.Length == 0) {
                result_out.TESTDATE = DateTime.Now.ToLocalTime().ToString("yyyyMMdd");
            } else {
                result_out.TESTDATE = result_out.TESTDATE.Substring(0, 10).Replace("-", "");
            }
            result_out.EPASS = GetFieldValue(FieldUL.EPASS, result_out.JCLSH);
            result_out.EPASS = result_out.EPASS == "合格" ? "1" : "0";
            strLog = "IF_EM_WQPF_3 Data: [TESTTYPE: " + result_out.TESTTYPE + ", TESTDATE: " + result_out.TESTDATE + ", EPASS: " + result_out.EPASS + "]";
            m_log.TraceInfo(strLog);

            // IF_EM_WQPF_5_1
            result_out.REAC = GetFieldValue(FieldUL.REAC, result_out.JCLSH, true, 3, 2);
            result_out.LEACMAX = GetFieldValue(FieldUL.LEACMAX, result_out.JCLSH, true, 3, 2);
            result_out.LEACMIN = GetFieldValue(FieldUL.LEACMIN, result_out.JCLSH, true, 3, 2);
            result_out.LRCO = GetFieldValue(FieldUL.LRCO, result_out.JCLSH, true, 3, 2);
            result_out.LLCO = GetFieldValue(FieldUL.LLCO, result_out.JCLSH, true, 3, 2);
            result_out.LRHC = GetFieldValue(FieldUL.LRHC, result_out.JCLSH, true, 4);
            result_out.LLHC = GetFieldValue(FieldUL.LLHC, result_out.JCLSH, true, 4);
            result_out.HRCO = GetFieldValue(FieldUL.HRCO, result_out.JCLSH, true, 3, 2);
            result_out.HLCO = GetFieldValue(FieldUL.HLCO, result_out.JCLSH, true, 3, 2);
            result_out.HRHC = GetFieldValue(FieldUL.HRHC, result_out.JCLSH, true, 4);
            result_out.HLHC = GetFieldValue(FieldUL.HLHC, result_out.JCLSH, true, 4);
            strLog = "IF_EM_WQPF_5_1 Data: [REAC: " + result_out.REAC + ", LEACMAX: " + result_out.LEACMAX + ", LEACMIN: " + result_out.LEACMIN;
            strLog += ", LRCO: " + result_out.LRCO;
            strLog += ", LLCO: " + result_out.LLCO;
            strLog += ", LRHC: " + result_out.LRHC;
            strLog += ", LLHC: " + result_out.LLHC;
            strLog += ", HRCO: " + result_out.HRCO;
            strLog += ", HLCO: " + result_out.HLCO;
            strLog += ", HRHC: " + result_out.HRHC;
            strLog += ", HLHC: " + result_out.HLHC + "]";
            m_log.TraceInfo(strLog);

            // IF_EM_WQPF_5_2
            result_out.ARHC5025 = GetFieldValue(FieldUL.ARHC5025, result_out.JCLSH, true, 4);
            result_out.ALHC5025 = GetFieldValue(FieldUL.ALHC5025, result_out.JCLSH, true, 4);
            result_out.ARCO5025 = GetFieldValue(FieldUL.ARCO5025, result_out.JCLSH, true, 3, 2);
            result_out.ALCO5025 = GetFieldValue(FieldUL.ALCO5025, result_out.JCLSH, true, 3, 2);
            result_out.ARNOX5025 = GetFieldValue(FieldUL.ARNOX5025, result_out.JCLSH, true, 4);
            result_out.ALNOX5025 = GetFieldValue(FieldUL.ALNOX5025, result_out.JCLSH, true, 4);
            result_out.ARHC2540 = GetFieldValue(FieldUL.ARHC2540, result_out.JCLSH, true, 4);
            result_out.ALHC2540 = GetFieldValue(FieldUL.ALHC2540, result_out.JCLSH, true, 4);
            result_out.ARCO2540 = GetFieldValue(FieldUL.ARCO2540, result_out.JCLSH, true, 3, 2);
            result_out.ALCO2540 = GetFieldValue(FieldUL.ALCO2540, result_out.JCLSH, true, 3, 2);
            result_out.ARNOX2540 = GetFieldValue(FieldUL.ARNOX2540, result_out.JCLSH, true, 4);
            result_out.ALNOX2540 = GetFieldValue(FieldUL.ALNOX2540, result_out.JCLSH, true, 4);
            strLog = "IF_EM_WQPF_5_2 Data: [ARHC5025: " + result_out.ARHC5025;
            strLog += ", ALHC5025: " + result_out.ALHC5025;
            strLog += ", ARCO5025: " + result_out.ARCO5025;
            strLog += ", ALCO5025: " + result_out.ALCO5025;
            strLog += ", ARNOX5025: " + result_out.ARNOX5025;
            strLog += ", ALNOX5025: " + result_out.ALNOX5025;
            strLog += ", ARHC2540: " + result_out.ARHC2540;
            strLog += ", ALHC2540: " + result_out.ALHC2540;
            strLog += ", ARCO2540: " + result_out.ARCO2540;
            strLog += ", ALCO2540: " + result_out.ALCO2540;
            strLog += ", ARNOX2540: " + result_out.ARNOX2540;
            strLog += ", ALNOX2540: " + result_out.ALNOX2540 + "]";
            m_log.TraceInfo(strLog);

            // IF_EM_WQPF_5_3
            result_out.VRHC = GetFieldValue(FieldUL.VRHC, result_out.JCLSH, true, 3, 2);
            result_out.VLHC = GetFieldValue(FieldUL.VLHC, result_out.JCLSH, true, 3, 2);
            result_out.VRCO_53 = GetFieldValue(FieldUL.VRCO_53, result_out.JCLSH, true, 3, 2);
            result_out.VLCO_53 = GetFieldValue(FieldUL.VLCO_53, result_out.JCLSH, true, 3, 2);
            result_out.VRNOX = GetFieldValue(FieldUL.VRNOX, result_out.JCLSH, true, 3, 2);
            result_out.VLNOX = GetFieldValue(FieldUL.VLNOX, result_out.JCLSH, true, 3, 2);
            strLog = "IF_EM_WQPF_5_3 Data: [VRHC: " + result_out.VRHC;
            strLog += ", VLHC: " + result_out.VLHC;
            strLog += ", VRCO_53: " + result_out.VRCO_53;
            strLog += ", VLCO_53: " + result_out.VLCO_53;
            strLog += ", VRNOX: " + result_out.VRNOX;
            strLog += ", VLNOX: " + result_out.VLNOX + "]";
            m_log.TraceInfo(strLog);

            // IF_EM_WQPF_5_4
            result_out.RATEREVUP = GetFieldValue(FieldUL.RATEREVUP, result_out.JCLSH, true, 5);
            result_out.RATEREVDOWN = GetFieldValue(FieldUL.RATEREVDOWN, result_out.JCLSH, true, 5);
            result_out.REV100 = GetFieldValue(FieldUL.REV100, result_out.JCLSH, true, 5);
            result_out.MAXPOWER = GetFieldValue(FieldUL.MAXPOWER, result_out.JCLSH, true, 4, 1);
            result_out.MAXPOWERLIMIT = GetFieldValue(FieldUL.MAXPOWERLIMIT, result_out.JCLSH, true, 4, 1);
            result_out.SMOKE100 = GetFieldValue(FieldUL.SMOKE100, result_out.JCLSH, true, 3, 2);
            result_out.SMOKE80 = GetFieldValue(FieldUL.SMOKE80, result_out.JCLSH, true, 3, 2);
            result_out.SMOKELIMIT = GetFieldValue(FieldUL.SMOKELIMIT, result_out.JCLSH, true, 3, 2);
            result_out.NOX = GetFieldValue(FieldUL.NOX, result_out.JCLSH, true, 4);
            result_out.NOXLIMIT = GetFieldValue(FieldUL.NOXLIMIT, result_out.JCLSH, true, 4);
            strLog = "IF_EM_WQPF_5_4 Data: [RATEREVUP: " + result_out.RATEREVUP;
            strLog += ", RATEREVDOWN: " + result_out.RATEREVDOWN;
            strLog += ", REV100: " + result_out.REV100;
            strLog += ", MAXPOWER: " + result_out.MAXPOWER;
            strLog += ", MAXPOWERLIMIT: " + result_out.MAXPOWERLIMIT;
            strLog += ", SMOKE100: " + result_out.SMOKE100;
            strLog += ", SMOKE80: " + result_out.SMOKE80;
            strLog += ", SMOKELIMIT: " + result_out.SMOKELIMIT;
            strLog += ", NOX: " + result_out.NOX;
            strLog += ", NOXLIMIT: " + result_out.NOXLIMIT + "]";
            m_log.TraceInfo(strLog);

            // IF_EM_WQPF_6
            string[] deviceInfo = GetDeviceInfo(FieldUL, iCNLenb);
            result_out.ANALYMANUF = deviceInfo[0];
            result_out.ANALYNAME = deviceInfo[1];
            result_out.ANALYMODEL = deviceInfo[2];
            result_out.DYNOMODEL = deviceInfo[3];
            result_out.DYNOMANUF = deviceInfo[4];
            strLog = "IF_EM_WQPF_6 Data: [ANALYMANUF: " + result_out.ANALYMANUF;
            strLog += ", ANALYNAME: " + result_out.ANALYNAME;
            strLog += ", ANALYMODEL: " + result_out.ANALYMODEL;
            strLog += ", DYNOMODEL: " + result_out.DYNOMODEL;
            strLog += ", DYNOMANUF: " + result_out.DYNOMANUF + "]";
            m_log.TraceInfo(strLog);
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
                GetUploadData(FieldUL, iCNLenb, result);
                resultList.Add(result);
            }
            return resultList;
        }

        public int SetUpload(int value, string JCLSH) {
            string strSQL = "update [已检车辆库] set [Upload] = '" + value.ToString() + "' where [JCLSH] = '" + JCLSH + "'";
            return RunSQL(strSQL);
        }

        public UploadField GetDynoDataByVIN(UploadField FieldUL, int iCNLenb, string strVIN) {
            UploadField result = null;
            string strSQL = "select [VIN], [JCLSH] from [已检车辆库] where [VIN] = '" + strVIN + "'";
            m_log.TraceInfo("==> T-SQL: " + strSQL);
            string[,] cars = SelectDB(strSQL);
            if (cars != null && cars.GetLength(0) > 0) {
                result = new UploadField {
                    VIN = cars[0, 0],
                    JCLSH = cars[0, 1]
                };
                string strLog = "===== GetDynoDataByVIN: [VIN: " + result.VIN + ", JCLSH: " + result.JCLSH + "] =====";
                m_log.TraceInfo(strLog);
                GetUploadData(FieldUL, iCNLenb, result);
            }
            return result;
        }

        public Dictionary<string, string> GetJCLSH(string strVIN) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string[,] result = SelectDB(string.Format("select [JCLSH] from [已检车辆库] where [VIN] = '{0}'", strVIN));
            ret.Add("VIN号", strVIN);
            if (result.GetLength(0) > 0) {
                ret.Add("检测流水号", result[0, 0]);
            }
            return ret;
        }

        public Dictionary<string, string> GetEnv(string JCLSH) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string[] env = GetEnvValue(JCLSH);
            ret.Add("相对湿度(%)", env[0]);
            ret.Add("环境温度(°C)", env[1]);
            ret.Add("大气压力(kPa)", env[2]);
            return ret;
        }

        public Dictionary<string, string> GetResult(UploadField FieldUL, string JCLSH) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string TESTTYPE = GetFieldValue(FieldUL.TESTTYPE, JCLSH);
            TESTTYPE = GetTestType(TESTTYPE);
            string TESTDATE = GetFieldValue(FieldUL.TESTDATE, JCLSH);
            if (TESTDATE.Length == 0) {
                TESTDATE = DateTime.Now.ToLocalTime().ToString("yyyyMMdd");
            } else {
                TESTDATE = TESTDATE.Substring(0, 10).Replace("-", "");
            }
            string EPASS = GetFieldValue(FieldUL.EPASS, JCLSH);
            EPASS = (EPASS == "合格") ? "1" : "0";
            ret.Add("检测方法", TESTTYPE);
            ret.Add("检测日期", TESTDATE);
            ret.Add("检测结果", EPASS);
            return ret;
        }

        public Dictionary<string, string> GetDevice(UploadField FieldUL, int iCNLenb) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string[] deviceInfo = GetDeviceInfo(FieldUL, iCNLenb);
            ret.Add("分析仪制造厂", deviceInfo[0]);
            ret.Add("分析仪名称", deviceInfo[1]);
            ret.Add("分析仪型号", deviceInfo[2]);
            ret.Add("测功机型号", deviceInfo[3]);
            ret.Add("测功机生产厂", deviceInfo[4]);
            return ret;
        }

        public Dictionary<string, string> Get51(UploadField FieldUL, string JCLSH) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "过量空气系数结果", GetFieldValue(FieldUL.REAC, JCLSH, true, 3, 2) },
                { "双怠速过量空气系数上限", GetFieldValue(FieldUL.LEACMAX, JCLSH, true, 3, 2) },
                { "双怠速过量空气系数下限", GetFieldValue(FieldUL.LEACMIN, JCLSH, true, 3, 2) },
                { "低怠速CO结果(%)", GetFieldValue(FieldUL.LRCO, JCLSH, true, 3, 2) },
                { "低怠速CO限值(%)", GetFieldValue(FieldUL.LLCO, JCLSH, true, 3, 2) },
                { "低怠速HC结果(10^-6)", GetFieldValue(FieldUL.LRHC, JCLSH, true, 4) },
                { "低怠速HC限值(10^-6)", GetFieldValue(FieldUL.LLHC, JCLSH, true, 4) },
                { "高怠速CO结果(%)", GetFieldValue(FieldUL.HRCO, JCLSH, true, 3, 2) },
                { "高怠速CO限值(%)", GetFieldValue(FieldUL.HLCO, JCLSH, true, 3, 2) },
                { "高怠速HC结果(10^-6)", GetFieldValue(FieldUL.HRHC, JCLSH, true, 4) },
                { "高怠速HC限值(10^-6)", GetFieldValue(FieldUL.HLHC, JCLSH, true, 4) }
            };
            return ret;
        }

        public Dictionary<string, string> Get52(UploadField FieldUL, string JCLSH) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "5025HC结果(10^-6)", GetFieldValue(FieldUL.ARHC5025, JCLSH, true, 4) },
                { "5025HC限值(10^-6)", GetFieldValue(FieldUL.ALHC5025, JCLSH, true, 4) },
                { "5025CO结果(%)", GetFieldValue(FieldUL.ARCO5025, JCLSH, true, 3, 2) },
                { "5025CO限值(%)", GetFieldValue(FieldUL.ALCO5025, JCLSH, true, 3, 2) },
                { "5025NO结果(10^-6)", GetFieldValue(FieldUL.ARNOX5025, JCLSH, true, 4) },
                { "5025NO限值(10^-6)", GetFieldValue(FieldUL.ALNOX5025, JCLSH, true, 4) },
                { "2540HC结果(10^-6)", GetFieldValue(FieldUL.ARHC2540, JCLSH, true, 4) },
                { "2540HC限值(10^-6)", GetFieldValue(FieldUL.ALHC2540, JCLSH, true, 4) },
                { "2540CO结果(%)", GetFieldValue(FieldUL.ARCO2540, JCLSH, true, 3, 2) },
                { "2540CO限值(%)", GetFieldValue(FieldUL.ALCO2540, JCLSH, true, 3, 2) },
                { "2540NO结果(10^-6)", GetFieldValue(FieldUL.ARNOX2540, JCLSH, true, 4) },
                { "2540NO限值(10^-6)", GetFieldValue(FieldUL.ALNOX2540, JCLSH, true, 4) }
            };
            return ret;
        }

        public Dictionary<string, string> Get53(UploadField FieldUL, string JCLSH) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "HC结果(g/km)", GetFieldValue(FieldUL.VRHC, JCLSH, true, 3, 2) },
                { "HC限值(g/km)", GetFieldValue(FieldUL.VLHC, JCLSH, true, 3, 2) },
                { "CO结果(g/km)", GetFieldValue(FieldUL.VRCO_53, JCLSH, true, 3, 2) },
                { "CO限值(g/km)", GetFieldValue(FieldUL.VLCO_53, JCLSH, true, 3, 2) },
                { "NOx结果(g/km)", GetFieldValue(FieldUL.VRNOX, JCLSH, true, 3, 2) },
                { "NOx限值(g/km)", GetFieldValue(FieldUL.VLNOX, JCLSH, true, 3, 2) }
            };
            return ret;
        }

        public Dictionary<string, string> Get54(UploadField FieldUL, string JCLSH) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "额定转速上限(r/min)", GetFieldValue(FieldUL.RATEREVUP, JCLSH, true, 5) },
                { "额定转速下限(r/min)", GetFieldValue(FieldUL.RATEREVDOWN, JCLSH, true, 5) },
                { "实测转速(r/min)", GetFieldValue(FieldUL.REV100, JCLSH, true, 5) },
                { "实测最大轮边功率(kw)", GetFieldValue(FieldUL.MAXPOWER, JCLSH, true, 4, 1) },
                { "最大轮边功率限值(kw)", GetFieldValue(FieldUL.MAXPOWERLIMIT, JCLSH, true, 4, 1) },
                { "100%点烟度(1/m)", GetFieldValue(FieldUL.SMOKE100, JCLSH, true, 3, 2) },
                { "80%点烟度(1/m)", GetFieldValue(FieldUL.SMOKE80, JCLSH, true, 3, 2) },
                { "烟度限值(1/m)", GetFieldValue(FieldUL.SMOKELIMIT, JCLSH, true, 3, 2) },
                { "氮氧化物测量值(10^-6)", GetFieldValue(FieldUL.NOX, JCLSH, true, 4) },
                { "氮氧化物限值(10^-6)", GetFieldValue(FieldUL.NOXLIMIT, JCLSH, true, 4) }
            };
            return ret;
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
