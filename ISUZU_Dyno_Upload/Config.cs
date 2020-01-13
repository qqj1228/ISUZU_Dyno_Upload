using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ISUZU_Dyno_Upload {
    public class ConfigFile<T> where T: new() {
        public string File_xml { get; }
        public T Data { get; set; }
        public string Name {
            get { return Path.GetFileName(File_xml).Split('.')[0]; }
        }

        public ConfigFile(string xml) {
            this.File_xml = xml;
        }
    }

    public class Config {
        private readonly Logger m_log;
        public ConfigFile<DBSetting> DB { get; set; }
        public ConfigFile<UploadField> FieldUL { get; set; }
        public ConfigFile<DynoParameter> DynoParam { get; set; }
        public ConfigFile<EmissionInfo> DynoSimData { get; set; }

        public Config(Logger logger) {
            m_log = logger;
            DB = new ConfigFile<DBSetting>(".\\Configs\\DBSetting.xml");
            FieldUL = new ConfigFile<UploadField>(".\\Configs\\UploadField.xml");
            DynoParam = new ConfigFile<DynoParameter>(".\\Configs\\DynoSetting.xml");
            DynoSimData = new ConfigFile<EmissionInfo>(".\\Configs\\DynoSimData.xml");
            LoadConfig(DB);
            LoadConfig(FieldUL);
            LoadConfig(DynoParam);
            LoadConfig(DynoSimData);
        }

        public void LoadConfig<T>(ConfigFile<T> config) where T : new() {
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (FileStream reader = new FileStream(config.File_xml, FileMode.Open)) {
                    config.Data = (T)serializer.Deserialize(reader);
                    reader.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Using default "+ config.Name +" because of failed to load them, reason: " + ex.Message);
                config.Data = new T();
            }
        }

        public void SaveConfig<T>(ConfigFile<T> config) where T : new() {
            if (config == null || config.Data == null) {
                throw new ArgumentNullException(nameof(config.Data));
            }
            try {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (TextWriter writer = new StreamWriter(config.File_xml)) {
                    xmlSerializer.Serialize(writer, config.Data);
                    writer.Close();
                }
            } catch (Exception ex) {
                m_log.TraceError("Save "+ config.Name +" error, reason: " + ex.Message);
            }
        }
    }
}
