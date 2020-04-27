using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISUZU_Dyno_Upload {
    public enum OracleDB {
        Auto,
        MES,
        Dyno,
    }

    [Serializable]
    public class OracleMES {
        public string Host { get; set; }
        public string Port { get; set; }
        public string ServiceName { get; set; }
        public string UserID { get; set; }
        public string PassWord { get; set; }

        public OracleMES() {
            // 此默认值是MES中间表默认参数
            Host = "10.50.252.106";
            Port = "1561";
            ServiceName = "IUAT2";
            UserID = "idevice";
            PassWord = "idevice";
        }

    }

    [Serializable]
    public class SqlServerNative {
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string DBName { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }

        public SqlServerNative() {
            UserName = "sa";
            PassWord = "sh49";
            DBName = "Orient_ASM";
            IP = "127.0.0.1";
            Port = "1433";
        }

    }

    [Serializable]
    public class DBSetting {
        public OracleMES Oracle { get; set; }
        public SqlServerNative SqlServer { get; set; }
        public OracleMES Dyno { get; set; }
        public int Interval { get; set; }
        public string Name { get; set; }
        public int LastID { get; set; }

        public DBSetting() {
            Oracle = new OracleMES();
            SqlServer = new SqlServerNative();
            Dyno = new OracleMES();
            Interval = 5;
            Name = "Emission";
            LastID = 0;
        }
    }

    [Serializable]
    public class UploadField {
        // IF_EM_WQPF_1
        public string VIN { get; set; }
        public string JCLSH { get; set; } // 检测流水号，在IF_EM_WQPF_1表中没有，存在这里，程序中需要用到
        // IF_EM_WQPF_2
        public string RH { get; set; }
        public string ET { get; set; }
        public string AP { get; set; }
        // IF_EM_WQPF_3
        public string TESTTYPE { get; set; }
        public string TESTNO { get; set; }
        public string TESTDATE { get; set; }
        public string APASS { get; set; }
        public string OPASS { get; set; }
        public string OTESTDATE { get; set; }
        public string EPASS { get; set; }
        public string RESULT { get; set; }
        // IF_EM_WQPF_4
        public string OBD { get; set; }
        public string ODO { get; set; }
        // IF_EM_WQPF_4_A
        public string MODULEID { get; set; }
        public string CALID { get; set; }
        public string CVN { get; set; }
        // IF_EM_WQPF_5_1
        public string REAC { get; set; }
        public string LEACMAX { get; set; }
        public string LEACMIN { get; set; }
        public string LRCO { get; set; }
        public string LLCO { get; set; }
        public string LRHC { get; set; }
        public string LLHC { get; set; }
        public string HRCO { get; set; }
        public string HLCO { get; set; }
        public string HRHC { get; set; }
        public string HLHC { get; set; }
        // IF_EM_WQPF_5_2
        public string ARHC5025 { get; set; }
        public string ALHC5025 { get; set; }
        public string ARCO5025 { get; set; }
        public string ALCO5025 { get; set; }
        public string ARNOX5025 { get; set; }
        public string ALNOX5025 { get; set; }
        public string ARHC2540 { get; set; }
        public string ALHC2540 { get; set; }
        public string ARCO2540 { get; set; }
        public string ALCO2540 { get; set; }
        public string ARNOX2540 { get; set; }
        public string ALNOX2540 { get; set; }
        // IF_EM_WQPF_5_3
        public string VRHC { get; set; }
        public string VLHC { get; set; }
        public string VRCO_53 { get; set; }
        public string VLCO_53 { get; set; }
        public string VRNOX { get; set; }
        public string VLNOX { get; set; }
        // IF_EM_WQPF_5_4
        public string RATEREVUP { get; set; }
        public string RATEREVDOWN { get; set; }
        public string REV100 { get; set; }
        public string MAXPOWER { get; set; }
        public string MAXPOWERLIMIT { get; set; }
        public string SMOKE100 { get; set; }
        public string SMOKE80 { get; set; }
        public string SMOKELIMIT { get; set; }
        public string NOX { get; set; }
        public string NOXLIMIT { get; set; }
        // IF_EM_WQPF_5_5
        public string RATEREV { get; set; }
        public string REV { get; set; }
        public string SMOKEK1 { get; set; }
        public string SMOKEK2 { get; set; }
        public string SMOKEK3 { get; set; }
        public string SMOKEAVG { get; set; }
        public string SMOKEKLIMIT { get; set; }
        // IF_EM_WQPF_5_6
        public string VRCO_56 { get; set; }
        public string VLCO_56 { get; set; }
        public string VRHCNOX { get; set; }
        public string VLHCNOX { get; set; }
        // IF_EM_WQPF_6
        public string ANALYMANUF { get; set; }
        public string ANALYNAME { get; set; }
        public string ANALYMODEL { get; set; }
        public string ANALYDATE { get; set; }
        public string DYNOMODEL { get; set; }
        public string DYNOMANUF { get; set; }

        public UploadField() {
            VIN = "";
            RH = "";
            ET = "";
            AP = "";
            TESTTYPE = "";
            TESTNO = "";
            TESTDATE = "";
            APASS = "";
            OPASS = "";
            OTESTDATE = "";
            EPASS = "";
            RESULT = "";
            OBD = "";
            ODO = "";
            MODULEID = "";
            CALID = "";
            CVN = "";
            REAC = "";
            LEACMAX = "";
            LEACMIN = "";
            LRCO = "";
            LLCO = "";
            LRHC = "";
            LLHC = "";
            HRCO = "";
            HLCO = "";
            HRHC = "";
            HLHC = "";
            ARHC5025 = "";
            ALHC5025 = "";
            ARCO5025 = "";
            ALCO5025 = "";
            ARNOX5025 = "";
            ALNOX5025 = "";
            ARHC2540 = "";
            ALHC2540 = "";
            ARCO2540 = "";
            ALCO2540 = "";
            ARNOX2540 = "";
            ALNOX2540 = "";
            VRHC = "";
            VLHC = "";
            VRCO_53 = "";
            VLCO_53 = "";
            VRNOX = "";
            VLNOX = "";
            RATEREVUP = "";
            RATEREVDOWN = "";
            REV100 = "";
            MAXPOWER = "";
            MAXPOWERLIMIT = "";
            SMOKE100 = "";
            SMOKE80 = "";
            SMOKELIMIT = "";
            NOX = "";
            NOXLIMIT = "";
            RATEREV = "";
            REV = "";
            SMOKEK1 = "";
            SMOKEK2 = "";
            SMOKEK3 = "";
            SMOKEAVG = "";
            SMOKEKLIMIT = "";
            VRCO_56 = "";
            VLCO_56 = "";
            VRHCNOX = "";
            VLHCNOX = "";
            ANALYMANUF = "";
            ANALYNAME = "";
            ANALYMODEL = "";
            ANALYDATE = "";
            DYNOMODEL = "";
            DYNOMANUF = "";
        }
    }

    [Serializable]
    public class DynoParameter {
        public bool Enable { get; set; }
        public int TCPPort { get; set; }
        public bool UseSimData { get; set; }

        public DynoParameter() {
            Enable = false;
            TCPPort = 50001;
            UseSimData = false;
        }
    }

    [Serializable]
    public class VehicleInfo1Class {
        public string License { get; set; }
        public string VIN { get; set; }
        public string RegisterDate { get; set; }
        public string ISQZ { get; set; }
        public string VehicleType { get; set; }
        public string CLXH { get; set; }
        public string FDJXH { get; set; }
        public string HasOBD { get; set; }
        public string FuelType { get; set; }
        public string Standard { get; set; }
        public string OBDCommCL { get; set; }
        public string OBDCommCX { get; set; }
    }

    [Serializable]
    public class VehicleInfo2Class {
        public string VehicleKind { get; set; }
        public string License { get; set; }
        public string VIN { get; set; }
        public string RegisterDate { get; set; }
        public string VehicleType { get; set; }
        public string Model { get; set; }
        public string GearBoxType { get; set; }
        public string AdmissionMode { get; set; }
        public string Volume { get; set; }
        public string Odometer { get; set; }
        public string FuelType { get; set; }
        public string SupplyMode { get; set; }
        public string RatedRev { get; set; }
        public string RatedPower { get; set; }
        public string DriveMode { get; set; }
        public string Owner { get; set; }
        public string Address { get; set; }
        public string MaxMass { get; set; }
        public string RefMass { get; set; }
        public string HasODB { get; set; }
        public string Phone { get; set; }
        public string HasPurge { get; set; }
        public string IsEFI { get; set; }
        public string MaxLoad { get; set; }
        public string CarOrTruck { get; set; }
        public string Cylinder { get; set; }
        public string IsTransform { get; set; }
        public string StandardID { get; set; }
        public string IsAsm { get; set; }
        public string QCZZCJ { get; set; }
        public string FDJZZC { get; set; }
        public string DDJXH { get; set; }
        public string XNZZXH { get; set; }
        public string CHZHQXH { get; set; }
        public string HPYS { get; set; }
        public string SCR { get; set; }
        public string SCRXH { get; set; }
        public string DPF { get; set; }
        public string DPFXH { get; set; }
        public string DCRL { get; set; }
        public string JCFF { get; set; }
    }

    [Serializable]
    public class LimitValueClass {
        public string AmbientCOUp { get; set; }
        public string AmbientCO2Up { get; set; }
        public string AmbientHCUp { get; set; }
        public string AmbientNOUp { get; set; }
        public string BackgroundCOUp { get; set; }
        public string BackgroundCO2Up { get; set; }
        public string BackgroundHCUp { get; set; }
        public string BackgroundNOUp { get; set; }
        public string ResidualHCUp { get; set; }
        public string CO5025 { get; set; }
        public string HC5025 { get; set; }
        public string NO5025 { get; set; }
        public string Lambda5025Up { get; set; }
        public string Lambda5025Below { get; set; }
        public string CO2540 { get; set; }
        public string HC2540 { get; set; }
        public string NO2540 { get; set; }
        public string Lambda2540Up { get; set; }
        public string Lambda2540Below { get; set; }
        public string COAndCO2 { get; set; }
        public string HighIdleCO { get; set; }
        public string HighIdleHC { get; set; }
        public string IdleCO { get; set; }
        public string IdleHC { get; set; }
        public string FASmokeHSU { get; set; }
        public string FASmokeK { get; set; }
        public string SmokeK { get; set; }
        public string SmokeHSU { get; set; }
        public string SmokeNO { get; set; }
        public string MaxPower { get; set; }
    }

    [Serializable]
    public class EmissionInfo {
        public VehicleInfo1Class VehicleInfo1 { get; set; }
        public VehicleInfo2Class VehicleInfo2 { get; set; }
        public LimitValueClass LimitValue { get; set; }
        public EmissionInfo() {
            VehicleInfo1 = new VehicleInfo1Class();
            VehicleInfo2 = new VehicleInfo2Class();
            LimitValue = new LimitValueClass();
        }
    }

}
