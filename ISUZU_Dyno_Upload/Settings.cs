﻿using System;
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

    // 表示外检报告编号/检测流水号的字段，作为外键使用
    [Serializable]
    public class F_KEY_S {
        public string Name { get; set; } // 外键的字段名
        public string Value { get; set; } // 外键的字段值
        public F_KEY_S() {
            Name = "";
            Value = "";
        }
    }

    [Serializable]
    public class UploadField {
        public F_KEY_S F_KEY { get; set; }
        // IF_EM_WQPF_1
        public string VIN { get; set; }
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
            F_KEY = new F_KEY_S();
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
    public class EmissionInfo {
        public string VehicleModel { get; set; } // 车辆型号
        public string OpenInfoSN { get; set; } // 信息公开编号
        public string VehicleMfr { get; set; } // 车辆生产企业
        public string EngineModel { get; set; } // 发动机型号
        public string EngineSN { get; set; } // 发动机编号
        public string EngineMfr { get; set; } // 发动机生产企业
        public double EngineVolume { get; set; } // 发动机排量
        public int CylinderQTY { get; set; } // 气缸数量
        public int FuelSupply { get; set; } // 燃油供给系统
        public double RatedPower { get; set; } // 额定功率
        public int RatedRPM { get; set; } // 额定转速
        public int EmissionStage { get; set; } // 车辆排放阶段
        public int Transmission { get; set; } // 变速箱形式
        public string CatConverter { get; set; } // 催化转化器
        public int RefMass { get; set; } // 基准质量
        public int MaxMass { get; set; } // 最大设计总质量
        public string OBDLocation { get; set; } // OBD接口位置
        public string PostProcessorType { get; set; } // 后处理类型
        public string PostProcessorModel { get; set; } // 后处理型号
        public string MotorModel { get; set; } // 电动机型号
        public string EnergyStorage { get; set; } // 储能装置型号
        public string BatteryCap { get; set; } // 电池容量
        public int TestMethod { get; set; } // 检测方法
        public string Name { get; set; } // 检验员名字
    }

}
