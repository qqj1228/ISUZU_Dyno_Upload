using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ISUZU_Dyno_Upload {
    public class ModelOracle {
        public readonly Logger m_log;
        private string ConnectionMes { get; set; }
        private string ConnectionDyno { get; set; }
        public bool Connected { get; set; }

        public ModelOracle(OracleMES oracleMES, OracleMES oracleDyno, Logger log) {
            m_log = log;
            ConnectionMes = ReadConfig(oracleMES);
            ConnectionDyno = ReadConfig(oracleDyno);
            Connected = false;
        }

        string ReadConfig(OracleMES cfg) {
            string strRet = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=";
            strRet += cfg.Host + ")(PORT=";
            strRet += cfg.Port + "))(CONNECT_DATA=(SERVICE_NAME=";
            strRet += cfg.ServiceName + ")));";
            strRet += "Persist Security Info=True;";
            strRet += "User ID=" + cfg.UserID + ";";
            strRet += "Password=" + cfg.PassWord + ";";
            return strRet;
        }

        public bool ConnectOracle(string Connection) {
            Connected = false;
            try {
                OracleConnection con = new OracleConnection(Connection);
                con.Open();
                Connected = true;
                con.Close();
                con.Dispose();
            } catch (Exception ex) {
                m_log.TraceError("Connection error: " + ex.Message);
                return Connected;
            }
            return Connected;
        }

        /// <summary>
        /// 执行update insert delete语句，失败了返回-1，成功了返回影响的行数,注意：自动commit
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        private int ExecuteNonQuery(string strSQL, string Connection) {
            using (OracleConnection connection = new OracleConnection(Connection)) {
                int val = -1;
                try {
                    connection.Open();
                    OracleCommand cmd = new OracleCommand(strSQL, connection);
                    val = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                } catch (OracleException ex) {
                    m_log.TraceError("Error SQL: " + strSQL);
                    m_log.TraceError(ex.Message);
                    throw new Exception(ex.Message);
                } finally {
                    if (connection.State != ConnectionState.Closed) {
                        connection.Close();
                    }
                }
                return val;
            }
        }

        private void Query(string strSQL, DataTable dt, string Connection) {
            using (OracleConnection connection = new OracleConnection(Connection)) {
                try {
                    connection.Open();
                    OracleDataAdapter adapter = new OracleDataAdapter(strSQL, connection);
                    adapter.Fill(dt);
                } catch (OracleException ex) {
                    m_log.TraceError("Error SQL: " + strSQL);
                    m_log.TraceError(ex.Message);
                    throw new Exception(ex.Message);
                } finally {
                    if (connection.State != ConnectionState.Closed) {
                        connection.Close();
                    }
                }
            }
        }

        private object QueryOne(string strSQL, string Connection) {
            using (OracleConnection connection = new OracleConnection(Connection)) {
                using (OracleCommand cmd = new OracleCommand(strSQL, connection)) {
                    try {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value))) {
                            return null;
                        } else {
                            return obj;
                        }
                    } catch (OracleException ex) {
                        m_log.TraceError("Error SQL: " + strSQL);
                        m_log.TraceError(ex.Message);
                        throw new Exception(ex.Message);
                    } finally {
                        if (connection.State != ConnectionState.Closed) {
                            connection.Close();
                        }
                    }
                }
            }
        }

        public int InsertRecords(string strTable, DataTable dt, OracleDB DBType = OracleDB.MES) {
            int iRet = 0;
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++) {
                string strSQL = "insert into " + strTable + " (ID,";
                for (int iCol = 1; iCol < dt.Columns.Count; iCol++) {
                    if (dt.Rows[iRow][iCol].ToString().Length != 0) {
                        strSQL += dt.Columns[iCol].ColumnName + ",";
                    }
                }
                strSQL = strSQL.Trim(',');
                strSQL += ") values (SEQ_EM_WQPF_ID.NEXTVAL,";
                for (int iCol = 1; iCol < dt.Columns.Count; iCol++) {
                    if (dt.Rows[iRow][iCol].ToString().Length != 0) {
                        if (dt.Columns[iCol].DataType == typeof(DateTime)) {
                            strSQL += "to_date('" + ((DateTime)dt.Rows[iRow][iCol]).ToString("yyyyMMdd-HHmmss") + "', 'yyyymmdd-HH24MISS'),";
                        } else {
                            strSQL += "'" + dt.Rows[iRow][iCol].ToString() + "',";
                        }
                    }
                }
                strSQL = strSQL.Trim(',');
                strSQL += ")";
                if (DBType == OracleDB.MES) {
                    iRet += ExecuteNonQuery(strSQL, ConnectionMes);
                } else if (DBType == OracleDB.Dyno) {
                    iRet += ExecuteNonQuery(strSQL, ConnectionDyno);
                }
            }
            return iRet;
        }

        public int UpdateRecords(string strTable, DataTable dt, string strWhereKey, string[] strWhereValues, OracleDB DBType = OracleDB.MES) {
            int iRet = 0;
            if (dt.Rows.Count != strWhereValues.Length) {
                return -1;
            }
            for (int iRow = 0; iRow < dt.Rows.Count; iRow++) {
                string strSQL = "update " + strTable + " set ";
                for (int iCol = 1; iCol < dt.Columns.Count; iCol++) {
                    if (dt.Rows[iRow][iCol].ToString().Length != 0 && dt.Rows[iRow][iCol].ToString() != dt.Columns[iCol].ColumnName) {
                        if (dt.Columns[iCol].DataType == typeof(DateTime)) {
                            strSQL += dt.Columns[iCol].ColumnName + "=" + "to_date('" + ((DateTime)dt.Rows[iRow][iCol]).ToString("yyyyMMdd-HHmmss") + "', 'yyyymmdd-HH24MISS'),";
                        } else {
                            strSQL += dt.Columns[iCol].ColumnName + "='" + dt.Rows[iRow][iCol].ToString() + "',";
                        }
                    }
                }
                strSQL = strSQL.Trim(',');
                strSQL += " where " + strWhereKey + "='" + strWhereValues[iRow] + "'";
                if (DBType == OracleDB.MES) {
                    iRet += ExecuteNonQuery(strSQL, ConnectionMes);
                } else if (DBType == OracleDB.Dyno) {
                    iRet += ExecuteNonQuery(strSQL, ConnectionDyno);
                }
            }
            return iRet;
        }

        public string[] GetValue(string strTable, string strField, string strWhereKey, string strWhereValue, OracleDB DBType = OracleDB.MES) {
            string strSQL = "select " + strField + " from " + strTable + " where " + strWhereKey + " = '" + strWhereValue + "'";
            DataTable dt = new DataTable();
            if (DBType == OracleDB.MES) {
                Query(strSQL, dt, ConnectionMes);
            } else if (DBType == OracleDB.Dyno) {
                Query(strSQL, dt, ConnectionDyno);
            }
            string[] values = new string[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; i++) {
                values[i] = dt.Rows[i][0].ToString();
            }
            Array.Sort(values);
            dt.Dispose();
            return values;
        }


        public int GetCNLenb(OracleDB DBType = OracleDB.MES) {
            int iRet = 0;
            string strSQL = "select lengthb('好') from dual";
            if (DBType == OracleDB.MES) {
                iRet = Convert.ToInt32(QueryOne(strSQL, ConnectionMes));
            } else if (DBType == OracleDB.Dyno) {
                iRet = Convert.ToInt32(QueryOne(strSQL, ConnectionDyno));
            }
            return iRet;
        }

        public void GetEmissionInfo(string strVIN, EmissionInfo ei) {
            string strSQL = "select * from IF_VEHICLEINFO1 where VIN = '" + strVIN + "'";
            DataTable dt1 = new DataTable("IF_VehicleInfo1");
            Query(strSQL, dt1, ConnectionDyno);
            if (dt1.Rows.Count > 0) {
                DataRow dr1 = dt1.Rows[dt1.Rows.Count - 1];
                ei.VehicleInfo1.VIN = dr1["VIN"].ToString();
                ei.VehicleInfo1.VehicleType = dr1["VEHICLETYPE"].ToString();
                ei.VehicleInfo1.ISQZ = dr1["ISQZ"].ToString();
                ei.VehicleInfo1.CLXH = dr1["CLXH"].ToString();
                ei.VehicleInfo1.FDJXH = dr1["FDJXH"].ToString();
                ei.VehicleInfo1.HasOBD = dr1["HASOBD"].ToString();
                ei.VehicleInfo1.FuelType = dr1["FUELTYPE"].ToString();
                ei.VehicleInfo1.Standard = dr1["STANDARD"].ToString();
            }
            strSQL = "select * from IF_VEHICLEINFO2 where VIN = '" + strVIN + "'";
            DataTable dt2 = new DataTable("IF_VehicleInfo2");
            Query(strSQL, dt2, ConnectionDyno);
            if (dt2.Rows.Count > 0) {
                DataRow dr2 = dt2.Rows[dt2.Rows.Count - 1];
                ei.VehicleInfo2.VIN = dr2["VIN"].ToString();
                ei.VehicleInfo2.VehicleKind = dr2["VEHICLEKIND"].ToString();
                ei.VehicleInfo2.VehicleType = dr2["VEHICLETYPE"].ToString();
                ei.VehicleInfo2.Model = dr2["MODEL"].ToString();
                ei.VehicleInfo2.GearBoxType = dr2["GEARBOXTYPE"].ToString();
                ei.VehicleInfo2.AdmissionMode = dr2["ADMISSIONMODE"].ToString();
                ei.VehicleInfo2.Volume = dr2["VOLUME"].ToString();
                ei.VehicleInfo2.FuelType = dr2["FUELTYPE"].ToString();
                ei.VehicleInfo2.RatedRev = dr2["RATEDREV"].ToString();
                ei.VehicleInfo2.RatedPower = dr2["RATEDPOWER"].ToString();
                ei.VehicleInfo2.DriveMode = dr2["DRIVEMODE"].ToString();
                ei.VehicleInfo2.MaxMass = dr2["MAXMASS"].ToString();
                ei.VehicleInfo2.RefMass = dr2["REFMASS"].ToString();
                ei.VehicleInfo2.HasODB = dr2["HASODB"].ToString();
                ei.VehicleInfo2.HasPurge = dr2["HASPURGE"].ToString();
                ei.VehicleInfo2.IsEFI = dr2["ISEFI"].ToString();
                ei.VehicleInfo2.MaxLoad = dr2["MAXLOAD"].ToString();
                ei.VehicleInfo2.CarOrTruck = dr2["CARORTRUCK"].ToString();
                ei.VehicleInfo2.Cylinder = dr2["CYLINDER"].ToString();
                ei.VehicleInfo2.IsTransform = dr2["ISTRANSFORM"].ToString();
                ei.VehicleInfo2.StandardID = dr2["STANDARDID"].ToString();
                ei.VehicleInfo2.IsAsm = dr2["ISASM"].ToString();
                ei.VehicleInfo2.QCZZCJ = dr2["QCZZCJ"].ToString();
                ei.VehicleInfo2.FDJZZC = dr2["FDJZZC"].ToString();
                ei.VehicleInfo2.DDJXH = dr2["DDJXH"].ToString();
                ei.VehicleInfo2.XNZZXH = dr2["XNZZXH"].ToString();
                ei.VehicleInfo2.CHZHQXH = dr2["CHZHQXH"].ToString();
                ei.VehicleInfo2.SCR = dr2["SCR"].ToString();
                ei.VehicleInfo2.SCRXH = dr2["SCRXH"].ToString();
                ei.VehicleInfo2.DPF = dr2["DPF"].ToString();
                ei.VehicleInfo2.DPFXH = dr2["DPFXH"].ToString();
                ei.VehicleInfo2.DCRL = dr2["DCRL"].ToString();
                ei.VehicleInfo2.JCFF = dr2["JCFF"].ToString();
            }
        }
    }
}
