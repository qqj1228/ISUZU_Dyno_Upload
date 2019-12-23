using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ISUZU_Dyno_Upload {
    public class Config {
        public const string DBSetting_xml = ".\\Configs\\DBSetting.xml";
        public const string UploadField_xml = ".\\Configs\\UploadField.xml";
        public const string DynoParameter_xml = ".\\Configs\\DynoParameter.xml";
        private readonly Logger m_log;
        public DBSetting DB { get; set; }
        public UploadField FieldUL { get; set; }
        public DynoParameter DynoParam { get; set; }

        public Config(Logger logger) {
            m_log = logger;
            LoadDBSetting();
            LoadUploadField();
            LoadDynoParameter();
        }

        public DBSetting LoadDBSetting() {
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(DBSetting));
                using (FileStream reader = new FileStream(DBSetting_xml, FileMode.Open)) {
                    DB = (DBSetting)serializer.Deserialize(reader);
                    reader.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Using default settings because of failed to load them, reason: " + ex.Message);
                DB = new DBSetting();
            }
            return DB;
        }

        public UploadField LoadUploadField() {
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(UploadField));
                using (FileStream reader = new FileStream(UploadField_xml, FileMode.Open)) {
                    FieldUL = (UploadField)serializer.Deserialize(reader);
                    reader.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Using default UploadField value because of failed to load them, reason: " + ex.Message);
                FieldUL = new UploadField();
            }
            return FieldUL;
        }

        public DynoParameter LoadDynoParameter() {
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(DynoParameter));
                using (FileStream reader = new FileStream(DynoParameter_xml, FileMode.Open)) {
                    DynoParam = (DynoParameter)serializer.Deserialize(reader);
                    reader.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Using default DynoParameter value because of failed to load them, reason: " + ex.Message);
                DynoParam = new DynoParameter();
            }
            return DynoParam;
        }

        public void SaveDBSetting() {
            if (this.DB is null) {
                throw new ArgumentNullException(nameof(this.DB));
            }
            try {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DBSetting));
                using (TextWriter writer = new StreamWriter(DBSetting_xml)) {
                    xmlSerializer.Serialize(writer, this.DB);
                    writer.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Save DB Settings error, reason: " + ex.Message);
            }
        }

        public void SaveUploadField() {
            if (this.FieldUL is null) {
                throw new ArgumentNullException(nameof(this.FieldUL));
            }
            try {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UploadField));
                using (TextWriter writer = new StreamWriter(UploadField_xml)) {
                    xmlSerializer.Serialize(writer, this.FieldUL);
                    writer.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Save UploadField error, reason: " + ex.Message);
            }
        }

        public void SaveDynoParameter() {
            if (this.DynoParam is null) {
                throw new ArgumentNullException(nameof(this.DynoParam));
            }
            try {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DynoParameter));
                using (TextWriter writer = new StreamWriter(DynoParameter_xml)) {
                    xmlSerializer.Serialize(writer, this.DynoParam);
                    writer.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Save DynoParameter error, reason: " + ex.Message);
            }
        }

    }
}
