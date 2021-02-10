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

        public void TestConnect() {
            using (SqlConnection sqlConn = new SqlConnection(StrConn)) {
                sqlConn.Open();
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
                m_log.TraceError("==> SQL: " + strSQL);
                m_log.TraceError("==> ERROR: " + ex.Message);
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
                m_log.TraceError("==> SQL: " + strSQL);
                m_log.TraceError("==> ERROR: " + ex.Message);
            }
            return records;
        }

        public int GetRecordCount(string strTable, Dictionary<string, string> whereDic) {
            string strSQL = "select * from " + strTable + " where ";
            foreach (string key in whereDic.Keys) {
                strSQL += key + " = '" + whereDic[key] + "' and ";
            }
            strSQL = strSQL.Substring(0, strSQL.Length - 5);
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

        private string GetFieldValue(string field, F_KEY_S fKey, bool bNumber = false, int precision = 0, int scale = 0) {
            string[,] vals = null;
            string strRet = string.Empty;
            if (field.Length > 0 && field != "--") {
                string strSQL = string.Format("select [{0}] from [{1}] where [{2}] = '{3}'", field.Split('.')[1], field.Split('.')[0], fKey.Name, fKey.Value);
                vals = SelectDB(strSQL);
            }
            if (vals != null && vals.GetLength(0) > 0) {
                if (bNumber) {
                    strRet = Normalize(vals[0, 0], precision, scale);
                    if (strRet != vals[0, 0]) {
                        m_log.TraceError(string.Format("original value\"{0}\" larger than specified precision\"{1}\" and scale\"{2}\"", vals[0, 0], precision, scale));
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
        /// <param name="fKey"></param>
        /// <returns></returns>
        private string[] GetEnvValue(F_KEY_S fKey) {
            string[] rets = { "", "", "" };
            Dictionary<string, EnvStructure[]> tables = new Dictionary<string, EnvStructure[]>();

            EnvStructure[] envs1 = new EnvStructure[3];
            envs1[0] = new EnvStructure("Humidity", 4, 2);
            envs1[1] = new EnvStructure("Temperature", 5, 2);
            envs1[2] = new EnvStructure("AirPressure", 5, 2);
            tables.Add("ASMResult", envs1);
            tables.Add("LDResult", envs1);

            EnvStructure[] envs2 = new EnvStructure[3];
            envs2[0] = new EnvStructure("Humidity", 4, 2);
            envs2[1] = new EnvStructure("Temperature", 5, 2);
            envs2[2] = new EnvStructure("Baro", 5, 2);
            tables.Add("FalResult", envs2);
            tables.Add("TsiResult", envs2);
            tables.Add("VmasResult", envs2);

            foreach (var item in tables) {
                string tableName = item.Key;
                EnvStructure[] envs = item.Value;
                string strSQL = string.Format("select [{0}], [{1}], [{2}] from [{3}] where [{4}] = '{5}'", envs[0].Column, envs[1].Column, envs[2].Column, tableName, fKey.Name, fKey.Value);
                string[,] vals = SelectDB(strSQL);
                if (vals != null && vals.GetLength(0) > 0) {
                    for (int i = 0; i < envs.Length; i++) {
                        rets[i] = Normalize(vals[0, i], envs[i].Precision, envs[i].Scale);
                        if (rets[i] != vals[0, i]) {
                            m_log.TraceError(string.Format("original value\"{0}\" larger than specified precision\"{1}\" and scale\"{2}\"", vals[0, i], envs[0].Precision, envs[0].Scale));
                        }
                    }
                    break;
                }
            }
            return rets;
        }

        private string[] GetDeviceInfo(UploadField FieldUL, int iCNLenb) {
            string[] ret = new string[5];
            if (FieldUL.ANALYMANUF.Length > 0 && FieldUL.ANALYMANUF != "--") {
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
            }
            return ret;
        }

        private void GetUploadData(UploadField FieldUL, int iCNLenb, UploadField resultOut) {
            // IF_EM_WQPF_2
            string[] env = GetEnvValue(resultOut.F_KEY);
            resultOut.RH = env[0];
            resultOut.ET = env[1];
            resultOut.AP = env[2];
            string strLog = "IF_EM_WQPF_2 Data: [RH: " + resultOut.RH + ", ET: " + resultOut.ET + ", AP: " + resultOut.AP + "]";
            m_log.TraceInfo(strLog);

            // IF_EM_WQPF_3
            resultOut.TESTTYPE = GetFieldValue(FieldUL.TESTTYPE, resultOut.F_KEY);
            resultOut.TESTTYPE = GetTestType(resultOut.TESTTYPE);
            resultOut.TESTDATE = GetFieldValue(FieldUL.TESTDATE, resultOut.F_KEY);
            if (resultOut.TESTDATE.Length == 0) {
                resultOut.TESTDATE = DateTime.Now.ToLocalTime().ToString("yyyyMMdd");
            } else {
                resultOut.TESTDATE = resultOut.TESTDATE.Substring(0, 10).Replace("-", "");
            }
            resultOut.EPASS = GetFieldValue(FieldUL.EPASS, resultOut.F_KEY);
            resultOut.EPASS = resultOut.EPASS == "合格" ? "1" : "0";
            strLog = "IF_EM_WQPF_3 Data: [TESTTYPE: " + resultOut.TESTTYPE + ", TESTDATE: " + resultOut.TESTDATE + ", EPASS: " + resultOut.EPASS + "]";
            m_log.TraceInfo(strLog);

            switch (resultOut.TESTTYPE) {
            case "1":
                // IF_EM_WQPF_5_1
                resultOut.REAC = GetFieldValue(FieldUL.REAC, resultOut.F_KEY, true, 3, 2);
                resultOut.LEACMAX = GetFieldValue(FieldUL.LEACMAX, resultOut.F_KEY, true, 3, 2);
                resultOut.LEACMIN = GetFieldValue(FieldUL.LEACMIN, resultOut.F_KEY, true, 3, 2);
                resultOut.LRCO = GetFieldValue(FieldUL.LRCO, resultOut.F_KEY, true, 3, 2);
                resultOut.LLCO = GetFieldValue(FieldUL.LLCO, resultOut.F_KEY, true, 3, 2);
                resultOut.LRHC = GetFieldValue(FieldUL.LRHC, resultOut.F_KEY, true, 4);
                resultOut.LLHC = GetFieldValue(FieldUL.LLHC, resultOut.F_KEY, true, 4);
                resultOut.HRCO = GetFieldValue(FieldUL.HRCO, resultOut.F_KEY, true, 3, 2);
                resultOut.HLCO = GetFieldValue(FieldUL.HLCO, resultOut.F_KEY, true, 3, 2);
                resultOut.HRHC = GetFieldValue(FieldUL.HRHC, resultOut.F_KEY, true, 4);
                resultOut.HLHC = GetFieldValue(FieldUL.HLHC, resultOut.F_KEY, true, 4);
                strLog = "IF_EM_WQPF_5_1 Data: [REAC: " + resultOut.REAC + ", LEACMAX: " + resultOut.LEACMAX + ", LEACMIN: " + resultOut.LEACMIN;
                strLog += ", LRCO: " + resultOut.LRCO;
                strLog += ", LLCO: " + resultOut.LLCO;
                strLog += ", LRHC: " + resultOut.LRHC;
                strLog += ", LLHC: " + resultOut.LLHC;
                strLog += ", HRCO: " + resultOut.HRCO;
                strLog += ", HLCO: " + resultOut.HLCO;
                strLog += ", HRHC: " + resultOut.HRHC;
                strLog += ", HLHC: " + resultOut.HLHC + "]";
                m_log.TraceInfo(strLog);
                break;
            case "2":
                // IF_EM_WQPF_5_2
                resultOut.ARHC5025 = GetFieldValue(FieldUL.ARHC5025, resultOut.F_KEY, true, 4);
                resultOut.ALHC5025 = GetFieldValue(FieldUL.ALHC5025, resultOut.F_KEY, true, 4);
                resultOut.ARCO5025 = GetFieldValue(FieldUL.ARCO5025, resultOut.F_KEY, true, 3, 2);
                resultOut.ALCO5025 = GetFieldValue(FieldUL.ALCO5025, resultOut.F_KEY, true, 3, 2);
                resultOut.ARNOX5025 = GetFieldValue(FieldUL.ARNOX5025, resultOut.F_KEY, true, 4);
                resultOut.ALNOX5025 = GetFieldValue(FieldUL.ALNOX5025, resultOut.F_KEY, true, 4);
                resultOut.ARHC2540 = GetFieldValue(FieldUL.ARHC2540, resultOut.F_KEY, true, 4);
                resultOut.ALHC2540 = GetFieldValue(FieldUL.ALHC2540, resultOut.F_KEY, true, 4);
                resultOut.ARCO2540 = GetFieldValue(FieldUL.ARCO2540, resultOut.F_KEY, true, 3, 2);
                resultOut.ALCO2540 = GetFieldValue(FieldUL.ALCO2540, resultOut.F_KEY, true, 3, 2);
                resultOut.ARNOX2540 = GetFieldValue(FieldUL.ARNOX2540, resultOut.F_KEY, true, 4);
                resultOut.ALNOX2540 = GetFieldValue(FieldUL.ALNOX2540, resultOut.F_KEY, true, 4);
                strLog = "IF_EM_WQPF_5_2 Data: [ARHC5025: " + resultOut.ARHC5025;
                strLog += ", ALHC5025: " + resultOut.ALHC5025;
                strLog += ", ARCO5025: " + resultOut.ARCO5025;
                strLog += ", ALCO5025: " + resultOut.ALCO5025;
                strLog += ", ARNOX5025: " + resultOut.ARNOX5025;
                strLog += ", ALNOX5025: " + resultOut.ALNOX5025;
                strLog += ", ARHC2540: " + resultOut.ARHC2540;
                strLog += ", ALHC2540: " + resultOut.ALHC2540;
                strLog += ", ARCO2540: " + resultOut.ARCO2540;
                strLog += ", ALCO2540: " + resultOut.ALCO2540;
                strLog += ", ARNOX2540: " + resultOut.ARNOX2540;
                strLog += ", ALNOX2540: " + resultOut.ALNOX2540 + "]";
                m_log.TraceInfo(strLog);
                break;
            case "3":
                // IF_EM_WQPF_5_3
                resultOut.VRHC = GetFieldValue(FieldUL.VRHC, resultOut.F_KEY, true, 3, 2);
                resultOut.VLHC = GetFieldValue(FieldUL.VLHC, resultOut.F_KEY, true, 3, 2);
                resultOut.VRCO_53 = GetFieldValue(FieldUL.VRCO_53, resultOut.F_KEY, true, 3, 2);
                resultOut.VLCO_53 = GetFieldValue(FieldUL.VLCO_53, resultOut.F_KEY, true, 3, 2);
                resultOut.VRNOX = GetFieldValue(FieldUL.VRNOX, resultOut.F_KEY, true, 3, 2);
                resultOut.VLNOX = GetFieldValue(FieldUL.VLNOX, resultOut.F_KEY, true, 3, 2);
                strLog = "IF_EM_WQPF_5_3 Data: [VRHC: " + resultOut.VRHC;
                strLog += ", VLHC: " + resultOut.VLHC;
                strLog += ", VRCO_53: " + resultOut.VRCO_53;
                strLog += ", VLCO_53: " + resultOut.VLCO_53;
                strLog += ", VRNOX: " + resultOut.VRNOX;
                strLog += ", VLNOX: " + resultOut.VLNOX + "]";
                m_log.TraceInfo(strLog);
                break;
            case "4":
                // IF_EM_WQPF_5_4
                resultOut.RATEREVUP = GetFieldValue(FieldUL.RATEREVUP, resultOut.F_KEY, true, 5);
                resultOut.RATEREVDOWN = GetFieldValue(FieldUL.RATEREVDOWN, resultOut.F_KEY, true, 5);
                resultOut.REV100 = GetFieldValue(FieldUL.REV100, resultOut.F_KEY, true, 5);
                resultOut.MAXPOWER = GetFieldValue(FieldUL.MAXPOWER, resultOut.F_KEY, true, 4, 1);
                resultOut.MAXPOWERLIMIT = GetFieldValue(FieldUL.MAXPOWERLIMIT, resultOut.F_KEY, true, 4, 1);
                resultOut.SMOKE100 = GetFieldValue(FieldUL.SMOKE100, resultOut.F_KEY, true, 3, 2);
                resultOut.SMOKE80 = GetFieldValue(FieldUL.SMOKE80, resultOut.F_KEY, true, 3, 2);
                resultOut.SMOKELIMIT = GetFieldValue(FieldUL.SMOKELIMIT, resultOut.F_KEY, true, 3, 2);
                resultOut.NOX = GetFieldValue(FieldUL.NOX, resultOut.F_KEY, true, 4);
                resultOut.NOXLIMIT = GetFieldValue(FieldUL.NOXLIMIT, resultOut.F_KEY, true, 4);
                strLog = "IF_EM_WQPF_5_4 Data: [RATEREVUP: " + resultOut.RATEREVUP;
                strLog += ", RATEREVDOWN: " + resultOut.RATEREVDOWN;
                strLog += ", REV100: " + resultOut.REV100;
                strLog += ", MAXPOWER: " + resultOut.MAXPOWER;
                strLog += ", MAXPOWERLIMIT: " + resultOut.MAXPOWERLIMIT;
                strLog += ", SMOKE100: " + resultOut.SMOKE100;
                strLog += ", SMOKE80: " + resultOut.SMOKE80;
                strLog += ", SMOKELIMIT: " + resultOut.SMOKELIMIT;
                strLog += ", NOX: " + resultOut.NOX;
                strLog += ", NOXLIMIT: " + resultOut.NOXLIMIT + "]";
                m_log.TraceInfo(strLog);
                break;
            case "6":
                // IF_EM_WQPF_5_5
                resultOut.RATEREV = GetFieldValue(FieldUL.RATEREV, resultOut.F_KEY, true, 5);
                resultOut.REV = GetFieldValue(FieldUL.REV, resultOut.F_KEY, true, 5);
                resultOut.SMOKEK1 = GetFieldValue(FieldUL.SMOKEK1, resultOut.F_KEY, true, 3, 2);
                resultOut.SMOKEK2 = GetFieldValue(FieldUL.SMOKEK2, resultOut.F_KEY, true, 3, 2);
                resultOut.SMOKEK3 = GetFieldValue(FieldUL.SMOKEK3, resultOut.F_KEY, true, 3, 2);
                resultOut.SMOKEAVG = GetFieldValue(FieldUL.SMOKEAVG, resultOut.F_KEY, true, 3, 2);
                resultOut.SMOKEKLIMIT = GetFieldValue(FieldUL.SMOKEKLIMIT, resultOut.F_KEY, true, 3, 2);
                strLog = "IF_EM_WQPF_5_5 Data: [RATEREV: " + resultOut.RATEREV;
                strLog += ", REV: " + resultOut.REV;
                strLog += ", SMOKEK1: " + resultOut.SMOKEK1;
                strLog += ", SMOKEK2: " + resultOut.SMOKEK2;
                strLog += ", SMOKEK3: " + resultOut.SMOKEK3;
                strLog += ", SMOKEAVG: " + resultOut.SMOKEAVG;
                strLog += ", SMOKEKLIMIT: " + resultOut.SMOKEKLIMIT + "]";
                m_log.TraceInfo(strLog);
                break;
            case "8":
                // IF_EM_WQPF_5_6
                resultOut.VRCO_56 = GetFieldValue(FieldUL.VRCO_56, resultOut.F_KEY, true, 3, 2);
                resultOut.VLCO_56 = GetFieldValue(FieldUL.VLCO_56, resultOut.F_KEY, true, 3, 2);
                resultOut.VRHCNOX = GetFieldValue(FieldUL.VRHCNOX, resultOut.F_KEY, true, 3, 2);
                resultOut.VLHCNOX = GetFieldValue(FieldUL.VLHCNOX, resultOut.F_KEY, true, 3, 2);
                strLog = "IF_EM_WQPF_5_6 Data: [VRCO: " + resultOut.VRCO_56;
                strLog += ", VLCO: " + resultOut.VLCO_56;
                strLog += ", VRHCNOX: " + resultOut.VRHCNOX;
                strLog += ", VLHCNOX: " + resultOut.VLHCNOX + "]";
                m_log.TraceInfo(strLog);
                break;
            }

            // IF_EM_WQPF_6
            string[] deviceInfo = GetDeviceInfo(FieldUL, iCNLenb);
            if (deviceInfo[0] != null && deviceInfo[0].Length > 0) {
                resultOut.ANALYMANUF = deviceInfo[0];
                resultOut.ANALYNAME = deviceInfo[1];
                resultOut.ANALYMODEL = deviceInfo[2];
                resultOut.DYNOMODEL = deviceInfo[3];
                resultOut.DYNOMANUF = deviceInfo[4];
                strLog = "IF_EM_WQPF_6 Data: [ANALYMANUF: " + resultOut.ANALYMANUF;
                strLog += ", ANALYNAME: " + resultOut.ANALYNAME;
                strLog += ", ANALYMODEL: " + resultOut.ANALYMODEL;
                strLog += ", DYNOMODEL: " + resultOut.DYNOMODEL;
                strLog += ", DYNOMANUF: " + resultOut.DYNOMANUF + "]";
                m_log.TraceInfo(strLog);
            }
        }

        public List<UploadField> GetDynoData(UploadField FieldUL, int iCNLenb) {
            List<UploadField> resultList = new List<UploadField>();
            string strSQL = string.Format("select [VIN], [{0}] from [FinishCheckVehicles]", FieldUL.F_KEY.Name);
            strSQL += " where [Upload] = '0' and [Skip] = '0' and [JCJG] = '合格' and ([JCLX] = '初检' or [JCLX] = '复检')";
            string[,] cars = SelectDB(strSQL);
            if (cars != null) {
                m_log.TraceInfo(cars.GetLength(0) + " record(s) will upload");
                for (int i = 0; i < cars.GetLength(0); i++) {
                    UploadField result = new UploadField {
                        VIN = cars[i, 0],
                    };
                    result.F_KEY.Name = FieldUL.F_KEY.Name;
                    result.F_KEY.Value = cars[i, 1];
                    string strLog = string.Format("===== Record {0}: [VIN: {1}, {2}: {3}] =====", (i + 1), result.VIN, result.F_KEY.Name, result.F_KEY.Value);
                    m_log.TraceInfo(strLog);
                    GetUploadData(FieldUL, iCNLenb, result);
                    resultList.Add(result);
                }
            } else {
                m_log.TraceError("==> SQL: " + strSQL);
                m_log.TraceError("==> SelectDB() return null");
            }
            return resultList;
        }

        public int SetUpload(int value, F_KEY_S key) {
            string strSQL = string.Format("update [FinishCheckVehicles] set [Upload] = '{0}' where [{1}] = '{2}'", value, key.Name, key.Value);
            return RunSQL(strSQL);
        }

        public UploadField GetDynoDataByVIN(UploadField FieldUL, int iCNLenb, string strVIN) {
            UploadField result = null;
            string strSQL = string.Format("select [VIN], [{0}] from [FinishCheckVehicles] where [VIN] = '{1}'", FieldUL.F_KEY.Name, strVIN);
            string[,] cars = SelectDB(strSQL);
            if (cars != null && cars.GetLength(0) > 0) {
                result = new UploadField {
                    VIN = cars[0, 0]
                };
                result.F_KEY.Name = FieldUL.F_KEY.Name;
                result.F_KEY.Value = cars[0, 1];
                string strLog = string.Format("===== GetDynoDataByVIN: [VIN: {0}, {1}: {2}] =====", result.VIN, result.F_KEY.Name, result.F_KEY.Value);
                m_log.TraceInfo(strLog);
                GetUploadData(FieldUL, iCNLenb, result);
            }
            return result;
        }

        public Dictionary<string, string> GetKeyData(string strVIN, string keyName) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string[,] result = SelectDB(string.Format("select [{0}] from [FinishCheckVehicles] where [VIN] = '{1}'", keyName, strVIN));
            ret.Add("VIN号", strVIN);
            if (result.GetLength(0) > 0) {
                ret.Add("外检报告编号", result[0, 0]);
            }
            return ret;
        }

        public Dictionary<string, string> GetEnv(F_KEY_S fKey) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string[] env = GetEnvValue(fKey);
            ret.Add("相对湿度(%)", env[0]);
            ret.Add("环境温度(°C)", env[1]);
            ret.Add("大气压力(kPa)", env[2]);
            return ret;
        }

        public Dictionary<string, string> GetResult(UploadField FieldUL) {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            string TESTTYPE = GetFieldValue(FieldUL.TESTTYPE, FieldUL.F_KEY);
            TESTTYPE = GetTestType(TESTTYPE);
            string TESTDATE = GetFieldValue(FieldUL.TESTDATE, FieldUL.F_KEY);
            if (TESTDATE.Length == 0) {
                TESTDATE = DateTime.Now.ToLocalTime().ToString("yyyyMMdd");
            } else {
                TESTDATE = TESTDATE.Substring(0, 10).Replace("-", "");
            }
            string EPASS = GetFieldValue(FieldUL.EPASS, FieldUL.F_KEY);
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

        public Dictionary<string, string> Get51(UploadField FieldUL) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "过量空气系数结果", GetFieldValue(FieldUL.REAC, FieldUL.F_KEY, true, 3, 2) },
                { "双怠速过量空气系数上限", GetFieldValue(FieldUL.LEACMAX, FieldUL.F_KEY, true, 3, 2) },
                { "双怠速过量空气系数下限", GetFieldValue(FieldUL.LEACMIN, FieldUL.F_KEY, true, 3, 2) },
                { "低怠速CO结果(%)", GetFieldValue(FieldUL.LRCO, FieldUL.F_KEY, true, 3, 2) },
                { "低怠速CO限值(%)", GetFieldValue(FieldUL.LLCO, FieldUL.F_KEY, true, 3, 2) },
                { "低怠速HC结果(10^-6)", GetFieldValue(FieldUL.LRHC, FieldUL.F_KEY, true, 4) },
                { "低怠速HC限值(10^-6)", GetFieldValue(FieldUL.LLHC, FieldUL.F_KEY, true, 4) },
                { "高怠速CO结果(%)", GetFieldValue(FieldUL.HRCO, FieldUL.F_KEY, true, 3, 2) },
                { "高怠速CO限值(%)", GetFieldValue(FieldUL.HLCO, FieldUL.F_KEY, true, 3, 2) },
                { "高怠速HC结果(10^-6)", GetFieldValue(FieldUL.HRHC, FieldUL.F_KEY, true, 4) },
                { "高怠速HC限值(10^-6)", GetFieldValue(FieldUL.HLHC, FieldUL.F_KEY, true, 4) }
            };
            return ret;
        }

        public Dictionary<string, string> Get52(UploadField FieldUL) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "5025HC结果(10^-6)", GetFieldValue(FieldUL.ARHC5025, FieldUL.F_KEY, true, 4) },
                { "5025HC限值(10^-6)", GetFieldValue(FieldUL.ALHC5025, FieldUL.F_KEY, true, 4) },
                { "5025CO结果(%)", GetFieldValue(FieldUL.ARCO5025, FieldUL.F_KEY, true, 3, 2) },
                { "5025CO限值(%)", GetFieldValue(FieldUL.ALCO5025, FieldUL.F_KEY, true, 3, 2) },
                { "5025NO结果(10^-6)", GetFieldValue(FieldUL.ARNOX5025, FieldUL.F_KEY, true, 4) },
                { "5025NO限值(10^-6)", GetFieldValue(FieldUL.ALNOX5025, FieldUL.F_KEY, true, 4) },
                { "2540HC结果(10^-6)", GetFieldValue(FieldUL.ARHC2540, FieldUL.F_KEY, true, 4) },
                { "2540HC限值(10^-6)", GetFieldValue(FieldUL.ALHC2540, FieldUL.F_KEY, true, 4) },
                { "2540CO结果(%)", GetFieldValue(FieldUL.ARCO2540, FieldUL.F_KEY, true, 3, 2) },
                { "2540CO限值(%)", GetFieldValue(FieldUL.ALCO2540, FieldUL.F_KEY, true, 3, 2) },
                { "2540NO结果(10^-6)", GetFieldValue(FieldUL.ARNOX2540, FieldUL.F_KEY, true, 4) },
                { "2540NO限值(10^-6)", GetFieldValue(FieldUL.ALNOX2540, FieldUL.F_KEY, true, 4) }
            };
            return ret;
        }

        public Dictionary<string, string> Get53(UploadField FieldUL) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "HC结果(g/km)", GetFieldValue(FieldUL.VRHC, FieldUL.F_KEY, true, 3, 2) },
                { "HC限值(g/km)", GetFieldValue(FieldUL.VLHC, FieldUL.F_KEY, true, 3, 2) },
                { "CO结果(g/km)", GetFieldValue(FieldUL.VRCO_53, FieldUL.F_KEY, true, 3, 2) },
                { "CO限值(g/km)", GetFieldValue(FieldUL.VLCO_53, FieldUL.F_KEY, true, 3, 2) },
                { "NOx结果(g/km)", GetFieldValue(FieldUL.VRNOX, FieldUL.F_KEY, true, 3, 2) },
                { "NOx限值(g/km)", GetFieldValue(FieldUL.VLNOX, FieldUL.F_KEY, true, 3, 2) }
            };
            return ret;
        }

        public Dictionary<string, string> Get54(UploadField FieldUL) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "额定转速上限(r/min)", GetFieldValue(FieldUL.RATEREVUP, FieldUL.F_KEY, true, 5) },
                { "额定转速下限(r/min)", GetFieldValue(FieldUL.RATEREVDOWN, FieldUL.F_KEY, true, 5) },
                { "实测转速(r/min)", GetFieldValue(FieldUL.REV100, FieldUL.F_KEY, true, 5) },
                { "实测最大轮边功率(kw)", GetFieldValue(FieldUL.MAXPOWER, FieldUL.F_KEY, true, 4, 1) },
                { "最大轮边功率限值(kw)", GetFieldValue(FieldUL.MAXPOWERLIMIT, FieldUL.F_KEY, true, 4, 1) },
                { "100%点烟度(1/m)", GetFieldValue(FieldUL.SMOKE100, FieldUL.F_KEY, true, 3, 2) },
                { "80%点烟度(1/m)", GetFieldValue(FieldUL.SMOKE80, FieldUL.F_KEY, true, 3, 2) },
                { "烟度限值(1/m)", GetFieldValue(FieldUL.SMOKELIMIT, FieldUL.F_KEY, true, 3, 2) },
                { "氮氧化物测量值(10^-6)", GetFieldValue(FieldUL.NOX, FieldUL.F_KEY, true, 4) },
                { "氮氧化物限值(10^-6)", GetFieldValue(FieldUL.NOXLIMIT, FieldUL.F_KEY, true, 4) }
            };
            return ret;
        }

        public Dictionary<string, string> Get55(UploadField FieldUL) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "额定转速(r/min)", GetFieldValue(FieldUL.RATEREV, FieldUL.F_KEY, true, 5) },
                { "实测转速(r/min)", GetFieldValue(FieldUL.REV, FieldUL.F_KEY, true, 5) },
                { "第一次烟度测量值(1/m)", GetFieldValue(FieldUL.SMOKEK1, FieldUL.F_KEY, true, 3, 2) },
                { "第二次烟度测量值(1/m)", GetFieldValue(FieldUL.SMOKEK2, FieldUL.F_KEY, true, 3, 2) },
                { "第三次烟度测量值(1/m)", GetFieldValue(FieldUL.SMOKEK3, FieldUL.F_KEY, true, 3, 2) },
                { "烟度测量平均值(1/m)", GetFieldValue(FieldUL.SMOKEAVG, FieldUL.F_KEY, true, 3, 2) },
                { "烟度限值(1/m)", GetFieldValue(FieldUL.SMOKEKLIMIT, FieldUL.F_KEY, true, 3, 2) },
            };
            return ret;
        }

        public Dictionary<string, string> Get56(UploadField FieldUL) {
            Dictionary<string, string> ret = new Dictionary<string, string> {
                { "CO结果(g/km)", GetFieldValue(FieldUL.VRCO_56, FieldUL.F_KEY, true, 3, 2) },
                { "CO限值(g/km)", GetFieldValue(FieldUL.VLCO_56, FieldUL.F_KEY, true, 3, 2) },
                { "HCNOX结果(g/km)", GetFieldValue(FieldUL.VRHCNOX, FieldUL.F_KEY, true, 3, 2) },
                { "HCNOX限值(g/km)", GetFieldValue(FieldUL.VLHCNOX, FieldUL.F_KEY, true, 3, 2) },
            };
            return ret;
        }

        public void AddUploadField() {
            string strSQL = "select top (1) [Upload] from [FinishCheckVehicles]";
            string[,] rets = SelectDB(strSQL);
            if (rets == null || rets.GetLength(0) < 1) {
                strSQL = "alter table [FinishCheckVehicles] add [Upload] int not null default(0)";
                RunSQL(strSQL);
            }
        }

        public void AddSkipField() {
            string strSQL = "select top (1) [Skip] from [FinishCheckVehicles]";
            string[,] rets = SelectDB(strSQL);
            if (rets == null || rets.GetLength(0) < 1) {
                strSQL = "alter table [FinishCheckVehicles] add [Skip] int not null default(0)";
                RunSQL(strSQL);
            }
        }

        public int SetSkip(int value, F_KEY_S fKey) {
            string strSQL = string.Format("update [FinishCheckVehicles] set [Skip] = '{0}' where [{1}] = '{2}'", value, fKey.Name, fKey.Value);
            return RunSQL(strSQL);
        }

    }

    public class EnvStructure {
        public string Column { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }

        public EnvStructure(string column, int precision, int scale) {
            Column = column;
            Precision = precision;
            Scale = scale;
        }
    }
}
